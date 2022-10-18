using BML.ScriptableObjectCore.Scripts.Variables;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BML.Scripts
{
    public class Test : MonoBehaviour
    {
        [ValidateInput("NotNegative", "Can't be negative", InfoMessageType.Warning)]
        [SerializeField] private FloatVariable a;


        private bool NotNegative(FloatVariable var)
        {
            if (var.Value <= 0)
            {
                Debug.LogError("Can't be negative!");
                return false;
            }
                

            return true;
        }
    }
}