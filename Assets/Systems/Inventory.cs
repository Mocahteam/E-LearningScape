using UnityEngine;
using FYFY;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;
using FYFY_plugins.PointerManager;
using System.Collections.Generic;
using TMPro;
using FYFY_plugins.Monitoring;

public class Inventory : FSystem {

    private Family inventory = FamilyManager.getFamily(new AnyOfTags("Inventory"));
    private Family player = FamilyManager.getFamily(new AnyOfTags("Player"));
    private Family cGO = FamilyManager.getFamily(new AllOfComponents(typeof(CollectableGO)), new AllOfProperties(PropertyMatcher.PROPERTY.ACTIVE_SELF));
    private Family ui = FamilyManager.getFamily(new AllOfComponents(typeof(Canvas)));
    private Family syllabusElems = FamilyManager.getFamily(new AnyOfTags("Syllabus"), new AllOfComponents(typeof(CollectableGO)), new NoneOfComponents(typeof(Image)));
    private Family inputfields = FamilyManager.getFamily(new AllOfComponents(typeof(InputField)));
    private Family pui = FamilyManager.getFamily(new AnyOfTags("PuzzleUI"));
    private Family elemsInventory = FamilyManager.getFamily(new AnyOfTags("InventoryElements"));
    private Family onElem = FamilyManager.getFamily(new AnyOfTags("InventoryElements", "PuzzleUI", "PuzzleButton", "IARTab"), new AllOfComponents(typeof(PointerOver)));
    private Family plank = FamilyManager.getFamily(new AnyOfTags("Plank"));
    private Family glassesBackgrounds = FamilyManager.getFamily(new AnyOfTags("GlassesBG"));
    private Family sUI = FamilyManager.getFamily(new AnyOfTags("ScrollUI"));
    private Family animatedSprites = FamilyManager.getFamily(new AllOfComponents(typeof(AnimatedSprites)));
    private Family backgrounds = FamilyManager.getFamily(new AnyOfTags("UIBackground"));
    private Family displayedBackground = FamilyManager.getFamily(new AnyOfTags("UIBackground"), new AllOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));
    private Family focusedMatFamily = FamilyManager.getFamily(new AllOfComponents(typeof(FocusedGOMaterial)));
    private Family pointerSensitive = FamilyManager.getFamily(new AllOfComponents(typeof(PointerSensitive)));

    public static bool playerEnabled = true;
    private GameObject displayer;
    private GameObject displayedElement;
    private GameObject selectedUI;
    private GameObject inventoryElemGO;
    private TextMeshProUGUI descriptionTitle;
    private TextMeshProUGUI descriptionText;
    private GameObject displayedDescriptionGO;

    private GameObject glassesBG1;
    private GameObject glassesUI1;
    private GameObject glassesBG2;
    private GameObject glassesUI2;

    private RaycastHit hit;
    private bool inputfieldFocused;

    private Dictionary<int, GameObject> puzzleUI;
    private GameObject puzzlePiece;
    private bool onPuzzle = false;
    private Vector3 posBeforeDrag;
    private GameObject draggedPuzzle = null;
    private GameObject selectedPuzzle;
    private Vector3 posFromMouse;
    private List<int> idTable;
    private GameObject puzzleRotationButtons;
    private GameObject wireUI;
    public static bool wireOn = true;
    private bool localWireOn = true;

    private Dictionary<string, GameObject> scrollUI;
    private GameObject scroll;

	private GameObject blackLight;
    
    private Material displayedBGMaterial;
    private bool backgroundTextureSet = false;

	//tmp variables used to loop in famillies
	private GameObject forGO;
	private GameObject forGO2;

    public Inventory()
    {
        if (Application.isPlaying)
        {
            foreach (Transform child in inventory.First().transform)
            {
                if (child.gameObject.name == "Enabled")
                {
                    child.gameObject.SetActive(false);
                }
                else if (child.gameObject.name == "Display")
                {
                    displayer = child.gameObject;
                }
                else if (child.gameObject.name == "Description")
                {
                    foreach (Transform c in child)
                    {
                        if (c.gameObject.name == "Title")
                        {
                            descriptionTitle = c.gameObject.GetComponent<TextMeshProUGUI>();
                        }
                        else if (c.gameObject.name == "Description")
                        {
                            descriptionText = c.gameObject.GetComponent<TextMeshProUGUI>();
                        }
                    }
                }
            }

            foreach (Transform child in elemsInventory.First().transform.parent)
            {
                if (child.gameObject.name == "Glasses1")
                {
                    glassesUI1 = child.gameObject;
                }
                else if (child.gameObject.name == "Glasses2")
                {
                    glassesUI2 = child.gameObject;
                }
                else if(child.gameObject.name == "Wire")
                {
                    wireUI = child.gameObject;
                }
            }

            int nb = glassesBackgrounds.Count;
            for (int i = 0; i < nb; i++)
            {
                forGO = glassesBackgrounds.getAt(i);
                if (forGO.name.Contains(1.ToString()))
                {
                    glassesBG1 = forGO;
                }
                else if (forGO.name.Contains(2.ToString()))
                {
                    glassesBG2 = forGO;
                }
            }

            scrollUI = new Dictionary<string, GameObject>();
            nb = sUI.Count;
            for (int i = 0; i < nb; i++)
            {
                forGO = sUI.getAt(i);
                scrollUI.Add(forGO.name, forGO);
            }

            puzzleUI = new Dictionary<int, GameObject>();
            int id;
            nb = pui.Count;
            for (int i = 0; i < nb; i++)
            {
                forGO = pui.getAt(i);
                int.TryParse(forGO.name.Substring(forGO.name.Length - 2, 2), out id);
                puzzleUI.Add(id, forGO);
            }

            foreach (Transform child in pui.getAt(0).transform.parent)
            {
                if (child.gameObject.name == "RotationButtons")
                {
                    puzzleRotationButtons = child.gameObject;
                    foreach (Transform c in child)
                    {
                        if (c.name.Contains("Counter"))
                        {
                            c.gameObject.GetComponent<Button>().onClick.AddListener(delegate
                            {
                                RotatePuzzleCounterClockWise(selectedPuzzle);
                            });
                        }
                        else
                        {
                            c.gameObject.GetComponent<Button>().onClick.AddListener(delegate
                            {
                                RotatePuzzleClockWise(selectedPuzzle);
                            });
                        }
                    }
                }
            }

            foreach (Transform child in player.First().GetComponentInChildren<Camera>().gameObject.transform)
            {
                if (child.gameObject.GetComponent<Light>())
                {
                    blackLight = child.gameObject;
                }
            }

            inventoryElemGO = elemsInventory.First().transform.parent.gameObject;

            displayedBGMaterial = focusedMatFamily.First().GetComponent<FocusedGOMaterial>().displayedBGMaterial;
        }
    }

	// Use this to update member variables when system pause. 
	// Advice: avoid to update your families inside this function.
	protected override void onPause(int currentFrame) {
	}

	// Use this to update member variables when system resume.
	// Advice: avoid to update your families inside this function.
	protected override void onResume(int currentFrame){
	}

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {
        bool tryRaycast = Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit);
		int nbCGO = cGO.Count;

        if(wireOn && !localWireOn)
        {
            wireUI.SetActive(true);
        }
        else if(!wireOn && localWireOn)
        {
            wireUI.SetActive(false);
        }
        localWireOn = wireOn;

		for(int i = 0; i < nbCGO; i++)
        {
			forGO = cGO.getAt (i);
            bool tryGO;
            if (tryRaycast)
            {
                tryGO = (Object.ReferenceEquals(forGO, hit.transform.gameObject) && (forGO.transform.position - Camera.main.transform.position).magnitude < 7) || (forGO.GetComponent<RectTransform>() && forGO.GetComponent<PointerOver>());
            }
            else
            {
                tryGO = forGO.GetComponent<RectTransform>() && forGO.GetComponent<PointerOver>();
            }
			if (Input.GetMouseButtonDown(0) && tryGO)
            {
				if (forGO.GetComponent<CollectableGO>().goui)
                {
                    if (!CollectableGO.onInventory)
                    {
                        GameObjectManager.setGameObjectState(forGO.GetComponent<CollectableGO>().goui, true);
                        if (forGO.tag == "Puzzle")
                        {
                            int nb, id;
                            int.TryParse(forGO.name.Substring(forGO.name.Length - 2, 2), out nb);
                            nb++;
                            for(int j = 0; j < nb; j++)
                            {
                                idTable = new List<int>(puzzleUI.Keys);
                                id = idTable[(int)(Random.value * idTable.Count)];
                                puzzleUI.TryGetValue(id, out puzzlePiece);
                                if (puzzlePiece)
                                {
                                    puzzlePiece.SetActive(true);
                                    puzzleUI.Remove(id);
                                }
                                else
                                {
                                    Debug.Log(string.Concat("Puzzle piece ", id.ToString(), " doesn't exist."));
                                }
                            }
                        }
                        else if (forGO.tag == "Scroll")
                        {
                            scrollUI.TryGetValue(forGO.name[0].ToString(), out scroll);
                            if (scroll)
                            {
                                scroll.SetActive(true);
                            }
                            else
                            {
                                Debug.Log(string.Concat("Scroll ", forGO.name.Substring(0, 1), " doesn't exist."));
                            }
                        }
                        else if(forGO.name == "Wire")
                        {
                            foreach (Transform child in plank.First().transform)
                            {
                                if (child.gameObject.name == "SubTitles")
                                {
                                    child.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "Maintenant je dois utiliser la corde";
                                    break;
                                }
                            }
                            if (forGO.GetComponent<ComponentMonitoring>())
                            {
                                MonitoringManager.trace(forGO.GetComponent<ComponentMonitoring>(), "perform", MonitoringManager.Source.PLAYER);
                            }
                        }
                        else if(forGO.name == "Intro_Scroll")
                        {
                            int nb = animatedSprites.Count;
                            for(int j = 0; j < nb; j++)
                            {
                                if(animatedSprites.getAt(j).name == "AppuisA")
                                {
                                    animatedSprites.getAt(j).SetActive(true);
                                    break;
                                }
                            }
                        }
                        GameObjectManager.setGameObjectState(forGO, false);
                    }
                }
                else
                {
					if (forGO.name == "Glasses1")
                    {
                        if (CollectableGO.usingGlasses1)
                        {
                            forGO.GetComponent<AnimatedSprites>().animate = false;
                            CollectableGO.usingGlasses1 = false;
                            glassesBG1.SetActive(false);
                            if(Object.ReferenceEquals(forGO, displayedDescriptionGO))
                            {
                                if (selectedUI)
                                {
                                    displayedDescriptionGO = selectedUI;
                                    descriptionTitle.text = displayedDescriptionGO.GetComponent<CollectableGO>().itemName;
                                    descriptionText.text = displayedDescriptionGO.GetComponent<CollectableGO>().description;
                                }
                                else if (CollectableGO.usingGlasses2)
                                {
                                    displayedDescriptionGO = glassesUI2;
                                    descriptionTitle.text = displayedDescriptionGO.GetComponent<CollectableGO>().itemName;
                                    descriptionText.text = displayedDescriptionGO.GetComponent<CollectableGO>().description;
                                }
                                else
                                {
                                    displayedDescriptionGO = null;
                                    descriptionTitle.text = string.Empty;
                                    descriptionText.text = string.Empty;
                                }
                            }
                        }
                        else
                        {
                            forGO.GetComponent<AnimatedSprites>().animate = true;
                            CollectableGO.usingGlasses1 = true;
                            glassesBG1.SetActive(true);
                            displayedDescriptionGO = forGO;
                            descriptionTitle.text = displayedDescriptionGO.GetComponent<CollectableGO>().itemName;
                            descriptionText.text = displayedDescriptionGO.GetComponent<CollectableGO>().description;
                        }
                    }
                    else if (forGO.name == "Glasses2")
                    {
                        if (CollectableGO.usingGlasses2)
                        {
                            forGO.GetComponent<AnimatedSprites>().animate = false;
                            CollectableGO.usingGlasses2 = false;
                            glassesBG2.SetActive(false);
                            if (Object.ReferenceEquals(forGO, displayedDescriptionGO))
                            {
                                if (selectedUI)
                                {
                                    displayedDescriptionGO = selectedUI;
                                    descriptionTitle.text = displayedDescriptionGO.GetComponent<CollectableGO>().itemName;
                                    descriptionText.text = displayedDescriptionGO.GetComponent<CollectableGO>().description;
                                }
                                else if (CollectableGO.usingGlasses1)
                                {
                                    displayedDescriptionGO = glassesUI1;
                                    descriptionTitle.text = displayedDescriptionGO.GetComponent<CollectableGO>().itemName;
                                    descriptionText.text = displayedDescriptionGO.GetComponent<CollectableGO>().description;
                                }
                                else
                                {
                                    displayedDescriptionGO = null;
                                    descriptionTitle.text = string.Empty;
                                    descriptionText.text = string.Empty;
                                }
                            }
                        }
                        else
                        {
                            forGO.GetComponent<AnimatedSprites>().animate = true;
                            CollectableGO.usingGlasses2 = true;
                            glassesBG2.SetActive(true);
                            displayedDescriptionGO = forGO;
                            descriptionTitle.text = displayedDescriptionGO.GetComponent<CollectableGO>().itemName;
                            descriptionText.text = displayedDescriptionGO.GetComponent<CollectableGO>().description;
                        }
                    }
                    else
                    {
                        CollectableGO.usingWire = false;
                        CollectableGO.usingKeyE03 = false;
						CollectableGO.usingKeyE08 = false;
						CollectableGO.usingLamp = false;
						blackLight.SetActive (false);
                        
						if (displayer.activeSelf && Object.ReferenceEquals(forGO, selectedUI))
                        {
                            if (displayedElement)
                            {
                                displayedElement.SetActive(false);
                                if (CollectableGO.onInventory)
                                {
                                    int nbElems = elemsInventory.Count;
                                    for (int j = 0; j < nbElems; j++)
                                    {
                                        elemsInventory.getAt(j).GetComponent<RectTransform>().localPosition = elemsInventory.getAt(j).GetComponent<CollectableGO>().positionMiddle;
                                    }
                                    inventoryElemGO.GetComponent<RectTransform>().localPosition = new Vector3(-340, -20, 0);
                                }
                            }
                            displayer.SetActive(false);
                            if (selectedUI)
                            {
                                selectedUI.GetComponent<AnimatedSprites>().animate = false;
                            }
                            selectedUI = null;
                            descriptionTitle.transform.parent.gameObject.SetActive(true);
                            if (Object.ReferenceEquals(forGO, displayedDescriptionGO))
                            {
                                if (CollectableGO.usingGlasses1)
                                {
                                    displayedDescriptionGO = glassesUI1;
                                    descriptionTitle.text = displayedDescriptionGO.GetComponent<CollectableGO>().itemName;
                                    descriptionText.text = displayedDescriptionGO.GetComponent<CollectableGO>().description;
                                }
                                else if (CollectableGO.usingGlasses2)
                                {
                                    displayedDescriptionGO = glassesUI2;
                                    descriptionTitle.text = displayedDescriptionGO.GetComponent<CollectableGO>().itemName;
                                    descriptionText.text = displayedDescriptionGO.GetComponent<CollectableGO>().description;
                                }
                                else
                                {
                                    displayedDescriptionGO = null;
                                    descriptionTitle.text = string.Empty;
                                    descriptionText.text = string.Empty;
                                }
                            }
                            if(forGO.GetComponent<ComponentMonitoring>())
                            {
                                MonitoringManager.trace(forGO.GetComponent<ComponentMonitoring>(), "turnOff", MonitoringManager.Source.PLAYER);
                            }
                        }
                        else
                        {
                            if (selectedUI)
                            {
                                selectedUI.GetComponent<AnimatedSprites>().animate = false;
                                if (selectedUI.GetComponent<ComponentMonitoring>())
                                {
                                    MonitoringManager.trace(selectedUI.GetComponent<ComponentMonitoring>(), "turnOff", MonitoringManager.Source.SYSTEM);
                                }
                            }
                            if (forGO.GetComponent<ComponentMonitoring>())
                            {
                                MonitoringManager.trace(forGO.GetComponent<ComponentMonitoring>(), "turnOn", MonitoringManager.Source.PLAYER);
                            }
                            selectedUI = forGO;
                            selectedUI.GetComponent<AnimatedSprites>().animate = true;
                            displayedDescriptionGO = forGO;
                            descriptionTitle.text = displayedDescriptionGO.GetComponent<CollectableGO>().itemName;
                            descriptionText.text = displayedDescriptionGO.GetComponent<CollectableGO>().description;
                            if (displayedElement)
                            {
                                displayedElement.SetActive(false);
                            }
                            displayer.SetActive(true);
							switch (forGO.name)
                            {
							    case "Syllabus":
								    bool elem1 = false;
								    bool elem2 = false;
									int nbSyllabus = syllabusElems.Count;
									for(int j = 0; j< nbSyllabus; j++)	
                                    {
										forGO2 = syllabusElems.getAt (j);
										if (forGO2.name.Contains(1.ToString()))
                                        {
											elem1 = !forGO2.activeSelf;
                                        }
										else if (forGO2.name.Contains(2.ToString()))
                                        {
											elem2 = !forGO2.activeSelf;
                                        }
                                    }
                                    if (elem1 && elem2)
                                    {
                                        foreach (Transform child in displayer.transform)
                                        {
                                            if (child.gameObject.name == "Syllabus_Complete")
                                            {
                                                displayedElement = child.gameObject;
                                                displayedElement.SetActive(true);
                                            }
                                        }
                                    }
                                    else if (elem1)
                                    {
                                        foreach (Transform child in displayer.transform)
                                        {
                                            if (child.gameObject.name == "Syllabus_Half1")
                                            {
                                                displayedElement = child.gameObject;
                                                displayedElement.SetActive(true);
                                            }
                                        }
                                    }
                                    else if (elem2)
                                    {
                                        foreach (Transform child in displayer.transform)
                                        {
                                            if (child.gameObject.name == "Syllabus_Half2")
                                            {
                                                displayedElement = child.gameObject;
                                                displayedElement.SetActive(true);
                                            }
                                        }
                                    }
                                    break;

                                case "ScrollIntro":
                                    foreach (Transform child in displayer.transform)
                                    {
                                        if (child.gameObject.name == "ScrollIntro")
                                        {
                                            displayedElement = child.gameObject;
                                            displayedElement.SetActive(true);
                                        }
                                    }
                                    break;

                                case "Wire":
                                    displayedElement = null;
                                    CollectableGO.usingWire = true;
                                    break;

                                case "KeyE03":
                                    displayedElement = null;
                                    CollectableGO.usingKeyE03 = true;
                                    break;

                                case "KeyE08":
                                    displayedElement = null;
                                    CollectableGO.usingKeyE08 = true;
                                    break;

                                case "TipE07":
                                    foreach (Transform child in displayer.transform)
                                    {
                                        if (child.gameObject.name == "Tip_E07")
                                        {
                                            displayedElement = child.gameObject;
                                            displayedElement.SetActive(true);
                                        }
                                    }
                                    break;

                                case "Scroll":
                                    foreach (Transform child in displayer.transform)
                                    {
                                        if (child.gameObject.name == "SMART")
                                        {
                                            displayedElement = child.gameObject;
                                            displayedElement.SetActive(true);
                                        }
                                    }
                                    break;

                                case "Puzzle":
                                    foreach (Transform child in displayer.transform)
                                    {
                                        if (child.gameObject.name == "Puzzles")
                                        {
                                            displayedElement = child.gameObject;
                                            displayedElement.SetActive(true);
                                            onPuzzle = true;
                                        }
                                    }
                                    break;

                                case "Lamp":
									displayedElement = null;
									CollectableGO.usingLamp = true;
									blackLight.SetActive (true);
									break;

								case "CodeE12":
									foreach (Transform child in displayer.transform)
									{
										if (child.gameObject.name == "Code_E12")
										{
											displayedElement = child.gameObject;
											displayedElement.SetActive(true);
										}
									}
								break;

								case "AlphabetE13":
									foreach (Transform child in displayer.transform)
									{
									if (child.gameObject.name == "Alphabet_E13")
										{
											displayedElement = child.gameObject;
											displayedElement.SetActive(true);
										}
									}
									break;

                                default:
                                    break;
                            }
                            if (CollectableGO.onInventory)
                            {
                                if (displayedElement)
                                {
                                    int nbElems = elemsInventory.Count;
                                    for (int j = 0; j < nbElems; j++)
                                    {
                                        elemsInventory.getAt(j).GetComponent<RectTransform>().localPosition = elemsInventory.getAt(j).GetComponent<CollectableGO>().positionDown;
                                    }
                                    inventoryElemGO.GetComponent<RectTransform>().localPosition = new Vector3(572, -335, 0);
                                    descriptionTitle.transform.parent.gameObject.SetActive(false);
                                }
                                else
                                {
                                    int nbElems = elemsInventory.Count;
                                    for (int j = 0; j < nbElems; j++)
                                    {
                                        elemsInventory.getAt(j).GetComponent<RectTransform>().localPosition = elemsInventory.getAt(j).GetComponent<CollectableGO>().positionMiddle;
                                    }
                                    inventoryElemGO.GetComponent<RectTransform>().localPosition = new Vector3(-340, -20, 0);
                                    descriptionTitle.transform.parent.gameObject.SetActive(true);
                                }
                            }
                        }
                    }
                }
            }
        }

        if(displayedBackground.Count > 0 && !backgroundTextureSet)
        {
            //pause shader
            displayedBGMaterial.mainTexture = displayedBackground.First().GetComponent<RenderTexture>();
            int nb = backgrounds.Count;
            for(int i = 0; i < nb; i++)
            {
                //backgrounds.getAt(i).GetComponent<Image>().material = displayedBGMaterial;
            }
        }
        else if(displayedBackground.Count == 0 && backgroundTextureSet)
        {
            backgroundTextureSet = false;
        }

		if (Input.GetKeyDown (KeyCode.A) || CollectableGO.askOpenInventory) {
            if (!CollectableGO.askOpenInventory)
            {
                inputfieldFocused = false;
                int nbInputFields = inputfields.Count;
                for (int i = 0; i < nbInputFields; i++)
                {
                    forGO = inputfields.getAt(i);
                    if (forGO.GetComponent<InputField>().isFocused)
                    {
                        inputfieldFocused = true;
                        break;
                    }
                }
            }
			if (!inputfieldFocused || CollectableGO.askOpenInventory)
            {
                CollectableGO.askOpenInventory = false;
				if (CollectableGO.onInventory) {
					CloseInventory ();
				} else
                {
                    GameObjectManager.setGameObjectState(inventory.First(), true);
                    foreach (Transform child in inventory.First().transform) {
						if (child.gameObject.name == "Enabled") {
							child.gameObject.SetActive (true);
						}
					}
					playerEnabled = player.First ().GetComponent<FirstPersonController> ().enabled;
					player.First ().GetComponent<FirstPersonController> ().enabled = false;
					Cursor.lockState = CursorLockMode.None;
					Cursor.lockState = CursorLockMode.Confined;
					Cursor.visible = true;
					int nb = ui.Count;
					for (int i = 0; i < nb; i++) {
						forGO = ui.getAt (i);
						if (forGO.name == "Cursor") {
							forGO.SetActive (false);
						}
					}
					nb = elemsInventory.Count;
                    for (int j = 0; j < nb; j++)
                    {
                        elemsInventory.getAt(j).GetComponent<RectTransform>().localPosition = elemsInventory.getAt(j).GetComponent<CollectableGO>().positionMiddle;
                    }
                    inventoryElemGO.GetComponent<RectTransform>().localPosition = new Vector3(-340, -20, 0);
                    descriptionTitle.transform.parent.gameObject.SetActive(true);
                    if (selectedUI) {
                        descriptionTitle.text = displayedDescriptionGO.GetComponent<CollectableGO>().itemName;
                        descriptionText.text = displayedDescriptionGO.GetComponent<CollectableGO>().description;
                    }
                    else
                    {
                        if (CollectableGO.usingGlasses1)
                        {
                            displayedDescriptionGO = glassesUI1;
                            descriptionTitle.text = displayedDescriptionGO.GetComponent<CollectableGO>().itemName;
                            descriptionText.text = displayedDescriptionGO.GetComponent<CollectableGO>().description;
                        }
                        else if (CollectableGO.usingGlasses2)
                        {
                            displayedDescriptionGO = glassesUI2;
                            descriptionTitle.text = displayedDescriptionGO.GetComponent<CollectableGO>().itemName;
                            descriptionText.text = displayedDescriptionGO.GetComponent<CollectableGO>().description;
                        }
                        else
                        {
                            displayedDescriptionGO = null;
                            descriptionTitle.text = string.Empty;
                            descriptionText.text = string.Empty;
                        }
                    }
                    onPuzzle = false;
					CollectableGO.onInventory = true;
				}
			}
		}
        if (onPuzzle)
        {
            if (draggedPuzzle)
            {
                if (Input.GetMouseButtonUp(0))
                {
                    if(!(draggedPuzzle.GetComponent<RectTransform>().position.x >0 && draggedPuzzle.GetComponent<RectTransform>().position.x < Camera.main.pixelWidth && draggedPuzzle.GetComponent<RectTransform>().position.y > 0 && draggedPuzzle.GetComponent<RectTransform>().position.y < Camera.main.pixelHeight))
                    {
                        draggedPuzzle.GetComponent<RectTransform>().position = posBeforeDrag;
                    }
                    draggedPuzzle = null;
                    //puzzleRotationButtons.SetActive(true);
                    puzzleRotationButtons.GetComponent<RectTransform>().localPosition = selectedPuzzle.GetComponent<RectTransform>().localPosition;
                }
                else
                {
                    if (Input.GetKeyDown(KeyCode.Q))
                    {
                        RotatePuzzleCounterClockWise(draggedPuzzle);
                    }
                    if (Input.GetKeyDown(KeyCode.D))
                    {
                        RotatePuzzleClockWise(draggedPuzzle);
                    }
                    draggedPuzzle.GetComponent<RectTransform>().position = Input.mousePosition - posFromMouse;
                }
            }
            else
            {
				int nbPuzzle = pui.Count;
				for(int i = 0; i < nbPuzzle; i++)
                {
					forGO = pui.getAt (i);
					if (forGO.GetComponent<PointerOver>() && Input.GetMouseButtonDown(0))
                    {
                        puzzleRotationButtons.SetActive(false);
                        if (selectedPuzzle)
                        {
                            selectedPuzzle.GetComponent<Image>().color = Color.white;
                        }
						draggedPuzzle = forGO;
                        selectedPuzzle = draggedPuzzle;
                        selectedPuzzle.GetComponent<Image>().color = Color.yellow;
                        posBeforeDrag = draggedPuzzle.GetComponent<RectTransform>().position;
                        posFromMouse = Input.mousePosition - draggedPuzzle.GetComponent<RectTransform>().position;
                        if (Input.GetMouseButtonUp(0))
                        {
                            draggedPuzzle = null;
                            //puzzleRotationButtons.SetActive(true);
                            puzzleRotationButtons.GetComponent<RectTransform>().localPosition = selectedPuzzle.GetComponent<RectTransform>().localPosition;
                        }
                    }
                }
            }
        }
        if (CollectableGO.askCloseInventory)
        {
            CloseInventory();
        }
	}

    private void CloseInventory()
    {
        inventoryElemGO.GetComponent<RectTransform>().localPosition = new Vector3(-340, -20, 0);
        if (displayedElement)
        {
            if (onPuzzle && draggedPuzzle) {
                draggedPuzzle.GetComponent<RectTransform>().position = posBeforeDrag;
                draggedPuzzle = null;
            }
            displayedElement.SetActive(false);
            displayer.SetActive(false);
            if (selectedUI)
            {
                selectedUI.GetComponent<AnimatedSprites>().animate = false;
            }
            selectedUI = null;
        }
        foreach (Transform child in inventory.First().transform)
        {
            if (child.gameObject.name == "Enabled")
            {
                child.gameObject.SetActive(false);
            }
        }
        if (playerEnabled)
        {
            player.First().GetComponent<FirstPersonController>().enabled = true;
            Cursor.lockState = CursorLockMode.None;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
			int nbCanvas = ui.Count;
			for(int i = 0; i < nbCanvas; i++)
            {
				forGO = ui.getAt (i);
				if (forGO.name == "Cursor")
                {
					forGO.SetActive(true);
                }
            }
        }
        if (selectedPuzzle)
        {
            selectedPuzzle.GetComponent<Image>().color = Color.white;
            puzzleRotationButtons.SetActive(false);
        }
        selectedPuzzle = null;
        descriptionTitle.text = string.Empty;
        descriptionText.text = string.Empty;
        CollectableGO.onInventory = false;
        CollectableGO.askCloseInventory = false;
        GameObjectManager.setGameObjectState(inventory.First(), false);
    }

    private void RotatePuzzleClockWise(GameObject puzzle)
    {
        //puzzle.GetComponent<RectTransform>().localRotation = Quaternion.Euler(puzzle.GetComponent<RectTransform>().localRotation.eulerAngles.x, puzzle.GetComponent<RectTransform>().localRotation.eulerAngles.y, puzzle.GetComponent<RectTransform>().localRotation.eulerAngles.z - 90);
    }

    private void RotatePuzzleCounterClockWise(GameObject puzzle)
    {
        //puzzle.GetComponent<RectTransform>().localRotation = Quaternion.Euler(puzzle.GetComponent<RectTransform>().localRotation.eulerAngles.x, puzzle.GetComponent<RectTransform>().localRotation.eulerAngles.y, puzzle.GetComponent<RectTransform>().localRotation.eulerAngles.z + 90);
    }
}