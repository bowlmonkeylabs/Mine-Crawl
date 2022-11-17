using System.Collections;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.UI;

namespace BML.Scripts.MMFFeedbacks
{
    /// <summary>
    /// This feedback will make the bound Image flicker for the set duration when played (and restore its initial color when stopped)
    /// </summary>
    [AddComponentMenu("")]
    [FeedbackHelp("This feedback lets you flicker the color of a specified Image for a certain duration, at the specified octave, and with the specified color.")]
    [FeedbackPath("UI/Flicker")]
    public class MMFlickerImage : MMF_Feedback
    {
        /// a static bool used to disable all feedbacks of this type at once
        public static bool FeedbackTypeAuthorized = true;
        /// sets the inspector color for this feedback
        #if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.RendererColor; } }
        public override bool EvaluateRequiresSetup() => (Image == null);
        public override string RequiredTargetText => Image != null ? Image.name : "";
        public override string RequiresSetupText => "This feedback requires that a Image be set to be able to work properly. You can set one below.";
        #endif
        

        [MMFInspectorGroup("Flicker", true, 61, true)]
        /// the image to flicker when played
        [Tooltip("the image to flicker when played")]
        public Image Image;
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

        /// the duration of this feedback is the duration of the flicker
        public override float FeedbackDuration { get { return ApplyTimeMultiplier(FlickerDuration); } set { FlickerDuration = value; } }

        protected Color _initialFlickerColor;
        protected Coroutine _coroutine;

        /// <summary>
        /// On init we grab our initial color and components
        /// </summary>
        /// <param name="owner"></param>
        protected override void CustomInitialization(MMF_Player owner)
        {
            if (Active && (Image == null) && (owner != null))
            {
                if (Owner.gameObject.MMFGetComponentNoAlloc<Image>() != null)
                {
                    Image = owner.GetComponent<Image>();
                }
                if (Image == null)
                {
                    Image = owner.GetComponentInChildren<Image>();
                }
            }

            if (Image == null)
            {
                Debug.LogWarning("[MMFeedbackFlicker] The flicker feedback on "+Owner.name+" doesn't have an Image component, it won't work. You need to specify a Image to flicker in its inspector.");    
            }

            _initialFlickerColor = Image.color;
        }

        /// <summary>
        /// On play we make our renderer flicker
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (!Active || !FeedbackTypeAuthorized || (Image == null))
            {
                return;
            }

            _coroutine = Owner.StartCoroutine(Flicker(_initialFlickerColor, FlickerColor, FlickerOctave, FeedbackDuration));
            
        }

        /// <summary>
        /// On reset we make our renderer stop flickering
        /// </summary>
        protected override void CustomReset()
        {
            base.CustomReset();
            
            if (Active && FeedbackTypeAuthorized && (Image != null))
            {
                SetColor(_initialFlickerColor);
            }

            if (InCooldown)
            {
                return;
            }
        }

        public virtual IEnumerator Flicker(Color initialColor, Color flickerColor, float flickerSpeed, float flickerDuration)
        {
            if (Image == null)
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
                SetColor(flickerColor);
                if (Timing.TimescaleMode == TimescaleModes.Scaled)
                {
                    yield return MMFeedbacksCoroutine.WaitFor(flickerSpeed);
                }
                else
                {
                    yield return MMFeedbacksCoroutine.WaitForUnscaled(flickerSpeed);
                }
                SetColor(initialColor);
                if (Timing.TimescaleMode == TimescaleModes.Scaled)
                {
                    yield return MMFeedbacksCoroutine.WaitFor(flickerSpeed);
                }
                else
                {
                    yield return MMFeedbacksCoroutine.WaitForUnscaled(flickerSpeed);
                }
            }

            SetColor(initialColor);
            IsPlaying = false;
        }

        protected virtual void SetColor(Color color)
        {
            Image.color = color;
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

            if (_coroutine != null)
            {
                Owner.StopCoroutine(_coroutine);    
            }
            _coroutine = null;    
            
        }
    }
}
