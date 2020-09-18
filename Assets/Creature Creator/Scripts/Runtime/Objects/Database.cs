using RotaryHeart.Lib.SerializableDictionary;
using System;
using UnityEngine;

namespace DanielLochner.Assets.CreatureCreator
{
    [CreateAssetMenu(fileName = "New Databse", menuName = "Database")]
    public class Database : ScriptableObject
    {
        public ObjectDictionary Objects = new ObjectDictionary();

        [Serializable] public class ObjectDictionary : SerializableDictionaryBase<string, UnityEngine.Object> { }
    }
}