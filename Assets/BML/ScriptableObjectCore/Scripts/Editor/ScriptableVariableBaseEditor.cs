using System.Linq;
using System.Runtime.CompilerServices;
using BML.ScriptableObjectCore.Scripts;
using BML.ScriptableObjectCore.Scripts.Variables;
using UnityEngine;

namespace BML.ScriptableObjectCoreEditor
{
    // Decompiled with JetBrains decompiler
    // Type: Raskulls.ScriptableSystemEditor.ScriptableVariableBaseEditor
    // Assembly: ScriptableSystemEditor, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
    // MVID: 993696AB-263C-4C4A-A2A0-37BF89B79867
    // Assembly location: C:\Users\Gabriel\Documents\UnityProjects\BMLAssetRefactor\Assets\Raskulls\ScriptableSystem\Editor\ScriptableSystemEditor.dll

    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using UnityEditor;
    using UnityEngine;
    using Object = UnityEngine.Object;

    [CustomEditor(typeof(ScriptableVariableBase), true)]
    public class ScriptableVariableBaseEditor : Editor
    {
        private List<MonoBehaviour> monoBehaviours = new List<MonoBehaviour>();
        private List<MonoBehaviour> monoBehavioursPrefabs = new List<MonoBehaviour>();
        private List<bool> monoBehavioursNotUseds = new List<bool>();
        private List<ScriptableObject> scriptableObjects = new List<ScriptableObject>();
        private List<bool> scriptableObjectsNotUseds = new List<bool>();
        private Color missingBackgroundColor = new Color(0.9294118f, 0.3921569f, 0.3176471f);
        private Color WarningBackgroundColor = new Color(0.8980392f, 0.8602679f, 0.3137255f);
        private bool monoBehavioursFoldout = true;
        private bool scriptableObjectsFoldout = true;
        private bool isInit;

