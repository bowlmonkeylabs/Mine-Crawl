using System.Collections.Generic;
using BML.Scripts.CaveV2.SpawnObjects;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BML.Scripts.CaveV2
{
    [InlineEditor()]
    [CreateAssetMenu(fileName = "DecorSpawningParam", menuName = "BML/Cave Gen/DecorSpawningParam", order = 0)]
    public class DecorSpawningParameters : ScriptableObject
    {
        [TitleGroup("Object Settings")] public LayerMask _roomBoundsLayerMask;
        [TitleGroup("Object Settings"),] public GameObject _prefabToSpawn;
        [TitleGroup("Object Settings")] public bool _spawnAsPrefab = true;

        [TitleGroup("Filtering")] public float _pointDensity = .25f;
        [TitleGroup("Filtering"), MinMaxSlider(0f, 180f)] public Vector2 _minMaxAngle = new Vector2(0f, 180f);
        [TitleGroup("Filtering"), Range(0, 1)] public float _noiseScale = 100f;
        [TitleGroup("Filtering"), Range(0, 1)] public float _noiseFilterValueMin = .5f;
        [TitleGroup("Filtering")] public bool _spawnInStartRoom;
        [TitleGroup("Filtering")] public bool _spawnInEndRoom;
        [TitleGroup("Filtering")] public int _maxDistanceFromMainPath = 10;
        [TitleGroup("Filtering")] public AnimationCurve _mainPathDecayCurve;
        
        [FoldoutGroup("Debug")] public float _pointRadiusDebug = .25f;
        [FoldoutGroup("Debug")] public float _pointNormalLengthDebug = 1f;
        [FoldoutGroup("Debug")] public bool _drawClosestPoints;
        [FoldoutGroup("Debug"), ShowInInspector, ReadOnly] public int pointCount;
        [FoldoutGroup("Debug"), ReadOnly] public List<DecorObjectSpawner.Point> _points = new List<DecorObjectSpawner.Point>();
        [FoldoutGroup("Debug")] public Vector3 _noiseBoxCount = new Vector3(100f, 100f, 100f);
        [FoldoutGroup("Debug"), ShowInInspector, ReadOnly] public List<DecorObjectSpawner.NoiseBox> _noiseBoxes = new List<DecorObjectSpawner.NoiseBox>();
    }
}