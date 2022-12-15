//FirstPersonController.cs extends the FirstPersonController.cs from Unity's Starter Assets - First Person Character Controller
//https://assetstore.unity.com/packages/essentials/starter-assets-first-person-character-controller-196525

using System;
using BML.ScriptableObjectCore.Scripts.Variables;
using BML.ScriptableObjectCore.Scripts.Events;
using BML.ScriptableObjectCore.Scripts.Variables.SafeValueReferences;
using BML.Scripts.Utils;
using KinematicCharacterController;
using UnityEngine;
using MoreMountains.Tools;
using Sirenix.OdinInspector;
using UnityEngine.Serialization;
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
using UnityEngine.InputSystem;
#endif

namespace BML.Scripts.Player
{
	[RequireComponent(typeof(KinematicCharacterMotor))]
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
	[RequireComponent(typeof(PlayerInput))]
#endif
	public class FirstPersonController : MonoBehaviour, ICharacterController
	{

		#region Inspector

		[Tooltip("Move speed of the character in m/s")]
		[SerializeField, FoldoutGroup("Player")] SafeFloatValueReference MoveSpeed;
		[Tooltip("Sprint speed of the character in m/s")]
		[SerializeField, FoldoutGroup("Player")] SafeFloatValueReference SprintSpeed;
		[Tooltip("Rotation speed of the character")]
		[SerializeField, FoldoutGroup("Player")] float RotationSpeed = 1.0f;
		[Tooltip("Acceleration and deceleration")]
		[SerializeField, FoldoutGroup("Player")] float SpeedChangeRate = 10.0f;
		[Tooltip("Curve for analog look input smoothing")]
		[SerializeField, FoldoutGroup("Player")] AnimationCurve AnalogMovementCurve;
		[Tooltip("Factor for look acceleration setting")]
		[SerializeField, FoldoutGroup("Player")] float lookAccelerationFactor = .0001f;
		[Tooltip("Rate of look acceleration")]
		[SerializeField, FoldoutGroup("Player")] FloatReference LookAcceleration;

