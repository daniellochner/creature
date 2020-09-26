using UnityEngine;

public class Menu : MonoBehaviour
{
    #region Fields
    protected Animator animator;
    #endregion

    #region Properties
    public bool Visible
    {
        get
        {
            return animator.IsInTransition(0) || animator.GetBool("Visible");
        }
    }
    #endregion

    #region Methods
    protected virtual void Awake()
    {
        animator = GetComponent<Animator>();

        animator.SetInteger("Default State", animator.GetBool("Visible") ? 1 : 0);
    }

    protected virtual void SetVisibility(bool visible)
    {
        animator.SetBool("Visible", visible);
    }

    public virtual void Display()
    {
        SetVisibility(true);
    }
    public virtual void Hide()
    {
        SetVisibility(false);
    }
    public void Toggle()
    {
        if (animator.GetBool("Visible")) { Hide(); }
        else { Display(); }
    }
    #endregion
}
