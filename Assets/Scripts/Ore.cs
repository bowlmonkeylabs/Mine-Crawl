using MoreMountains.Feedbacks;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace BML.Scripts
{
    public class Ore : MonoBehaviour
    {
        [SerializeField] private int _health = 2;
        public int Health => _health;

        [SerializeField] private MMF_Player _onDamageFeedbacks;
        [SerializeField] private MMF_Player _onDeathFeedbacks;

        [SerializeField] private UnityEvent _onDeath;

        public void DoDamage(System.Object damage)
        {
            int dmg = (int) damage;
            
            _health -= dmg;
            _onDamageFeedbacks.PlayFeedbacks();
            if (_health <= 0)
            {
                OnDeath();
            }
        }

        public void OnDeath()
        {
            _onDeath.Invoke();
            _onDeathFeedbacks.PlayFeedbacks();
            Destroy(this.gameObject);
        }
    }
}