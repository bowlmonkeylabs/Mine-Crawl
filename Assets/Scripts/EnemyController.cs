using MoreMountains.Feedbacks;
using UnityEngine;

namespace BML.Scripts
{
    public class EnemyController : MonoBehaviour
    {
        [SerializeField] private BMLAIDestinationSetter _destinationSetter;
        [SerializeField] private GameObject _idleArms;
        [SerializeField] private GameObject _alertedArms;
        [SerializeField] private MMF_Player _alertedFeedbacks;

        bool isAlerted;
        
        public void SetAlerted(bool alerted)
        {
            if (alerted && isAlerted)
                return;
            
            _idleArms.SetActive(!alerted);
            _alertedArms.SetActive(alerted);

            _destinationSetter.enabled = alerted;
            _alertedFeedbacks.PlayFeedbacks();

            isAlerted = alerted;
        }
    }
}