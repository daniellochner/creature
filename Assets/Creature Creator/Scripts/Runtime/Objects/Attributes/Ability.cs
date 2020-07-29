﻿using UnityEngine;

namespace DanielLochner.Assets.CreatureCreator
{
    public abstract class Ability : Attribute
    {
        [Header("Ability")]
        [SerializeField] private int level;
    }
}