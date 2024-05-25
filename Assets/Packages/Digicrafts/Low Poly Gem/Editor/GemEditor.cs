using UnityEngine;
using UnityEditor;
using System.Collections;
using System;

namespace Digicrafts.Gem {

	[CustomEditor(typeof(Gem))]
	[CanEditMultipleObjects]
	public class GemEditor : Editor {
			
		public override void OnInspectorGUI()
		{	
			serializedObject.Update();	
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Basic",EditorStyles.boldLabel);
			EditorGUILayout.Separator();
			EditorGUILayout.PropertyField(serializedObject.FindProperty("color"));
			EditorGUILayout.Slider(serializedObject.FindProperty("opacity"),0,1);
			EditorGUILayout.Slider(serializedObject.FindProperty("reflection"),0,1);
			EditorGUILayout.Slider(serializedObject.FindProperty("refraction"),0,1);
			EditorGUILayout.Slider(serializedObject.FindProperty("lighting"),0,1);
			EditorGUILayout.HelpBox("Settings will applied on run.",MessageType.Info);
			EditorGUILayout.Separator();
			EditorGUILayout.LabelField("Animation",EditorStyles.boldLabel);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("glowMinMax"),GUILayout.Height(0));
			EditorGUILayout.HelpBox("Glow will applied on run.",MessageType.Info);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("glowAnimationTime"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("rotateAnimationTime"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("floatAnimationTime"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("floatAnimationHeight"));
			EditorGUILayout.HelpBox("0 will disable the animation.",MessageType.Info);

			serializedObject.ApplyModifiedProperties();
		}

	}
}
