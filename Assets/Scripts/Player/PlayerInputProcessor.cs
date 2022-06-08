//PlayerInputProcessor.cs extends the StartAssetsInput.cs from Unity's Starter Assets - First Person Character Controller
//https://assetstore.unity.com/packages/essentials/starter-assets-first-person-character-controller-196525

using System;
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
		[SerializeField] private BoolReference _isGameLost;
		[SerializeField] private BoolReference _isGameWon;
		private bool isUsingUi => (_isPaused != null && _isPaused.Value) ||
		                          (_isGameLost != null && _isGameLost.Value) ||
		                          (_isGameWon != null && _isGameWon.Value);
		
		[SerializeField] private PlayerInput playerInput;

		private void OnEnable()
		{
			_isPaused.Subscribe(ApplyUiState);
			_isGameLost.Subscribe(ApplyUiState);
			_isGameWon.Subscribe(ApplyUiState);
			ApplyUiState();
		}

		private void OnDisable()
		{
			_isPaused.Unsubscribe(ApplyUiState);
			_isGameLost.Unsubscribe(ApplyUiState);
			_isGameWon.Unsubscribe(ApplyUiState);
		}

		private void OnDestroy()
		{
			_isPaused.Unsubscribe(ApplyUiState);
			_isGameLost.Unsubscribe(ApplyUiState);
			_isGameWon.Unsubscribe(ApplyUiState);
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
			// Debug.Log($"Apply Pause State: {_isPaused?.Value}");
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