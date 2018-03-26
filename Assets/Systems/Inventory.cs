using UnityEngine;
using FYFY;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;
using FYFY_plugins.PointerManager;

public class Inventory : FSystem {

    private Family inventory = FamilyManager.getFamily(new AnyOfTags("Inventory"));
    private Family player = FamilyManager.getFamily(new AnyOfTags("Player"));
    private Family cGO = FamilyManager.getFamily(new AllOfComponents(typeof(CollectableGO)), new AllOfProperties(PropertyMatcher.PROPERTY.ENABLED));
    private Family ui = FamilyManager.getFamily(new AllOfComponents(typeof(Canvas)));
    private Family syllabusElems = FamilyManager.getFamily(new AnyOfTags("Syllabus"), new AllOfComponents(typeof(CollectableGO)), new NoneOfComponents(typeof(Image)));
    private Family inputfields = FamilyManager.getFamily(new AllOfComponents(typeof(InputField)));

    private bool playerEnabled = true;
    private GameObject displayer;
    private GameObject displayedElement;
    private GameObject selection;
    private GameObject selectedUI;

    private GameObject glassesBG;
    private GameObject glassesUI;
    private GameObject glassesSelected;

    private RaycastHit hit;
    private bool inputfieldFocused;

    public Inventory()
    {
        foreach (Transform child in inventory.First().transform)
        {
            if(child.gameObject.name == "Enabled")
            {
                foreach(Transform c in child)
                {
                    if(c.gameObject.name == "CloseButton")
                    {
                        //initialise button with listener
                        c.gameObject.GetComponent<Button>().onClick.AddListener(CloseInventory);
                    }
                }
                child.gameObject.SetActive(false);
            }
            else if(child.gameObject.name == "Display")
            {
                displayer = child.gameObject;
            }
            else if (child.gameObject.name == "Selected")
            {
                selection = child.gameObject;
            }
            else if (child.gameObject.name == "Glasses")
            {
                glassesUI = child.gameObject;
            }
            else if (child.gameObject.name == "GlassesBackground")
            {
                glassesBG = child.gameObject;
            }
            else if (child.gameObject.name == "GlassesSelected")
            {
                glassesSelected = child.gameObject;
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
        Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit);
        foreach (GameObject go in cGO)
        {
            if ((Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)) && (Object.ReferenceEquals(go, hit.transform.gameObject) || (go.GetComponent<RectTransform>() && go.GetComponent<PointerOver>())))
            {
                if (go.GetComponent<CollectableGO>().goui)
                {
                    GameObjectManager.setGameObjectState(go.GetComponent<CollectableGO>().goui, true);
                    GameObjectManager.setGameObjectState(go, false);
                }
                else
                {
                    if (go.name == "Glasses")
                    {
                        if (CollectableGO.usingGlasses)
                        {
                            CollectableGO.usingGlasses = false;
                            glassesSelected.SetActive(false);
                            glassesBG.SetActive(false);
                        }
                        else
                        {
                            glassesSelected.SetActive(true);
                            glassesSelected.GetComponent<RectTransform>().localPosition = glassesUI.GetComponent<RectTransform>().localPosition;
                            CollectableGO.usingGlasses = true;
                            glassesBG.SetActive(true);
                        }
                    }
                    else
                    {
                        CollectableGO.usingWire = false;
                        CollectableGO.usingKeyE03 = false;
                        CollectableGO.usingKeyE08 = false;

                        selection.SetActive(true);
                        selection.GetComponent<RectTransform>().localPosition = go.GetComponent<RectTransform>().localPosition;
                        if (displayer.activeSelf && Object.ReferenceEquals(go, selectedUI))
                        {
                            if (displayedElement)
                            {
                                displayedElement.SetActive(false);
                            }
                            displayer.SetActive(false);
                            selection.SetActive(false);
                            selectedUI = null;
                        }
                        else
                        {
                            selectedUI = go;
                            if (displayedElement)
                            {
                                displayedElement.SetActive(false);
                            }
                            displayer.SetActive(true);
                            switch (go.name)
                            {
                                case "Syllabus":
                                    bool elem1 = false;
                                    bool elem2 = false;
                                    foreach (GameObject elem in syllabusElems)
                                    {
                                        if (elem.name.Contains(1.ToString()))
                                        {
                                            elem1 = !elem.activeSelf;
                                        }
                                        else if (elem.name.Contains(2.ToString()))
                                        {
                                            elem2 = !elem.activeSelf;
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

                                default:
                                    break;
                            }
                        }
                    }
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            inputfieldFocused = false;
            foreach (GameObject go in inputfields)
            {
                if (go.GetComponent<InputField>().isFocused && go.GetComponent<InputField>().contentType == InputField.ContentType.Standard)
                {
                    inputfieldFocused = true;
                    break;
                }
            }
            if (!inputfieldFocused)
            {
                if (CollectableGO.onInventory)
                {
                    CloseInventory();
                }
                else
                {
                    foreach (Transform child in inventory.First().transform)
                    {
                        if (child.gameObject.name == "Enabled")
                        {
                            child.gameObject.SetActive(true);
                        }
                    }
                    playerEnabled = player.First().GetComponent<FirstPersonController>().enabled;
                    player.First().GetComponent<FirstPersonController>().enabled = false;
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.lockState = CursorLockMode.Confined;
                    Cursor.visible = true;
                    foreach (GameObject canvas in ui)
                    {
                        if (canvas.name == "Cursor")
                        {
                            canvas.SetActive(false);
                        }
                    }
                    CollectableGO.onInventory = true;
                }
            }
        }
	}

    private void CloseInventory()
    {
        if (displayedElement)
        {
            displayedElement.SetActive(false);
            displayer.SetActive(false);
            selection.SetActive(false);
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
            foreach (GameObject canvas in ui)
            {
                if (canvas.name == "Cursor")
                {
                    canvas.SetActive(true);
                }
            }
        }
        CollectableGO.onInventory = false;
    }
}