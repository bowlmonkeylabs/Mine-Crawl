using System;
using UnityEngine;
using UnityEngine.Events;

namespace BML.Scripts
{
    /// <summary>
    /// Provides methods for chest animation events to call, which then trigger the appropriate chest logic.
    /// </summary>
    public class ChestAnimationEvents : MonoBehaviour
    {
        [SerializeField] UnityEvent _onDispenseReward;
        [SerializeField] UnityEvent _onPlayCloseSound;
        [SerializeField] UnityEvent _onEnableInteractable;

        /// <summary>
        /// Triggers at the moment in the chest opening animation when the reward should be dispensed to the player, allowing for the reward to be given at the correct time in the animation.
        /// </summary>
        public void DispenseReward()
        {
            _onDispenseReward.Invoke();
        }

        /// <summary>
        /// Triggers at the moment in the chest closing animation when the chest should play its closing sound effect, allowing for the sound effect to be played at the correct time in the animation.
        /// </summary>
        public void PlayCloseSound()
        {
            _onPlayCloseSound.Invoke();
        }

        /// <summary>
        /// Triggers at the moment in the chest opening animation when the chest should become interactable again after opening/closing.
        /// </summary>
        public void EnableInteractable()
        {
            _onEnableInteractable.Invoke();
        }
    }
}