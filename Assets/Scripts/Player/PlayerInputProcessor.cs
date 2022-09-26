//PlayerInputProcessor.cs extends the StartAssetsInput.cs from Unity's Starter Assets - First Person Character Controller
//https://assetstore.unity.com/packages/essentials/starter-assets-first-person-character-controller-196525

using System;
using System.Linq;
using BML.ScriptableObjectCore.Scripts.Events;
using BML.ScriptableObjectCore.Scripts.Variables;
using UnityEngine;
using UnityEngine.Serialization;
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
using UnityEngine.InputSystem;
#endif

namespace BML.Scripts.Player
{
	public class PlayerInputProcessor : MonoBehaviour
	{
		[Header("Character Input Values")]
		public Vector2 move;
		public Vector2 look;
		public Vector2 lookUnscaled;
		public Vector2 lookScaleMouse = new Vector2(0.005f, 0.005f);
		public Vector2 lookScaleGamepad = new Vector2(12.5f, 12.5f);
		public bool jump;
		public bool sprint;

		[Header("Movement Settings")]
		public bool analogMovement;

		[FormerlySerializedAs("cursorLocked")] [Header("Mouse Cursor Settings")]
		public bool playerCursorLocked = true;
		[SerializeField] private FloatReference _mouseSensitivity;
		[SerializeField] private BoolReference _invertedLookY;
		
		[SerializeField] private BoolReference _isPaused;
		[SerializeField] private BoolReference _isStoreOpen;
		[SerializeField] private BoolReference _isDebugOverlayOpen; // This is specifically exluceded from _containerUiMenuStates, so this acts as an overlay rather than an interactable menu.
		[SerializeField] private BoolReference _isGodModeEnabled;
		[SerializeField] private BoolReference _isNoClipEnabled;
		[SerializeField] private GameEvent _onAddEnemyHealth;
		[SerializeField] private GameEvent _onFreezeTime;
		[SerializeField] private GameEvent _onResetTimeScale;
		[SerializeField] private GameEvent _onIncreaseTimeScale;
		[SerializeField] private GameEvent _onDecreaseTimeScale;
		[SerializeField] private GameEvent _onSkipFrame;

		[SerializeField] private VariableContainer _containerUiMenuStates;
		
		[SerializeField] private PlayerInput playerInput;

		private InputAction jumpAction;
		private InputAction crouchAction;

		private bool jumpHeld;
		private bool crouchHeld;

		public bool JumpHeld => jumpHeld;
		public bool CrouchHeld => crouchHeld;
		
		private bool isUsingUi
		{
			get
			{
				return _containerUiMenuStates
					.GetBoolVariables()
					.Any(b => (b != null && b.Value));
			}
		}
		
		private bool IsCurrentDeviceMouse
		{
			get => playerInput.currentControlScheme == "Keyboard&Mouse";
			
		}
		
		#region Unity lifecycle

		private void Awake()
		{
			jumpAction = playerInput.actions.FindAction("Jump");
			crouchAction = playerInput.actions.FindAction("Crouch");
		}

		private void OnEnable()
		{
			ApplyUiState();
			_containerUiMenuStates.GetBoolVariables().ForEach(b => b.Subscribe(ApplyUiState));

			jumpAction.performed += SetJumpHeld;
			jumpAction.canceled += SetJumpHeld;
			crouchAction.performed += SetCrouchHeld;
			crouchAction.canceled += SetCrouchHeld;
		}

		private void OnDisable()
		{
			_containerUiMenuStates.GetBoolVariables().ForEach(b => b.Unsubscribe(ApplyUiState));
			
			jumpAction.performed -= SetJumpHeld;
			jumpAction.canceled -= SetJumpHeld;
			crouchAction.performed -= SetCrouchHeld;
			crouchAction.canceled -= SetCrouchHeld;
		}

		private void OnDestroy()
		{
			_containerUiMenuStates.GetBoolVariables().ForEach(b => b.Unsubscribe(ApplyUiState));
		}
		
