using System.IO;
using System.Linq;
using PolyTerrains.Sources;
using PolyTerrains.Sources.BlockMaterial;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace PolyTerrains.Editor
{
    [CustomEditor(typeof(BlockMaterialMaker))]
    public class BlockMaterialMakerEditor : UnityEditor.Editor
    {
        private BlockMaterialMaker maker;

        private static readonly int MainTex = Shader.PropertyToID("_MainTex");
        private static readonly int BaseColorMap = Shader.PropertyToID("_BaseColorMap");
        private static readonly int BaseMap = Shader.PropertyToID("_BaseMap");

        public void OnEnable()
        {
            maker = (BlockMaterialMaker)target;
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("This tools allows to create Materials and BlockUVs assets to use with PolyTerrain (blocky style).\n\n" +
                                    "Each layer corresponds to a terrain layer. For example, block (block) defined for Layer 1 will be used at places where the first terrain layer is used, " +
                                    "block defined for Layer 2 will be used at places where the second terrain layer is used, etc.", MessageType.Info);

            if (GUILayout.Button("Add new layer") || maker.Blocks.Count == 0) {
                var newBlock = ScriptableObject.CreateInstance<Block>();
                AssetDatabase.CreateAsset(newBlock, AssetDatabase.GenerateUniqueAssetPath($"Assets/block-{maker.Blocks.Count}.asset"));
                maker.Blocks.Add(newBlock);
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            }

            EditorGUILayout.Space();

            var options = maker.Blocks.Select((_, i) => $"Layer {i}").ToArray();
            maker.selectedTerrainLayerIndex = Mathf.Min(maker.Blocks.Count - 1, EditorGUILayout.Popup("Layer", maker.selectedTerrainLayerIndex, options));

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            var block = maker.Blocks[maker.selectedTerrainLayerIndex];
            var fieldBlock = (Block)EditorGUILayout.ObjectField("Layer asset", block, typeof(Block), false);
            if (fieldBlock != block) {
                block = fieldBlock;
                maker.Blocks[maker.selectedTerrainLayerIndex] = block;
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            }

            var tex = (Texture2D)EditorGUILayout.ObjectField("Top texture", block.top, typeof(Texture2D), false);
            if (tex != block.top) {
                SetTextureImporterFormat(tex, true);
                block.top = tex;
                AssetDatabase.ForceReserializeAssets(new[] { AssetDatabase.GetAssetPath(block) });
                AssetDatabase.SaveAssets();
            }

            tex = (Texture2D)EditorGUILayout.ObjectField("Sides texture", block.side, typeof(Texture2D), false);
            if (tex != block.side) {
                SetTextureImporterFormat(tex, true);
                block.side = tex;
                AssetDatabase.ForceReserializeAssets(new[] { AssetDatabase.GetAssetPath(block) });
                AssetDatabase.SaveAssets();
            }

            tex = (Texture2D)EditorGUILayout.ObjectField("Bottom texture", block.bottom, typeof(Texture2D), false);
            if (tex != block.bottom) {
                SetTextureImporterFormat(tex, true);
                block.bottom = tex;
                AssetDatabase.ForceReserializeAssets(new[] { AssetDatabase.GetAssetPath(block) });
                AssetDatabase.SaveAssets();
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            if (GUILayout.Button("Generate material")) {
                if (!Directory.Exists("Assets/BlockMaterials"))
                    AssetDatabase.CreateFolder("Assets", "BlockMaterials");

                var atlas = maker.GenerateAtlas(out var blockUVs);
                atlas = atlas.Uncompress();
                var blockUVsAsset = ScriptableObject.CreateInstance<BlockUVsAsset>();
                blockUVsAsset.blockUVs = blockUVs.ToArray();
                var atlasPath = AssetDatabase.GenerateUniqueAssetPath($"Assets/BlockMaterials/atlas.png");
                File.WriteAllBytes(atlasPath, atlas.EncodeToPNG());
                AssetDatabase.ImportAsset(atlasPath);
                var atlasAsset = AssetDatabase.LoadAssetAtPath<Texture2D>(atlasPath);
                SetTextureImporterFilterMode(atlasAsset, FilterMode.Point);
                blockUVsAsset.atlas = atlasAsset;

                var shader = Shader.Find("Standard");
                if (GraphicsSettings.currentRenderPipeline) {
                    shader = GraphicsSettings.currentRenderPipeline.defaultMaterial.shader;
                }

                var mat = new Material(shader);
                mat.SetTexture(MainTex, atlasAsset);
                mat.SetTexture(BaseColorMap, atlasAsset); // For HDRP
                mat.SetTexture(BaseMap, atlasAsset); // For URP
                AssetDatabase.CreateAsset(mat, AssetDatabase.GenerateUniqueAssetPath($"Assets/BlockMaterials/BlockMaterial.mat"));
                blockUVsAsset.material = mat;

                AssetDatabase.CreateAsset(blockUVsAsset, AssetDatabase.GenerateUniqueAssetPath($"Assets/BlockMaterials/blockUVs.asset"));
                AssetDatabase.SaveAssets();
                
                var polyMaster = maker.GetComponent<PolyTerrainMaster>();
                if (polyMaster) {
                    polyMaster.blockUVsAsset = blockUVsAsset;
                    polyMaster.style = PolyStyle.Blocky;
                    EditorUtility.SetDirty(polyMaster);
                    polyMaster.ApplySettings();
                } else {
                    var poly = maker.GetComponent<PolyTerrain>();
                    if (poly) {
                        poly.materials = new[] { mat };
                        poly.blockUVsAsset = blockUVsAsset;
                        poly.style = PolyStyle.Blocky;
                        EditorUtility.SetDirty(poly);
                        poly.Refresh();
                    }
                }

                Selection.activeObject = mat;
            }
        }

        private static void SetTextureImporterFormat(Texture2D texture, bool isReadable)
        {
            if (!texture)
                return;

            var assetPath = AssetDatabase.GetAssetPath(texture);
            var tImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (!tImporter)
                return;

            tImporter.isReadable = isReadable;
            AssetDatabase.ImportAsset(assetPath);
            AssetDatabase.Refresh();
        }

        private static void SetTextureImporterFilterMode(Texture2D texture, FilterMode textureFilterMode)
        {
            if (!texture)
                return;

            var assetPath = AssetDatabase.GetAssetPath(texture);
            var tImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (!tImporter)
                return;

            tImporter.textureType = TextureImporterType.Default;
            tImporter.filterMode = textureFilterMode;
            AssetDatabase.ImportAsset(assetPath);
            AssetDatabase.Refresh();
        }
    }

    public static class ExtensionMethod
    {
        public static Texture2D Uncompress(this Texture2D source)
        {
            var renderTex = RenderTexture.GetTemporary(
                source.width,
                source.height,
                0,
                RenderTextureFormat.Default,
                RenderTextureReadWrite.Default);

            Graphics.Blit(source, renderTex);
            var previous = RenderTexture.active;
            RenderTexture.active = renderTex;
            var readableText = new Texture2D(source.width, source.height);
            readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
            readableText.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(renderTex);
            return readableText;
        }
    }
}