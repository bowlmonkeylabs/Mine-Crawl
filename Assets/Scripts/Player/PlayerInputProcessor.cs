//PlayerInputProcessor.cs extends the StartAssetsInput.cs from Unity's Starter Assets - First Person Character Controller
//https://assetstore.unity.com/packages/essentials/starter-assets-first-person-character-controller-196525

using System;
using System.Linq;
using AdvancedSceneManager.Utility;
using BML.ScriptableObjectCore.Scripts.Events;
using BML.ScriptableObjectCore.Scripts.Variables;
using MoreMountains.Feedbacks;
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
		public bool dash;

		[Header("Movement Settings")]
		public bool analogMovement;

		[FormerlySerializedAs("cursorLocked")] [Header("Mouse Cursor Settings")]
		public bool playerCursorLocked = true;
		[SerializeField] private FloatReference _mouseSensitivity;
		[SerializeField] private BoolReference _invertedLookY;

		[SerializeField] private Vector2Reference _mouseInput;
		[SerializeField] private Vector2Reference _moveInput;
		[SerializeField] private BoolReference _isPaused;
		[SerializeField] private BoolReference _isMinimapOpen;
		[SerializeField] private BoolReference _isStoreOpen;
		[SerializeField] private BoolReference _isUpgradeStoreOpen;
		[SerializeField] private GameEvent _onStoreFailOpen;
		[SerializeField] private BoolReference _isPlayerInCombat;
        [SerializeField] private IntReference _upgradesAvailable;
		[SerializeField] private BoolReference _isDebugConsoleOpen;
		[SerializeField] private BoolReference _settingDoFreezeOnDebugConsole;
		[SerializeField] private BoolReference _isDebugOverlayOpen; // This is specifically excluded from _containerUiMenuStates, so this acts as an overlay rather than an interactable menu.
		[SerializeField] private BoolReference _isGodModeEnabled;
		[SerializeField] private BoolReference _isNoClipEnabled;
		[SerializeField] private GameEvent _onAddEnemyHealth;
		[FormerlySerializedAs("_onFreezeTime")] [SerializeField] private GameEvent _onToggleFreezeTime;
		[SerializeField] private GameEvent _onFreezeTime;
		[SerializeField] private GameEvent _onUnfreezeTime;
		[SerializeField] private GameEvent _onResetTimeScale;
		[SerializeField] private GameEvent _onIncreaseTimeScale;
		[SerializeField] private GameEvent _onDecreaseTimeScale;
		[SerializeField] private GameEvent _onSkipFrame;

		[FormerlySerializedAs("_containerUiMenuStates")]
		[Tooltip("Include UI INTERACTABLE states which should take FULL CONTROL (meaning they need to DISABLE player control)")]
		[SerializeField] private VariableContainer _containerUiMenuStates_NoPlayerControl;
		
		[Tooltip("Include UI INTERACTABLE states which OVERLAY gameplay (meaning the player has full UI control AND limited player control, and the mouse is UNLOCKED to interact with the overlays)")]
		[SerializeField] private VariableContainer _containerUiMenuStates_InteractableOverlay;
		
		[Tooltip("Include UI INTERACTABLE states which OVERLAY gameplay (meaning the player has full UI control AND limited player control, but mouse is still LOCKED)")]
		[SerializeField] private VariableContainer _containerUiMenuStates_InteractableOverlayLockedCursor;
		
		[Tooltip("Include UI INTERACTABLE states which OVERLAY spectator gameplay (meaning the player has full UI control AND limited SPECTATOR control, and the mouse is UNLOCKED to interact with the overlays)")]
		[SerializeField] private VariableContainer _containerUiMenuStates_InteractableOverlaySpectator;

		[Tooltip("Include UI INTERACTABLE states which should FREEZE TIME.")] 
		[SerializeField] private VariableContainer _containerUiMenuStates_Frozen;
		// Freezing is actually handled by the TimeManager game object; this is just referenced here to note its existence.

		[Tooltip("Include UI INTERACTABLE states which should HIDE PLAYER HUD.")]
		[SerializeField] private VariableContainer _containerUiMenuStates_HidePlayerHUD;
		// Hiding is handled the each respective UI element by referencing the value assigned to _isUsingUi_Out_HidePlayerHUD.

        [Tooltip("Include UI INTERACTABLE states which can be closed by other menus opening")]
		[SerializeField] private VariableContainer _containerUiMenuStates_NonBlocking;

        [Tooltip("Include UI INTERACTABLE states which can NOT be closed by other menus opening")]
		[SerializeField] private VariableContainer _containerUiMenuStates_Blocking;

		[SerializeField] private PlayerInput playerInput;

		[SerializeField] private BoolVariable _isUsingUi_Out_Any;
		[SerializeField] private BoolVariable _isUsingUi_Out_HidePlayerHUD;
		[SerializeField] private BoolVariable _isUsingUi_Out_InteractableOverlay;
		[SerializeField] private BoolVariable _isUsingUi_Out_InteractableOverlayLockedCursor;
		[SerializeField] private BoolVariable _isUsingUi_Out_InteractableOverlaySpectator;
		
		private InputAction jumpAction;
		private InputAction crouchAction;

		private bool jumpHeld;
		private bool crouchHeld;

		public bool JumpHeld => jumpHeld;
		public bool CrouchHeld => crouchHeld;
		
		private bool IsUsingUi_NoPlayerControl
		{
			get
			{
				return _containerUiMenuStates_NoPlayerControl
					.GetBoolVariables()
					.Any(b => (b != null && b.Value));
			}
		}
		
		private bool IsUsingUi_InteractableOverlay
		{
			get
			{
				return _containerUiMenuStates_InteractableOverlay
					.GetBoolVariables()
					.Any(b => (b != null && b.Value));
			}
		}
		
		private bool IsUsingUi_InteractableOverlayLockedCursor
		{
			get
			{
				return _containerUiMenuStates_InteractableOverlayLockedCursor
					.GetBoolVariables()
					.Any(b => (b != null && b.Value));
			}
		}
		
		private bool IsUsingUi_InteractableOverlaySpectator
		{
			get
			{
				return _containerUiMenuStates_InteractableOverlaySpectator
					.GetBoolVariables()
					.Any(b => (b != null && b.Value));
			}
		}
		
		private bool IsUsingUi_Frozen
		{
			get
			{
				return _containerUiMenuStates_Frozen
					.GetBoolVariables()
					.Any(b => (b != null && b.Value));
			}
		}
		
		private bool IsUsingUi_HidePlayerHUD
		{
			get
			{
				return _containerUiMenuStates_HidePlayerHUD
					.GetBoolVariables()
					.Any(b => (b != null && b.Value));
			}
		}

        private bool IsUsingUi_Blocking
		{
			get
			{
				return (_isUpgradeStoreOpen.Value) ||
                    _containerUiMenuStates_Blocking
					.GetBoolVariables()
					.Any(b => (b != null && b.Value));
			}
		}

		private bool IsUsingUi
		{
			get
			{
				return IsUsingUi_InteractableOverlay
				       || IsUsingUi_InteractableOverlayLockedCursor
				       || IsUsingUi_InteractableOverlaySpectator
				       || IsUsingUi_NoPlayerControl
				       || _isDebugConsoleOpen.Value;
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
			_containerUiMenuStates_InteractableOverlayLockedCursor.GetBoolVariables().ForEach(b => b.Subscribe(ApplyInputState));
			_containerUiMenuStates_InteractableOverlaySpectator.GetBoolVariables().ForEach(b => b.Subscribe(ApplyInputState));
			_isDebugConsoleOpen.Subscribe(ApplyInputState);
			_isDebugConsoleOpen.Subscribe(OnIsDebugConsoleOpenUpdated);

			jumpAction.performed += SetJumpHeld;
			jumpAction.canceled += SetJumpHeld;
			crouchAction.performed += SetCrouchHeld;
			crouchAction.canceled += SetCrouchHeld;
		}

		private void OnDisable()
		{
			_containerUiMenuStates_NoPlayerControl.GetBoolVariables().ForEach(b => b.Unsubscribe(ApplyInputState));
			_containerUiMenuStates_InteractableOverlay.GetBoolVariables().ForEach(b => b.Unsubscribe(ApplyInputState));
			_containerUiMenuStates_InteractableOverlayLockedCursor.GetBoolVariables().ForEach(b => b.Unsubscribe(ApplyInputState));
			_containerUiMenuStates_InteractableOverlaySpectator.GetBoolVariables().ForEach(b => b.Unsubscribe(ApplyInputState));
			_isDebugConsoleOpen.Unsubscribe(ApplyInputState);
			_isDebugConsoleOpen.Unsubscribe(OnIsDebugConsoleOpenUpdated);

			jumpAction.performed -= SetJumpHeld;
			jumpAction.canceled -= SetJumpHeld;
			crouchAction.performed -= SetCrouchHeld;
			crouchAction.canceled -= SetCrouchHeld;
		}

		private void Update()
		{
			if (LoadingScreenUtility.IsAnyLoadingScreenOpen && playerInput.enabled)
			{
				playerInput.enabled = false;
			}
			else if (!LoadingScreenUtility.IsAnyLoadingScreenOpen && !playerInput.enabled)
			{
				playerInput.enabled = true;
			}
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
			Vector2 moveInput = value.Get<Vector2>();
			MoveInput(moveInput);
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

		public void OnDash(InputValue value)
		{
			DashInput(value.isPressed);
		}
		
		public void OnPause()
		{
            // Do nothing if blocking menu is already open
			if (!_isPaused.Value && IsUsingUi_Blocking)
			{
				return;
			}

			//if going to open menu, close other non blocking menus
            if(!_isPaused.Value) {
                _containerUiMenuStates_NonBlocking.GetBoolVariables().ForEach((menuState) => {
	                if(!menuState.Equals(_isPaused)) {
                        menuState.Value = false;
                    }
                });
            }
			
			_isPaused.Value = true;
			// The action map switching should now be handled by a subscription to value changes of _isPaused
			// playerInput.SwitchCurrentActionMap("UI");
			// SetCursorState(false);
		}

		public void OnToggleMinimap()
		{
			// Do nothing if blocking menu is already open
			if (!_isMinimapOpen.Value && IsUsingUi_Blocking)
			{
				return;
			}

			if (!_isMinimapOpen.Value)
			{
				foreach (var menuStateBoolVariable in _containerUiMenuStates_NonBlocking.GetBoolVariables())
				{
					if (!menuStateBoolVariable.Equals(_isMinimapOpen))
					{
						menuStateBoolVariable.Value = false;
					}
				}
			}

			_isMinimapOpen.Value = !_isMinimapOpen.Value;
		}

		public void OnCloseMinimap()
		{
			// Do nothing if minimap is not open
			if (!_isMinimapOpen.Value)
			{
				return;
			}
			
			_isMinimapOpen.Value = !_isMinimapOpen.Value;
		}

		public void OnToggleStore()
		{
			// Do nothing if blocking menu is already open
			if (!_isStoreOpen.Value && IsUsingUi_Blocking)
			{
				return;
			}

			// Play feedback if store fails to open due to combat
			if (!_isStoreOpen.Value && _isPlayerInCombat.Value)
			{
				_onStoreFailOpen.Raise();
				return;
			}

			// If opening the store, close other menus
			if (!_isStoreOpen.Value)
			{
				foreach (var menuStateBoolVariable in _containerUiMenuStates_NonBlocking.GetBoolVariables())
				{
					if (!menuStateBoolVariable.Equals(_isStoreOpen))
					{
						menuStateBoolVariable.Value = false;
					}
				}
			}

			_isStoreOpen.Value = !_isStoreOpen.Value;
		}
		
		public void OnToggleUpgradeStore()
		{
			// Do nothing if blocking menu is already open
			if (!_isUpgradeStoreOpen.Value && (IsUsingUi_Blocking || _upgradesAvailable.Value <= 0))
			{
				return;
			}

			// Play feedback if store fails to open due to combat
			// if (!_isUpgradeStoreOpen.Value && _isPlayerInCombat.Value)
			// {
			// 	_onStoreFailOpen.Raise();
            //     Debug.Log("player combat");
			// 	return;
			// }
			
			if (!_isUpgradeStoreOpen.Value)
			{
				foreach (var menuStateBoolVariable in _containerUiMenuStates_NonBlocking.GetBoolVariables())
				{
					if (!menuStateBoolVariable.Equals(_isUpgradeStoreOpen))
					{
						menuStateBoolVariable.Value = false;
					}
				}
			}

            _isUpgradeStoreOpen.Value = !_isUpgradeStoreOpen.Value;
		}

		// Deprecated?
		private void OpenUpgradeStore()
		{
			// If opening the store, close other menus
			if (!_isUpgradeStoreOpen.Value)
			{
				foreach (var menuStateBoolVariable in _containerUiMenuStates_NonBlocking.GetBoolVariables())
				{
					if (!menuStateBoolVariable.Equals(_isUpgradeStoreOpen))
					{
						menuStateBoolVariable.Value = false;
					}
				}
			}

			_isUpgradeStoreOpen.Value = !_isUpgradeStoreOpen.Value;
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
			_isDebugOverlayOpen.Value = !_isDebugOverlayOpen.Value;
		}

        public void OnUnlockCursor()
		{
			SetCursorState(false);
		}
		
		public void MoveInput(Vector2 newMoveDirection)
		{
			move = newMoveDirection;
			_moveInput.Value = move;
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

			_mouseInput.Value = look;
		}

		public void DashInput(bool newDashState)
		{
			dash = newDashState;
		}

		public void OnFreezeTime()
		{
			_onToggleFreezeTime.Raise();
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

		private void OnIsDebugConsoleOpenUpdated()
		{
			if (!_settingDoFreezeOnDebugConsole.Value) return;
				
			if (_isDebugConsoleOpen.Value) _onFreezeTime.Raise();
			else _onUnfreezeTime.Raise();
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
			else if (IsUsingUi_NoPlayerControl)
			{
				SwitchCurrentActionMap(playerInput, true, "Debug_FKeys", "Debug_Extended", "UI");
				SetCursorState(false);
			}
			else if (IsUsingUi_InteractableOverlay)
			{
				SwitchCurrentActionMap(playerInput, true, "Debug_FKeys", "Debug_Extended", "UI", "UI_Player");
				SetCursorState(false);
			}
			else if (IsUsingUi_InteractableOverlayLockedCursor)
			{
				SwitchCurrentActionMap(playerInput, true, "Debug_FKeys", "Debug_Extended", "UI", "UI_Player");
				SetCursorState(playerCursorLocked);
			}
			else if (IsUsingUi_InteractableOverlaySpectator)
			{
				SwitchCurrentActionMap(playerInput, true, "Debug_FKeys", "Debug_Extended", "UI", "UI_Spectator");
				SetCursorState(false);
			}
			else
			{
				SwitchCurrentActionMap(playerInput, true, "Debug_FKeys", "Debug_Extended", "Debug_FKeys", "Player");
				SetCursorState(playerCursorLocked);
			}

			_isUsingUi_Out_Any.Value = IsUsingUi;
			_isUsingUi_Out_HidePlayerHUD.Value = IsUsingUi_HidePlayerHUD;
			_isUsingUi_Out_InteractableOverlay.Value = IsUsingUi_InteractableOverlay;
			_isUsingUi_Out_InteractableOverlayLockedCursor.Value = IsUsingUi_InteractableOverlayLockedCursor;
			_isUsingUi_Out_InteractableOverlaySpectator.Value = IsUsingUi_InteractableOverlaySpectator;
			
			if (_enableLogs) Debug.Log($"ApplyInputState Inputs (NoPlayerControl {IsUsingUi_NoPlayerControl}) (InteractableOverlay {IsUsingUi_InteractableOverlay}) (InteractableOverlayLockedCursor {IsUsingUi_InteractableOverlayLockedCursor}) => updated input state to: (ActionMap {playerInput.currentActionMap.name}) (CursorLocked {Cursor.lockState})");
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="newState"></param>
		/// <param name="visible">Even when 'true', the cursor will not be visible when it is locked.</param>
		private void SetCursorState(bool newState, bool visible = true)
		{
			Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
			Cursor.visible = visible && !newState;
		}
	}
	
}