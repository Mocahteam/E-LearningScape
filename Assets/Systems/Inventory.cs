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
    private Family hud = FamilyManager.getFamily(new AnyOfTags("HUDInputs"));
    private Family newFeedback = FamilyManager.getFamily(new AnyOfTags("NewItemFeedback"));

    public static bool playerEnabled = true;
    private GameObject displayer;
    private GameObject displayedElement;
    private GameObject selectedUI;
    private GameObject inventoryElemGO;
    private TextMeshProUGUI descriptionTitle;
    private TextMeshProUGUI descriptionText;
    private string previousDescriptionTitle = "";
    private string previousDescriptionText = "";
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
    private Camera backgroundTextureCamera;

    private bool pointerOverUIItem = false;
    private GameObject pointerOverGO = null;
    private bool previousDescriptionTextState = true;

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
                    GameObjectManager.setGameObjectState(child.gameObject,false);
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
            foreach(Camera c in Camera.main.gameObject.GetComponentsInChildren<Camera>())
            {
                if(c.name == "BGCamera")
                {
                    backgroundTextureCamera = c;
                    break;
                }
            }
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
            GameObjectManager.setGameObjectState(wireUI,true);
        }
        else if(!wireOn && localWireOn)
        {
            GameObjectManager.setGameObjectState(wireUI,false);
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
            if (pointerOverUIItem)
            {
                if (!pointerOverGO.GetComponent<PointerOver>())
                {
                    pointerOverGO = null;
                    pointerOverUIItem = false;
                    descriptionTitle.text = previousDescriptionTitle;
                    descriptionText.text = previousDescriptionText;
                    GameObjectManager.setGameObjectState(descriptionText.gameObject, previousDescriptionTextState);
                    if (displayedElement)
                    {
                        GameObjectManager.setGameObjectState(displayedElement, true);
                        GameObjectManager.setGameObjectState(descriptionText.gameObject, false);
                    }
                }
            }
            if(forGO.GetComponent<RectTransform>() && forGO.GetComponent<PointerOver>())
            {
                if(!Object.ReferenceEquals(forGO, selectedUI))
                {
                    if (displayedElement)
                    {
                        GameObjectManager.setGameObjectState(displayedElement, false);
                        GameObjectManager.setGameObjectState(descriptionText.gameObject, true);
                    }
                    pointerOverUIItem = true;
                    pointerOverGO = forGO;
                    previousDescriptionTextState = descriptionText.gameObject.activeSelf;
                    descriptionTitle.text = forGO.GetComponent<CollectableGO>().itemName;
                    descriptionText.text = forGO.GetComponent<CollectableGO>().description;
                }
                int nb = newFeedback.Count;
                for(int j = 0; j < nb; j++)
                {
                    forGO2 = newFeedback.getAt(j);
                    if (Object.ReferenceEquals(forGO, forGO2.transform.parent.gameObject))
                    {
                        GameObjectManager.setGameObjectState(forGO2,false);
                        break;
                    }
                }
            }
			if (Input.GetMouseButtonDown(0) && tryGO)
            {
                pointerOverUIItem = false;
				if (forGO.GetComponent<CollectableGO>().goui)
                {
                    if (!CollectableGO.onInventory)
                    {
                        GameObjectManager.setGameObjectState(forGO.GetComponent<CollectableGO>().goui, true);
                        int nbNewFeedback = newFeedback.Count;
                        for(int j = 0; j < nbNewFeedback; j++)
                        {
                            forGO2 = newFeedback.getAt(j);
                            if(Object.ReferenceEquals(forGO.GetComponent<CollectableGO>().goui, forGO2.transform.parent.gameObject))
                            {
                                GameObjectManager.setGameObjectState(forGO2,true);
                                break;
                            }
                        }
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
                                    GameObjectManager.setGameObjectState(puzzlePiece,true);
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
                                GameObjectManager.setGameObjectState(scroll,true);
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
                                    GameObjectManager.setGameObjectState(animatedSprites.getAt(j),true);
                                    break;
                                }
                            }
                            foreach (Transform child in hud.First().transform)
                            {
                                if (child.gameObject.name == "Inventory")
                                {
                                    GameObjectManager.setGameObjectState(child.gameObject,true);
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
                            GameObjectManager.setGameObjectState(glassesBG1,false);
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
                            GameObjectManager.setGameObjectState(glassesBG1,true);
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
                            GameObjectManager.setGameObjectState(glassesBG2,false);
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
                            GameObjectManager.setGameObjectState(glassesBG2,true);
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
                        GameObjectManager.setGameObjectState(blackLight,false);
                        
						if (displayer.activeSelf && Object.ReferenceEquals(forGO, selectedUI))
                        {
                            if (displayedElement)
                            {
                                GameObjectManager.setGameObjectState(displayedElement,false);
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
                            displayedElement = null;
                            GameObjectManager.setGameObjectState(displayer,false);
                            if (selectedUI)
                            {
                                selectedUI.GetComponent<AnimatedSprites>().animate = false;
                            }
                            selectedUI = null;
                            GameObjectManager.setGameObjectState(descriptionTitle.transform.parent.gameObject,true);
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
                                GameObjectManager.setGameObjectState(displayedElement,false);
                            }
                            GameObjectManager.setGameObjectState(displayer,true);
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
                                                GameObjectManager.setGameObjectState(displayedElement,true);
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
                                                GameObjectManager.setGameObjectState(displayedElement,true);
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
                                                GameObjectManager.setGameObjectState(displayedElement,true);
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
                                            GameObjectManager.setGameObjectState(displayedElement,true);
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
                                            GameObjectManager.setGameObjectState(displayedElement,true);
                                        }
                                    }
                                    break;

                                case "Scroll":
                                    foreach (Transform child in displayer.transform)
                                    {
                                        if (child.gameObject.name == "SMART")
                                        {
                                            displayedElement = child.gameObject;
                                            GameObjectManager.setGameObjectState(displayedElement,true);
                                        }
                                    }
                                    break;

                                case "Puzzle":
                                    foreach (Transform child in displayer.transform)
                                    {
                                        if (child.gameObject.name == "Puzzles")
                                        {
                                            displayedElement = child.gameObject;
                                            GameObjectManager.setGameObjectState(displayedElement,true);
                                            onPuzzle = true;
                                        }
                                    }
                                    break;

                                case "Lamp":
									displayedElement = null;
									CollectableGO.usingLamp = true;
                                    GameObjectManager.setGameObjectState(blackLight,true);
									break;

								case "CodeE12":
									foreach (Transform child in displayer.transform)
									{
										if (child.gameObject.name == "Code_E12")
										{
											displayedElement = child.gameObject;
                                            GameObjectManager.setGameObjectState(displayedElement,true);
										}
									}
								break;

								case "AlphabetE13":
									foreach (Transform child in displayer.transform)
									{
									if (child.gameObject.name == "Alphabet_E13")
										{
											displayedElement = child.gameObject;
                                            GameObjectManager.setGameObjectState(displayedElement,true);
										}
									}
									break;

                                default:
                                    break;
                            }
                            if (CollectableGO.onInventory)
                            {
                                if (displayedElement && onPuzzle)
                                {
                                    int nbElems = elemsInventory.Count;
                                    for (int j = 0; j < nbElems; j++)
                                    {
                                        elemsInventory.getAt(j).GetComponent<RectTransform>().localPosition = elemsInventory.getAt(j).GetComponent<CollectableGO>().positionDown;
                                    }
                                    inventoryElemGO.GetComponent<RectTransform>().localPosition = new Vector3(572, -335, 0);
                                    GameObjectManager.setGameObjectState(descriptionTitle.transform.parent.gameObject,false);
                                }
                                else
                                {
                                    int nbElems = elemsInventory.Count;
                                    for (int j = 0; j < nbElems; j++)
                                    {
                                        elemsInventory.getAt(j).GetComponent<RectTransform>().localPosition = elemsInventory.getAt(j).GetComponent<CollectableGO>().positionMiddle;
                                    }
                                    inventoryElemGO.GetComponent<RectTransform>().localPosition = new Vector3(-340, -20, 0);
                                    GameObjectManager.setGameObjectState(descriptionTitle.transform.parent.gameObject,true);
                                }
                            }
                        }
                    }
                    previousDescriptionTitle = descriptionTitle.text;
                    previousDescriptionText = descriptionText.text;
                }
                GameObjectManager.setGameObjectState(descriptionText.gameObject, displayedElement == null);
            }
        }
        
        if (displayedBackground.Count > 0 && /*!*/backgroundTextureSet)
        {
            backgroundTextureSet = true;
            backgroundTextureCamera.enabled = false;
            displayedBGMaterial.mainTexture = backgroundTextureCamera.targetTexture;
            int nb = backgrounds.Count;
            for(int i = 0; i < nb; i++)
            {
                backgrounds.getAt(i).GetComponent<Image>().material = displayedBGMaterial;
            }
        }
        else if(displayedBackground.Count == 0 && backgroundTextureSet)
        {
            backgroundTextureCamera.enabled = true;
            backgroundTextureSet = false;
        }

		if ((Input.GetKeyDown (KeyCode.A) || CollectableGO.askOpenInventory) && !StoryDisplaying.reading && !DreamFragmentCollect.onFragment) {
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
				if (!CollectableGO.onInventory)
                {
                    GameObjectManager.setGameObjectState(inventory.First(), true);
                    foreach (Transform child in inventory.First().transform) {
						if (child.gameObject.name == "Enabled")
                        {
                            GameObjectManager.setGameObjectState(child.gameObject, true);
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
                            GameObjectManager.setGameObjectState(forGO,false);
						}
					}
					nb = elemsInventory.Count;
                    for (int j = 0; j < nb; j++)
                    {
                        elemsInventory.getAt(j).GetComponent<RectTransform>().localPosition = elemsInventory.getAt(j).GetComponent<CollectableGO>().positionMiddle;
                    }
                    inventoryElemGO.GetComponent<RectTransform>().localPosition = new Vector3(-340, -20, 0);
                    GameObjectManager.setGameObjectState(descriptionTitle.transform.parent.gameObject,true);
                    if (selectedUI) {
                        if (selectedUI.activeSelf)
                        {
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
                    previousDescriptionTitle = descriptionTitle.text;
                    previousDescriptionText = descriptionText.text;
                    GameObjectManager.setGameObjectState(descriptionText.gameObject, true);
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
                    //GameObjectManager.setGameObjectState(puzzleRotationButtons,true);
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
                        GameObjectManager.setGameObjectState(puzzleRotationButtons,false);
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
                            //GameObjectManager.setGameObjectState(puzzleRotationButtons,true);
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
            GameObjectManager.setGameObjectState(displayedElement,false);
            GameObjectManager.setGameObjectState(displayer,false);
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
                GameObjectManager.setGameObjectState(child.gameObject, false);
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
                    GameObjectManager.setGameObjectState(forGO,true);
                }
            }
        }
        if (selectedPuzzle)
        {
            selectedPuzzle.GetComponent<Image>().color = Color.white;
            GameObjectManager.setGameObjectState(puzzleRotationButtons,false);
        }
        selectedPuzzle = null;
        descriptionTitle.text = string.Empty;
        descriptionText.text = string.Empty;
        previousDescriptionTitle = descriptionTitle.text;
        previousDescriptionText = descriptionText.text;
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