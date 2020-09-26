// Creature Creator - https://github.com/daniellochner/SPORE-Creature-Creator
// Version: 1.0.0
// Author: Daniel Lochner

using System;
using System.Collections.Generic;
using UnityEngine;

namespace DanielLochner.Assets.CreatureCreator
{
    [Serializable]
    public class CreatureData
    {
        #region Fields
        public List<Bone> bones = new List<Bone>();
        public List<AttachedBodyPart> attachedBodyParts = new List<AttachedBodyPart>();

        public string patternID = "";
        public Color primaryColour = Color.white;
        public Color secondaryColour = Color.black;
        #endregion
    }
}