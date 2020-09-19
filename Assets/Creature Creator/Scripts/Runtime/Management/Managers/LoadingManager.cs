using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DanielLochner.Games.NEST
{
    public class LoadingManager : MonoBehaviour
    {
        #region Fields
        [SerializeField] private Slider loadingBarSlider;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private TextMeshProUGUI hintText;
        [SerializeField] private TextMeshProUGUI progressText;
        [Space]
        [SerializeField] private Sprite[] backgroundImages = new Sprite[] { null };
        [SerializeField] private string[] hints = new string[] { "Default Hint" };

        private static string sceneToLoad = "";
        private static float fadeDuration = 0.25f;
        #endregion

        #region Methods
        private void Start()
        {
            FadeUtility.Fade(false, fadeDuration, delegate { StartCoroutine(LoadSceneRoutine(sceneToLoad)); });

            backgroundImage.sprite = backgroundImages[Random.Range(0, backgroundImages.Length)];
            hintText.text = "<b>Hint:</b> " + hints[Random.Range(0, hints.Length)];
        }
        private IEnumerator LoadSceneRoutine(string scene)
        {
            AsyncOperation loadingAsyncOperation = SceneManager.LoadSceneAsync(scene);

            loadingAsyncOperation.allowSceneActivation = false;
            loadingAsyncOperation.completed += delegate { FadeUtility.Fade(false, fadeDuration); };

            while (!loadingAsyncOperation.isDone)
            {
                float loadProgress = Mathf.Clamp01(loadingAsyncOperation.progress / 0.9f);
                loadingBarSlider.value = loadProgress;
                progressText.text = Mathf.RoundToInt(loadProgress * 100) + "%";

                if (loadProgress >= 1f)
                {
                    break;
                }
                else { yield return null; }
            }

            FadeUtility.Fade(true, fadeDuration, delegate { loadingAsyncOperation.allowSceneActivation = true; });
        }

        public static void LoadScene(string scene)
        {
            sceneToLoad = scene;
            FadeUtility.Fade(true, fadeDuration, delegate { SceneManager.LoadScene("Loading Screen"); });
        }
        #endregion

        #region Inner Classes
        public class FadeUtility : MonoBehaviour
        {
            private static CanvasGroup fadeCanvasGroup = null;
            private static int maximumSortingOrder = 100;

            public static void Fade(bool targetVisibility, float fadeDuration, FadeEvent endFadeEvent = null)
            {
                #region Fade Canvas
                GameObject fadeCanvasGO = new GameObject("Fade");
                fadeCanvasGO.transform.SetAsLastSibling();
                FadeUtility fader = fadeCanvasGO.AddComponent<FadeUtility>();

                Canvas fadeCanvas = fadeCanvasGO.AddComponent<Canvas>();
                fadeCanvas.sortingOrder = maximumSortingOrder;
                fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

                RawImage rawImage = fadeCanvasGO.AddComponent<RawImage>();
                rawImage.color = Color.black;

                fadeCanvasGroup = fadeCanvasGO.AddComponent<CanvasGroup>();
                fadeCanvasGroup.alpha = targetVisibility ? 0 : 1;
                #endregion

                fader.StartCoroutine(fader.FadeRoutine(targetVisibility, fadeDuration, endFadeEvent));
            }

            public IEnumerator FadeRoutine(bool targetVisibility, float fadeDuration, FadeEvent endFadeEvent = null)
            {
                yield return StartCoroutine(FadeCanvasGroupRoutine(fadeCanvasGroup, targetVisibility, fadeDuration));

                if (endFadeEvent != null) { endFadeEvent.Invoke(); }
            }
            public static IEnumerator FadeCanvasGroupRoutine(CanvasGroup canvasGroup, bool visible, float duration, bool setInteractable = true)
            {
                if (setInteractable) { canvasGroup.blocksRaycasts = canvasGroup.interactable = visible; }

                if (visible)
                {
                    for (float i = canvasGroup.alpha; i < 1; i += Time.unscaledDeltaTime / duration)
                    {
                        canvasGroup.alpha = i;
                        yield return null;
                    }
                    canvasGroup.alpha = 1;
                }
                else
                {
                    for (float i = canvasGroup.alpha; i > 0; i -= Time.unscaledDeltaTime / duration)
                    {
                        canvasGroup.alpha = i;
                        yield return null;
                    }
                    canvasGroup.alpha = 0;
                }
            }
        }
        #endregion

        #region Delegates
        public delegate void FadeEvent();
        #endregion
    }
}