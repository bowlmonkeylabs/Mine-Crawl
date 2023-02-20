using BML.Scripts.Utils;
using KinematicCharacterController;
using UnityEngine;

namespace BML.Scripts.Player.Utils
{
    public class PlayerUtils
    {
        public static void MovePlayer(GameObject player, Vector3 destination)
        {
            // If in play mode, move player using kinematicCharController motor to avoid race condition
            if (ApplicationUtils.IsPlaying_EditorSafe)
            {
                KinematicCharacterMotor motor = player.GetComponent<KinematicCharacterMotor>();
                if (motor != null)
                {
                    motor.SetPosition(destination);
                }
                else
                {
                    player.transform.position = destination;
                    Debug.LogWarning("Could not find KinematicCharacterMotor on player! " +
                                     "Moving player position directly via Transform.");
                }
            }
            else
            {
                player.transform.position = destination;
            }
        }
    }
}