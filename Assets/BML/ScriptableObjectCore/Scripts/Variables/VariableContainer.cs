using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BML.ScriptableObjectCore.Scripts.Utils;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace BML.ScriptableObjectCore.Scripts.Variables
{
    [Flags]
    public enum VariableContainerKey
    {
        None = 0,
        ResetOnRestartGame = 1 << 0,
        ResetOnStartAnyLevel = 1 << 1,
        ResetOnStartLevel0 = 1 << 2,
        ResetOnStartLevel1 = 1 << 3,
    }
    
    [Required]
    [CreateAssetMenu(fileName = "VariableContainer", menuName = "BML/Variables/VariableContainer", order = 0)]
    public class VariableContainer : ScriptableObject
    {
        private enum ContainerPopulateMode
        {
            Manual = 0,
            Folder = 1,
            ContainerKey = 2,
        }
        
        [TitleGroup("Populate Container"), PropertyOrder(-3)]
        [SerializeField] private ContainerPopulateMode _populateMode = ContainerPopulateMode.Manual;
        
        [TitleGroup("Populate Container"), ShowIf("@_populateMode == ContainerPopulateMode.Folder")]
        [FolderPath (RequireExistingPath = true), PropertyOrder(-2)]
        [InlineButton("SetThisFolder", "This Folder"), PropertyTooltip("Sets 'Folder Path' for auto-population to the folder where this container is located.")]
        [SerializeField] private string FolderPath;

#if UNITY_EDITOR
        // [PropertyTooltip("Sets 'Folder Path' for auto-population to the folder where this container is located.")]
        public void SetThisFolder()
        {
            string fullPath = AssetDatabase.GetAssetPath(this);
            FolderPath = fullPath.Substring(0, fullPath.LastIndexOf('/'));
        }
#endif
        
        [TitleGroup("Populate Container"), ShowIf("@_populateMode == ContainerPopulateMode.Folder")]
        [SerializeField] private bool IncludeSubdirectories = false;

        [TitleGroup("Populate Container"), ShowIf("@_populateMode == ContainerPopulateMode.ContainerKey")]
        [SerializeField] private VariableContainerKey ContainerKey;

        [TextArea (5, 10)] public String Description;
        
        [Required] [PropertySpace(SpaceBefore = 10, SpaceAfter = 10)] [ListDrawerSettings(ShowPaging = false)]
        [SerializeField] private List<TriggerVariable> TriggerVariables = new List<TriggerVariable>();
        
        [Required] [PropertySpace(SpaceBefore = 0, SpaceAfter = 10)] [ListDrawerSettings(ShowPaging = false)]
        [SerializeField] private List<BoolVariable> BoolVariables = new List<BoolVariable>();
        
        [Required] [PropertySpace(SpaceBefore = 0, SpaceAfter = 10)] [ListDrawerSettings(ShowPaging = false)]
        [SerializeField] private List<IntVariable> IntVariables = new List<IntVariable>();
        
        [Required] [PropertySpace(SpaceBefore = 0, SpaceAfter = 10)] [ListDrawerSettings(ShowPaging = false)]
        [SerializeField] private List<FloatVariable> FloatVariables = new List<FloatVariable>();
        
        [Required] [PropertySpace(SpaceBefore = 0, SpaceAfter = 10)] [ListDrawerSettings(ShowPaging = false)]
        [SerializeField] private List<Vector2Variable> Vector2Variables = new List<Vector2Variable>();
        
        [Required] [PropertySpace(SpaceBefore = 0, SpaceAfter = 10)] [ListDrawerSettings(ShowPaging = false)]
        [SerializeField] private List<Vector3Variable> Vector3Variables = new List<Vector3Variable>();
        
        [Required] [PropertySpace(SpaceBefore = 0, SpaceAfter = 10)] [ListDrawerSettings(ShowPaging = false)]
        [SerializeField] private List<QuaternionVariable> QuaternionVariables = new List<QuaternionVariable>();
        
        [Required] [PropertySpace(SpaceBefore = 0, SpaceAfter = 10)] [ListDrawerSettings(ShowPaging = false)]
        [SerializeField] private List<TimerVariable> TimerVariables = new List<TimerVariable>();

        [Required] [PropertySpace(SpaceBefore = 0, SpaceAfter = 10)] [ListDrawerSettings(ShowPaging = false)]
        [SerializeField] private List<FunctionVariable> FunctionVariables = new List<FunctionVariable>();

        [Required] [PropertySpace(SpaceBefore = 0, SpaceAfter = 10)] [ListDrawerSettings(ShowPaging = false)]
        [SerializeField] private List<LootTableVariable> LootTableVariables = new List<LootTableVariable>();
        
        public List<TriggerVariable> GetTriggerVariables() => TriggerVariables;
        public List<BoolVariable> GetBoolVariables() => BoolVariables;
        public List<IntVariable> GetIntVariables() => IntVariables;
        public List<FloatVariable> GetFloatVariables() => FloatVariables;
        public List<Vector2Variable> GetVector2Variables() => Vector2Variables;
        public List<Vector3Variable> GetVector3Variables() => Vector3Variables;
        public List<QuaternionVariable> GetQuaternionVariables() => QuaternionVariables;
        public List<TimerVariable> GetTimerVariables() => TimerVariables;
        public List<FunctionVariable> GetFunctionVariables() => FunctionVariables;
        public List<LootTableVariable> GetLootTableVariables() => LootTableVariables;

    #if UNITY_EDITOR

        [GUIColor(0, 1, 0)]
        [TitleGroup("Populate Container"), PropertyOrder(0), ShowIf("@_populateMode != ContainerPopulateMode.Manual")]
        [Button(ButtonSizes.Large), DisableIf("@(_populateMode == ContainerPopulateMode.Folder && string.IsNullOrEmpty(FolderPath))")]
        public virtual void PopulateContainer()
        {
            if (_populateMode == ContainerPopulateMode.Manual)
            {
                return;
            }
            
            TriggerVariables.Clear();
            BoolVariables.Clear();
            IntVariables.Clear();
            FloatVariables.Clear();
            Vector2Variables.Clear();
            Vector3Variables.Clear();
            QuaternionVariables.Clear();
            TimerVariables.Clear();
            FunctionVariables.Clear();
            LootTableVariables.Clear();

            if (_populateMode == ContainerPopulateMode.Folder)
            {
                TriggerVariables = AssetDatabaseUtils.FindAndLoadAssetsOfType<TriggerVariable>(FolderPath, IncludeSubdirectories).ToList();
                BoolVariables = AssetDatabaseUtils.FindAndLoadAssetsOfType<BoolVariable>(FolderPath, IncludeSubdirectories).ToList();
                IntVariables = AssetDatabaseUtils.FindAndLoadAssetsOfType<IntVariable>(FolderPath, IncludeSubdirectories).ToList();
                FloatVariables = AssetDatabaseUtils.FindAndLoadAssetsOfType<FloatVariable>(FolderPath, IncludeSubdirectories).ToList();
                Vector2Variables = AssetDatabaseUtils.FindAndLoadAssetsOfType<Vector2Variable>(FolderPath, IncludeSubdirectories).ToList();
                Vector3Variables = AssetDatabaseUtils.FindAndLoadAssetsOfType<Vector3Variable>(FolderPath, IncludeSubdirectories).ToList();
                QuaternionVariables = AssetDatabaseUtils.FindAndLoadAssetsOfType<QuaternionVariable>(FolderPath, IncludeSubdirectories).ToList();
                TimerVariables = AssetDatabaseUtils.FindAndLoadAssetsOfType<TimerVariable>(FolderPath, IncludeSubdirectories).ToList();
                FunctionVariables = AssetDatabaseUtils.FindAndLoadAssetsOfType<FunctionVariable>(FolderPath, IncludeSubdirectories).ToList();
                LootTableVariables = AssetDatabaseUtils.FindAndLoadAssetsOfType<LootTableVariable>(FolderPath, IncludeSubdirectories).ToList();
            }
            else if (_populateMode == ContainerPopulateMode.ContainerKey)
            {
                // Only populate lists of resettable variable types
                // TriggerVariables = FindAndLoadAssetsOfType<TriggerVariable>().Where(variable => (variable.IncludeInContainers & ContainerKey) == ContainerKey).ToList();
                BoolVariables = AssetDatabaseUtils.FindAndLoadAssetsOfType<BoolVariable>().Where(variable => (variable.IncludeInContainers & ContainerKey) == ContainerKey).ToList();
                IntVariables = AssetDatabaseUtils.FindAndLoadAssetsOfType<IntVariable>().Where(variable => (variable.IncludeInContainers & ContainerKey) == ContainerKey).ToList();
                FloatVariables = AssetDatabaseUtils.FindAndLoadAssetsOfType<FloatVariable>().Where(variable => (variable.IncludeInContainers & ContainerKey) == ContainerKey).ToList();
                Vector2Variables = AssetDatabaseUtils.FindAndLoadAssetsOfType<Vector2Variable>().Where(variable => (variable.IncludeInContainers & ContainerKey) == ContainerKey).ToList();
                Vector3Variables = AssetDatabaseUtils.FindAndLoadAssetsOfType<Vector3Variable>().Where(variable => (variable.IncludeInContainers & ContainerKey) == ContainerKey).ToList();
                QuaternionVariables = AssetDatabaseUtils.FindAndLoadAssetsOfType<QuaternionVariable>().Where(variable => (variable.IncludeInContainers & ContainerKey) == ContainerKey).ToList();
                TimerVariables = AssetDatabaseUtils.FindAndLoadAssetsOfType<TimerVariable>().Where(variable => (variable.IncludeInContainers & ContainerKey) == ContainerKey).ToList();
                // FunctionVariables = FindAndLoadAssetsOfType<FunctionVariable>().Where(variable => (variable.IncludeInContainers & ContainerKey) == ContainerKey).ToList();
                LootTableVariables = AssetDatabaseUtils.FindAndLoadAssetsOfType<LootTableVariable>().Where(variable => (variable.IncludeInContainers & ContainerKey) == ContainerKey).ToList();
            }

            Debug.Log($"{TriggerVariables.Count} Triggers" +
                      $" | {BoolVariables.Count} Bools" +
                      $" | {IntVariables.Count} Ints" +
                      $" | {FloatVariables.Count} Floats" +
                      $" | {Vector2Variables.Count} Vector2s" +
                      $" | {Vector3Variables.Count} Vector3s" +
                      $" | {QuaternionVariables.Count} Quaternions" +
                      $" | {TimerVariables.Count} Timers" +
                      $" | {FunctionVariables.Count} Functions" +
                      $" | {LootTableVariables.Count} Loot tables");
        }

#endif
        
    }


}

