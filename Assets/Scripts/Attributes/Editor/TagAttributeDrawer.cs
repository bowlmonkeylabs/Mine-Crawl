using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace BML.Scripts.Attributes.Editor {
    public class TagAttributeDrawer : OdinAttributeDrawer<TagAttribute, string>
    {
        private GUIContent m_buttonContent = new GUIContent();
	
        protected override void Initialize() => UpdateButtonContent();

        private void UpdateButtonContent()
        {
            m_buttonContent.text = ValueEntry.SmartValue;
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            var rect = EditorGUILayout.GetControlRect(label != null);

            if (label == null)
                rect = EditorGUI.IndentedRect(rect);
            else
                rect = EditorGUI.PrefixLabel(rect, label);
            
            if (!EditorGUI.DropdownButton(rect, m_buttonContent, FocusType.Passive)) return;

            var selector = new GenericSelector<string>(UnityEditorInternal.InternalEditorUtility.tags);
            selector.SetSelection(ValueEntry.SmartValue);
            selector.ShowInPopup(rect.position);
                
            selector.SelectionChanged += x =>
            {
                ValueEntry.Property.Tree.DelayAction(() =>
                {
                    ValueEntry.SmartValue = x.FirstOrDefault();
                    
                    UpdateButtonContent();
                });
            };
        }
    }

    public abstract class TagStringListBaseDrawer<T> : OdinAttributeDrawer<TagAttribute, T> where T : IList<string>
    {
        private GUIContent m_buttonContent = new GUIContent();

        protected override void Initialize() => UpdateButtonContent();

        private void UpdateButtonContent()
        {
            m_buttonContent.text = m_buttonContent.tooltip = string.Join(", ", ValueEntry.SmartValue);
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            var rect = EditorGUILayout.GetControlRect(label != null);

            if (label == null)
                rect = EditorGUI.IndentedRect(rect);
            else
                rect = EditorGUI.PrefixLabel(rect, label);
            
            if (!EditorGUI.DropdownButton(rect, m_buttonContent, FocusType.Passive)) return;

            var tags = UnityEditorInternal.InternalEditorUtility.tags;
            if(this.Attribute.Filter != null) {
                tags = tags.Where(tag => tag.StartsWith(this.Attribute.Filter)).ToArray();
            }
            var selector = new TagSelector(tags);

            rect.y += rect.height;
            
            selector.SetSelection(ValueEntry.SmartValue);
            selector.ShowInPopup(rect.position);

            selector.SelectionChanged += x =>
            {
                ValueEntry.Property.Tree.DelayAction(() =>
                {
                    UpdateValue(x);
                    UpdateButtonContent();
                });
            };
        }

        protected abstract void UpdateValue(IEnumerable<string> x);
    }

    [DrawerPriority(1)]
    [DontApplyToListElements]
    public class TagStringArrayDrawer : TagStringListBaseDrawer<string[]>
    {
        protected override void UpdateValue(IEnumerable<string> x) => ValueEntry.SmartValue = x.ToArray();
    }

    [DrawerPriority(1)]
    [DontApplyToListElements]
    public class TagStringListDrawer : TagStringListBaseDrawer<List<string>>
    {
        protected override void UpdateValue(IEnumerable<string> x) => ValueEntry.SmartValue = x.ToList();
    }

    public class TagSelector : GenericSelector<string>
    {
        private FieldInfo m_requestCheckboxUpdate;

        public TagSelector(string[] tags) : base(tags)
        {
            CheckboxToggle = true;

            m_requestCheckboxUpdate = typeof(GenericSelector<string>).GetField("requestCheckboxUpdate",
                BindingFlags.NonPublic | BindingFlags.Instance);
        }

        protected override void DrawSelectionTree()
        {
            base.DrawSelectionTree();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("None"))
            {
                SetSelection(new List<string>());
                
                m_requestCheckboxUpdate.SetValue(this, true);
                TriggerSelectionChanged();
            }
            
            if (GUILayout.Button("All"))
            {
                SetSelection(UnityEditorInternal.InternalEditorUtility.tags);
                
                m_requestCheckboxUpdate.SetValue(this, true);
                TriggerSelectionChanged();
            }
            
            EditorGUILayout.EndHorizontal();
        }
    }
}
