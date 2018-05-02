using UnityEngine;
using FYFY;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;
using FYFY_plugins.PointerManager;
using System.Collections.Generic;

public class Inventory : FSystem {

    private Family inventory = FamilyManager.getFamily(new AnyOfTags("Inventory"));
    private Family player = FamilyManager.getFamily(new AnyOfTags("Player"));
    private Family cGO = FamilyManager.getFamily(new AllOfComponents(typeof(CollectableGO)), new AllOfProperties(PropertyMatcher.PROPERTY.ENABLED));
    private Family ui = FamilyManager.getFamily(new AllOfComponents(typeof(Canvas)));
    private Family syllabusElems = FamilyManager.getFamily(new AnyOfTags("Syllabus"), new AllOfComponents(typeof(CollectableGO)), new NoneOfComponents(typeof(Image)));
    private Family inputfields = FamilyManager.getFamily(new AllOfComponents(typeof(InputField)));
    private Family pui = FamilyManager.getFamily(new AnyOfTags("PuzzleUI"));
    private Family elemsInventory = FamilyManager.getFamily(new AnyOfTags("InventoryElements"));
    private Family onElem = FamilyManager.getFamily(new AnyOfTags("InventoryElements", "PuzzleUI"), new AllOfComponents(typeof(PointerOver)));

    private bool playerEnabled = true;
    private GameObject displayer;
    private GameObject displayedElement;
    private bool displayedElementWasNull = true;
    private GameObject selection;
    private GameObject selectedUI;

    private GameObject glassesBG;
    private GameObject glassesUI;
    private GameObject glassesSelected;

    private RaycastHit hit;
    private bool inputfieldFocused;

    private Dictionary<int, GameObject> puzzleUI;
    private GameObject puzzlePiece;
    private bool onPuzzle = false;
    private Vector3 posBeforeDrag;
    private GameObject draggedPuzzle = null;
    private Vector3 posFromMouse;

	private GameObject blackLight;

	//tmp variables used to loop in famillies
	private GameObject forGO;
	private GameObject forGO2;

