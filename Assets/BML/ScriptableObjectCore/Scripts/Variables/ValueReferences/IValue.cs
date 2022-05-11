using BML.ScriptableObjectCore.Scripts.Variables;
using UnityEngine;

namespace BML.ScriptableObjectCore.Scripts.Variables.ValueReferences
{
    public interface IValue<T>
    {
        string GetName();

        string GetDescription();

        T GetValue(System.Type type);

        void Subscribe(OnUpdate callback);

        void Unsubscribe(OnUpdate callback);

        bool Save(string folderPath, string name = "");
    }
}