using UnityEngine;
using FYFY;
using FYFY_plugins.PointerManager;
using System.Collections.Generic;

public class WhiteBoardManager : FSystem {
    
    // this system manage the whiteboard and effacer

    private Family f_whiteBoard = FamilyManager.getFamily(new AnyOfTags("Board"));
    private Family f_focusedWhiteBoard = FamilyManager.getFamily(new AnyOfTags("Board"), new AllOfComponents(typeof(ReadyToWork)));
    private Family f_closeWhiteBoard = FamilyManager.getFamily (new AnyOfTags ("Board", "Eraser", "BoardTexture", "InventoryElements", "HUD_TransparentOnMove"), new AllOfComponents(typeof(PointerOver)));
    private Family f_eraserFocused = FamilyManager.getFamily(new AnyOfTags("Eraser"), new AllOfComponents(typeof(PointerOver)));
    private Family f_boardRemovableWords = FamilyManager.getFamily(new AnyOfTags("BoardRemovableWords"));
    private Family f_boardUnremovableWords = FamilyManager.getFamily(new AnyOfTags("BoardUnremovableWords"));
    private Family f_iarBackground = FamilyManager.getFamily(new AnyOfTags("UIBackground"), new AnyOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));
    private Family f_boardTexture = FamilyManager.getFamily(new AllOfComponents(typeof(ChangePixelColor)));
    private Family f_activatedBoard = FamilyManager.getFamily(new AnyOfTags("Board"), new AnyOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));

    private Family f_player = FamilyManager.getFamily(new AnyOfTags("Player"));

    //board
    private GameObject selectedBoard;
    private GameObject eraser;
    public static bool eraserDragged = false;
    private float distToBoard;
    private Color prevColor;

    //List of triggers still active for each word (the words are differenciated by there position)
    private Dictionary<Vector3, List<GameObject>> triggeredSpheres;

    private List<Vector2> tmpListVector2;
    private Texture2D tmpTex;

    public static WhiteBoardManager instance;

    public WhiteBoardManager()
    {
        if (Application.isPlaying)
        {
            //initialise variables
            eraser = f_whiteBoard.First().transform.GetChild(2).gameObject;

            // set render order only when the object is activated, else unity doesn't take it in account
            f_activatedBoard.addEntryCallback(SetRenderOrder);

            f_focusedWhiteBoard.addEntryCallback(onReadyToWorkOnWhiteBoard);
            f_eraserFocused.addEntryCallback(onEnterEraser);
            f_eraserFocused.addExitCallback(onExitEraser);
        }
        instance = this;
    }

    private void onReadyToWorkOnWhiteBoard(GameObject go)
    {
        selectedBoard = go;
        distToBoard = (f_player.First().transform.position - selectedBoard.transform.position).magnitude;

        // Launch this system
        instance.Pause = false;

        GameObjectManager.addComponent<ActionPerformed>(go, new { name = "turnOn", performedBy = "player" });
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

    //Calculate points equally distributed on an area depending on width, height and eraserWidth
    private List<Vector2> PointsFromCenter(float width, float height)
    {
        if (width == 0 || height == 0)
            return null;
        
        float eraserWidth = 2.1f;
        List<Vector2> points = new List<Vector2>();

        int nbOfHorizontal, nbOfVertical;
        nbOfHorizontal = (int)(width / eraserWidth) + 1;
        nbOfVertical = (int)(height / eraserWidth) + 1;

        float horizontalStep, verticalStep;
        horizontalStep = width / (nbOfHorizontal + 1);
        verticalStep = height / (nbOfVertical + 1);

        for (int i = 0; i < nbOfHorizontal; i++)
            for (int j = 0; j < nbOfVertical; j++)
                points.Add(new Vector2(-width / 2 + horizontalStep * (i + 1), -height / 2 + verticalStep * (j + 1)) / 7);
        
        return points;
    }

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount)
    {
        if (selectedBoard)
        {
            // "close" ui (give back control to the player) when clicking on nothing or Escape is pressed and IAR is closed (because Escape close IAR)
            if (((f_closeWhiteBoard.Count == 0 && Input.GetButtonDown("Fire1")) || (Input.GetButtonDown("Cancel") && f_iarBackground.Count == 0)))
                ExitWhiteBoard();
            else
            {
                if (eraser.GetComponent<PointerOver>() && Input.GetButtonDown("Fire1"))
                {
                    //start dragging eraser when it s clicked
                    eraserDragged = true;

                    GameObjectManager.addComponent<ActionPerformed>(eraser, new { name = "turnOn", performedBy = "player" });
                    GameObjectManager.addComponent<ActionPerformedForLRS>(eraser, new { verb = "dragged", objectType = "draggable", objectName = eraser.name });
                }
                if (eraserDragged)
                {
                    if (Input.GetButtonUp("Fire1"))
                    {
                        //stop dragging eraser when the click is released
                        eraserDragged = false;
                        GameObjectManager.addComponent<ActionPerformed>(eraser, new { name = "turnOff", performedBy = "player" });
                        GameObjectManager.addComponent<ActionPerformedForLRS>(eraser, new { verb = "dropped", objectType = "draggable", objectName = eraser.name });
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
        GameObjectManager.addComponent<ActionPerformedForLRS>(selectedBoard, new { verb = "exited", objectType = "interactable", objectName = selectedBoard.name });

        selectedBoard = null;

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
        if (f_boardTexture.First() != null)
            f_boardTexture.First().GetComponent<Renderer>().material.renderQueue = 2002;
        foreach (GameObject word in f_boardUnremovableWords)
        {
            foreach (Renderer r in word.GetComponentsInChildren<Renderer>())
                r.material.renderQueue = 2003;
        }
    }
}