// Creature Creator - https://github.com/daniellochner/SPORE-Creature-Creator
// Version: 1.0.0
// Author: Daniel Lochner

using RotaryHeart.Lib.SerializableDictionary;
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
        [SerializeField] private BodyPartTransformations transformations;
        #endregion

        #region Properties
        public string BodyPartID { get { return bodyPartID; } }
        public int BoneIndex { get { return boneIndex; } set { boneIndex = value; } }
        public SerializableTransform Transform { get { return transform; } set { transform = value; } }
        public BodyPartTransformations Transformations { get { return transformations; } set { transformations = value; } }
        #endregion

        #region Methods
        public AttachedBodyPart(string bodyPartID, int boneIndex, SerializableTransform transform)
        {
            this.bodyPartID = bodyPartID;
            this.boneIndex = boneIndex;
            this.transform = transform;
        }
        #endregion

        #region Inner Classes
        [Serializable] public class BodyPartTransformations : SerializableDictionaryBase<Transformation, float> { }
        #endregion
    }
}