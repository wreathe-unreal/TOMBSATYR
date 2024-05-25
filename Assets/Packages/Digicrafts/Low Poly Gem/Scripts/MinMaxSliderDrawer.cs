#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;

namespace Digicrafts.Gem {

	[CustomPropertyDrawer (typeof (MinMaxSliderAttribute))]
	class MinMaxSliderDrawer : PropertyDrawer {

		public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {

			if (property.propertyType == SerializedPropertyType.Vector2) {
				Vector2 range = property.vector2Value;
				float min = (float)Math.Round(range.x,2);
				float max = (float)Math.Round(range.y,2);
				MinMaxSliderAttribute attr = attribute as MinMaxSliderAttribute;
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel(label);
				EditorGUILayout.LabelField(min.ToString(),GUILayout.Width(30));
				EditorGUI.BeginChangeCheck ();
				EditorGUILayout.MinMaxSlider(ref min, ref max, attr.min, attr.max);
				if (EditorGUI.EndChangeCheck ()) {
					range.x = min;
					range.y = max;
					property.vector2Value = range;
				}
				EditorGUILayout.LabelField(max.ToString(),GUILayout.Width(30));
				EditorGUILayout.EndHorizontal();
			} else {
				EditorGUI.LabelField (position, label, "Use only with Vector2");
			}
		}
	}

}
#endif