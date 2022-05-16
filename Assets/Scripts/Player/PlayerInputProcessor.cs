//PlayerInputProcessor.cs extends the StartAssetsInput.cs from Unity's Starter Assets - First Person Character Controller
//https://assetstore.unity.com/packages/essentials/starter-assets-first-person-character-controller-196525

using System;
using BML.ScriptableObjectCore.Scripts.Events;
using BML.ScriptableObjectCore.Scripts.Variables;
using UnityEngine;
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

		[Header("Mouse Cursor Settings")]
		public bool cursorLocked = true;
		public bool cursorInputForLook = true;
		[SerializeField] private FloatReference _mouseSensitivity;
		
		[Header("Pause settings")]
		[SerializeField] private BoolReference _isPaused;
		[SerializeField] private GameEvent _onUnpause;
		
		[SerializeField] private PlayerInput playerInput;

		private void OnEnable()
		{
			_onUnpause.Subscribe(InvokeUnpause);
		}

		private void OnDisable()
		{
			_onUnpause.Unsubscribe(InvokeUnpause);
		}

		private void OnDestroy()
		{
			_onUnpause.Unsubscribe(InvokeUnpause);
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
				// _isPaused.Value = false;
				// playerInput.SwitchCurrentActionMap("UI");
				// SetCursorState(false);
			}
		}
#endif
		public void InvokeUnpause()
		{
			playerInput.SwitchCurrentActionMap("Player");
			SetCursorState(cursorLocked);
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
			SetCursorState(cursorLocked);
		}

		private void SetCursorState(bool newState)
		{
			Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
			Debug.Log($"Cursor state: {Cursor.lockState}, Action map: {playerInput.currentActionMap.name}");
		}
	}
	
}