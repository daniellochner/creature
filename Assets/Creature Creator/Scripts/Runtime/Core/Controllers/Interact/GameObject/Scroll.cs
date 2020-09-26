// Creature Creator - https://github.com/daniellochner/SPORE-Creature-Creator
// Version: 1.0.0
// Author: Daniel Lochner

using UnityEngine;
using UnityEngine.Events;

namespace DanielLochner.Assets.CreatureCreator
{
    public class Scroll : MonoBehaviour
    {
        #region Properties
        public UnityEvent OnScrollUp { get; set; } = new UnityEvent();
        public UnityEvent OnScrollDown { get; set; } = new UnityEvent();
        #endregion

        #region Methods
        private void OnMouseOver()
        {
            if (Input.mouseScrollDelta.y > 0)
            {
                OnScrollUp.Invoke();
            }
            else if (Input.mouseScrollDelta.y < 0)
            {
                OnScrollDown.Invoke();
            }
        }
        #endregion
    }
}