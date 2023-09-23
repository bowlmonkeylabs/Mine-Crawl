using System;
using BML.ScriptableObjectCore.Scripts.CustomAttributes;
using BML.ScriptableObjectCore.Scripts.Utils;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace BML.ScriptableObjectCore.Scripts.Variables
{
    public delegate void OnUpdate();
    public delegate void OnUpdate<T>(T previousValue, T currentValue);

    [Serializable]
    [ShowOdinSerializedPropertiesInInspector]
    [InlineEditor(InlineEditorObjectFieldModes.Foldout)]
    public abstract class Variable<T> : ScriptableVariableBase, ISupportsPrefabSerialization, ISerializationCallbackReceiver
    {
        #region Inspector

        [TextArea(7, 10)] [HideInInlineEditors] public String Description;
        
#if UNITY_EDITOR
        private void OnIncludeInContainersChanged()
        {
            EditorUtility.SetDirty(this);
            VariableContainer.PopulateAllRelatedContainers(includeInContainers);
        }
#endif
        private Color _colorIncludeInContainers => includeInContainers != VariableContainerKey.None ? Color.green : Color.white;
        [OnValueChanged("OnIncludeInContainersChanged")]
        [SerializeField, HideInInlineEditors, GUIColor("_colorIncludeInContainers")] private VariableContainerKey includeInContainers;
        public VariableContainerKey IncludeInContainers => includeInContainers;

        private Color _colorEnableLogs => enableLogs ? Color.yellow : Color.gray;
        [InfoBox("Logging is enabled for this scriptable object", InfoMessageType.Warning, visibleIfMemberName:"enableLogs")]
        [FormerlySerializedAs("_enableLogs")] [SerializeField, HideInInlineEditors, GUIColor("_colorEnableLogs")] protected bool enableLogs;

        [LabelText("Default"), LabelWidth(65f)] [InlineButton("Reset", label:"  Reset  ")]
        [SerializeField] protected T defaultValue;
        [LabelText("Runtime"), LabelWidth(65f)] [OnValueChanged("BroadcastUpdate")] [InlineButton("BroadcastUpdate", label:" Update")]
        [SerializeField] protected T runtimeValue;

        protected T prevValue;

        [SerializeField, HideInInspector]
        private SerializationData serializationData;

        SerializationData ISupportsPrefabSerialization.SerializationData { get { return this.serializationData; } set { this.serializationData = value; } }
        
        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            UnitySerializationUtility.DeserializeUnityObject(this, ref this.serializationData);
        }
        
        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            UnitySerializationUtility.SerializeUnityObject(this, ref this.serializationData);
        }

        public abstract T Value { get; set; }

        public T DefaultValue
        {
            get => defaultValue;
        }

        // [Button]
        // [HorizontalGroup("Buttons")]
        public void BroadcastUpdate()
        {
            if (enableLogs) Debug.Log($"BroadcastUpdate {this.name}");
            
            OnUpdate?.Invoke();
            OnUpdateDelta?.Invoke(prevValue, runtimeValue);
            
            prevValue = runtimeValue;
            // Setting this means that 'prevValue' technically only contains the previous value during the frame an update occurs, otherwise it is in sync with 'runtimeValue'
            // But this allows delta updates to work even when we edit values through the inspector (which circumvents the Value.set() method)
        }
        
        #endregion

        #region Events
        
        protected event OnUpdate OnUpdate;
        protected event OnUpdate<T> OnUpdateDelta;

        protected void InvokeOnUpdate()
        {
            if (enableLogs) Debug.Log($"InvokeOnUpdate {this.name}");
            
            OnUpdate?.Invoke();
        }

        protected void ResetOnUpdate()
        {
            if (enableLogs) Debug.Log($"ResetOnUpdate {this.name}");
            
            OnUpdate = null;
        }
        
        protected void InvokeOnUpdateDelta(T prev, T curr)
        {
            if (enableLogs) Debug.Log($"InvokeOnUpdateDelta {this.name} (Prev {prev}) (Curr {curr})");
            
            OnUpdateDelta?.Invoke(prev, curr);
        }

        protected void ResetOnUpdateDelta()
        {
            if (enableLogs) Debug.Log($"ResetOnUpdateDelta {this.name}");
            
            OnUpdateDelta = null;
        }
        
        public void Subscribe(OnUpdate callback)
        {
            if (enableLogs) Debug.Log($"Subscribe {this.name} (OnUpdate {callback.Method.Name})");
            
            this.OnUpdate += callback;
        }

        public void Subscribe(OnUpdate<T> callback)
        {
            if (enableLogs) Debug.Log($"Subscribe {this.name} (OnUpdate<T> {callback.Method.Name})");
            
            this.OnUpdateDelta += callback;
        }

        public void Unsubscribe(OnUpdate callback)
        {
            if (enableLogs) Debug.Log($"Unsubscribe {this.name} (OnUpdate {callback.Method.Name})");
            
            this.OnUpdate -= callback;
        }

        public void Unsubscribe(OnUpdate<T> callback)
        {
            if (enableLogs) Debug.Log($"Unsubscribe {this.name} (OnUpdate<T> {callback.Method.Name})");
            
            this.OnUpdateDelta -= callback;
        }

        #endregion

        public void SetValue(T value)
        {
            if (enableLogs) Debug.Log($"SetValue {this.name} (Current {Value}) (New {value})");
            
            Value = value;
        }

        // [Button]
        // [HorizontalGroup("Buttons")]
        public virtual void Reset()
        {
            if (enableLogs) Debug.Log($"Reset {this.name} (Runtime value {runtimeValue})");
            
            prevValue = runtimeValue;
            runtimeValue = defaultValue;
            this.OnUpdateDelta?.Invoke(prevValue, runtimeValue);
            this.OnUpdate?.Invoke();
        }

        public bool Save(string folderPath, string name = "")
        {
            return this.SaveInstance(folderPath, name);
        }

        private void OnEnable()
        {
            if (enableLogs) Debug.Log($"OnEnable {this.name}");
            
            hideFlags = HideFlags.DontUnloadUnusedAsset;
            Reset();
        }

        private void OnDisable()
        {
            if (enableLogs) Debug.Log($"OnDisable {this.name}");
            
            OnUpdate = null;
            //Undo.DestroyObjectImmediate(this);
            //AssetDatabase.SaveAssets();
        }
    }

    [Serializable]
    [InlineProperty]
    [SynchronizedHeader]
    public class Reference<T, VT> where VT : Variable<T>
    {
        #region Inspector

        [VerticalGroup("Top")]
        [HorizontalGroup("Top/Split", LabelWidth = 0.001f)]
        [HorizontalGroup("Top/Split/Left", LabelWidth = .01f)]
        [BoxGroup("Top/Split/Left/Left", ShowLabel = false)]
        [PropertyTooltip("$Tooltip")]
        [LabelText("C")]
        [LabelWidth(10f)]
        [SerializeField]
        protected bool UseConstant = false;

        [HorizontalGroup("Top/Split/Left/Right", LabelWidth = .01f)]
        [BoxGroup("Top/Split/Left/Right/Right", ShowLabel = false)]
        [PropertyTooltip("$Tooltip")]
        [LabelText("I")]
        [LabelWidth(10f)]
        [OnValueChanged("ResetInstanceOptions")]
        [SerializeField]
        protected bool EnableInstanceOptions = false;

        [VerticalGroup("Middle")]
        [VerticalGroup("Middle/Box/Top")]
        [LabelText("Name")]
        [LabelWidth(40f)]
        [ShowIf("InstanceNotConstant")]
        [SerializeField]
        protected String InstanceName;

        [VerticalGroup("Middle/Box/Middle")]
        [LabelText("Folder")]
        [LabelWidth(40f)]
        [FolderPath(ParentFolder = "Assets/MyAssets/ScriptableObjects/", RequireExistingPath = true)]
        [ShowIf("InstanceNotConstant")]
        [SerializeField]
        protected String InstancePath = "IntermediateProperties";

        public String LabelText => UseConstant ? "" : "?";

        [HorizontalGroup("Top/Split", LabelWidth = 0.001f, Width = .7f)]
        [BoxGroup("Top/Split/Right", ShowLabel = false)]
        [HideLabel]
        [ShowIf("UseConstant")]
        [SerializeField]
        protected T ConstantValue;

        [BoxGroup("Top/Split/Right", ShowLabel = false)]
        [HideLabel]
        [HideIf("UseConstant")]
        [InlineEditor()]
        [SerializeField]
        protected VT Variable;

        public String Tooltip => Variable != null && !UseConstant ? $"{Variable.name}:\n{Variable.Description}" : "";

        protected bool InstanceNotConstant => (!UseConstant && EnableInstanceOptions);

        #endregion

        //WARNING: will update subscribers synchronously within whatever time cycle the var is updated (Awake, LateUpdate, etc.)
        public void Subscribe(OnUpdate callback)
        {
            Variable?.Subscribe(callback);
        }

        public void Unsubscribe(OnUpdate callback)
        {
            Variable?.Unsubscribe(callback);
        }

        public void Subscribe(OnUpdate<T> callback)
        {
            Variable?.Subscribe(callback);
        }

        public void Unsubscribe(OnUpdate<T> callback)
        {
            Variable?.Unsubscribe(callback);
        }

        [PropertyTooltip("Create an Instance SO")]
        [BoxGroup("Middle/Box", ShowLabel = false)]
        [VerticalGroup("Middle/Box/Bottom")]
        [LabelWidth(.01f)]
        [GUIColor(.85f, 1f, .9f)]
        [ShowIf("InstanceNotConstant")]
        [Button("Create Instance", ButtonSizes.Small)]
        public void CreateInstance()
        {
            UseConstant = false;
            VT tempScriptableObj = ScriptableObject.CreateInstance(typeof(VT)) as VT;
            String fullPath = $"Assets/MyAssets/ScriptableObjects/{InstancePath}/";

            //Try saving scriptable object to desired path
            if(tempScriptableObj.Save(fullPath, InstanceName))
            {
                Variable = tempScriptableObj;
                EnableInstanceOptions = false;
            }
        }

        private void ResetInstanceOptions()
        {
            InstanceName = "{I} ";
            InstancePath = "IntermediateProperties";
        }

        public void Reset()
        {
            if(Variable != null && !UseConstant)
                Variable.Reset();
            else
                Debug.Log($"Trying to reset a SO <{Name}> using a constant value1 Nothing will happen.");
        }
        
        public String Name
        {
            get
            {
                if(UseConstant)
                    return $"<Const>{ConstantValue}";

                return (Variable != null) ? Variable.name : "<Missing Float>";
            }
        }

        public T Value
        {
            get
            {
                if(Variable != null)
                    return UseConstant ? ConstantValue : Variable.Value;
                else
                {
                    if(UseConstant)
                        return ConstantValue;
                    else
                    {
                        Debug.LogError($"Trying to access {Name} variable but none set in inspector!");
                        return default(T);
                    }
                }
            }
            set
            {
                if(Variable != null) Variable.Value = value;
                else if(!UseConstant) Debug.LogError($"Trying to set {Name} variable that is null!");
            }
        }
    }

}