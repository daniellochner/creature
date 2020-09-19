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
        [SerializeField] private RectTransform bodyPartsRT;
        [SerializeField] private BodyPartGrids bodyPartGrids;
        [Space]
        [SerializeField] private int cash = 1000;
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
        [SerializeField] private RectTransform patternsRT;
        [SerializeField] private ToggleGroup patternsToggleGroup;
        [SerializeField] private ColourPicker primaryColourPicker;
        [SerializeField] private ColourPicker secondaryColourPicker;

        [Header("Other")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip whooshAudioClip;
        [SerializeField] private AudioClip errorAudioClip;
        [SerializeField] private AudioClip createAudioClip;

        private Material patternMaterial;
        #endregion

        #region Methods
        private void Start()
        {
            #region Creature
            creature.Add(0, creature.transform.position, creature.transform.rotation, 0f);
            creature.AddToBack();

            creature.SetSelected(false);
            creature.SetTextured(false);
            #endregion

            #region UI
            cashText.text = "$" + cash;

            foreach (string bodyPartID in DatabaseManager.GetDatabase("Body Parts").Objects.Keys)
            {
                BodyPart bodyPart = DatabaseManager.GetDatabaseEntry<BodyPart>("Body Parts", bodyPartID);

                GameObject bodyPartIcon = Instantiate(bodyPartPrefab, bodyPartGrids[bodyPart.GetType().Name].GetComponent<RectTransform>());

                DragUI dragUI = bodyPartIcon.GetComponent<DragUI>();
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

                        Ray ray = cameraOrbit.Camera.ScreenPointToRay(Input.mousePosition);
                        Plane plane = new Plane(cameraOrbit.Camera.transform.forward, Vector3.zero);

                        if (plane.Raycast(ray, out float distance))
                        {
                            BodyPartController bpc = Instantiate(bodyPart.Prefab, ray.GetPoint(distance), Quaternion.identity, Dynamic.Transform).GetComponent<BodyPartController>();

                            bpc.gameObject.name = bodyPartID;

                            bpc.drag.Plane = plane;
                            creature.SetupBodyPart(bpc);
                            bpc.drag.OnMouseDown();
                        }
                    }
                });

                bodyPartIcon.transform.Find("Icon").GetComponent<Image>().sprite = bodyPart.Icon;
            }

            patternMaterial = new Material(Shader.Find("Creature Creator/Pattern"));
            foreach (string patternID in DatabaseManager.GetDatabase("Patterns").Objects.Keys)
            {
                Texture pattern = DatabaseManager.GetDatabaseEntry<Texture>("Patterns", patternID);

                GameObject patternGO = Instantiate(patternPrefab, patternsRT.transform);
                patternGO.name = patternID;

                Toggle patternToggle = patternGO.GetComponent<Toggle>();
                Image graphic = patternToggle.graphic as Image;
                Image targetGraphic = patternToggle.targetGraphic as Image;

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

            UpdateCreatures(); // Loadable creatures.
            #endregion
        }
        private void Update()
        {
            UpdateStatistics();
        }

        public void Save()
        {
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                creatureName.text = creatureName.text.Replace(c.ToString(), "");
            }
            creatureName.text = creatureName.text.Trim();

            if (string.IsNullOrEmpty(creatureName.text)) { return; }

            creature.Save(creatureName.text);

            UpdateCreatures();
        }
        public void Load()
        {
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

            // Creature
            creature.Load(selectedCreatureName);

            // UI
            primaryColourPicker.SetColour(creature.Data.primaryColour);
            secondaryColourPicker.SetColour(creature.Data.secondaryColour);

            if (creature.Data.patternID != "")
            {
                patternsRT.Find(creature.Data.patternID).GetComponent<Toggle>().SetIsOnWithoutNotify(true);
            }
            else
            {
                patternsToggleGroup.SetAllTogglesOff();
            }
            creature.SetTextured(creature.Textured);
            patternMaterial.SetColor("_PrimaryCol", primaryColourPicker.Colour);
            patternMaterial.SetColor("_SecondaryCol", secondaryColourPicker.Colour);

            UpdateStatistics();
        }

        public void UpdateCreatures()
        {
            string creaturesPath = Application.persistentDataPath + "/Creatures";
            if (Directory.Exists(creaturesPath))
            {
                foreach (string creaturePath in Directory.GetFiles(creaturesPath))
                {
                    string creature = Path.GetFileNameWithoutExtension(creaturePath);

                    if (!creaturesRT.Find(creature))
                    {
                        GameObject creatureGO = Instantiate(creaturePrefab, creaturesRT);

                        creatureGO.name = creature;
                        creatureGO.GetComponentInChildren<TextMeshProUGUI>().text = creature;

                        Toggle toggle = creatureGO.GetComponent<Toggle>();
                        toggle.group = creaturesToggleGroup;
                        toggle.onValueChanged.AddListener(delegate
                        {
                            if (toggle.isOn)
                            {
                            }
                        });
                    }
                }
            }
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
            cameraOrbit.OffsetPosition = new Vector3(0.75f, 1f, cameraOrbit.OffsetPosition.z);

            buildMenu.Hide();
            testMenu.Hide();
            paintMenu.Display();

            audioSource.PlayOneShot(whooshAudioClip);

            creature.SetInteractable(false);
            creature.SetSelected(false);
            creature.SetTextured(true);
        }

        public void AddCash(int cash)
        {
            this.cash += cash;
            cashText.text = "$" + this.cash;
        }
        #endregion

        #region Inner Classes
        [Serializable] public class BodyPartGrids : SerializableDictionaryBase<string, GridLayoutGroup> { }
        #endregion
    }
}