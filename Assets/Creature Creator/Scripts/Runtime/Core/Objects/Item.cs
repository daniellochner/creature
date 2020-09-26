using UnityEngine;

namespace DanielLochner.Assets.CreatureCreator
{
    public abstract class Item : ScriptableObject
    {
        #region Fields
        [Header("Item")]
        [SerializeField] private string description;
        [SerializeField] private Sprite icon;
        #endregion

        #region Properties
        public string Description { get { return description; } }
        public Sprite Icon { get { return icon; } }
        #endregion
    }
}