		private void OnApplicationFocus(bool hasFocus)
		{
			// SetCursorState(playerCursorLocked);
			ApplyUiState();
		}
		
		#endregion
		
		#region Input callbacks
		
		public void OnMove(InputValue value)
		{
			MoveInput(value.Get<Vector2>());
		}

		public void OnLook(InputValue value)
		{
			var inputValue = value.Get<Vector2>();
			LookInput(inputValue);
		}

		public void OnJump(InputValue value)
		{
			jump = value.isPressed;
		}

		public void OnSprint(InputValue value)
		{
			SprintInput(value.isPressed);
		}
		
		public void OnPause()
		{
			if (_isPaused != null)
			{
				_isPaused.Value = true;
				// The action map switching should now be handled by a subscription to value changes of _isPaused
				// playerInput.SwitchCurrentActionMap("UI");
				// SetCursorState(false);
			}
		}

		public void OnToggleStore()
		{
			_isStoreOpen.Value = !_isStoreOpen.Value;
		}

		public void OnToggleGodMode()
		{
			_isGodModeEnabled.Value = !_isGodModeEnabled.Value;
			Debug.Log($"Enable God Mode: {_isGodModeEnabled.Value}");
		}
		
		public void OnToggleNoClipMode()
		{
			_isNoClipEnabled.Value = !_isNoClipEnabled.Value;
			Debug.Log($"No Clip Mode: {_isNoClipEnabled.Value}");
		}
		
		public void OnAddEnemyHealth()
		{
			_onAddEnemyHealth.Raise();
			Debug.Log($"Added enemy health");
		}

        public void OnOpenDebugUi()
		{
            Debug.Log($"Pressed Debug Ui");
			_isDebugOverlayOpen.Value = !_isDebugOverlayOpen.Value;
		}
        
		public void OnUnlockCursor()
		{
			SetCursorState(false);
		}
		
		public void MoveInput(Vector2 newMoveDirection)
		{
			move = newMoveDirection;
		} 

		public void LookInput(Vector2 newLookDirection)
		{
			lookUnscaled = newLookDirection;

			if (IsCurrentDeviceMouse)
				look = lookUnscaled * lookScaleMouse * _mouseSensitivity.Value;
			else
				look = lookUnscaled * lookScaleGamepad * _mouseSensitivity.Value;

			if (_invertedLookY.Value)
				look.y = -look.y;
		}

		public void SprintInput(bool newSprintState)
		{
			sprint = newSprintState;
		}

		public void OnFreezeTime()
		{
			_onFreezeTime.Raise();
		}
		
		public void OnIncreaseTimeScale()
		{
			_onIncreaseTimeScale.Raise();
		}
		
		public void OnDecreaseTimeScale()
		{
			_onDecreaseTimeScale.Raise();
		}
		
		public void OnResetTimeScale()
		{
			_onResetTimeScale.Raise();
		}
		
		public void OnSkipFrame()
		{
			_onSkipFrame.Raise();
		}
		
		#endregion

		private void SetJumpHeld(InputAction.CallbackContext ctx)
		{
			if (ctx.performed)
				jumpHeld = true;
			else if (ctx.canceled)
				jumpHeld = false;
		}
		
		private void SetCrouchHeld(InputAction.CallbackContext ctx)
		{
			if (ctx.performed)
				crouchHeld = true;
			else if (ctx.canceled)
				crouchHeld = false;
		}
		
		public void ApplyUiState()
		{
			// Debug.Log($"Apply Ui State: {isUsingUi} => {playerInput.currentActionMap.name} {Cursor.lockState}");
			if (isUsingUi)
			{
				playerInput.SwitchCurrentActionMap("UI");
				SetCursorState(false);
			}
			else
			{
				playerInput.SwitchCurrentActionMap("Player");
				SetCursorState(playerCursorLocked);
			}
			// Debug.Log($"Updated to: {playerInput.currentActionMap.name} {Cursor.lockState}");
		}
		
		private void SetCursorState(bool newState)
		{
			Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
		}
	}
	
}