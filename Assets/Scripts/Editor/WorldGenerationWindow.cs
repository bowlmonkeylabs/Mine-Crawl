using BML.Scripts.CaveV2;
using BML.Scripts.CaveV2.MudBun;
using BML.Scripts.CaveV2.SpawnObjects;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace BML.Scripts.Editor
{
    public class WorldGenerationWindow : OdinEditorWindow
    {
        [MenuItem("Window/World Generation")]
        private static void OpenWindow()
        {
            GetWindow<WorldGenerationWindow>().Show();
        }
        
        [TitleGroup("Cave Graph"), Button, LabelText("Generate Cave Graph")]
        //[EnableIf("$IsGenerationEnabled")]
        private void GenerateCaveGraphButton()
        {
            FindObjectOfType<CaveGenComponentV2>().GenerateCaveGraphButton();
        }
        
        [TitleGroup("Cave Graph"), Button]
        //[EnableIf("$IsGenerationEnabled")]
        private void DestroyCaveGraph()
        {
            FindObjectOfType<CaveGenComponentV2>().DestroyCaveGraph();
        }
        
        [TitleGroup("Mudbun"), Button("Generate Mud Bun")]
        protected virtual void GenerateMudBunInternal()
        {
            FindObjectOfType<MudBunGenerator>().GenerateMudBunInternalButton();
        }

        [TitleGroup("Mudbun"), Button]
        public void DestroyMudBun()
        {
            FindObjectOfType<MudBunGenerator>().DestroyMudBun();
        }
        
        [TitleGroup("Mudbun"), Button]
        public void LockMesh()
        {
            FindObjectOfType<MudBunGenerator>().LockMesh();
        }
        
        [TitleGroup("Mudbun"), Button]
        private void UnlockMesh()
        {
            FindObjectOfType<MudBunGenerator>().UnlockMesh();
        }
        
        [TitleGroup("Mudbun"), Button]
        public void RelockMesh()
        {
            FindObjectOfType<MudBunGenerator>().RelockMesh();
        }
        
        [TitleGroup("Level Object Spawner"), Button, LabelText("Spawn Level Objects")]
        public void SpawnLevelObjectsButton()
        {
            FindObjectOfType<LevelObjectSpawner>().SpawnLevelObjectsButton();
        }
        
        [TitleGroup("Level Object Spawner"), Button]
        public void DestroyLevelObjects()
        {
            FindObjectOfType<LevelObjectSpawner>().DestroyLevelObjects();
        }
    }
}