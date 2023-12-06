using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// This feedback will make the bound renderer flicker for the set duration when played (and restore its initial color when stopped)
    /// </summary>
    [AddComponentMenu("")]
    [FeedbackHelp("This feedback lets you flicker the color of a specified list of renderers (sprite, mesh, etc) for a certain duration, at the specified octave, and with the specified color. Useful when a character gets hit, for example (but so much more!).")]
    [FeedbackPath("Renderer/BML Flicker")]
    public class BML_Flicker : MMF_Feedback
    {
        /// a static bool used to disable all feedbacks of this type at once
        public static bool FeedbackTypeAuthorized = true;
        /// sets the inspector color for this feedback
        #if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.RendererColor; } }
        public override bool EvaluateRequiresSetup() => 
            (BoundRenderers == null || BoundRenderers.Count == 0 || BoundRenderers.Any(r => r == null));
        public override string RequiredTargetText => (BoundRenderers != null
            ? String.Join(",", BoundRenderers.Select(r => (r.SafeIsUnityNull() ? "" : r.name)))
            : "");
        public override string RequiresSetupText => "This feedback requires that a BoundRenderer be set to be able to work properly. You can set one below.";
        #endif

        /// the possible modes
        /// Color : will control material.color
        /// PropertyName : will target a specific shader property by name
        public enum Modes { Color, PropertyName }

        [MMFInspectorGroup("Flicker", true, 61, true)]
        /// the renderer to flicker when played
        [Tooltip("the renderers to flicker when played")]
        public List<Renderer> BoundRenderers = new List<Renderer>();
        /// the selected mode to flicker the renderer 
        [Tooltip("the selected mode to flicker the renderer")]
        public Modes Mode = Modes.Color;
        /// the name of the property to target
        [MMFEnumCondition("Mode", (int)Modes.PropertyName)]
        [Tooltip("the name of the property to target")]
        public string PropertyName = "_Tint";
        /// the duration of the flicker when getting damage
        [Tooltip("the duration of the flicker when getting damage")]
        public float FlickerDuration = 0.2f;
        /// the frequency at which to flicker
        [Tooltip("the frequency at which to flicker")]
        public float FlickerOctave = 0.04f;
        /// the color we should flicker the sprite to 
        [Tooltip("the color we should flicker the sprite to")]
        [ColorUsage(true, true)]
        public Color FlickerColor = new Color32(255, 20, 20, 255);
        /// the list of material indexes we want to flicker on the target renderer. If left empty, will only target the material at index 0 
        [Tooltip("the list of material indexes we want to flicker on the target renderer. If left empty, will only target the material at index 0")]
        public int[] MaterialIndexes;
        /// if this is true, this component will use material property blocks instead of working on an instance of the material.
        [Tooltip("if this is true, this component will use material property blocks instead of working on an instance of the material.")] 
        public bool UseMaterialPropertyBlocks = false;

        /// the duration of this feedback is the duration of the flicker
        public override float FeedbackDuration { get { return ApplyTimeMultiplier(FlickerDuration); } set { FlickerDuration = value; } }

        protected const string _colorPropertyName = "_Color";
        
        protected BoundRendererData[] _boundRendererDatas;

        public class BoundRendererData
        {
            public Renderer BoundRenderer;
            public Color[] InitialFlickerColors;
            public int[] PropertyIds;
            public bool[] PropertiesFound;
            public Coroutine[] Coroutines;
            public MaterialPropertyBlock PropertyBlock;

            public BoundRendererData(Renderer boundRenderer, int[] materialIndexesToFlicker)
            {
                BoundRenderer = boundRenderer;
                InitialFlickerColors = new Color[materialIndexesToFlicker.Length];
                PropertyIds = new int[materialIndexesToFlicker.Length];
                PropertiesFound = new bool[materialIndexesToFlicker.Length];
                Coroutines = new Coroutine[materialIndexesToFlicker.Length];
                PropertyBlock = new MaterialPropertyBlock();
            }
        }

        /// <summary>
        /// On init we grab our initial color and components
        /// </summary>
        /// <param name="owner"></param>
        protected override void CustomInitialization(MMF_Player owner)
        {
            if (MaterialIndexes.Length == 0)
            {
                MaterialIndexes = new int[1];
                MaterialIndexes[0] = 0;
            }

            _boundRendererDatas = BoundRenderers.Select(r => new BoundRendererData(r, MaterialIndexes)).ToArray();

            if (Active && (BoundRenderers.Count == 0) && (owner != null))
            {
                if (Owner.gameObject.MMFGetComponentNoAlloc<Renderer>() != null)
                {
                    BoundRenderers.Add(owner.GetComponent<Renderer>());
                }
                if (BoundRenderers.Count == 0)
                {
                    BoundRenderers.Add(owner.GetComponentInChildren<Renderer>());
                }
            }

            if (BoundRenderers.Count == 0)
            {
                Debug.LogWarning("[MMFeedbackFlicker] The flicker feedback on "+Owner.name+" doesn't have a bound renderer, it won't work. You need to specify a renderer to flicker in its inspector.");    
            }

            if (Active)
            {
                for (int i = 0; i < _boundRendererDatas.Length; i++)
                {
                    _boundRendererDatas[i].BoundRenderer.GetPropertyBlock(_boundRendererDatas[i].PropertyBlock);
                }
            }  
            
            for (int j = 0; j < _boundRendererDatas.Length; j++)
            {
                var curr = _boundRendererDatas[j];
                
                for (int i = 0; i < MaterialIndexes.Length; i++)
                {
                    curr.PropertiesFound[i] = false;

                    if (Active && (curr.BoundRenderer != null))
                    {
                        if (Mode == Modes.Color)
                        {
                            curr.PropertiesFound[i] = UseMaterialPropertyBlocks 
                                ? curr.BoundRenderer.sharedMaterials[i].HasProperty(_colorPropertyName) 
                                : curr.BoundRenderer.materials[i].HasProperty(_colorPropertyName);
                            if (curr.PropertiesFound[i])
                            {
                                curr.InitialFlickerColors[i] = UseMaterialPropertyBlocks 
                                    ? curr.BoundRenderer.sharedMaterials[i].color 
                                    : curr.BoundRenderer.materials[i].color;
                            }
                        }
                        else
                        {
                            curr.PropertiesFound[i] = UseMaterialPropertyBlocks 
                                ? curr.BoundRenderer.sharedMaterials[i].HasProperty(PropertyName) 
                                : curr.BoundRenderer.materials[i].HasProperty(PropertyName); 
                            if (curr.PropertiesFound[i])
                            {
                                curr.PropertyIds[i] = Shader.PropertyToID(PropertyName);
                                curr.InitialFlickerColors[i] = UseMaterialPropertyBlocks 
                                    ? curr.BoundRenderer.sharedMaterials[i].GetColor(curr.PropertyIds[i]) 
                                    : curr.BoundRenderer.materials[i].GetColor(curr.PropertyIds[i]);
                            }
                        }
                    }
                }
            }
            
        }

        /// <summary>
        /// On play we make our renderer flicker
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (!Active || !FeedbackTypeAuthorized || (BoundRenderers == null || BoundRenderers.Count == 0))
            {
                return;
            }

            for (int j = 0; j < _boundRendererDatas.Length; j++)
            {
                var curr = _boundRendererDatas[j];
                
                for (int i = 0; i < MaterialIndexes.Length; i++)
                {
                    curr.Coroutines[i] = Owner.StartCoroutine(
                        Flicker(curr.BoundRenderer, j, i, curr.InitialFlickerColors[i], FlickerColor, FlickerOctave, FeedbackDuration));
                }
            }
            
        }

        /// <summary>
        /// On reset we make our renderer stop flickering
        /// </summary>
        protected override void CustomReset()
        {
            base.CustomReset();

            if (InCooldown)
            {
                return;
            }

            if (Active && FeedbackTypeAuthorized && (BoundRenderers != null && BoundRenderers.Count > 0))
            {
                for (int j = 0; j < _boundRendererDatas.Length; j++)
                {
                    var curr = _boundRendererDatas[j];
                    
                    for (int i = 0; i < MaterialIndexes.Length; i++)
                    {
                        SetColor(j, i, curr.InitialFlickerColors[i]);
                    }   
                }
            }
        }

        public virtual IEnumerator Flicker(Renderer renderer, int rendererIndex, int materialIndex, Color initialColor, Color flickerColor, float flickerSpeed, float flickerDuration)
        {
            if (renderer == null)
            {
                yield break;
            }

            if (!_boundRendererDatas[rendererIndex].PropertiesFound[materialIndex])
            {
                yield break;
            }

            if (initialColor == flickerColor)
            {
                yield break;
            }

            float flickerStop = FeedbackTime + flickerDuration;
            IsPlaying = true;
            
            while (FeedbackTime < flickerStop)
            {
                SetColor(rendererIndex, materialIndex, flickerColor);
                if (Timing.TimescaleMode == TimescaleModes.Scaled)
                {
                    yield return MMFeedbacksCoroutine.WaitFor(flickerSpeed);
                }
                else
                {
                    yield return MMFeedbacksCoroutine.WaitForUnscaled(flickerSpeed);
                }
                SetColor(rendererIndex, materialIndex, initialColor);
                if (Timing.TimescaleMode == TimescaleModes.Scaled)
                {
                    yield return MMFeedbacksCoroutine.WaitFor(flickerSpeed);
                }
                else
                {
                    yield return MMFeedbacksCoroutine.WaitForUnscaled(flickerSpeed);
                }
            }

            SetColor(rendererIndex, materialIndex, initialColor);
            IsPlaying = false;
        }

        protected virtual void SetColor(int rendererIndex, int materialIndex, Color color)
        {
            if (!_boundRendererDatas[rendererIndex].PropertiesFound[materialIndex])
            {
                return;
            }

            var curr = _boundRendererDatas[rendererIndex];
            
            if (Mode == Modes.Color)
            {
                if (UseMaterialPropertyBlocks)
                {
                    curr.BoundRenderer.GetPropertyBlock(curr.PropertyBlock);
                    curr.PropertyBlock.SetColor(_colorPropertyName, color);
                    curr.BoundRenderer.SetPropertyBlock(curr.PropertyBlock, materialIndex);
                }
                else
                {
                    curr.BoundRenderer.materials[materialIndex].color = color;
                }
            }
            else
            {
                if (UseMaterialPropertyBlocks)
                {
                    curr.BoundRenderer.GetPropertyBlock(curr.PropertyBlock);
                    curr.PropertyBlock.SetColor(curr.PropertyIds[materialIndex], color);
                    curr.BoundRenderer.SetPropertyBlock(curr.PropertyBlock, materialIndex);
                }
                else
                {
                    curr.BoundRenderer.materials[materialIndex].SetColor(curr.PropertyIds[materialIndex], color);
                }
            }            
        }
        
        /// <summary>
        /// Stops this feedback
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
        {
            if (!Active || !FeedbackTypeAuthorized)
            {
                return;
            }
            base.CustomStopFeedback(position, feedbacksIntensity);
            
            IsPlaying = false;
            for (int j = 0; j < _boundRendererDatas.Length; j++)
            {
                var curr = _boundRendererDatas[j];
                
                for (int i = 0; i < curr.Coroutines.Length; i++)
                {
                    if (curr.Coroutines[i] != null)
                    {
                        Owner.StopCoroutine(curr.Coroutines[i]);    
                    }
                    curr.Coroutines[i] = null;    
                }
            }
            
        }
    }
}
