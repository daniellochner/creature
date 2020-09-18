using System;
using System.Collections.Generic;
using UnityEngine;
using static DanielLochner.Assets.CreatureCreator.CreatureController;

namespace DanielLochner.Assets.CreatureCreator
{
    [Serializable]
    public class CreatureData
    {
        public List<Bone> bones = new List<Bone>();
        public List<AttachedBodyPart> attachedBodyParts = new List<AttachedBodyPart>();

        public string patternID = "";
        public Color primaryColour = Color.white;
        public Color secondaryColour = Color.black;
    }
}
