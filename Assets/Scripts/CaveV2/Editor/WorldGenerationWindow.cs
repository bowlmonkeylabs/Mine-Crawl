using BML.Scripts.CaveV2;
using BML.Scripts.CaveV2.MudBun;
using BML.Scripts.CaveV2.SpawnObjects;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;

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
        
        [TitleGroup("Cave Graph"), Button, LabelText("Generate Cave Graph")]
        //[EnableIf("$IsGenerationEnabled")]
        private void GenerateCaveGraphButton()
        {
            _caveGenerator.GenerateCaveGraphButton();
        }
        
        [TitleGroup("Cave Graph"), Button]
        //[EnableIf("$IsGenerationEnabled")]
        private void DestroyCaveGraph()
        {
            _caveGenerator.DestroyCaveGraph();
        }
        
        [TitleGroup("Mudbun"), Button("Generate Mud Bun")]
        protected virtual void GenerateMudBunInternal()
        {
            _mudBunRenderer.GenerateMudBunInternalButton();
        }

        [TitleGroup("Mudbun"), Button]
        public void DestroyMudBun()
        {
            _mudBunRenderer.DestroyMudBun();
        }
        
        [TitleGroup("Mudbun"), Button]
        public void LockMesh()
        {
            _mudBunRenderer.LockMesh();
        }
        
        [TitleGroup("Mudbun"), Button]
        private void UnlockMesh()
        {
            _mudBunRenderer.UnlockMesh();
        }
        
        [TitleGroup("Mudbun"), Button]
        public void RelockMesh()
        {
            _mudBunRenderer.RelockMesh();
        }
        
        [TitleGroup("Level Object Spawner"), Button, LabelText("Spawn Level Objects")]
        public void SpawnLevelObjectsButton()
        {
            _levelObjectSpawner.SpawnLevelObjectsButton();
        }
        
        [TitleGroup("Level Object Spawner"), Button]
        public void DestroyLevelObjects()
        {
            _levelObjectSpawner.DestroyLevelObjects();
        }
    }
}