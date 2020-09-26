// Creature Creator - https://github.com/daniellochner/SPORE-Creature-Creator
// Version: 1.0.0
// Author: Daniel Lochner

using System;
using UnityEngine;

namespace DanielLochner.Assets.CreatureCreator
{
    [Serializable]
    public class Bone
    {
        #region Fields
        [SerializeField] private Vector3 position;
        [SerializeField] private Quaternion rotation;
        [SerializeField] private float size;
        #endregion

        #region Properties
        public Vector3 Position { get { return position; } set { position = value; } }
        public Quaternion Rotation { get { return rotation; } set { rotation = value; } }
        public float Size { get { return size; } set { size = value; } }
        #endregion

        #region Methods
        public Bone(Vector3 position, Quaternion rotation, float size)
        {
            this.position = position;
            this.rotation = rotation;
            this.size = size;
        }
        #endregion
    }
}