using MoreMountains.Feedbacks;
using UnityEngine;

namespace BML.Scripts.MMFFeedbacks
{
    /// <summary>
    /// This feedback will instantiate a projectile and set the colliders it should ignore
    /// </summary>
    [ExecuteAlways]
    [AddComponentMenu("")]
    [FeedbackPath("GameObject/Layer")]
    [FeedbackHelp("This feedback modify the layer of a GameObject")]
    public class MMGameObjectLayer : MMF_Feedback
    {
        /// a static bool used to disable all feedbacks of this type at once
        public static bool FeedbackTypeAuthorized = true;
        
        [MMFInspectorGroup("Modify Layer", true, 37, true)]
        /// the object whose layer to modify
        [Tooltip("the object whose layer to modify")]
        public GameObject ObjectToModify;
        
        [MMFInspectorGroup("Modify Layer", true, 37, true)]
        /// The name of the layer to assign to the object
        [Tooltip("The name of the layer to assign to the object")]
        public string LayerToAssign;


        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1)
        {
            if (!Active || !FeedbackTypeAuthorized || (ObjectToModify == null))
            {
                return;
            }

            ObjectToModify.layer = LayerMask.NameToLayer(LayerToAssign);
        }
    }
}