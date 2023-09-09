using UnityEditor;
using UnityEngine;
using XNodeEditor;

namespace BML.Scripts.ItemTreeGraph.Editor
{
    [NodeEditor.CustomNodeEditorAttribute(typeof(ItemTreeGraphNode))]
    public class ItemTreeGraphNodeEditor : NodeEditor
    {
        public override int GetWidth() => 500;

        public override void OnBodyGUI()
        {

            EditorGUIUtility.labelWidth = 350;
            base.OnBodyGUI();
            
            //End the current GUI Area that is restricted to node's dimensions
            GUILayout.EndArea();
            
            ItemTreeGraphNode nodeAsItemGraphNode = target as ItemTreeGraphNode;
            nodeAsItemGraphNode.name = (nodeAsItemGraphNode.Item != null) ? nodeAsItemGraphNode.Item.Name : "Missing";
            
            //Show Label Above node
            Vector2 nodeLabelPos = NodeEditorWindow.current.GridToWindowPositionNoClipped(target.position + 
                new Vector2(0f, -60f));
            
            NodeEditorPreferences.Settings prefs = NodeEditorPreferences.GetSettings();
            GUIStyle labelStyle = XNodeUtils.ZoomBasedStyle(35f, 85f, NodeEditorWindow.current.zoom,
                prefs.minZoom, prefs.maxZoom,   new Color(.85f, .85f, 1f), FontStyle.Bold, TextAnchor.LowerCenter, false);
            
            GUI.Label(new Rect(nodeLabelPos, new Vector2(GetWidth(), 50f)), nodeAsItemGraphNode.name,
                labelStyle);

            //Put back the GUI area that is restricted to node's dimensions
            Vector2 nodePos = NodeEditorWindow.current.GridToWindowPositionNoClipped(target.position);
            GUILayout.BeginArea(new Rect(nodePos, new Vector2(GetWidth(), 4000)));
        }

        /// <summary> Add items for the context menu when right-clicking this node. Override to add custom menu items. </summary>
        public override void AddContextMenuItems(GenericMenu menu) {
            bool canRemove = true;
            // Actions if only one node is selected
            if (Selection.objects.Length == 1 && Selection.activeObject is XNode.Node) {
                XNode.Node node = Selection.activeObject as XNode.Node;
                menu.AddItem(new GUIContent("Edit Script"), false, () =>
                {
                    string assetPath = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(target));
                    AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<MonoScript>(assetPath));
                });
                menu.AddItem(new GUIContent("Move To Top"), false, () => NodeEditorWindow.current.MoveNodeToTop(node));
                menu.AddItem(new GUIContent("Rename"), false, NodeEditorWindow.current.RenameSelectedNode);

                canRemove = NodeGraphEditor.GetEditor(node.graph, NodeEditorWindow.current).CanRemove(node);
            }

            // Add actions to any number of selected nodes
            menu.AddItem(new GUIContent("Copy"), false, NodeEditorWindow.current.CopySelectedNodes);
            menu.AddItem(new GUIContent("Duplicate"), false, NodeEditorWindow.current.DuplicateSelectedNodes);

            if (canRemove) menu.AddItem(new GUIContent("Remove"), false, NodeEditorWindow.current.RemoveSelectedNodes);
            else menu.AddItem(new GUIContent("Remove"), false, null);

            // Custom sections if only one node is selected
            if (Selection.objects.Length == 1 && Selection.activeObject is XNode.Node) {
                XNode.Node node = Selection.activeObject as XNode.Node;
                menu.AddCustomContextMenuItems(node);
            }
        }
        
        public override Color GetTint()
        {
            Color tint;
            ItemTreeGraphNode nodeAsItemGraphNode = target as ItemTreeGraphNode;

            if (nodeAsItemGraphNode.Obtained)
                tint = new Color(40f/255f, 128f/255f, 70f/255f, 1f);
            else
                tint = new Color(110f/255f, 60/255f, 60f/255f, 1f);

            return tint;
        }
    }
}