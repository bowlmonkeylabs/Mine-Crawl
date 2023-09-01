using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace BML.ScriptableObjectCore.Scripts.SceneReferences
{
    [CreateAssetMenu(fileName = "GameObjectSceneReference", menuName = "BML/SceneReferences/GameObjectSceneReference",
        order = 0)]
    public class GameObjectSceneReference : SceneReference<GameObject>
    {
        public override GameObject Value
        {
            get => base.Value;
            set
            {
                if (!_cacheComponentType.IsNullOrWhitespace())
                {
                    CachedComponent = value.GetComponent(_cacheComponentType);
                }

                base.Value = value;
            }
        }

        [SerializeField] private string _cacheComponentType;
        public dynamic CachedComponent { get; private set; }
        
    }
}

