using System.Collections;
using System.Collections.Generic;
using BML.ScriptableObjectCore.Scripts.Variables;
using UnityEngine;

namespace BML.Scripts
{
    public class LoadVariables : MonoBehaviour
    {
        [SerializeField] private VariableContainer _variables;
        [SerializeField] private string _filePath = "Settings.es3";

        public void Load() {
            foreach (var variable in _variables.GetFloatVariables())
            {
                variable.Value = ES3.Load<float>(variable.name, _filePath, variable.DefaultValue);
            }

            foreach (var variable in _variables.GetIntVariables())
            {
                variable.Value = ES3.Load<int>(variable.name, _filePath, variable.DefaultValue);
            }

            foreach (var variable in _variables.GetQuaternionVariables())
            {
                variable.Value = ES3.Load<Quaternion>(variable.name, _filePath, variable.DefaultValue);
            }
            
            foreach (var variable in _variables.GetVector2Variables())
            {
                variable.Value = ES3.Load<Vector2>(variable.name, _filePath, variable.DefaultValue);
            }

            foreach (var variable in _variables.GetVector3Variables())
            {
                variable.Value = ES3.Load<Vector3>(variable.name, _filePath, variable.DefaultValue);
            }

            foreach (var variable in _variables.GetBoolVariables())
            {
                variable.Value = ES3.Load<bool>(variable.name, _filePath, variable.DefaultValue);
            }
        }
    }
}
