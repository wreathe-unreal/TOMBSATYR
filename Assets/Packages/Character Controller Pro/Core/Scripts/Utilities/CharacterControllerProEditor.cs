#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Presets;
using Lightbug.Utilities;

namespace Lightbug.CharacterControllerPro.Core
{

    class CCPAssetPostprocessor : AssetPostprocessor
    {
        public const string RootFolder = "Assets/Character Controller Pro";
        
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (string importedAsset in importedAssets)
            {
                if (importedAsset.Equals(RootFolder))
                {
                    WelcomeWindow window = EditorWindow.GetWindow<WelcomeWindow>(true, "Welcome");
                }
            }
        }
    }

    public class CharacterControllerProEditor : Editor
    {
        [MenuItem("Window/Character Controller Pro/Welcome", false, 0)]
        public static void WelcomeMessage()
        {
            WelcomeWindow window = EditorWindow.GetWindow<WelcomeWindow>(true, "Welcome");
        }

        [MenuItem("Window/Character Controller Pro/Documentation", false, 0)]
        public static void Documentation()
        {
            Application.OpenURL("https://lightbug14.gitbook.io/ccp/");
        }

        [MenuItem("Window/Character Controller Pro/API Reference", false, 0)]
        public static void APIReference()
        {
            Application.OpenURL("https://lightbug14.github.io/lightbug-web/character-controller-pro/Documentation/html/index.html");
        }

        [MenuItem("Window/Character Controller Pro/About", false, 100)]
        public static void About()
        {
            EditorWindow.GetWindow<AboutWindow>(true, "About");
        }

    }

    public abstract class CharacterControllerProWindow : EditorWindow
    {
        protected GUIStyle titleStyle = new GUIStyle();
        protected GUIStyle subtitleStyle = new GUIStyle();
        protected GUIStyle descriptionStyle = new GUIStyle();


        protected virtual void OnEnable()
        {
            titleStyle.fontSize = 50;
            titleStyle.alignment = TextAnchor.MiddleCenter;
            titleStyle.padding.top = 4;
            titleStyle.padding.bottom = 4;
            titleStyle.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;

            subtitleStyle.fontSize = 18;
            subtitleStyle.alignment = TextAnchor.MiddleCenter;
            subtitleStyle.padding.top = 4;
            subtitleStyle.padding.bottom = 4;
            subtitleStyle.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;

            descriptionStyle.fontSize = 15;
            descriptionStyle.wordWrap = true;
            descriptionStyle.padding.left = 10;
            descriptionStyle.padding.right = 10;
            descriptionStyle.padding.top = 4;
            descriptionStyle.padding.bottom = 4;
            descriptionStyle.richText = true;
            descriptionStyle.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;

        }
    }

    public class AboutWindow : CharacterControllerProWindow
    {
        const float Width = 200f;
        const float Height = 100f;

        protected override void OnEnable()
        {
            this.position = new Rect((Screen.width - Width) / 2f, (Screen.height - Height) / 2f, Width, Height);
            this.maxSize = this.minSize = this.position.size;
            this.titleContent = new GUIContent("About");
        }

        void OnGUI()
        {
            EditorGUILayout.SelectableLabel("Version: 1.4.8", GUILayout.Height(15f));
            EditorGUILayout.SelectableLabel("Author : Juan Sálice (Lightbug)", GUILayout.Height(15f));
            EditorGUILayout.SelectableLabel("Email : lightbug14@gmail.com", GUILayout.Height(15f));
        }
    }


    public class WelcomeWindow : CharacterControllerProWindow
    {

        protected override void OnEnable()
        {
            base.OnEnable();

            this.position = new Rect(10f, 10f, 1000f, 850f);
            this.maxSize = this.minSize = this.position.size;

        }

        [MenuItem("Window/Character Controller Pro/Upgrade guide 1.4.x", false, 0)]
        public static void OpenUpgradeGuide()
        {
            string[] results = AssetDatabase.FindAssets("Upgrade guide 1.4.x");
            if (results.Length == 0)
                return;
            
            OpenProjectFile(results[0]);            
        }

        static void OpenProjectFile(string guid)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var projectPathWithoutAssets = Application.dataPath.Substring(0, Application.dataPath.Length - 6);
            Application.OpenURL(projectPathWithoutAssets + path);            
        }

        void OnGUI()
        {

            GUILayout.Label("Character Controller Pro", titleStyle);

            GUILayout.Space(20f);

            GUILayout.BeginVertical("Box");


            GUILayout.Label("<b>Upgrade to 1.4.X</b>", subtitleStyle);

            GUILayout.Label("Are you upgrading to 1.4.x?\nGo and take a look at the <b>upgrade guide</b> (<i>Character Controller Pro/Documentation/Upgrade guide 1.4.0.pdf</i>)", descriptionStyle);
            if (GUILayout.Button("Upgrade guide"))
                OpenUpgradeGuide();
            GUILayout.EndVertical();

            GUILayout.Space(10f);

            GUILayout.BeginVertical("Box");
            GUILayout.Label("<b>Demo setup</b>", subtitleStyle);

            GUILayout.Label(
            "In order to play the demo scenes included with the asset, you must modify some settings from your project. " +
            "These settings involve inputs, layers and tags. \n\n" +
            "<b>This step is required only for demo purposes, the asset by itself (core + implementation) does not require any previous setup in order to work properly.</b>", descriptionStyle);
            


            GUILayout.Space(10f);

            //CustomUtilities.DrawEditorLayoutHorizontalLine(Color.black);

            //GUILayout.Label(
            //"<b>Inputs</b>: By default the asset uses Unity's input manager, this is why some specific axis must be defined. " + 
            //"If this is ignored, a message will appear on console.", descriptionStyle);
            //GUILayout.Label(
            //"<b>Tags & Layers</b>: Some specifics tags and layers were defined for the demo.", descriptionStyle);


            GUILayout.Label(
            "1. Open the <b>Input manager settings</b>.\n" +
            "2. Load <i><color=yellow>Preset_Inputs.preset</color></i>.\n" +
            "3. Open the <b>Tags and Layers settings</b>.\n" +
            "4. Load <i><color=yellow>Preset_TagsAndLayers.preset</color></i>.\n", descriptionStyle);


            GUILayout.Space(10f);

            GUILayout.Label("For more information about the setup, please visit the section \"Setting up the project\".", descriptionStyle);
            if (GUILayout.Button("Setting up the project", EditorStyles.miniButton))
            {
                Application.OpenURL("https://lightbug14.gitbook.io/ccp/package/setting-up-the-project");
            }
            GUILayout.EndVertical();

            GUILayout.Space(10f);

            GUILayout.BeginVertical("Box");
            GUILayout.Label("<b>Known issues</b>", subtitleStyle);
            GUILayout.Label("Also, please check the \"Known issues\" section if you are experiencing " +
            "some problems while testing the demo content. These issues might be related to the Unity editor itself (most of the time).", descriptionStyle);
            if (GUILayout.Button("Known issues", EditorStyles.miniButton))
            {
                Application.OpenURL("https://lightbug14.gitbook.io/ccp/package/setting-up-the-project#known-issues-editor");
            }

            GUILayout.EndVertical();

            GUILayout.Space(10f);

            GUILayout.Label("You can open this window by using the top menu: \n<i>Window/Character Controller Pro/Welcome</i>", descriptionStyle);

        }

    }

}

#endif
