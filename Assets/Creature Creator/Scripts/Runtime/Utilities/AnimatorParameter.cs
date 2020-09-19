using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimatorParameter : MonoBehaviour
{
    #region Fields
    [SerializeField] private string parameter;

    private Animator animator;
    #endregion

    #region Methods
    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void SetBool(bool value)
    {
        animator.SetBool(parameter, value);
    }
    public void SetFloat(float value)
    {
        animator.SetFloat(parameter, value);
    }
    public void SetInteger(int value)
    {
        animator.SetInteger(parameter, value);
    }
    #endregion
}
