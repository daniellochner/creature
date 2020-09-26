using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ColourPicker : MonoBehaviour
{
    #region Fields
    [SerializeField] private Color[] colours;
    [SerializeField] private GameObject colourPrefab;
    [SerializeField] private Image preview;
    [SerializeField] private RectTransform coloursRT;
    [SerializeField] private TextMeshProUGUI foregroundText;
    [Space]
    [SerializeField] private Color startColour;
    [SerializeField] private Vector2 size;
    [SerializeField] private UnityEvent onColourPick;
    #endregion

    #region Properties
    public Color Colour { get; private set; }
    #endregion

    #region Methods
    private void Start()
    {
        preview.color = (Colour = startColour);
        foregroundText.color = startColour.grayscale > 0.5f ? Color.black : Color.white;

        int colourIndex = 0;
        for (int y = 0; y < size.y; y++)
        {
            HorizontalLayoutGroup hlg = new GameObject(y.ToString()).AddComponent<HorizontalLayoutGroup>();
            hlg.transform.SetParent(coloursRT);
            hlg.transform.localScale = Vector3.one;

            hlg.childControlHeight = hlg.childControlWidth = true;

            for (int x = 0; x < size.x; x++)
            {
                GameObject colourGO = Instantiate(colourPrefab, hlg.transform);

                Color colour = colours[colourIndex];
                colourGO.GetComponent<Image>().color = colour;
                colourGO.GetComponent<Button>().onClick.AddListener(delegate
                {
                    preview.color = (Colour = colour);
                    coloursRT.gameObject.SetActive(false);

                    foregroundText.color = colour.grayscale > 0.5f ? Color.black : Color.white;

                    onColourPick.Invoke();
                });

                colourIndex++;
            }
        }
    }
    public void SetColour(Color colour)
    {
        preview.color = (Colour = colour);
    }
    #endregion
}