    public Inventory()
    {
        foreach (Transform child in inventory.First().transform)
        {
            if(child.gameObject.name == "Enabled")
            {
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

        puzzleUI = new Dictionary<int, GameObject>();
        int id;
		int forCount = pui.Count;
		for(int i = 0; i < forCount; i++)
        {
			forGO = pui.getAt (i);
			int.TryParse(forGO.name.Substring(forGO.name.Length - 2, 2), out id);
			puzzleUI.Add(id, forGO);
        }

		foreach (Transform child in player.First().GetComponentInChildren<Camera>().gameObject.transform) {
			if (child.gameObject.GetComponent<Light> ()) {
				blackLight = child.gameObject;
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
		for(int i = 0; i < nbCGO; i++)
        {
			forGO = cGO.getAt (i);
            bool tryGO;
            if (tryRaycast)
            {
                tryGO = Object.ReferenceEquals(forGO, hit.transform.gameObject) || (forGO.GetComponent<RectTransform>() && forGO.GetComponent<PointerOver>());
            }
            else
            {
                tryGO = forGO.GetComponent<RectTransform>() && forGO.GetComponent<PointerOver>();
            }
			if ((Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)) && tryGO)
            {
				if (forGO.GetComponent<CollectableGO>().goui)
                {
                    if (!CollectableGO.onInventory)
                    {
                        GameObjectManager.setGameObjectState(forGO.GetComponent<CollectableGO>().goui, true);
                        if (forGO.tag == "Puzzle")
                        {
                            int id;
                            int.TryParse(forGO.name.Substring(forGO.name.Length - 2, 2), out id);
                            puzzleUI.TryGetValue(id, out puzzlePiece);
                            if (puzzlePiece)
                            {
                                puzzlePiece.SetActive(true);
                            }
                            else
                            {
                                Debug.Log(string.Concat("Puzzle piece ", id.ToString(), " doesn't exist."));
                            }
                        }
                        GameObjectManager.setGameObjectState(forGO, false);
                    }
                }
                else
                {
					if (forGO.name == "Glasses")
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
						CollectableGO.usingLamp = false;
						blackLight.SetActive (false);

                        selection.SetActive(true);
						selection.GetComponent<RectTransform>().localPosition = forGO.GetComponent<RectTransform>().localPosition;
						if (displayer.activeSelf && Object.ReferenceEquals(forGO, selectedUI))
                        {
                            if (displayedElement)
                            {
                                displayedElement.SetActive(false);
								int nbElems = elemsInventory.Count;
								for(int j = 0; j < nbElems; j++)
                                {
									elemsInventory.getAt(j).GetComponent<RectTransform>().localPosition += Vector3.left * (Camera.main.pixelWidth / 2 - 250) + Vector3.up * (Camera.main.pixelHeight / 2 - 100);
                                }
                                glassesSelected.GetComponent<RectTransform>().localPosition = glassesUI.GetComponent<RectTransform>().localPosition;
                            }
                            displayer.SetActive(false);
                            selection.SetActive(false);
                            selectedUI = null;
                        }
                        else
                        {
							selectedUI = forGO;
                            if (displayedElement)
                            {
                                displayedElement.SetActive(false);
                            }
                            displayedElementWasNull = !(displayedElement && displayer.activeSelf);
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

                                case "Puzzle":
                                    foreach(Transform child in displayer.transform)
                                    {
                                        if(child.gameObject.name == "Puzzles")
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
                            if (displayedElement)
                            {
								int nbElems = elemsInventory.Count;
								for (int j = 0; j< nbElems; j++)
                                {
									elemsInventory.getAt(j).GetComponent<RectTransform>().localPosition += Vector3.right * (Camera.main.pixelWidth / 2 - 250) + Vector3.down * (Camera.main.pixelHeight / 2 - 100);
                                }
                                selection.GetComponent<RectTransform>().localPosition = selectedUI.GetComponent<RectTransform>().localPosition;
                                glassesSelected.GetComponent<RectTransform>().localPosition = glassesUI.GetComponent<RectTransform>().localPosition;
                            }
                            else if (!displayedElementWasNull)
							{
								int nbElems = elemsInventory.Count;
								for (int j = 0; j< nbElems; j++)
                                {
									elemsInventory.getAt(j).GetComponent<RectTransform>().localPosition += Vector3.left * (Camera.main.pixelWidth / 2 - 250) + Vector3.up * (Camera.main.pixelHeight / 2 - 100);
                                }
                                selection.GetComponent<RectTransform>().localPosition = selectedUI.GetComponent<RectTransform>().localPosition;
                                glassesSelected.GetComponent<RectTransform>().localPosition = glassesUI.GetComponent<RectTransform>().localPosition;
                            }
                        }
                    }
                }
            }
        }

		if (Input.GetKeyDown (KeyCode.A)) {
			inputfieldFocused = false;
			int nbInputFields = inputfields.Count;
			for (int i = 0; i < nbInputFields; i++) {
				forGO = inputfields.getAt (i);
				if (forGO.GetComponent<InputField> ().isFocused && forGO.GetComponent<InputField> ().contentType == InputField.ContentType.Standard) {
					inputfieldFocused = true;
					break;
				}
			}
			if (!inputfieldFocused) {
				if (CollectableGO.onInventory) {
					CloseInventory ();
				} else {
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
					for (int i = 0; i < nb; i++) {
						forGO = elemsInventory.getAt (i);
						forGO.GetComponent<RectTransform> ().localPosition += Vector3.left * (Camera.main.pixelWidth / 2 - 250) + Vector3.up * (Camera.main.pixelHeight / 2 - 100);
					}
					if (selectedUI) {
						selection.GetComponent<RectTransform> ().localPosition = selectedUI.GetComponent<RectTransform> ().localPosition;
					}
					glassesSelected.GetComponent<RectTransform> ().localPosition = glassesUI.GetComponent<RectTransform> ().localPosition;
					onPuzzle = false;
					CollectableGO.onInventory = true;
				}
			}
		} else if (CollectableGO.onInventory) {
			if (onElem.Count == 0 && Input.GetMouseButtonDown (0)) {
				CloseInventory ();
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
                }
                else
                {
                    if (Input.GetKeyDown(KeyCode.Q))
                    {
                        draggedPuzzle.GetComponent<RectTransform>().localRotation = Quaternion.Euler(draggedPuzzle.GetComponent<RectTransform>().localRotation.eulerAngles.x, draggedPuzzle.GetComponent<RectTransform>().localRotation.eulerAngles.y, draggedPuzzle.GetComponent<RectTransform>().localRotation.eulerAngles.z + 90);
                    }
                    if (Input.GetKeyDown(KeyCode.D))
                    {
                        draggedPuzzle.GetComponent<RectTransform>().localRotation = Quaternion.Euler(draggedPuzzle.GetComponent<RectTransform>().localRotation.eulerAngles.x, draggedPuzzle.GetComponent<RectTransform>().localRotation.eulerAngles.y, draggedPuzzle.GetComponent<RectTransform>().localRotation.eulerAngles.z - 90);
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
						draggedPuzzle = forGO;
                        posBeforeDrag = draggedPuzzle.GetComponent<RectTransform>().position;
                        posFromMouse = Input.mousePosition - draggedPuzzle.GetComponent<RectTransform>().position;
                    }
                }
            }
        }
	}

    private void CloseInventory()
    {
        if (!(displayedElement && displayer.activeSelf))
        {
			int nbElems = elemsInventory.Count;
			for(int i = 0; i < nbElems; i++)
            {
				elemsInventory.getAt(i).GetComponent<RectTransform>().localPosition += Vector3.right * (Camera.main.pixelWidth / 2 - 250) + Vector3.down * (Camera.main.pixelHeight / 2 - 100);
            }
            if (selectedUI)
            {
                selection.GetComponent<RectTransform>().localPosition = selectedUI.GetComponent<RectTransform>().localPosition;
            }
            glassesSelected.GetComponent<RectTransform>().localPosition = glassesUI.GetComponent<RectTransform>().localPosition;
        }
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
        CollectableGO.onInventory = false;
    }
}