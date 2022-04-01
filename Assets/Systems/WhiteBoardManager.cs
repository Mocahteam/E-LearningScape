using UnityEngine;
using FYFY;
using FYFY_plugins.PointerManager;

public class WhiteBoardManager : FSystem {
    
    // this system manage the whiteboard and effacer

    private Family f_focusedWhiteBoard = FamilyManager.getFamily(new AnyOfTags("Board"), new AllOfComponents(typeof(ReadyToWork)));
    private Family f_closeWhiteBoard_1 = FamilyManager.getFamily (new AnyOfTags ("Board", "Eraser", "BoardTexture", "InventoryElements"), new AllOfComponents(typeof(PointerOver)));
    private Family f_closeWhiteBoard_2 = FamilyManager.getFamily(new AllOfComponents(typeof(PointerOver), typeof(HUD_TransparentOnMove)));
    private Family f_eraserFocused = FamilyManager.getFamily(new AnyOfTags("Eraser"), new AllOfComponents(typeof(PointerOver)));
    private Family f_boardRemovableWords = FamilyManager.getFamily(new AnyOfTags("BoardRemovableWords"));
    private Family f_boardUnremovableWords = FamilyManager.getFamily(new AnyOfTags("BoardUnremovableWords"));
    private Family f_iarBackground = FamilyManager.getFamily(new AnyOfTags("UIBackground"), new AnyOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));
    private Family f_activatedBoard = FamilyManager.getFamily(new AnyOfTags("Board"), new AnyOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));

    //board
    public GameObject eraser;
    private GameObject selectedBoard;
    public static bool eraserDragged = false;
    private float distToBoard;
    private Color prevColor;
    public GameObject boardTexture;

    public GameObject player;

    public static WhiteBoardManager instance;

    public WhiteBoardManager()
    {
        instance = this;
    }

    protected override void onStart()
    {
        // set render order only when the object is activated, else unity doesn't take it in account
        f_activatedBoard.addEntryCallback(SetRenderOrder);

        f_focusedWhiteBoard.addEntryCallback(onReadyToWorkOnWhiteBoard);
        f_eraserFocused.addEntryCallback(onEnterEraser);
        f_eraserFocused.addExitCallback(onExitEraser);
    }

    private void onReadyToWorkOnWhiteBoard(GameObject go)
    {
        selectedBoard = go;
        distToBoard = (player.transform.position - selectedBoard.transform.position).magnitude;

        // Launch this system
        instance.Pause = false;

        GameObjectManager.addComponent<ActionPerformed>(go, new { name = "turnOn", performedBy = "player" });
        
        // Add RigidBody and collider to eraser
        GameObjectManager.addComponent<Rigidbody>(eraser, new { isKinematic = true });
        GameObjectManager.addComponent<CapsuleCollider>(eraser);
        // Add Collider to boardTexture
        GameObjectManager.addComponent<MeshCollider>(boardTexture);

        GameObjectManager.addComponent<ActionPerformedForLRS>(go, new
        {
            verb = "accessed",
            objectType = "whiteBoard"
        });
    }

    private void onEnterEraser (GameObject go)
    {
        Renderer rend = eraser.GetComponent<Renderer>();
        prevColor = rend.material.GetColor("_EmissionColor");
        rend.material.EnableKeyword("_EMISSION");
        rend.material.SetColor("_EmissionColor", Color.yellow * Mathf.LinearToGammaSpace(0.8f));
    }

    private void onExitEraser(int instanceId)
    {
        eraser.GetComponent<Renderer>().material.SetColor("_EmissionColor", prevColor);
    }

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount)
    {
        if (selectedBoard)
        {
            // "close" ui (give back control to the player) when clicking on nothing or Escape is pressed and IAR is closed (because Escape close IAR)
            if (((f_closeWhiteBoard_1.Count == 0 && f_closeWhiteBoard_2.Count == 0 && Input.GetButtonDown("Fire1")) || (Input.GetButtonDown("Cancel") && f_iarBackground.Count == 0)))
                ExitWhiteBoard();
            else
            {
                if (eraser.GetComponent<PointerOver>() && Input.GetButtonDown("Fire1"))
                {
                    //start dragging eraser when it s clicked
                    eraserDragged = true;

                    GameObjectManager.addComponent<ActionPerformed>(eraser, new { name = "turnOn", performedBy = "player" });
                    GameObjectManager.addComponent<ActionPerformedForLRS>(eraser, new 
                    {
                        verb = "dragged",
                        objectType = "eraser"
                    });
                }
                if (eraserDragged)
                {
                    if (Input.GetButtonUp("Fire1"))
                    {
                        //stop dragging eraser when the click is released
                        eraserDragged = false;
                        GameObjectManager.addComponent<ActionPerformed>(eraser, new { name = "turnOff", performedBy = "player" });
                        GameObjectManager.addComponent<ActionPerformedForLRS>(eraser, new 
                        {
                            verb = "dropped",
                            objectType = "eraser"
                        });
                    }
                    else
                    {
                        //move eraser to mouse position
                        Vector3 mousePos = selectedBoard.transform.InverseTransformPoint(Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, distToBoard)));
                        eraser.transform.localPosition = new Vector3(mousePos.x, eraser.transform.localPosition.y, mousePos.z);
                        //prevent eraser from going out of the board
                        if (eraser.transform.localPosition.x > 0.021f)
                        {
                            eraser.transform.localPosition += Vector3.right * (0.021f - eraser.transform.localPosition.x);
                        }
                        else if (eraser.transform.localPosition.x < -0.021f)
                        {
                            eraser.transform.localPosition += Vector3.right * (-0.021f - eraser.transform.localPosition.x);
                        }
                        if (eraser.transform.localPosition.z > 0.016f)
                        {
                            eraser.transform.localPosition += Vector3.forward * (0.016f - eraser.transform.localPosition.z);
                        }
                        else if (eraser.transform.localPosition.z < -0.016f)
                        {
                            eraser.transform.localPosition += Vector3.forward * (-0.016f - eraser.transform.localPosition.z);
                        }
                    }
                }
            }
        }
	}

    private void ExitWhiteBoard()
    {
        if(eraserDragged)
            GameObjectManager.addComponent<ActionPerformed>(eraser, new { name = "turnOff", performedBy = "system" });
        eraserDragged = false;
        // remove ReadyToWork component to release selected GameObject
        GameObjectManager.removeComponent<ReadyToWork>(selectedBoard);

        GameObjectManager.addComponent<ActionPerformed>(selectedBoard, new { name = "turnOff", performedBy = "player" });
        GameObjectManager.addComponent<ActionPerformedForLRS>(selectedBoard, new 
        {
            verb = "exited",
            objectType = "whiteBoard"
        });

        selectedBoard = null;

        // Add RigidBody and collider to eraser
        GameObjectManager.removeComponent<Rigidbody>(eraser);
        GameObjectManager.removeComponent<CapsuleCollider>(eraser);
        // Add Collider to boardTexture
        GameObjectManager.removeComponent<MeshCollider>(boardTexture);

        // Pause this system
        instance.Pause = true;
    }

    public void SetRenderOrder(GameObject unused)
    {
        // set all board's removable words to "occludable" and unremovable words to "not occludable"
        // the occlusion is then made by an invisible material that hides all objects behind it having the "occludable" setting
        foreach (GameObject word in f_boardRemovableWords)
        {
            foreach (Renderer r in word.GetComponentsInChildren<Renderer>())
                r.material.renderQueue = 2001;
        }
        boardTexture.GetComponent<Renderer>().material.renderQueue = 2002;
        foreach (GameObject word in f_boardUnremovableWords)
        {
            foreach (Renderer r in word.GetComponentsInChildren<Renderer>())
                r.material.renderQueue = 2003;
        }
    }
}