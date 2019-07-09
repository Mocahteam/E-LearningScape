using UnityEngine;
using UnityEngine.UI;
using FYFY;
using FYFY_plugins.PointerManager;
using FYFY_plugins.Monitoring;

public class SatchelManager : FSystem {

    // this system manage the satchel and its paper

    //all selectable objects
    private Family f_bag = FamilyManager.getFamily(new AnyOfTags("Bag"));
    private Family f_selectedBag = FamilyManager.getFamily(new AnyOfTags("Bag"), new AllOfComponents(typeof(ReadyToWork)));
    private Family f_closeBag = FamilyManager.getFamily(new AnyOfTags("Bag", "InventoryElements"), new AllOfComponents(typeof(PointerOver)));
    private Family f_iarBackground = FamilyManager.getFamily(new AnyOfTags("UIBackground"), new AnyOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));
    private Family f_itemSelected = FamilyManager.getFamily(new AnyOfTags("InventoryElements"), new AllOfComponents(typeof(SelectedInInventory)));
    private Family f_itemUnselected = FamilyManager.getFamily(new AnyOfTags("InventoryElements"), new NoneOfComponents(typeof(SelectedInInventory)));

    private float speed;
    private float dist;

    //bag
    private bool moveBag = false;               //true during the animation to move the player above the table
    private bool showBagPaper = false;
    private Vector3 bagTargetPos;
    private Vector3 bagPaperInitialPos;
    private GameObject selectedBag;
    private GameObject bagPadlock;

    // paper
    private GameObject paper;
    private Image paperImg;
    private BagImage paperImgRef;

    private bool openBag = false;
    private bool prepareclosing = false;
    private bool checkPadLock = false;
    private bool getOutPaper = false;
    private bool paperOut = false;

    private int currentPaperViews = 0; // 0 means no glasses, 1 means glass 1 is on, 2 means glass 2 is on, 3 means both glasses are on

    public static SatchelManager instance;

    public SatchelManager()
    {
        if (Application.isPlaying)
        {
            bagPaperInitialPos = f_bag.First().GetComponentInChildren<Canvas>().gameObject.transform.parent.localPosition;
            paper = f_bag.First().transform.GetChild(1).gameObject;
            bagPadlock = f_bag.First().transform.GetChild(3).gameObject;
            paperImg = f_bag.First().GetComponentInChildren<Image>();
            paperImgRef = f_bag.First().GetComponentInChildren<BagImage>(); 

            f_selectedBag.addEntryCallback(onReadyToWorkOnSatchel);
            f_itemSelected.addEntryCallback(onItemSelectedInInventory);
            f_itemUnselected.addEntryCallback(onItemUnselectedInInventory);
        }
        instance = this;
    }

    private void onReadyToWorkOnSatchel(GameObject go)
    {
        selectedBag = go;
        openBag = true;
        prepareclosing = false;
        moveBag = true;
        checkPadLock = false;
        getOutPaper = false;
        paperOut = false;
        bagTargetPos = go.transform.position + Vector3.up*0.5f;

        // Launch this system
        instance.Pause = false;

        GameObjectManager.addComponent<ActionPerformed>(go, new { name = "turnOn", performedBy = "player" });
    }

    private void updatePictures()
    {
        // switch appropriate image depending on glasses worn
        switch (currentPaperViews)
        {
            case 0:
                paperImg.sprite = paperImgRef.image0;
                break;
            case 1:
                paperImg.sprite = paperImgRef.image1;
                break;
            case 2:
                paperImg.sprite = paperImgRef.image2;
                break;
            default:
                paperImg.sprite = paperImgRef.image3;
                break;
        }
        GameObjectManager.addComponent<ActionPerformed>(paper, new { overrideName = ("activatePaper"+currentPaperViews), performedBy = "system" });
        GameObjectManager.addComponent<ActionPerformedForLRS>(paper, new { verb = "accessed", objectType = "interactable", objectName = ("paper"+ currentPaperViews) });
    }

    private void onItemSelectedInInventory(GameObject go)
    {
        if (go.name == "Glasses1" || go.name == "Glasses2")
        {
            if (go.name == "Glasses1")
                currentPaperViews = currentPaperViews + 1;
            else
                currentPaperViews = currentPaperViews + 2;
            if (paperOut)
                updatePictures();
        }
    }

    private void onItemUnselectedInInventory(GameObject go)
    {
        if (go.name == "Glasses1" || go.name == "Glasses2")
        {
            if (go.name == "Glasses1")
                currentPaperViews = currentPaperViews - 1;
            else
                currentPaperViews = currentPaperViews - 2;
            if (paperOut)
                updatePictures();
        }
    }

    // return true if UI with name "name" is selected into inventory
    private GameObject isSelected(string name)
    {
        foreach (GameObject go in f_itemSelected)
            if (go.name == name)
                return go;
        return null;
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
	protected override void onProcess(int familiesUpdateCount)
    {
        speed = 8f * Time.deltaTime;

        if (selectedBag)
        {
            // "close" ui (give back control to the player) when clicking on nothing or Escape is pressed and paper is out of the bag and IAR is closed (because Escape close IAR)
            if (((f_closeBag.Count == 0 && Input.GetMouseButtonDown(0)) || (Input.GetKeyDown(KeyCode.Escape) && f_iarBackground.Count == 0)) && (paperOut || bagPadlock.activeSelf))
            {
                // ask to close bag
                prepareclosing = true;
            }

            if (moveBag)
            {
                //animation to move the bag in front of the player
                selectedBag.transform.position = Vector3.MoveTowards(selectedBag.transform.position, bagTargetPos, speed);
                //when the bag arrives
                if (selectedBag.transform.position == bagTargetPos)
                {
                    moveBag = false;
                    if (openBag)
                    {
                        checkPadLock = true;
                        openBag = false;
                    }
                    else
                    {
                        // means we want exit the bag

                        // remove ReadyToWork component to release selected GameObject
                        GameObjectManager.removeComponent<ReadyToWork>(selectedBag);

                        GameObjectManager.addComponent<ActionPerformed>(selectedBag, new { name = "turnOff", performedBy = "player" });
                        GameObjectManager.addComponent<ActionPerformedForLRS>(selectedBag, new { verb = "exited", objectType = "interactable", objectName = selectedBag.name });

                        selectedBag = null;

                        // Pause this system
                        instance.Pause = true;
                        return;
                    }
                }
            }

            if (checkPadLock)
            {
                if (bagPadlock.activeSelf)
                {
                    if (isSelected("KeySatchel"))
                    {
                        //move up and rotate the padlock
                        dist = 2f - bagPadlock.transform.position.y;
                        bagPadlock.transform.position = Vector3.MoveTowards(bagPadlock.transform.position, bagPadlock.transform.position + Vector3.up * dist, (dist + 1) / 2 * Time.deltaTime);
                        bagPadlock.transform.localRotation = Quaternion.Euler(bagPadlock.transform.localRotation.eulerAngles + Vector3.up * (bagPadlock.transform.localPosition.y - 0.2f) * 350 * 20 * Time.deltaTime);
                        if (bagPadlock.transform.position.y > 1.9f)
                        {
                            //stop animation when the padlock reaches a certain height
                            GameObjectManager.setGameObjectState(bagPadlock, false);
                            //remove key from inventory
                            GameObjectManager.setGameObjectState(isSelected("KeySatchel"), false);
                            checkPadLock = false;
                            showBagPaper = true;
                            getOutPaper = true;
                            updatePictures();

                            GameObjectManager.addComponent<ActionPerformed>(bagPadlock, new { name = "perform", performedBy = "system" });
                            GameObjectManager.addComponent<ActionPerformedForLRS>(selectedBag, new { verb = "unlocked", objectType = "interactable", objectName = selectedBag.name });
                        }
                    }
                }
                else
                {
                    checkPadLock = false;
                    showBagPaper = true;
                    getOutPaper = true;
                    updatePictures();
                }
            }

            if (showBagPaper)
            {
                if (getOutPaper)
                {
                    //take off the paper from the bag
                    paper.transform.position = Vector3.MoveTowards(paper.transform.position, selectedBag.transform.TransformPoint(bagPaperInitialPos) + Vector3.up * 0.8f, speed / 10);
                    if (paper.transform.position == selectedBag.transform.TransformPoint(bagPaperInitialPos) + Vector3.up * 0.8f)
                    {
                        //show the paper on the camera screen when it reaches the final position
                        getOutPaper = false;
                        Canvas canvas = selectedBag.GetComponentInChildren<Canvas>();
                        canvas.renderMode = RenderMode.ScreenSpaceCamera;
                        canvas.scaleFactor = 0.85f + (Screen.height-720)/900f; // 900 is magic value to add 0.4f when passing from default resolution (1280/720) to high resolution (1980/1080)
                        paper.transform.position = selectedBag.transform.TransformPoint(bagPaperInitialPos) + Vector3.up * 0.8f;
                        
                        paperOut = true;
                    }
                }
            }

            if (prepareclosing)
            {
                if (paperOut)
                {
                    //put the paper back in the bag
                    selectedBag.GetComponentInChildren<Canvas>().scaleFactor = 1;
                    selectedBag.GetComponentInChildren<Canvas>().renderMode = RenderMode.WorldSpace;
                    selectedBag.GetComponentInChildren<Canvas>().gameObject.GetComponent<RectTransform>().localPosition = Vector3.forward * (-0.51f - paper.transform.localPosition.z);
                    paper.transform.position = Vector3.MoveTowards(paper.transform.position, selectedBag.transform.TransformPoint(bagPaperInitialPos), speed / 5);
                    if (paper.transform.position.y == selectedBag.transform.TransformPoint(bagPaperInitialPos).y)
                    {
                        moveBag = true;
                        paperOut = false;
                        bagTargetPos = selectedBag.transform.position - Vector3.up * 0.5f;
                        prepareclosing = false;
                    }
                }
                else
                {
                    moveBag = true;
                    paperOut = false;
                    bagTargetPos = selectedBag.transform.position - Vector3.up * 0.5f;
                    prepareclosing = false;
                }
            }
        }
	}
}