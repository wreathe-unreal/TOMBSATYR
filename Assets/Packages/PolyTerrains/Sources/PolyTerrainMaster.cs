using PolyTerrains.Sources.BlockMaterial;
using UnityEngine;

namespace PolyTerrains.Sources
{
    [AddComponentMenu("PolyTerrains/Poly Terrain Master")]
    public class PolyTerrainMaster : MonoBehaviour
    {
        [SerializeField] public bool draw = true;
        [SerializeField] public PolyStyle style = PolyStyle.LowPoly;
        [SerializeField] public bool showDebug;
        [SerializeField] public bool enableOcclusionCulling;
        [SerializeField] public int layer = 0;
        [SerializeField] public string chunksTag = "Untagged";
        [SerializeField] public BlockUVsAsset blockUVsAsset;
        [SerializeField] public bool autoDisableTerrainCollider = true;
        [SerializeField] public int chunkSize = 33;

        public void ApplySettings()
        {
            var polyTerrains = FindObjectsOfType<PolyTerrain>();
            foreach (var poly in polyTerrains) {
                poly.draw = draw;
                poly.showDebug = showDebug;
                poly.enableOcclusionCulling = enableOcclusionCulling;
                poly.blockUVsAsset = blockUVsAsset;
                poly.style = style;
                poly.layer = layer;
                poly.chunksTag = chunksTag;
                poly.autoDisableTerrainCollider = autoDisableTerrainCollider;
                poly.ChunkSize = chunkSize;
                poly.Refresh();
            }
        }
    }
}