using UnityEngine;
using UnityEngine.Rendering;

namespace Haipeng.Ghost_trail
{
    public class DrawingMesh
    {
        public Mesh mesh;
        public Material material;
        public Matrix4x4 matrix;
        public float show_time;
        public float left_time;
        public ShadowCastingMode shadowCastingMode; // Add this line

    }
}