        private void Init()
        {
            MonoBehaviour[] allMonoBehaviours = Resources.FindObjectsOfTypeAll<MonoBehaviour>();
            BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            monoBehavioursPrefabs.Clear();
            monoBehaviours.Clear();
            for(int index = 0; index < allMonoBehaviours.Length; ++index)
            {
                if((allMonoBehaviours[index]).hideFlags == HideFlags.None)
                {
                    if(PrefabUtility.IsPartOfPrefabAsset(allMonoBehaviours[index]))
                        monoBehavioursPrefabs.Add(allMonoBehaviours[index]);
                    for(Type type = (allMonoBehaviours[index]).GetType(); type != null && type != typeof(MonoBehaviour); type = type.BaseType)
                    {
                        foreach(FieldInfo member in type.FindMembers(MemberTypes.Field, bindingFlags, null, null))
                        {
                            object memberValue = member.GetValue(allMonoBehaviours[index]);
                            if(member.FieldType == target.GetType() && memberValue != null && memberValue.Equals(target))
                            {
                                if(!monoBehaviours.Contains(allMonoBehaviours[index]))
                                {
                                    monoBehaviours.Add(allMonoBehaviours[index]);
                                    break;
                                }
                                break;
                            }
                            if(member.FieldType != target.GetType() && memberValue != null)
                            {
                                foreach(FieldInfo childMember in member.FieldType.FindMembers(MemberTypes.Field, bindingFlags, null, null))
                                {
                                    object childMemberValue = childMember.GetValue(memberValue);
                                    if(childMember.FieldType == target.GetType() && childMemberValue != null && childMemberValue.Equals(target))
                                    {
                                        if(!monoBehaviours.Contains(allMonoBehaviours[index]))
                                        {
                                            monoBehaviours.Add(allMonoBehaviours[index]);
                                            break;
                                        }
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            for(int i = 0; i < this.monoBehavioursPrefabs.Count; i++)
            {
                if(this.monoBehaviours.FindAll((Predicate<MonoBehaviour>)(x => ((Object)x).name == ((Object)this.monoBehavioursPrefabs[i]).name)).Count > 1)
                    this.monoBehaviours.Remove(this.monoBehavioursPrefabs[i]);
            }
            this.monoBehavioursPrefabs.Clear();
            this.monoBehavioursNotUseds.Clear();
            GameObject gameObject = new GameObject();
            gameObject.SetActive(false);
            for(int index = 0; index < this.monoBehaviours.Count; ++index)
            {
                Type type = ((object)this.monoBehaviours[index]).GetType();
                string scriptableObjectAssetName = ScriptUtility.GetScriptableObjectAssetName((ScriptableObject)this.target);
                string str1 = char.ToLower(scriptableObjectAssetName[0]).ToString() + scriptableObjectAssetName.Substring(1);
                bool flag = false;
                for(; type != (Type)null && type != typeof(MonoBehaviour); type = type.BaseType)
                {
                    string str2 = ScriptUtility.RemoveCommentsFromScript(((TextAsset)MonoScript.FromMonoBehaviour(gameObject.AddComponent(type) as MonoBehaviour)).text);
                    if(!str2.Contains(scriptableObjectAssetName + ".") && !str2.Contains(str1 + "."))
                    {
                        flag = true;
                    }
                    else
                    {
                        flag = false;
                        break;
                    }
                }
                this.monoBehavioursNotUseds.Add(flag);
            }
            Object.DestroyImmediate((Object)gameObject);
            ScriptableObject[] objectsOfTypeAll2 = Resources.FindObjectsOfTypeAll<ScriptableObject>();
            this.scriptableObjects.Clear();
            for(int index = 0; index < objectsOfTypeAll2.Length; ++index)
            {
                if((objectsOfTypeAll2[index]).hideFlags == HideFlags.None)
                {
                    for(Type type = ((object)objectsOfTypeAll2[index]).GetType(); type != (Type)null && type != typeof(ScriptableObject); type = type.BaseType)
                    {
                        foreach(FieldInfo member in type.FindMembers(MemberTypes.Field, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, (MemberFilter)null, (object)null))
                        {
                            object obj = member.GetValue((object)objectsOfTypeAll2[index]);
                            if(obj != null && member.FieldType == ((object)this.target).GetType() && obj.ToString() == ((object)this.target).ToString())
                            {
                                if(!this.scriptableObjects.Contains(objectsOfTypeAll2[index]))
                                {
                                    this.scriptableObjects.Add(objectsOfTypeAll2[index]);
                                    break;
                                }
                                break;
                            }
                        }
                    }
                }
            }
            this.scriptableObjectsNotUseds.Clear();
            for(int index = 0; index < this.scriptableObjects.Count; ++index)
            {
                Type type = ((object)this.scriptableObjects[index]).GetType();
                string scriptableObjectAssetName = ScriptUtility.GetScriptableObjectAssetName((ScriptableObject)this.target);
                string str3 = char.ToLower(scriptableObjectAssetName[0]).ToString() + scriptableObjectAssetName.Substring(1);
                bool flag = false;
                //type != typeof (GameEventBase)
                while(type != (Type)null && type != typeof(ScriptableVariableBase) && type != typeof(ScriptableObject))
                {
                    ScriptableObject instance = ScriptableObject.CreateInstance(type);
                    MonoScript monoScript = MonoScript.FromScriptableObject(instance);
                    if(monoScript != null)
                    {
                        string str4 = ScriptUtility.RemoveCommentsFromScript(((TextAsset)monoScript).text);
                        if(!str4.Contains(scriptableObjectAssetName + ".") && !str4.Contains(str3 + "."))
                        {
                            flag = true;
                        }
                        else
                        {
                            flag = false;
                            Object.DestroyImmediate((Object)instance);
                            break;
                        }
                    }
                    type = type.BaseType;
                    Object.DestroyImmediate((Object)instance);
                }
                this.scriptableObjectsNotUseds.Add(flag);
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Broadcast Update Test"))
            {
                var test = this.target as dynamic;
                test?.BroadcastUpdate();
            }

            if (GUILayout.Button("Find References"))
            {
                if (!this.isInit)
                {
                    this.isInit = true;
                    this.Init();
                }
            }

            //if (this.monoBehaviours.Count > 0)
            //{
            EditorGUILayout.Space(10f);
            this.monoBehavioursFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(this.monoBehavioursFoldout, "All Scripts That Have Reference", (GUIStyle)null, (Action<Rect>)null, (GUIStyle)null);
            if(this.monoBehavioursFoldout)
                this.DrawListOfObjects<MonoBehaviour>(this.monoBehaviours, this.missingBackgroundColor, this.monoBehavioursNotUseds);
            EditorGUILayout.EndFoldoutHeaderGroup();
            //}
            if(this.scriptableObjects.Count <= 0)
                return;
            EditorGUILayout.Space(10f);
            this.scriptableObjectsFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(this.scriptableObjectsFoldout, "All Scriptable Objects That Have Reference", (GUIStyle)null, (Action<Rect>)null, (GUIStyle)null);
            if(this.scriptableObjectsFoldout)
                this.DrawListOfObjects<ScriptableObject>(this.scriptableObjects, this.WarningBackgroundColor, this.scriptableObjectsNotUseds);
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawListOfObjects<T>(List<T> objects, Color missingColor, List<bool> notUseds = null)
        {
            EditorGUILayout.BeginVertical(GUI.skin.box, Array.Empty<GUILayoutOption>());
            GUIStyle guiStyle = new GUIStyle(GUI.skin.window);
            guiStyle.padding.top = 0;
            for(int index = 0; index < objects.Count; ++index)
            {
                EditorGUILayout.BeginVertical(guiStyle, Array.Empty<GUILayoutOption>());
                Object @object = (object)objects[index] as Object;
                EditorGUILayout.ObjectField((PrefabUtility.IsPartOfPrefabAsset(@object) ? "(Prefab Asset) " : "") + ((object)@object).GetType().Name, @object, typeof(T), false, (GUILayoutOption[])null);
                if(notUseds != null && notUseds.Count > index && notUseds[index])
                    EditorGUILayout.LabelField("This script takes reference but does not use It", new GUIStyle(EditorStyles.label)
                    {
                        normal = {
              textColor = missingColor
            }
                    }, Array.Empty<GUILayoutOption>());
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(5f);
            }
            EditorGUILayout.EndVertical();
        }
    }
}

