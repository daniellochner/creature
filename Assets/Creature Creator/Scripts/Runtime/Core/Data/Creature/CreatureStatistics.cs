// Creature Creator - https://github.com/daniellochner/SPORE-Creature-Creator
// Version: 1.0.0
// Author: Daniel Lochner

using System;
using System.Collections.Generic;
using UnityEngine;

namespace DanielLochner.Assets.CreatureCreator
{
    [Serializable]
    public class CreatureStatistics
    {
        #region Fields
        [SerializeField] private int complexity = 0;
        [SerializeField] private Diet diet = Diet.None;
        [SerializeField] private int speed = 0;
        [SerializeField] private int health = 0;
        [SerializeField] private List<Misc> miscAbilities;
        [SerializeField] private List<Combat> combatAbilities;
        [SerializeField] private List<Social> socialAbilities;
        #endregion

        #region Properties
        public int Complexity { get { return complexity; } set { complexity = value; } }
        public Diet Diet { get { return diet; } set { diet = value; } }
        public int Speed { get { return speed; } set { speed = value; } }
        public int Health { get { return health; } set { health = value; } }
        public List<Misc> MiscAbilities { get { return miscAbilities; } }
        public List<Combat> CombatAbilities { get { return combatAbilities; } }
        public List<Social> SocialAbilities { get { return socialAbilities; } }
        #endregion
    }
}