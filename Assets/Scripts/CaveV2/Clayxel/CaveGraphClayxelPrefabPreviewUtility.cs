using System.Collections.Generic;
using BML.Scripts.Utils;
using Clayxels;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace CaveV2.Clayxel
{
    [ExecuteAlways]
    [RequireComponent(typeof(Grid))]
    public class CaveGraphClayxelPrefabPreviewUtility : ClayxelGenerator
    {
        [SerializeField] private Grid _displayGrid;
        [SerializeField] private int _gridDisplayWidth = 4;
        [InlineEditor, SerializeField] private CaveGraphClayxelRendererParameters _renderParams;

        protected override void GenerateClayxels(ClayContainer clayContainer, bool instanceAsPrefabs)
        {
            base.GenerateClayxels(clayContainer, instanceAsPrefabs);

            Vector3Int cellOriginOffset = new Vector3Int(_gridDisplayWidth / 2, 0, 0);
            var allRoomPrefabs = _renderParams.GetAllRooms();
            allRoomPrefabs.ForEach((roomPrefab, index) =>
            {
                Vector3Int cellPosition = new Vector3Int(index % _gridDisplayWidth, 0, index / _gridDisplayWidth) - cellOriginOffset;
                var worldPosition = _displayGrid.CellToWorld(cellPosition);

                var newGameObject = GameObjectUtils.SafeInstantiate(instanceAsPrefabs, roomPrefab, clayContainer.transform);
                newGameObject.transform.position = worldPosition;
            });
            
        }

    }
}