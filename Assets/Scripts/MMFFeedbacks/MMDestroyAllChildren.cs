using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;

namespace BML.Scripts.MMFFeedbacks
{
    [AddComponentMenu("")]
    [FeedbackHelp("This feedback allows you destroy all children of the given transform.")]
    [FeedbackPath("GameObject/Destroy All Children")]
    public class MMDestroyAllChildren : MMF_Feedback
    {
        [MMFInspectorGroup("Target", true, 40)]
        public Transform parent;
        
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1)
        {
            parent.MMDestroyAllChildren();
        }
    }
}