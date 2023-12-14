using UnityEngine;

namespace BML.Scripts.Player.Items.ItemEffects
{
    public class AddResourceItemEffect : ItemEffect
    {
        #region Inspector

        [SerializeField] private PlayerResource _resource;
        [SerializeField] private int _addAmount;
        
        #endregion

        protected override bool ApplyEffectInternal()
        {
            if (!CanAddResource())
            {
                return false;
            }

            _resource.PlayerAmount += _addAmount;
            return true;
        }

        protected override bool UnapplyEffectInternal()
        {
            return false;
        }

        public PlayerResource Resource => _resource;

        public bool CanAddResource()
        {
            var hypotheticalAmount = (_resource.PlayerAmount + _addAmount);
            return hypotheticalAmount >= 0 && (_resource.PlayerAmountLimit < 0 || hypotheticalAmount <= _resource.PlayerAmountLimit);
        }
        
    }
}