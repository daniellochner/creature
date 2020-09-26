// Creature Creator - https://github.com/daniellochner/SPORE-Creature-Creator
// Version: 1.0.0
// Author: Daniel Lochner

using RotaryHeart.Lib.SerializableDictionary;
using System;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DanielLochner.Assets.CreatureCreator
{
    public class CreatureCreator : MonoBehaviour
    {
        #region Singleton
        public static CreatureCreator Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }
        #endregion

        #region Fields
        [SerializeField] private CreatureController creature;
        [SerializeField] private CameraOrbit cameraOrbit;

        [Header("Options")]
        [SerializeField] private TMP_InputField creatureName;
        [SerializeField] private GameObject creaturePrefab;
        [SerializeField] private RectTransform creaturesRT;
        [SerializeField] private ToggleGroup creaturesToggleGroup;

        [Header("Build")]
        [SerializeField] private Menu buildMenu;
        [SerializeField] private GameObject bodyPartPrefab;
        [SerializeField] private GameObject statisticsMenuPrefab;
        [SerializeField] private RectTransform bodyPartsRT;
        [SerializeField] private BodyPartGrids bodyPartGrids;
        [Space]
        [SerializeField] private int startingCash = 1000;
        [SerializeField] private TextMeshProUGUI cashText;
        [SerializeField] private Animator cashWarningAnimator;
        [SerializeField] private TextMeshProUGUI complexityText;
        [SerializeField] private Animator complexityWarningAnimator;
        [SerializeField] private TextMeshProUGUI dietText;
        [SerializeField] private TextMeshProUGUI speedText;
        [SerializeField] private TextMeshProUGUI healthText;
        [SerializeField] private GameObject abilityPrefab;
        [SerializeField] private RectTransform miscAbilities;
        [SerializeField] private RectTransform combatAbilities;
        [SerializeField] private RectTransform socialAbilities;

        [Header("Test")]
        [SerializeField] private Menu testMenu;

        [Header("Paint")]
        [SerializeField] private Menu paintMenu;
        [SerializeField] private GameObject patternPrefab;
        [SerializeField] private Material patternMaterial;
        [SerializeField] private RectTransform patternsRT;
        [SerializeField] private ToggleGroup patternsToggleGroup;
        [SerializeField] private ColourPicker primaryColourPicker;
        [SerializeField] private ColourPicker secondaryColourPicker;

        [Header("Other")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip whooshAudioClip;
        [SerializeField] private AudioClip errorAudioClip;
        [SerializeField] private AudioClip createAudioClip;

        private int cash;
        private StatisticsMenu statisticsMenu;
        #endregion

        #region Properties
        public CameraOrbit CameraOrbit { get { return cameraOrbit; } }
        #endregion

        #region Methods
        private void Start()
        {
            SetCash(startingCash);

            #region Creature
            creature.Add(0, new Vector3(0, 0.75f, 0), Quaternion.identity, 0f);
            creature.AddToBack();

            creature.SetSelected(false);
            creature.SetTextured(false);
            creature.SetInteractable(true);
            #endregion

            #region UI
            statisticsMenu = Instantiate(statisticsMenuPrefab, Dynamic.Canvas).GetComponent<StatisticsMenu>();
            patternMaterial = new Material(patternMaterial);

            foreach (string bodyPartID in DatabaseManager.GetDatabase("Body Parts").Objects.Keys)
            {
                BodyPart bodyPart = DatabaseManager.GetDatabaseEntry<BodyPart>("Body Parts", bodyPartID);

                GameObject bodyPartGO = Instantiate(bodyPartPrefab, bodyPartGrids[bodyPart.GetType().Name].GetComponent<RectTransform>());
                Animator bodyPartAnimator = bodyPartGO.GetComponent<Animator>();

                DragUI dragUI = bodyPartGO.GetComponent<DragUI>();
                dragUI.OnPress.AddListener(delegate
                {
                    bodyPartAnimator.SetBool("Expanded", false);
                    statisticsMenu.Hide();
                });
                dragUI.OnRelease.AddListener(delegate
                {
                    bodyPartGrids[bodyPart.GetType().Name].enabled = false;
                    bodyPartGrids[bodyPart.GetType().Name].enabled = true;
                });
                dragUI.OnDrag.AddListener(delegate
                {
                    bool notEnoughCash = (cash < bodyPart.Price);
                    bool tooComplex = (creature.Statistics.Complexity + bodyPart.Complexity > creature.Settings.MaximumComplexity);
                    if (notEnoughCash || tooComplex)
                    {
                        dragUI.OnPointerUp(null);
                        audioSource.PlayOneShot(errorAudioClip);

                        if (notEnoughCash && !cashWarningAnimator.IsInTransition(0) && !cashWarningAnimator.GetCurrentAnimatorStateInfo(0).IsName("Warning"))
                        {
                            cashWarningAnimator.SetTrigger("Warn");
                        }
                        if (tooComplex && !complexityWarningAnimator.IsInTransition(0) && !complexityWarningAnimator.GetCurrentAnimatorStateInfo(0).IsName("Warning"))
                        {
                            complexityWarningAnimator.SetTrigger("Warn");
                        }
                    }

                    if (!RectTransformUtility.RectangleContainsScreenPoint(bodyPartsRT, Input.mousePosition))
                    {
                        dragUI.OnPointerUp(null);
                        audioSource.PlayOneShot(createAudioClip);
                        creature.AddToStatistics(bodyPartID);

                        Ray ray = cameraOrbit.Camera.ScreenPointToRay(Input.mousePosition);
                        Plane plane = new Plane(cameraOrbit.Camera.transform.forward, Vector3.zero);

                        if (plane.Raycast(ray, out float distance))
                        {
                            BodyPartController bpc = Instantiate(bodyPart.Prefab, ray.GetPoint(distance), Quaternion.identity, Dynamic.Transform).GetComponent<BodyPartController>();

                            bpc.gameObject.name = bodyPartID;

                            bpc.Drag.Plane = plane;
                            creature.SetupBodyPart(bpc);
                            bpc.Drag.OnMouseDown();
                        }
                    }
                });

                HoverUI hoverUI = bodyPartGO.GetComponent<HoverUI>();
                hoverUI.OnEnter.AddListener(delegate
                {
                    if (!Input.GetMouseButton(0))
                    {
                        bodyPartAnimator.SetBool("Expanded", true);

                        statisticsMenu.nameText.text = bodyPart.name + " ($" + bodyPart.Price + ")";
                        statisticsMenu.complexityText.text = bodyPart.Complexity.ToString();
                        statisticsMenu.healthText.text = bodyPart.Health.ToString();

                        statisticsMenu.dietText.transform.parent.gameObject.SetActive(bodyPart is Mouth);
                        if (bodyPart is Mouth)
                        {
                            statisticsMenu.dietText.text = (bodyPart as Mouth).Diet.ToString();
                        }

                        statisticsMenu.speedText.transform.parent.gameObject.SetActive(bodyPart is Limb);
                        if (bodyPart is Limb)
                        {
                            statisticsMenu.speedText.text = (bodyPart as Limb).Speed.ToString();
                        }

                        statisticsMenu.Display();
                        statisticsMenu.Entered = true;
                    }
                });
                hoverUI.OnExit.AddListener(delegate
                {
                    bodyPartAnimator.SetBool("Expanded", false);
                    statisticsMenu.Entered = false;

                    Invoke("HideStatistics", 0.125f);
                });

                bodyPartGO.transform.Find("Icon").GetComponent<Image>().sprite = bodyPart.Icon;
            }

            foreach (string patternID in DatabaseManager.GetDatabase("Patterns").Objects.Keys)
            {
                Texture pattern = DatabaseManager.GetDatabaseEntry<Texture>("Patterns", patternID);

                GameObject patternGO = Instantiate(patternPrefab, patternsRT.transform);
                patternGO.name = patternID;

                Toggle patternToggle = patternGO.GetComponent<Toggle>();
                Image graphic = patternToggle.graphic as Image;
                Image targetGraphic = patternToggle.targetGraphic as Image;

                Animator patternAnimator = patternGO.GetComponent<Animator>();
                HoverUI hoverUI = patternGO.GetComponent<HoverUI>();
                hoverUI.OnEnter.AddListener(delegate
                {
                    if (!Input.GetMouseButton(0))
                    {
                        patternAnimator.SetBool("Expanded", true);
                    }
                });
                hoverUI.OnExit.AddListener(delegate
                {
                    patternAnimator.SetBool("Expanded", false);
                });

                graphic.sprite = targetGraphic.sprite = Sprite.Create(pattern as Texture2D, new Rect(0, 0, pattern.width, pattern.height), new Vector2(0.5f, 0.5f));
                graphic.material = targetGraphic.material = patternMaterial;

                patternToggle.onValueChanged.AddListener(delegate
                {
                    if (patternToggle.isOn)
                    {
                        creature.SetPattern(patternID);
                    }
                    else
                    {
                        creature.SetPattern("");
                    }
                });
                patternToggle.group = patternsToggleGroup;
            }

            string creaturesDirectory = Application.persistentDataPath + "/Creatures/";
            if (Directory.Exists(creaturesDirectory))
            {
                foreach (string creaturePath in Directory.GetFiles(creaturesDirectory))
                {
                    AddCreature(Path.GetFileNameWithoutExtension(creaturePath));
                }
            }
            #endregion
        }
        private void Update()
        {
            statisticsMenu.transform.position = Input.mousePosition;
            UpdateStatistics();

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ConfirmationMenu.Confirm("Quit", "Are you sure you want to exit?", yesEvent: delegate { Application.Quit(); });
            }
        }

        public void SaveCreature()
        {
            if (string.IsNullOrEmpty(creatureName.text)) { return; }

            // Data
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                creatureName.text = creatureName.text.Replace(c.ToString(), "");
            }
            creatureName.text = creatureName.text.Trim();
            creature.Save(creatureName.text);

            // UI
            AddCreature(creatureName.text);
            creaturesRT.Find(creatureName.text).GetComponent<Toggle>().SetIsOnWithoutNotify(true);
        }
        public void LoadCreature()
        {
            SetCash(startingCash);

            #region Creature
            string selectedCreatureName = "";
            foreach (Toggle creatureToggle in creaturesRT.GetComponentsInChildren<Toggle>())
            {
                if (creatureToggle.isOn)
                {
                    selectedCreatureName = creatureToggle.name;
                    break;
                }
            }
            if (string.IsNullOrEmpty(selectedCreatureName)) { return; }

            creature.Load(selectedCreatureName);
            #endregion

            #region UI
            primaryColourPicker.SetColour(creature.Data.primaryColour);
            secondaryColourPicker.SetColour(creature.Data.secondaryColour);

            patternsToggleGroup.SetAllTogglesOff(false);
            if (!string.IsNullOrEmpty(creature.Data.patternID))
            {
                patternsRT.Find(creature.Data.patternID).GetComponent<Toggle>().SetIsOnWithoutNotify(true);
            }
            patternMaterial.SetColor("_PrimaryCol", primaryColourPicker.Colour);
            patternMaterial.SetColor("_SecondaryCol", secondaryColourPicker.Colour);
            #endregion
        }
        public void ResetCreature()
        {
            creature.Clear();

            creature.Add(0, new Vector3(0, 0.75f, 0), Quaternion.identity, 0f);
            creature.AddToBack();

            creature.SetSelected(false);
            creature.SetTextured(creature.Textured);
            creature.SetInteractable(creature.Interactable);

            SetCash(startingCash);
        }

        public void UpdateColours()
        {
            creature.SetColours(primaryColourPicker.Colour, secondaryColourPicker.Colour);

            patternMaterial.SetColor("_PrimaryCol", primaryColourPicker.Colour);
            patternMaterial.SetColor("_SecondaryCol", secondaryColourPicker.Colour);
        }
        public void UpdateStatistics()
        {
            complexityText.text = "<b>Complexity:</b> " + creature.Statistics.Complexity + "/" + creature.Settings.MaximumComplexity;
            dietText.text = "<b>Diet:</b> " + creature.Statistics.Diet;
            speedText.text = "<b>Speed:</b> " + creature.Statistics.Speed;
            healthText.text = "<b>Health:</b> " + creature.Statistics.Health;
        }

        public void Build()
        {
            if (buildMenu.Visible) { return; }

            cameraOrbit.OffsetPosition = new Vector3(-0.75f, 1f, cameraOrbit.OffsetPosition.z);

            buildMenu.Display();
            testMenu.Hide();
            paintMenu.Hide();

            audioSource.PlayOneShot(whooshAudioClip);

            creature.SetInteractable(true);
            creature.SetTextured(false);
        }
        public void Test()
        {
            if (testMenu.Visible) { return; }

            cameraOrbit.OffsetPosition = new Vector3(0f, 1f, cameraOrbit.OffsetPosition.z);

            buildMenu.Hide();
            testMenu.Display();
            paintMenu.Hide();

            audioSource.PlayOneShot(whooshAudioClip);

            creature.SetInteractable(false);
            creature.SetSelected(false);
            creature.SetTextured(true);
        }
        public void Paint()
        {
            if (paintMenu.Visible) { return; }

            cameraOrbit.OffsetPosition = new Vector3(0.75f, 1f, cameraOrbit.OffsetPosition.z);

            buildMenu.Hide();
            testMenu.Hide();
            paintMenu.Display();

            audioSource.PlayOneShot(whooshAudioClip);

            creature.SetInteractable(false);
            creature.SetSelected(false);
            creature.SetTextured(true);
        }

        public void SetCash(int cash)
        {
            this.cash = cash;
            cashText.text = "$" + cash;
        }
        public void AddCash(int cash)
        {
            SetCash(this.cash + cash);
        }
        public void AddCreature(string creatureName)
        {
            if (!creaturesRT.Find(creatureName))
            {
                GameObject creatureGO = Instantiate(creaturePrefab, creaturesRT);

                creatureGO.transform.SetAsFirstSibling();
                creatureGO.name = creatureName;
                creatureGO.GetComponentInChildren<TextMeshProUGUI>().text = creatureName;

                Toggle toggle = creatureGO.GetComponent<Toggle>();
                toggle.group = creaturesToggleGroup;

                Button button = creatureGO.GetComponentInChildren<Button>();
                button.onClick.AddListener(delegate
                {
                    File.Delete(Application.persistentDataPath + "/Creatures/" + creatureName + ".json");
                    Destroy(creatureGO);
                });
            }
        }

        private void HideStatistics()
        {
            if (!statisticsMenu.Entered)
            {
                statisticsMenu.Hide();
            }
        }
        #endregion

        #region Inner Classes
        [Serializable] public class BodyPartGrids : SerializableDictionaryBase<string, GridLayoutGroup> { }
        #endregion
    }
}