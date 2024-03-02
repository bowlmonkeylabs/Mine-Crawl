using System;
using System.Linq;
using Mono.CSharp;
using UnityEngine;

namespace BML.Scripts.Utils
{
    public static class ComponentUtil
    {
        public static void MoveComponentBelow(Component component, Component targetComponent)
        {
            #if UNITY_EDITOR
            
            if (component.gameObject != targetComponent.gameObject)
            {
                throw new Exception("Components must be on the same GameObject.");
            }
            
            var components = component.gameObject.GetComponents<Component>().ToList();
            var componentIndex = components.IndexOf(component);
            var targetComponentIndex = components.IndexOf(targetComponent);
            var targetIndex = targetComponentIndex + 1;

            var move = targetIndex - componentIndex;
            var moveFunc = (move < 0
                ? (Func<Component, bool>)UnityEditorInternal.ComponentUtility.MoveComponentUp
                : (Func<Component, bool>)UnityEditorInternal.ComponentUtility.MoveComponentDown);
            for (int i=0; i<Mathf.Abs(move); i++)
            {
                moveFunc(component);
            }
            
            #endif
        }
        
    }
}