// Creature Creator - https://github.com/daniellochner/SPORE-Creature-Creator
// Version: 1.0.0
// Author: Daniel Lochner

using UnityEngine;
using UnityEngine.Events;

namespace DanielLochner.Assets.CreatureCreator
{
    public class Hover : MonoBehaviour
    {
        #region Properties
        public UnityEvent OnEnter { get; set; } = new UnityEvent();
        public UnityEvent OnExit { get; set; } = new UnityEvent();

        public bool IsOver { get; private set; }
        #endregion

        #region Methods
        private void OnMouseEnter()
        {
            OnEnter.Invoke();
            IsOver = true;
        }
        private void OnMouseExit()
        {
            OnExit.Invoke();
            IsOver = false;
        }
        #endregion
    }
}