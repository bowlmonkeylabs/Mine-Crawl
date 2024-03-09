using MoreMountains.Feedbacks;
using UnityEngine;

namespace BML.Scripts.Player.Items.ItemEffects
{
    public class TeleportItemEffect : ItemEffect
    {
        #region Inspector

        [SerializeField] private string _teleportPointName;
        
        #endregion

        private PlayerController _playerController;
        public void PrimeEffect(PlayerController playerController)
        {
            _playerController = playerController;
        }

        protected override bool ApplyEffectInternal()
        {
            _playerController.TpToWaypoint(_teleportPointName);
            return true;
        }

        protected override bool UnapplyEffectInternal()
        {
            return false;
        }
        
    }
}