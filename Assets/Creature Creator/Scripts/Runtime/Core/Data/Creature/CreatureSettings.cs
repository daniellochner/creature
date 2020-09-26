// Creature Creator - https://github.com/daniellochner/SPORE-Creature-Creator
// Version: 1.0.0
// Author: Daniel Lochner

using System;
using UnityEngine;

namespace DanielLochner.Assets.CreatureCreator
{
    [Serializable]
    public class CreatureSettings
    {
        #region Fields
        [SerializeField] private int maximumComplexity = 20;
        [SerializeField] private float mergeThreshold = 0.01f;
        [SerializeField] private Vector2Int minMaxBones = new Vector2Int(2, 20);
        [Space]
        [SerializeField] private float radius = 0.05f;
        [SerializeField] private float length = 0.1f;
        [SerializeField] [Range(4, 25)] private int segments = 12;
        [SerializeField] [Range(2, 25)] private int rings = 4;
        #endregion

        #region Properties
        public int MaximumComplexity { get { return maximumComplexity; } }
        public float MergeThreshold { get { return mergeThreshold; } }
        public Vector2Int MinMaxBones { get { return minMaxBones; } }

        public float Radius { get { return radius; } }
        public float Length { get { return length; } }
        public int Segments { get { return segments; } }
        public int Rings { get { return rings; } }
        #endregion
    }
}