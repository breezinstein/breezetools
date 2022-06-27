using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace Breezinstein.Tools.UI
{
    [CustomEditor(typeof(RadialLayoutGroup), true)]
    public class RadialLayoutGroupInspector : Editor
    {

        public override void OnInspectorGUI()
        {
            RadialLayoutGroup group = target as RadialLayoutGroup;
            serializedObject.Update();

            group.Arc = EditorGUILayout.Slider("Arc:", group.Arc, 0, 360);
            group.Offset = EditorGUILayout.Slider("Offset:", group.Offset, 0, 360);
            group.Radius = EditorGUILayout.FloatField("Radius: ", group.Radius);
            group.StartFrom = (RadialLayoutGroup.RadialLayoutStart)EditorGUILayout.EnumPopup("Start From: ", group.StartFrom);


            serializedObject.ApplyModifiedProperties();
            group.SetLayoutHorizontal();
        }


    }
}