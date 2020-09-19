using UnityEngine;

namespace DanielLochner.Games.NEST
{
    public class ArrowFlipper : MonoBehaviour
    {
        public void Flip()
        {
            transform.Rotate(0, 0, 180);
        }
    }
}