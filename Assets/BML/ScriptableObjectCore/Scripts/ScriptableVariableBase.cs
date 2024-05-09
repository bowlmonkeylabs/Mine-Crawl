using Sirenix.OdinInspector;
using UnityEngine;

namespace BML.ScriptableObjectCore.Scripts
{
    public class ScriptableVariableBase : ScriptableObject
    {
        private Color _colorEnableLogs => enableLogs ? Color.yellow : Color.gray;
        [InfoBox("Logging is enabled for this scriptable object", InfoMessageType.Warning, visibleIfMemberName:"enableLogs")]
        [SerializeField, HideInInlineEditors, GUIColor("_colorEnableLogs")] protected bool enableLogs;

        /// <summary>
        /// Method is virtual but REQUIRED on all children.
        /// </summary>
        public virtual void Reset() { }
    }
}
