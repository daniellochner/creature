using BasicTools.ButtonInspector;
using RotaryHeart.Lib.SerializableDictionary;
using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static DanielLochner.Assets.CreatureCreator.CreatureController;

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

        private Material patternMaterial;
        private int cash = 0, complexity;
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
            UpdateStatistics();

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
                    // Prevent body part from being added.
                    if (cash < bodyPart.Price)
                    {
                        dragUI.OnPointerUp(null);
                        if (!cashWarningAnimator.GetCurrentAnimatorStateInfo(0).IsName("Warning"))
                        {
                            cashWarningAnimator.SetTrigger("Warn");
                        }
                    }
                    if (complexity + bodyPart.Complexity > creature.MaximumComplexity)
                    {
                        dragUI.OnPointerUp(null);
                        if (!complexityWarningAnimator.GetCurrentAnimatorStateInfo(0).IsName("Warning"))
                        {
                            complexityWarningAnimator.SetTrigger("Warn");
                        }
                    }

                    // Drag body part into build area.
                    if (!RectTransformUtility.RectangleContainsScreenPoint(bodyPartsRT, Input.mousePosition))
                    {
                        dragUI.OnPointerUp(null);
                        SetCash(cash - bodyPart.Price);

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

                bodyPartIcon.GetComponent<Image>().sprite = bodyPart.Icon;
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
            primaryColourPicker.SetColour(creature.CreatureData.primaryColour);
            secondaryColourPicker.SetColour(creature.CreatureData.secondaryColour);

            if (creature.CreatureData.patternID != "")
            {
                patternsRT.Find(creature.CreatureData.patternID).GetComponent<Toggle>().SetIsOnWithoutNotify(true);
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
            int complexity = 0;
            Diet diet = Diet.None;
            int speed = 0;
            int health = 0;
            List<Ability> abilities = new List<Ability>();

            foreach (AttachedBodyPart attachedBodyPart in creature.CreatureData.attachedBodyParts)
            {
                BodyPart bodyPart = DatabaseManager.GetDatabaseEntry<BodyPart>("Body Parts", attachedBodyPart.BodyPartID);

                complexity += bodyPart.Complexity;
                health += bodyPart.Health;

                if (bodyPart is Mouth && diet != Diet.Omnivore) // Omnivore is the preferred diet.
                {
                    diet = (bodyPart as Mouth).Diet;
                }
                else if (bodyPart is Limb)
                {
                    speed += (bodyPart as Limb).Speed;
                }

                //foreach (Ability ability in bodyPart.Attributes) // Determine the best abilities.
                //{
                //    for (int i = 0; i < abilities.Count; i++)
                //    {
                //        if ((ability.GetType() == abilities[i].GetType()) && (ability.Level > abilities[i].Level))
                //        {
                //            abilities.RemoveAt(i);
                //            abilities.Add(ability);
                //            break;
                //        }
                //    }
                //}
            }

            complexityText.text = "<b>Complexity:</b> " + complexity + "/" + creature.MaximumComplexity;
            dietText.text = "<b>Diet:</b> " + diet;
            speedText.text = "<b>Speed:</b> " + speed;
            healthText.text = "<b>Health:</b> " + health;


        }

        public void Build()
        {
            cameraOrbit.OffsetPosition = new Vector3(-0.75f, 1f, cameraOrbit.OffsetPosition.z);

            buildMenu.Display();
            testMenu.Hide();
            paintMenu.Hide();

            creature.SetInteractable(true);
            creature.SetTextured(false);
        }
        public void Test()
        {
            cameraOrbit.OffsetPosition = new Vector3(0f, 1f, cameraOrbit.OffsetPosition.z);

            buildMenu.Hide();
            testMenu.Display();
            paintMenu.Hide();

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

            creature.SetInteractable(false);
            creature.SetSelected(false);
            creature.SetTextured(true);
        }

        private void SetCash(int cash)
        {
            this.cash = cash;
            cashText.text = "$" + cash;
        }
        #endregion

        #region Inner Classes
        [Serializable] public class BodyPartGrids : SerializableDictionaryBase<string, GridLayoutGroup> { }
        #endregion
    }
}