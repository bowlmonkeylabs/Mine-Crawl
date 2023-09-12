using System;
using System.Collections;
using System.Collections.Generic;
using BML.ScriptableObjectCore.Scripts.CustomAttributes;
using BML.ScriptableObjectCore.Scripts.Variables.SafeValueReferences;
using BML.ScriptableObjectCore.Scripts.Variables.ValueReferences;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace BML.ScriptableObjectCore.Scripts.Variables
{
    public delegate void OnUpdate_();
    public delegate void OnFinished_();

    [Required]
    [InlineEditor(InlineEditorObjectFieldModes.Foldout)]
    [CreateAssetMenu(fileName = "TimerVariable", menuName = "BML/Variables/TimerVariable", order = 0)]
    public class TimerVariable : ScriptableObject
    {
        [SerializeField] private SafeFloatValueReference _duration;

        public float Duration
        {
            get => _duration.Value;
            set => _duration.Value = value;
        }
        
        [ShowInInspector, NonSerialized, ReadOnly] private float? remainingTime = null;
        [ShowInInspector, NonSerialized, ReadOnly] private bool isFinished = false;
        [TextArea (7, 10)] [HideInInlineEditors] public String Description;
        
        public event OnUpdate_ OnUpdate;
        public event OnFinished_ OnFinished;

        // public float Duration => duration;
        public float? RemainingTime => remainingTime;
        [ShowInInspector, ReadOnly]
        public float ElapsedTime => Duration - (remainingTime ?? Duration);
        public bool IsFinished => isFinished;
        
        [ShowInInspector, ReadOnly]
        public bool IsStopped => isStopped;

        [ShowInInspector, ReadOnly]
        public bool IsStarted => isStarted;

        [NonSerialized]
        private float startTime = Mathf.NegativeInfinity;
        
        [NonSerialized]
        private float lastUpdateTime = Mathf.NegativeInfinity;

        [NonSerialized]
        private bool isStopped = true;

        [NonSerialized] 
        private bool isStarted = false;

        public void StartTimer()
        {
            isStarted = true;
            isStopped = false;
            isFinished = false;
            startTime = Time.time;
            lastUpdateTime = startTime;
            remainingTime = Duration;
            OnUpdate?.Invoke();
        }

        public void ResetTimer()
        {
            isStarted = false;
            isStopped = true;
            isFinished = false;
            startTime = Time.time;
            lastUpdateTime = startTime;
            remainingTime = Duration;
            OnUpdate?.Invoke();
        }

        public void StopTimer()
        {
            isStopped = true;
        }

        public void Subscribe(OnUpdate_ callback)
        {
            this.OnUpdate += callback;
        }

        public void Unsubscribe(OnUpdate_ callback)
        {
            this.OnUpdate -= callback;
        }
        
        public void SubscribeFinished(OnFinished_ callback)
        {
            this.OnFinished += callback;
        }

        public void UnsubscribeFinished(OnFinished_ callback)
        {
            this.OnFinished -= callback;
        }

        public void UpdateTime(float multiplier = 1f)
        {
            if (!isStopped && !isFinished)
            {
                var updateTime = Time.time;
                var deltaTime = (updateTime - lastUpdateTime);
                lastUpdateTime = updateTime;
                
                remainingTime -= deltaTime * multiplier;
                remainingTime = Mathf.Max(0f, remainingTime.Value);

                OnUpdate?.Invoke();
                if (remainingTime == 0)
                {
                    isFinished = true;
                    OnFinished?.Invoke();
                }
            }
            
        }
        
    }

    [Serializable]
    [InlineProperty]
    [SynchronizedHeader]
    public class TimerReference
    {
        [HorizontalGroup("Split", LabelWidth = .01f, Width = .2f)] [PropertyTooltip("$Tooltip")]
        [BoxGroup("Split/Left", ShowLabel = false)] [LabelText("$LabelText")] [LabelWidth(10f)]
        [SerializeField] private bool UseConstant = false;

        public void SetVariable(TimerVariable timerVariable)
        {
            UseConstant = false;
            Variable = timerVariable;
        }
        
        [BoxGroup("Split/Right", ShowLabel = false)] [HideLabel] [ShowIf("UseConstant")]
        [SerializeField] private SafeFloatValueReference ConstantDuration;
        
        [SerializeField] [ShowIf("UseConstant")] [DisableIf("AlwaysTrue")]
        private float? ConstantRemainingTime = null;

        private bool AlwaysTrue => true;
        private bool isConstantStarted = true;
        private bool isConstantStopped = true;
        private bool isConstantFinished = false;

        public float ElapsedTime
        {
            get
            {
                if (UseConstant)
                    return ConstantDuration.Value - (ConstantRemainingTime ?? ConstantDuration.Value);
                if (Variable != null)
                    return Variable.ElapsedTime;
                
                Debug.LogError("Trying to access elapsed time for timer variable that is not set!");
                return Mathf.Infinity;
            }
        }

        public bool IsStarted => (UseConstant) ? isConstantStarted : Variable.IsStarted;
        public bool IsStopped => (UseConstant) ? isConstantStopped : Variable.IsStopped;

        [BoxGroup("Split/Right", ShowLabel = false)] [HideLabel] [HideIf("UseConstant")] 
        [SerializeField] private TimerVariable Variable;

        private float startTime = Mathf.NegativeInfinity;
        private float lastUpdateTime = Mathf.NegativeInfinity;

        public String Tooltip => Variable != null && !UseConstant ? Variable.Description : "";
        public String LabelText => UseConstant ? "" : "?";

        public float Duration
        {
            get
            {
                if (UseConstant)
                    return ConstantDuration.Value;
                if (Variable != null)
                    return Variable.Duration;
                
                Debug.LogError("Trying to access duration for timer variable that is not set!");
                return Mathf.Infinity;
            }
            set
            {
                if (UseConstant)
                {
                    ConstantDuration.Value = value;
                    return;
                }
                if (Variable != null)
                {
                    Variable.Duration = value;
                    return;
                }
                
                Debug.LogError("Trying to access duration for timer variable that is not set!");
            }
        }

        public float? RemainingTime
        {
            get
            {
                if (UseConstant)
                    return ConstantRemainingTime;
                if (Variable != null)
                    return Variable.RemainingTime;
                
                Debug.LogError("Trying to access remaining time for timer variable that is not set!");
                return Mathf.Infinity;
            }
        }
        
        public bool IsFinished
        {
            get
            {
                if (UseConstant)
                    return isConstantFinished;
                if (Variable != null)
                    return Variable.IsFinished;
                
                Debug.LogError("Trying to access IsFinished for timer variable that is not set!");
                return false;
            }
        } 


        public String Name
        {
            get
            {
                if (UseConstant) 
                    return $"<Const>{ConstantRemainingTime}";
                    
                return (Variable != null) ? Variable.name : "<Missing Timer>";
            }
        }


        public void RestartTimer()
        {
            Variable?.StartTimer();
            isConstantStarted = true;
            isConstantStopped = false;
            isConstantFinished = false;
            startTime = Time.time;
            lastUpdateTime = startTime;
            ConstantRemainingTime = Duration;
        }

        public void ResetTimer()
        {
            Variable?.ResetTimer();
            isConstantStarted = false;
            isConstantStopped = true;
            isConstantFinished = false;
            startTime = Time.time;
            lastUpdateTime = startTime;
            ConstantRemainingTime = Duration;
        }

        public void StopTimer()
        {
            Variable?.StopTimer();
            isConstantStopped = true;
        }

        public void UpdateTime(float multiplier = 1f)
        {
            Variable?.UpdateTime(multiplier);

            var updateTime = Time.time;
            var deltaTime = (updateTime - lastUpdateTime);
            lastUpdateTime = updateTime;

            if (!isConstantStopped && !isConstantFinished)
            {
                ConstantRemainingTime -= deltaTime * multiplier;
                ConstantRemainingTime = Mathf.Max(0f, ConstantRemainingTime.Value);
                
                if (ConstantRemainingTime == 0)
                {
                    isConstantFinished = true;
                }
            }
        }
        
        public void Subscribe(OnUpdate_ callback)
        {
            Variable?.Subscribe(callback);
        }

        public void Unsubscribe(OnUpdate_ callback)
        {
            Variable?.Unsubscribe(callback);
        }
        
        public void SubscribeFinished(OnFinished_ callback)
        {
            Variable?.SubscribeFinished(callback);
        }

        public void UnsubscribeFinished(OnFinished_ callback)
        {
            Variable?.UnsubscribeFinished(callback);
        }
    }
}
