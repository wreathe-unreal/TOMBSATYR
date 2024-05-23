using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(FS_ShadowSimple))]
public class FS_ShadowSimpleEditor : Editor {

	private static GUIContent
		maxProjectionDistanceContent = new GUIContent("Max projection distance"),
		girthContent = new GUIContent("Shadow size"),
		shadowHoverHeightContent = new GUIContent("Shadow hover height"),
		isStaticContent = new GUIContent("Static"),
		shadowMaterialContent = new GUIContent("Shadow Material"),
		useLightSourceContent = new GUIContent("Use light source game object"),
		lightSourceContent = new GUIContent("Light Source"),
		perspectiveProjectionContent = new GUIContent("Use Perspective Projection"),
		lightDirectionContent = new GUIContent("Light Direction"),
		doVisiblitityCullingGUIContent = new GUIContent("Cull non-visible", "Cull shadows outside the fustrum of the 'main' camera. Culling incurs some performance overhead" +
															" so if most of your shadows are always visible you may be better off not using.");

	
	private SerializedObject simpleShadow;
	private SerializedProperty
		layerMask,
		maxProjectionDistance,
		girth,
		shadowHoverHeight,
		isStatic,
		shadowMaterial,
		uvs,
		useLightSource,
		lightSource,
		isPerspectiveProjection,
		lightDirection,
		doVisibilityCulling;

	void _Init() {
		simpleShadow = new SerializedObject(target);
		layerMask = simpleShadow.FindProperty("layerMask");
		maxProjectionDistance = simpleShadow.FindProperty("maxProjectionDistance");
		girth = simpleShadow.FindProperty("girth");
		shadowHoverHeight = simpleShadow.FindProperty("shadowHoverHeight");
		isStatic = simpleShadow.FindProperty("isStatic");
		shadowMaterial = simpleShadow.FindProperty("shadowMaterial");
		uvs = simpleShadow.FindProperty("uvs");
		useLightSource = simpleShadow.FindProperty("useLightSource");
		lightSource = simpleShadow.FindProperty("lightSource");
		isPerspectiveProjection = simpleShadow.FindProperty("isPerspectiveProjection");
		lightDirection = simpleShadow.FindProperty("lightDirection");
		doVisibilityCulling = simpleShadow.FindProperty("doVisibilityCulling");
	}

	public override void OnInspectorGUI() {	
		if (simpleShadow == null) _Init ();
		simpleShadow.Update();
		EditorGUILayout.PropertyField(layerMask);
		EditorGUILayout.PropertyField(maxProjectionDistance,maxProjectionDistanceContent);
		EditorGUILayout.PropertyField(girth,girthContent);
		EditorGUILayout.PropertyField(shadowHoverHeight,shadowHoverHeightContent);
		EditorGUILayout.PropertyField(isStatic,isStaticContent);
		EditorGUILayout.PropertyField(shadowMaterial,shadowMaterialContent);//
		EditorGUILayout.PropertyField(uvs);
		EditorGUILayout.HelpBox("Incoming light direction can be specified by\n" +
		                        "  - a vector (infinately distant light source)\n" +
		                        "  - a game object (usually a light)",
		                        MessageType.Info);
		EditorGUILayout.PropertyField(useLightSource,useLightSourceContent);
		if (useLightSource.boolValue){
			EditorGUILayout.PropertyField(lightSource,lightSourceContent);
			EditorGUILayout.HelpBox("With prospective projection shadows will get smaller as object approaches light source",
			                        MessageType.Info);
			EditorGUILayout.PropertyField(isPerspectiveProjection,perspectiveProjectionContent);
		} else {
			EditorGUILayout.PropertyField(lightDirection,lightDirectionContent);
		}
		EditorGUILayout.PropertyField(doVisibilityCulling,doVisiblitityCullingGUIContent);
		simpleShadow.ApplyModifiedProperties();
	}

}