		[Space(10)]
		[Tooltip("The height the player can jump")]
		[SerializeField, FoldoutGroup("Player")] float JumpHeight = 1.2f;
		[Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
		[SerializeField, FoldoutGroup("Player")] float Gravity = -15.0f;

		[Space(10)]
		[Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
		[SerializeField, FoldoutGroup("Player")] float JumpTimeout = 0.1f;
		[Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
		[SerializeField, FoldoutGroup("Player")] float FallTimeout = 0.15f;
		
		[Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
		[SerializeField, FoldoutGroup("Player Grounded")] bool Grounded = true;
		[SerializeField, FoldoutGroup("Player Grounded")] BoolVariable IsGrounded;
		
		[SerializeField, FoldoutGroup("Knockback")] float KnockbackVerticalForce = 10f;
		[SerializeField, FoldoutGroup("Knockback")] float KnockbackDuration = .2f;
		[SerializeField, FoldoutGroup("Knockback")] AnimationCurve KnockbackHorizontalForceCurve;

		[SerializeField, FoldoutGroup("RopeMovement")] private GameEvent _playerEnteredRopeEvent;
		[SerializeField, FoldoutGroup("RopeMovement")] private BoolReference _isRopeMovementEnabled;
		[SerializeField, FoldoutGroup("RopeMovement")] private float _ropeMovementSpeed = 15;
		[SerializeField, FoldoutGroup("RopeMovement")] private float _ropeGravitySpeed = 1;
        [SerializeField, FoldoutGroup("RopeMovement"), Sirenix.OdinInspector.ReadOnly] private bool reachedRopeBottom = false;
        [SerializeField, FoldoutGroup("RopeMovement"), Sirenix.OdinInspector.ReadOnly] private bool reachedRopeTop = false;
        
		[SerializeField, FoldoutGroup("No Clip Mode")] private BoolVariable isNoClipEnabled;
		[SerializeField, FoldoutGroup("No Clip Mode")] private float noClipUpDownVelocity = 15f;
		[SerializeField, FoldoutGroup("No Clip Mode")] private float noClipSprintMultiplier = 3f;
		[SerializeField, FoldoutGroup("No Clip Mode")] private LayerMask noClipCollisionMask;

		[Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
		[SerializeField, FoldoutGroup("Cinemachine")] GameObject CinemachineCameraTarget;
		[Tooltip("How far in degrees can you move the camera up")]
		[SerializeField, FoldoutGroup("Cinemachine")] float TopClamp = 90.0f;
		[Tooltip("How far in degrees can you move the camera down")]
		[SerializeField, FoldoutGroup("Cinemachine")] float BottomClamp = -90.0f;

		#endregion

		// cinemachine
		private float _cinemachineTargetYaw;
		private float _cinemachineTargetPitch;

		// player
		private float _speed;
		private float _rotationVelocity;
		private float _verticalVelocity;
		private float _terminalVelocity = 53.0f;

		// timeout deltatime
		private float _jumpTimeoutDelta;
		private float _fallTimeoutDelta;
		
		// knockback
		private Vector3 _knockbackDir;
		private bool _knockbackActive = false;
		private float knockbackStartTime = Mathf.NegativeInfinity;
		private float percentToEndKnockback = 0f;
		
		// no clip mode
		private float originalGravity;
		private LayerMask orignalCollisionMask;

		private PlayerInput _playerInput;
		private KinematicCharacterMotor _motor;
		private PlayerInputProcessor _input;
		private GameObject _mainCamera;
		private float previouRotSpeed = 0f;
		
		private bool IsCurrentDeviceMouse
		{
			get => _playerInput.currentControlScheme == "Keyboard&Mouse";
			
		}

		#region Unity Lifecycle

		private void Awake()
		{
			// get a reference to our main camera
			if (_mainCamera == null)
			{
				_mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
			}
			
			_motor = GetComponent<KinematicCharacterMotor>();
			_motor.CharacterController = this;
			_input = GetComponent<PlayerInputProcessor>();
			_playerInput = GetComponent<PlayerInput>();
			originalGravity = Gravity;
			orignalCollisionMask = _motor.CollidableLayers;
		}

		private void Start()
		{
			// reset our timeouts on start
			_jumpTimeoutDelta = JumpTimeout;
			_fallTimeoutDelta = FallTimeout;
		}
		
		private void OnEnable()
		{
			isNoClipEnabled.Subscribe(SetNoClip);
            _playerEnteredRopeEvent.Subscribe(OnPlayerEnteredRope);
		}

		private void OnDisable()
		{
			isNoClipEnabled.Unsubscribe(SetNoClip);
            _playerEnteredRopeEvent.Unsubscribe(OnPlayerEnteredRope);
		}

		private void Update()
		{
			if (knockbackStartTime + KnockbackDuration < Time.time)
				_knockbackActive = false;
			else
			{
				percentToEndKnockback = (Time.time - knockbackStartTime) / KnockbackDuration;
			}
		}

		private void LateUpdate()
		{
			CameraRotation();
		}

		#endregion

		#region Kinematic Character Controller

		public void BeforeCharacterUpdate(float deltaTime)
	    {
	        // This is called before the motor does anything
	        JumpAndGravity();
	        GroundedCheck();
	    }

	    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
	    {
		    
	    }

	    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
	    {
		    // Kill Y velocity if just get grounded so not to slide
		    // Without this, y velocity is converted to horizontal velocity when land
		    if (_motor.GroundingStatus.IsStableOnGround && !_motor.LastGroundingStatus.IsStableOnGround)
		    {
		        currentVelocity = Vector3.ProjectOnPlane(currentVelocity , _motor.CharacterUp);
		        currentVelocity  = _motor.GetDirectionTangentToSurface(currentVelocity ,
			        _motor.GroundingStatus.GroundNormal) * currentVelocity .magnitude;
		    }

            if(_isRopeMovementEnabled.Value) {
                float moveDirection = _input.move.y;
                float movementSpeed = _ropeMovementSpeed;

                if(_input.move.y != 0) {
                    if(reachedRopeBottom && moveDirection < 0) {
                    moveDirection = 0;
                }

                if(reachedRopeTop && moveDirection > 0) {
                    moveDirection = 0;
                }
                } else {
                    moveDirection = !reachedRopeBottom ? -1 : 0;
                    movementSpeed = _ropeGravitySpeed;
                }

                currentVelocity = movementSpeed * Vector3.up * moveDirection;
                return;
            }
		    
		    // set target speed based on move speed, sprint speed and if sprint is pressed
		    float sprintSpeed = isNoClipEnabled.Value ? SprintSpeed.Value * noClipSprintMultiplier : SprintSpeed.Value;
		    float targetSpeed = _input.sprint ? sprintSpeed : MoveSpeed.Value;

		    // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

		    // note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
		    // if there is no input, set the target speed to 0
		    if (_input.move == Vector2.zero) targetSpeed = 0.0f;

		    // a reference to the players current horizontal velocity
		    float currentHorizontalSpeed = currentVelocity.xoz().magnitude;

		    float speedOffset = 0.1f;
		    float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

		    // accelerate or decelerate to target speed
		    if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
		    {
			    // creates curved result rather than a linear one giving a more organic speed change
			    // note T in Lerp is clamped, so we don't need to clamp our speed
			    _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);

			    // round speed to 3 decimal places
			    _speed = Mathf.Round(_speed * 1000f) / 1000f;
		    }
		    else
		    {
			    _speed = targetSpeed;
		    }
		    
		    Vector3 inputDirection = (_input.move.y * _mainCamera.transform.forward.xoz().normalized +
		                              _input.move.x * _mainCamera.transform.right.xoz().normalized)
									.normalized;
		    
			if (isNoClipEnabled.Value)
				inputDirection = (_input.move.y * _mainCamera.transform.forward.normalized +
				                  _input.move.x * _mainCamera.transform.right.normalized)
					.normalized;

			Vector3 horizontalVelocity = inputDirection * _speed;
		    
		    if (_knockbackActive)
			    horizontalVelocity = _knockbackDir * KnockbackHorizontalForceCurve.Evaluate(percentToEndKnockback);

		    // move the player
		    currentVelocity = horizontalVelocity + 
		                      new Vector3(0.0f, _verticalVelocity, 0.0f);
	    }

	    public void AfterCharacterUpdate(float deltaTime)
	    {
		    // This is called after the motor has finished everything in its update
	    }

	    public bool IsColliderValidForCollisions(Collider coll)
	    {
	        // This is called after when the motor wants to know if the collider can be collided with (or if we just go through it)
	        return true;
	    }

	    public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint,
	        ref HitStabilityReport hitStabilityReport)
	    {
	        // This is called when the motor's ground probing detects a ground hit
	    }

        void OnTriggerEnter(Collider collider) {
            if(_isRopeMovementEnabled.Value && collider.gameObject.tag == "RopeTop") {
                reachedRopeTop = true;
            }

            if(_isRopeMovementEnabled.Value && collider.gameObject.tag == "RopeBottom") {
                reachedRopeBottom = true;
            }
        }

        void OnTriggerExit(Collider collider) {
            if(collider.gameObject.tag == "RopeTop") {
                reachedRopeTop = false;
            }

            if(collider.gameObject.tag == "RopeBottom") {
                reachedRopeBottom = false;
            }
        }

	    public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint,
	        ref HitStabilityReport hitStabilityReport)
	    {
            // This is called when the motor's movement logic detects a hit
	    }

	    public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint,
	        Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
	    {
	        // This is called after every hit detected in the motor, to give you a chance to modify the HitStabilityReport any way you want
	    }

	    public void PostGroundingUpdate(float deltaTime)
	    {
	        // This is called after the motor has finished its ground probing, but before PhysicsMover/Velocity/etc.... handling
	    }

	    public void OnDiscreteCollisionDetected(Collider hitCollider)
	    {
	        // This is called by the motor when it is detecting a collision that did not result from a "movement hit".
	    }

		#endregion

		private void GroundedCheck()
		{
			Grounded = _motor.GroundingStatus.FoundAnyGround;
            if(_isRopeMovementEnabled.Value && Grounded) {
                _motor.ForceUnground();
                Grounded = false;
            }
            
            //Only update when changed
            if (IsGrounded.Value != Grounded)
	            IsGrounded.Value = Grounded;
		}

		private void CameraRotation()
		{
			//Don't multiply mouse input by Time.deltaTime
			float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;
			
			float rotSpeed = RotationSpeed;
			float lookAcceleration = LookAcceleration.Value * lookAccelerationFactor;
			
			if (!IsCurrentDeviceMouse)
			{
				//For analog movement, dont interpolate linearly
				rotSpeed *= AnalogMovementCurve.Evaluate(_input.lookUnscaled.magnitude);
				
				float dummy = 0f;
				float rotSpeedAccelerated;

				//Accelerate to higher values but stop immediately
				if (rotSpeed > previouRotSpeed)
					rotSpeedAccelerated = Mathf.SmoothDamp(previouRotSpeed, rotSpeed, ref dummy, lookAcceleration);
				else
					rotSpeedAccelerated = rotSpeed;
				
				//Debug.Log($"prev: {previouRotSpeed} | target: {String.Format("{0:0.00}", rotSpeed)}");

				rotSpeed = rotSpeedAccelerated;
				previouRotSpeed = rotSpeedAccelerated;
			}
			else
			{
				float dummy = 0f;
				float rotSpeedAccelerated;

				//Accelerate to higher values but stop immediately
				if (rotSpeed > previouRotSpeed)
					rotSpeedAccelerated = Mathf.SmoothDamp(previouRotSpeed, rotSpeed, ref dummy, lookAcceleration);
				else
					rotSpeedAccelerated = rotSpeed;

				rotSpeed = rotSpeedAccelerated;
				previouRotSpeed = rotSpeedAccelerated;
			}

			if (Mathf.Approximately(0f, _input.look.magnitude))
				previouRotSpeed = 0f;
				
			
			_cinemachineTargetYaw += _input.look.x * rotSpeed * deltaTimeMultiplier;
			_cinemachineTargetPitch += _input.look.y * rotSpeed * deltaTimeMultiplier;

			// clamp our pitch rotation
			_cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

			// Update Cinemachine camera target pitch
			CinemachineCameraTarget.transform.localRotation = Quaternion.Euler(_cinemachineTargetPitch, _cinemachineTargetYaw, 0.0f);
		}

		private void JumpAndGravity()
		{
			if (Grounded || _isRopeMovementEnabled.Value)
			{
				// reset the fall timeout timer
				_fallTimeoutDelta = FallTimeout;
				
				// stop our velocity dropping infinitely when grounded
				if (_verticalVelocity < 0.0f)
				{
					_verticalVelocity = 0.0f;
				}

				// Jump
				if (_input.jump && _jumpTimeoutDelta <= 0.0f)
				{
                    if(_isRopeMovementEnabled.Value) {
                        Gravity = originalGravity;
                        _isRopeMovementEnabled.Value = false;
                    }
					// the square root of H * -2 * G = how much velocity needed to reach desired height
					_motor.ForceUnground();
					_verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
				}

				// jump timeout
				if (_jumpTimeoutDelta >= 0.0f)
				{
					_jumpTimeoutDelta -= Time.deltaTime;
				}
			}
			else
			{
				// reset the jump timeout timer
				_jumpTimeoutDelta = JumpTimeout;

				// fall timeout
				if (_fallTimeoutDelta >= 0.0f)
				{
					_fallTimeoutDelta -= Time.deltaTime;
				}

				// if we are not grounded, do not jump
				_input.jump = false;
			}

			// apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
			if (_verticalVelocity < _terminalVelocity)
			{
				_verticalVelocity += Gravity * Time.deltaTime;
			}
			
			//Move up and down in NoClip mode
			if (isNoClipEnabled.Value)
			{
				if (_input.JumpHeld)
					_verticalVelocity = noClipUpDownVelocity;
				
				else if (_input.CrouchHeld)
					_verticalVelocity = -noClipUpDownVelocity;
				else
					_verticalVelocity = 0f;
			}
		}

		public void Knockback(HitInfo hitInfo)
		{
			if (_knockbackActive)
				return;

			_motor.ForceUnground();
			_knockbackDir = hitInfo.HitDirection.xoz().normalized;
			_verticalVelocity = KnockbackVerticalForce;
			knockbackStartTime = Time.time;
			_knockbackActive = true;
		}

        private void OnPlayerEnteredRope() {
            if(!_isRopeMovementEnabled.Value) {
                 originalGravity = Gravity;
                Gravity = 0f;
                _motor.ForceUnground();
                _isRopeMovementEnabled.Value = true;
            }
        }

		private void SetNoClip()
		{
			int playerLayer = LayerMask.NameToLayer("Player");
			int terrainLayer = LayerMask.NameToLayer("Terrain");
			int defaultlayer = LayerMask.NameToLayer("Default");
			
			if (isNoClipEnabled.Value)
			{
				originalGravity = Gravity;
				Gravity = 0f;
				_verticalVelocity = 0f;
				orignalCollisionMask = _motor.CollidableLayers;
				_motor.CollidableLayers = noClipCollisionMask;

			}
			else
			{
				Gravity = originalGravity;
				_motor.CollidableLayers = orignalCollisionMask;
			}
		}

		private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
		{
			if (lfAngle < -360f) lfAngle += 360f;
			if (lfAngle > 360f) lfAngle -= 360f;
			return Mathf.Clamp(lfAngle, lfMin, lfMax);
		}

		private void OnDrawGizmosSelected()
		{
			Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
			Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

			if (Grounded) Gizmos.color = transparentGreen;
			else Gizmos.color = transparentRed;
		}
	}
}