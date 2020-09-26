using UnityEngine;

public class SelfDestructor : MonoBehaviour
{
    #region Fields
    [SerializeField] private float lifetime = 0.1f;
    private float elapsedTime = 0f;
    #endregion

    #region Methods
    private void Update()
    {
        if (elapsedTime < lifetime)
        {
            elapsedTime += Time.deltaTime;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion
}