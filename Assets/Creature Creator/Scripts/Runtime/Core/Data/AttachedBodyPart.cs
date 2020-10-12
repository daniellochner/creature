// Creature Creator - https://github.com/daniellochner/SPORE-Creature-Creator
// Version: 1.0.0
// Author: Daniel Lochner

using System;
using UnityEngine;

namespace DanielLochner.Assets.CreatureCreator
{
    [Serializable]
    public class AttachedBodyPart
    {
        #region Fields
        [SerializeField] private string bodyPartID;
        [SerializeField] private int boneIndex;
        [SerializeField] private SerializableTransform transform;
        #endregion

        #region Properties
        public string BodyPartID { get { return bodyPartID; } }
        public int BoneIndex { get { return boneIndex; } set { boneIndex = value; } }
        public SerializableTransform Transform { get { return transform; } set { transform = value; } }
        #endregion

        #region Methods
        public AttachedBodyPart(string bodyPartID, int boneIndex, SerializableTransform transform)
        {
            this.bodyPartID = bodyPartID;
            this.boneIndex = boneIndex;
            this.transform = transform;
        }
        #endregion
    }

    [Serializable]
    public class AttachedLimb : AttachedBodyPart
    {
        #region Fields
        [SerializeField] private string extremityID;
        #endregion

        #region Properties
        public string ExtremityID { get { return extremityID; } set { extremityID = value; } }
        #endregion

        #region Methods
        public AttachedLimb(string bodyPartID, int boneIndex, SerializableTransform transform, string extremityID) : base(bodyPartID, boneIndex, transform)
        {
            this.extremityID = extremityID;
        }
        #endregion
    }
}