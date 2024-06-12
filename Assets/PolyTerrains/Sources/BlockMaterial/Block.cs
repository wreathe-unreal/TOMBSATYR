using System;
using UnityEngine;

namespace PolyTerrains.Sources.BlockMaterial
{
    [Serializable]
    public class Block : ScriptableObject
    {
        [SerializeField] public Texture2D top;
        [SerializeField] public Texture2D side;
        [SerializeField] public Texture2D bottom;
    }
}