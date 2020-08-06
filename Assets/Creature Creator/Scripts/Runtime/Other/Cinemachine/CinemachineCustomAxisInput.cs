using DanielLochner.Assets.CreatureCreator;
using UnityEngine;

namespace Cinemachine
{
    public class CinemachineCustomAxisInput : MonoBehaviour
    {
        #region Methods
        private void Start()
        {
            CinemachineCore.GetInputAxis = GetCustomAxisInput;
        }
        private float GetCustomAxisInput(string axisName)
        {
            if (axisName == "Mouse X")
            {
                if (Input.GetMouseButton(0) && !Body.IsModifyingMesh)
                {
                    return Input.GetAxis("Mouse X");
                }
                else
                {
                    return 0;
                }
            }
            else if (axisName == "Mouse Y")
            {
                if (Input.GetMouseButton(0) && !Body.IsModifyingMesh)
                {
                    return Input.GetAxis("Mouse Y");
                }
                else
                {
                    return 0;
                }
            }
            return Input.GetAxis(axisName);
        }
        #endregion
    }
}