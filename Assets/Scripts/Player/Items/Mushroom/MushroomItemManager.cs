using System;
using System.Collections.Generic;
using System.Linq;
using BML.Scripts.CaveV2;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BML.Scripts.Player.Items.Mushroom
{
    public class MushroomItemManager : MonoBehaviour
    {
        [SerializeField] private List<MushroomItem> _mushroomItems;

        private bool _notEnoughVisualsForItems => _mushroomItemVisuals.Count < _mushroomItems.Count;
        [InfoBox("Not enough unique visuals for all of the provided items1.", InfoMessageType.Error, "_notEnoughVisualsForItems")]
        [SerializeField] private List<MushroomItemVisual> _mushroomItemVisuals;

        private void Start()
        {
            RandomizeMushroomVisuals();
        }

        private void RandomizeMushroomVisuals()
        {
            SeedManager.Instance.GetSteppedSeed("MushroomItemManager");
            var randomlyOrderedVisuals = _mushroomItemVisuals
                .OrderBy(v => Random.value).ToList();
            for (int i = 0; i < _mushroomItems.Count; i++)
            {
                // Reset "is known" (because the items are scriptable objects, they can't reset on their own)
                _mushroomItems[i].ResetScriptableObject();
                
                // Assign random visual
                _mushroomItems[i].MushroomItemVisual = randomlyOrderedVisuals[i];
            }
        }
        
    }
}