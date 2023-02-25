using System.Collections;
using System.Collections.Generic;
using BML.ScriptableObjectCore.Scripts.Variables;
using UnityEngine;

namespace BML.Scripts
{
    public class PersistVariables : MonoBehaviour
    {
        [SerializeField] private VariableContainer _variables;
        [SerializeField] private string _filePath = "Settings.es3";

        public void Save() {
            foreach (var variable in _variables.GetFloatVariables())
            {
                ES3.Save<float>(variable.name, variable.Value, _filePath);
            }

            foreach (var variable in _variables.GetIntVariables())
            {
                ES3.Save<int>(variable.name, variable.Value, _filePath);
            }

            foreach (var variable in _variables.GetQuaternionVariables())
            {
                ES3.Save<Quaternion>(variable.name, variable.Value, _filePath);
            }
            
            foreach (var variable in _variables.GetVector2Variables())
            {
                ES3.Save<Vector2>(variable.name, variable.Value, _filePath);
            }

            foreach (var variable in _variables.GetVector3Variables())
            {
                ES3.Save<Vector3>(variable.name, variable.Value, _filePath);
            }

            foreach (var variable in _variables.GetBoolVariables())
            {
                ES3.Save<bool>(variable.name, variable.Value, _filePath);
            }
        }
    }
}
