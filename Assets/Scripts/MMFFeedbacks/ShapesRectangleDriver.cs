using System.Collections;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using ThisOtherThing.UI.Shapes;
using UnityEngine;
using UnityEngine.Serialization;

namespace MMFFeedbacks
{
	/// <summary>
	/// This feedback will animate the size of the target rectangle over time when played
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackPath("Shapes/RectangleDriver")]
	[FeedbackHelp("This feedback will animate the size of the target rectangle over time when played")]
	public class ShapesRectangleDriver : MMF_Feedback
    {
        /// a static bool used to disable all feedbacks of this type at once
        public static bool FeedbackTypeAuthorized = true;
        public enum TimeScales { Scaled, Unscaled }
        /// sets the inspector color for this feedback
#if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.TransformColor; } }
        public override bool EvaluateRequiresSetup() { return (TargetRectangleObj == null); }
        public override string RequiredTargetText { get { return TargetRectangleObj != null ? TargetRectangleObj.name : "";  } }
        public override string RequiresSetupText { get { return "This feedback requires that an AnimateScaleTarget be set to be able to work properly. You can set one below."; } }
        public override bool HasCustomInspectors { get { return true; } }
#endif
	    [MMFInspectorGroup("Scale Animation", true, 13)]
        /// if this is true, calling that feedback will trigger it, even if it's in progress. If it's false, it'll prevent any new Play until the current one is over
        [Tooltip("if this is true, calling that feedback will trigger it, even if it's in progress. If it's false, it'll prevent any new Play until the current one is over")] 
        public bool AllowAdditivePlays = false;
        [Tooltip("the duration of the animation")]
        public float AnimateScaleDuration = 0.2f;
        [Tooltip("the object to animate")]
        public GameObject TargetRectangleObj;
        [Tooltip("the value to remap the curve's 0 value to")]
        public float RemapCurveZero = 1f;
        /// the value to remap the curve's 1 value to
        [Tooltip("the value to remap the curve's 1 value to")]
        [FormerlySerializedAs("Multiplier")]
        public float RemapCurveOne = 2f;
        [Tooltip("the x scale animation definition")]
        public MMTweenType AnimateScaleTweenX = new MMTweenType( new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1.5f), new Keyframe(1, 0)));

        /// the duration of this feedback is the duration of the scale animation
        public override float FeedbackDuration { get { return ApplyTimeMultiplier(AnimateScaleDuration); } set { AnimateScaleDuration = value; } }
        
        protected Coroutine _coroutine;
        protected Rectangle targetRectangle;

        /// <summary>
        /// On Play, triggers the scale animation
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
	        targetRectangle = TargetRectangleObj.GetComponent<Rectangle>();
	        
            if (!Active || !FeedbackTypeAuthorized || (TargetRectangleObj == null))
            {
                return;
            }
            
            
            float intensityMultiplier = ComputeIntensity(feedbacksIntensity, position);
            if (Active || Owner.AutoPlayOnEnable)
            {
                if (!AllowAdditivePlays && (_coroutine != null))
                {
                    return;
                }
                _coroutine = Owner.StartCoroutine(AnimateScale(targetRectangle, 0f, 
	                FeedbackDuration, AnimateScaleTweenX, RemapCurveZero * intensityMultiplier,
	                RemapCurveOne * intensityMultiplier));
            }
        }
        
        /// <summary>
		/// An internal coroutine used to animate the scale over time
		/// </summary>
		/// <param name="target"></param>
		/// <param name="vector"></param>
		/// <param name="duration"></param>
		/// <param name="curveX"></param>
        /// <param name="multiplier"></param>
		/// <returns></returns>
		protected virtual IEnumerator AnimateScale(Rectangle target, float value, float duration, MMTweenType curveX, float remapCurveZero = 0f, float remapCurveOne = 1f)
		{
			if (target == null)
			{
				yield break;
			}

			if (curveX == null)
			{
				yield break;
			}

			if (duration == 0f)
			{
				yield break;
			}
            
			float journey = NormalPlayDirection ? 0f : duration;
			
			IsPlaying = true;
			
			while ((journey >= 0) && (journey <= duration) && (duration > 0))
			{
				float percent = Mathf.Clamp01(journey / duration);

				//Animate
				value = curveX.Evaluate(percent);
				value = MMFeedbacksHelpers.Remap(value, 0f, 1f, remapCurveZero, remapCurveOne);
				

				target.ShadowProperties.Shadows[0].Size = value;
				target.ForceMeshUpdate();

				journey += NormalPlayDirection ? FeedbackDeltaTime : -FeedbackDeltaTime;
                
				yield return null;
			}
            
			value = 0;

			//Animate
			value = curveX.Evaluate(FinalNormalizedTime);
			value = MMFeedbacksHelpers.Remap(value, 0f, 1f, remapCurveZero, remapCurveOne);

			target.ShadowProperties.Shadows[0].Size = value;
			IsPlaying = false;
			_coroutine = null;
			yield return null;
		}
        
        /// <summary>
        /// On disable we reset our coroutine
        /// </summary>
        public override void OnDisable()
        {
	        _coroutine = null;
        }
        
        /// <summary>
        /// On stop, we interrupt movement if it was active
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
	        if (!Active || !FeedbackTypeAuthorized || (_coroutine == null))
	        {
		        return;
	        }
	        IsPlaying = false;
	        Owner.StopCoroutine(_coroutine);
	        _coroutine = null;
            
        }

    }
}