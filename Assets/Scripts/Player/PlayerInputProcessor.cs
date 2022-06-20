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
		public bool jump;
		public bool sprint;

		[Header("Movement Settings")]
		public bool analogMovement;

		[FormerlySerializedAs("cursorLocked")] [Header("Mouse Cursor Settings")]
		public bool playerCursorLocked = true;
		public bool cursorInputForLook = true;
		[SerializeField] private FloatReference _mouseSensitivity;
		
		[SerializeField] private BoolReference _isPaused;

		[SerializeField] private VariableContainer _containerUiMenuStates;

		private bool isUsingUi
		{
			get
			{
				return _containerUiMenuStates
					.GetBoolVariables()
					.Any(b => (b != null && b.Value));
			}
		}

		[SerializeField] private PlayerInput playerInput;

		private void OnEnable()
		{
			ApplyUiState();
			_containerUiMenuStates.GetBoolVariables().ForEach(b => b.Subscribe(ApplyUiState));
		}

		private void OnDisable()
		{
			_containerUiMenuStates.GetBoolVariables().ForEach(b => b.Unsubscribe(ApplyUiState));
		}

		private void OnDestroy()
		{
			_containerUiMenuStates.GetBoolVariables().ForEach(b => b.Unsubscribe(ApplyUiState));
		}

#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
		public void OnMove(InputValue value)
		{
			MoveInput(value.Get<Vector2>());
		}

		public void OnLook(InputValue value)
		{
			if(cursorInputForLook)
			{
				var inputValue = value.Get<Vector2>() * _mouseSensitivity.Value;
				LookInput(inputValue);
			}
		}

		public void OnJump(InputValue value)
		{
			JumpInput(value.isPressed);
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
		
		public void OnUnlockCursor()
		{
			SetCursorState(false);
		}
#endif
		public void ApplyUiState()
		{
			Debug.Log($"Apply Ui State: {isUsingUi} => {playerInput.currentActionMap.name} {Cursor.lockState}");
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
			Debug.Log($"Updated to: {playerInput.currentActionMap.name} {Cursor.lockState}");
		}

		public void MoveInput(Vector2 newMoveDirection)
		{
			move = newMoveDirection;
		} 

		public void LookInput(Vector2 newLookDirection)
		{
			look = newLookDirection;
		}

		public void JumpInput(bool newJumpState)
		{
			Debug.Log($"Jump input {newJumpState}");
			jump = newJumpState;
		}

		public void SprintInput(bool newSprintState)
		{
			sprint = newSprintState;
		}
		
		private void OnApplicationFocus(bool hasFocus)
		{
			// SetCursorState(playerCursorLocked);
			ApplyUiState();
		}

		private void SetCursorState(bool newState)
		{
			Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
		}
	}
	
}