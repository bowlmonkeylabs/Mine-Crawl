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
		[SerializeField] private bool _enableLogs = true;
		
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
		[SerializeField] private BoolReference _isDebugConsoleOpen;
		[SerializeField] private BoolReference _isDebugOverlayOpen; // This is specifically excluded from _containerUiMenuStates, so this acts as an overlay rather than an interactable menu.
		[SerializeField] private BoolReference _isGodModeEnabled;
		[SerializeField] private BoolReference _isNoClipEnabled;
		[SerializeField] private GameEvent _onAddEnemyHealth;
		[SerializeField] private GameEvent _onFreezeTime;
		[SerializeField] private GameEvent _onResetTimeScale;
		[SerializeField] private GameEvent _onIncreaseTimeScale;
		[SerializeField] private GameEvent _onDecreaseTimeScale;
		[SerializeField] private GameEvent _onSkipFrame;

		[FormerlySerializedAs("_containerUiMenuStates")]
		[Tooltip("Include UI INTERACTABLE states which should take FULL CONTROL (meaning they need to DISABLE player control)")]
		[SerializeField] private VariableContainer _containerUiMenuStates_NoPlayerControl;
		
		[Tooltip("Include UI INTERACTABLE states which OVERLAY gameplay (meaning player still has movement, but mouse is unlocked to interact with the overlays)")]
		[SerializeField] private VariableContainer _containerUiMenuStates_InteractableOverlay;

		[SerializeField] private PlayerInput playerInput;

		private InputAction jumpAction;
		private InputAction crouchAction;

		private bool jumpHeld;
		private bool crouchHeld;

		public bool JumpHeld => jumpHeld;
		public bool CrouchHeld => crouchHeld;
		
		private bool isUsingUi_NoPlayerControl
		{
			get
			{
				return _containerUiMenuStates_NoPlayerControl
					.GetBoolVariables()
					.Any(b => (b != null && b.Value));
			}
		}
		
		private bool isUsingUi_InteractableOverlay
		{
			get
			{
				return _containerUiMenuStates_InteractableOverlay
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
			ApplyInputState();
			_containerUiMenuStates_NoPlayerControl.GetBoolVariables().ForEach(b => b.Subscribe(ApplyInputState));
			_containerUiMenuStates_InteractableOverlay.GetBoolVariables().ForEach(b => b.Subscribe(ApplyInputState));
			_isDebugConsoleOpen.Subscribe(ApplyInputState);

			jumpAction.performed += SetJumpHeld;
			jumpAction.canceled += SetJumpHeld;
			crouchAction.performed += SetCrouchHeld;
			crouchAction.canceled += SetCrouchHeld;
		}

		private void OnDisable()
		{
			_containerUiMenuStates_NoPlayerControl.GetBoolVariables().ForEach(b => b.Unsubscribe(ApplyInputState));
			_containerUiMenuStates_InteractableOverlay.GetBoolVariables().ForEach(b => b.Unsubscribe(ApplyInputState));
			_isDebugConsoleOpen.Unsubscribe(ApplyInputState);

			jumpAction.performed -= SetJumpHeld;
			jumpAction.canceled -= SetJumpHeld;
			crouchAction.performed -= SetCrouchHeld;
			crouchAction.canceled -= SetCrouchHeld;
		}

		private void OnDestroy()
		{
			_containerUiMenuStates_NoPlayerControl.GetBoolVariables().ForEach(b => b.Unsubscribe(ApplyInputState));
			_containerUiMenuStates_InteractableOverlay.GetBoolVariables().ForEach(b => b.Unsubscribe(ApplyInputState));
			_isDebugConsoleOpen.Unsubscribe(ApplyInputState);
			
			jumpAction.performed -= SetJumpHeld;
			jumpAction.canceled -= SetJumpHeld;
			crouchAction.performed -= SetCrouchHeld;
			crouchAction.canceled -= SetCrouchHeld;
		}
		
		private void OnApplicationFocus(bool hasFocus)
		{
			// SetCursorState(playerCursorLocked);
			ApplyInputState();
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
			if (_isStoreOpen.Value && !_isPaused.Value) return;
			
			_isPaused.Value = true;
			// The action map switching should now be handled by a subscription to value changes of _isPaused
			// playerInput.SwitchCurrentActionMap("UI");
			// SetCursorState(false);
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

		private static void SwitchCurrentActionMap(PlayerInput playerInput, bool disableAllOthers, params string[] actionMapNames)
		{
			if (disableAllOthers)
			{
				foreach (var actionsActionMap in playerInput.actions.actionMaps)
				{
					actionsActionMap.Disable();
				}
			}
			
			foreach (var actionMapName in actionMapNames)
			{
				playerInput.actions.FindActionMap(actionMapName, true).Enable();
			}
		}

		public void ApplyInputState()
		{
			if (_enableLogs) Debug.Log($"ApplyInputState: Previous state is (ActionMap {playerInput.currentActionMap.name}) (CursorLocked {Cursor.lockState})");
			if (_isDebugConsoleOpen.Value)
			{
				SwitchCurrentActionMap(playerInput, true, "Debug_FKeys", "UI");
				SetCursorState(false);
			}
			else if (isUsingUi_NoPlayerControl)
			{
				SwitchCurrentActionMap(playerInput, true, "Debug_FKeys", "Debug_Extended", "UI");
				SetCursorState(false);
			}
			else if (isUsingUi_InteractableOverlay)
			{
				SwitchCurrentActionMap(playerInput, true, "Debug_FKeys", "Debug_Extended", "UI", "UI_Player");
				SetCursorState(false);
			}
			else
			{
				SwitchCurrentActionMap(playerInput, true, "Debug_FKeys", "Debug_Extended", "Debug_FKeys", "Player");
				SetCursorState(playerCursorLocked);
			}
			if (_enableLogs) Debug.Log($"ApplyInputState Inputs (NoPlayerControl {isUsingUi_NoPlayerControl}) (InteractableOverlay {isUsingUi_InteractableOverlay}) => updated input state to: (ActionMap {playerInput.currentActionMap.name}) (CursorLocked {Cursor.lockState})");
		}
		
		private void SetCursorState(bool newState)
		{
			Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
		}
	}
	
}