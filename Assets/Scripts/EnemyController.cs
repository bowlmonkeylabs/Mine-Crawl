using UnityEngine;

namespace BML.Scripts
{
    public class EnemyController : MonoBehaviour
    {
        [SerializeField] private BMLAIDestinationSetter _destinationSetter;
        [SerializeField] private GameObject _idleArms;
        [SerializeField] private GameObject _alertedArms;
        
        public void SetAlerted(bool alerted)
        {
            _idleArms.SetActive(!alerted);
            _alertedArms.SetActive(alerted);

            _destinationSetter.enabled = alerted;
        }
    }
}