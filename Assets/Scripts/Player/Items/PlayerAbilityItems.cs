using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BML.ScriptableObjectCore.Scripts.Variables;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BML.Scripts.Player.Items
{
    public enum PlayerAbilityType {
        Movement = 1,
        PickaxeSecondary = 2
    }

    [CreateAssetMenu(fileName = "PlayerAbilityItems", menuName = "BML/Player/PlayerAbilityItems", order = 0)]
    public class PlayerAbilityItems : SerializedScriptableObject
    {
        [SerializeField] private PlayerInventory _playerInventory;
        [SerializeField] private IntVariable _currentLevel;
        [SerializeField] private PlayerResource _upgrades;
        [SerializeField] private List<int> _levelsForAbilityUpgrade;
        [SerializeField] private Dictionary<PlayerAbilityType, List<PlayerItem>> _playerAbilities;

        public List<PlayerAbilityType> UnobtainedAbilityTypes() {
            return _playerAbilities.Keys.Where(playerAbilityType => {
                return !_playerAbilities[playerAbilityType].Any(abilityItem => _playerInventory.AbilityItems.Items.Contains(abilityItem));
            }).ToList();
        }

        public List<PlayerItem> GetAbilityOptionsForType(PlayerAbilityType playerAbilityType) {
            return _playerAbilities[playerAbilityType];
        }

        public bool AbilityUpgradeAvailable() {
            var playerAbilityCount = _playerInventory.AbilityItems.Where(item => item != null).Count();
            if(playerAbilityCount > _levelsForAbilityUpgrade.Count - 1) {
                return false;
            }

            var levelForAbilityUpgrade = _levelsForAbilityUpgrade[playerAbilityCount] - 1;
            if((_currentLevel.Value - _upgrades.PlayerAmount) == levelForAbilityUpgrade) {
                return true;
            }
            
            return false;
        }
    }
}
