using BML.Scripts.CaveV2;
using BML.Scripts.CaveV2.MudBun;
using BML.Scripts.CaveV2.SpawnObjects;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace BML.Scripts.CaveV2.Editor
{
    public class WorldGenerationWindow : OdinEditorWindow
    {
        private CaveGenComponentV2 _caveGenerator => FindObjectOfType<CaveGenComponentV2>();
        private CaveGraphMudBunRenderer _mudBunRenderer => FindObjectOfType<CaveGraphMudBunRenderer>();
        private LevelObjectSpawner _levelObjectSpawner => FindObjectOfType<LevelObjectSpawner>();
        
        [MenuItem("Window/BML/World Generation")]
        private static void OpenWindow()
        {
            GetWindow<WorldGenerationWindow>().Show();
        }

        [TitleGroup("Cave Graph"), Button("Select")]
        private void SelectCaveGeneratorButton()
        {
            var go = _caveGenerator?.gameObject;
            if (go == null) Debug.LogError($"No cave generator component found.");
            Selection.activeGameObject = go;
        }
        
        [TitleGroup("Cave Graph"), Button("Generate")]
        [ButtonGroup("Cave Graph/Buttons")]
        //[EnableIf("$IsGenerationEnabled")]
        private void GenerateCaveGraphButton()
        {
            _caveGenerator.GenerateCaveGraphButton();
        }
        
        [TitleGroup("Cave Graph"), Button("Destroy")]
        [ButtonGroup("Cave Graph/Buttons")]
        //[EnableIf("$IsGenerationEnabled")]
        private void DestroyCaveGraph()
        {
            _caveGenerator.DestroyCaveGraph();
        }
        
        [TitleGroup("MudBun"), Button("Select")]
        private void SelectMudBunButton()
        {
            var go = _mudBunRenderer?.gameObject;
            if (go == null) Debug.LogError($"No CaveGraphMudBunRenderer component found.");
            Selection.activeGameObject = go;
        }
        
        [TitleGroup("MudBun"), Button("Generate")]
        [ButtonGroup("MudBun/Buttons 1")]
        protected virtual void GenerateMudBunInternal()
        {
            _mudBunRenderer.GenerateMudBunInternalButton();
        }

        [TitleGroup("MudBun"), Button("Destroy")]
        [ButtonGroup("MudBun/Buttons 1")]
        public void DestroyMudBun()
        {
            _mudBunRenderer.DestroyMudBun();
        }
        
        [TitleGroup("MudBun"), Button("Lock")]
        [ButtonGroup("MudBun/Buttons 2")]
        public void LockMesh()
        {
            _mudBunRenderer.LockMesh();
        }
        
        [TitleGroup("MudBun"), Button("Unlock")]
        [ButtonGroup("MudBun/Buttons 2")]
        private void UnlockMesh()
        {
            _mudBunRenderer.UnlockMesh();
        }
        
        [TitleGroup("MudBun"), Button("Re-lock")]
        [ButtonGroup("MudBun/Buttons 2")]
        public void RelockMesh()
        {
            _mudBunRenderer.RelockMesh();
        }
        
        [TitleGroup("Level Object Spawner"), Button("Select")]
        private void SelectLevelObjectSpawnerButton()
        {
            var go = _levelObjectSpawner?.gameObject;
            if (go == null) Debug.LogError($"No level object spawner component found.");
            Selection.activeGameObject = go;
        }
        
        [TitleGroup("Level Object Spawner"), Button("Generate")]
        [ButtonGroup("Level Object Spawner/Buttons")]
        public void SpawnLevelObjectsButton()
        {
            _levelObjectSpawner.SpawnLevelObjectsButton();
        }
        
        [TitleGroup("Level Object Spawner"), Button("Destroy")]
        [ButtonGroup("Level Object Spawner/Buttons")]
        public void DestroyLevelObjects()
        {
            _levelObjectSpawner.DestroyLevelObjects();
        }
    }
}