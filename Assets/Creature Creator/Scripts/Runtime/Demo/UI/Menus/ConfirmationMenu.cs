using TMPro;
using UnityEngine.UI;

public class ConfirmationMenu : Menu
{
    #region Singleton
    public static ConfirmationMenu Instance { get; set; }
    protected override void Awake()
    {
        base.Awake();

        Instance = this;
    }
    #endregion

    #region Fields
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI confirmationMessageText;
    public TextMeshProUGUI yesText;
    public TextMeshProUGUI noText;
    public Button yesButton;
    public Button noButton;
    #endregion

    #region Methods
    public static void Confirm(string title = "Title", string confirmationMessage = "Message", string yes = "Yes", string no = "No", ConfirmationEvent yesEvent = null, ConfirmationEvent noEvent = null)
    {
        Instance.titleText.text = title;
        Instance.confirmationMessageText.text = confirmationMessage;
        Instance.yesText.text = yes;
        Instance.noText.text = no;
        Instance.titleText.text = title;

        Instance.yesButton.onClick.RemoveAllListeners();
        Instance.noButton.onClick.RemoveAllListeners();
        Instance.yesButton.onClick.AddListener(delegate
        {
            Instance.Hide();

            if (yesEvent != null)
            {
                yesEvent.Invoke();
            }
        });
        Instance.noButton.onClick.AddListener(delegate
        {
            Instance.Hide();

            if (noEvent != null)
            {
                noEvent.Invoke();
            }
        });

        Instance.Display();
    }
    #endregion

    #region Delegates
    public delegate void ConfirmationEvent();
    #endregion
}