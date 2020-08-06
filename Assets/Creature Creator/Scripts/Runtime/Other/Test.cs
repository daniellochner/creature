using UnityEngine;

namespace DanielLochner.Assets.CreatureCreator
{
    public class Test : MonoBehaviour
    {
        private void OnMouseEnter()
        {
            Debug.Log("test1");
            Body.IsModifyingMesh = true;
        }
        private void OnMouseExit()
        {
            Debug.Log("test2");
            Body.IsModifyingMesh = false;
        }
    }
}