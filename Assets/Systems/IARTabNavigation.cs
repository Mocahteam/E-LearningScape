using UnityEngine;
using FYFY;
using UnityEngine.UI;
using FYFY_plugins.PointerManager;

public class IARTabNavigation : FSystem {

    // Manage base IAR integration (Open/Close + tab switching)

    private Family tabs = FamilyManager.getFamily(new AnyOfTags("IARTab"), new AllOfComponents(typeof(LinkedWith), typeof(Button)));
    private Family fgm = FamilyManager.getFamily(new AllOfComponents(typeof(FocusedGOMaterial)));
    private Family f_iarBackground = FamilyManager.getFamily(new AnyOfTags("UIBackground"), new AllOfComponents(typeof(PointerSensitive)));
    private Family HUD_A = FamilyManager.getFamily(new AnyOfTags("HUD_A"));

    private Sprite selectedTabSprite;
    private Sprite defaultTabSprite;

    private GameObject iar;
    private GameObject iarBackground;

    private bool openedAtLeastOnce = false;

    public static IARTabNavigation instance;

    public IARTabNavigation()
    {
        if (Application.isPlaying)
        {
            foreach (GameObject tab in tabs)
            {
                tab.GetComponent<Button>().onClick.AddListener(delegate {
                    SwitchTab(tab);
                });
            }

            selectedTabSprite = fgm.First().GetComponent<FocusedGOMaterial>().selectedTabSprite;
            defaultTabSprite = fgm.First().GetComponent<FocusedGOMaterial>().defaultTabSprite;

            iarBackground = f_iarBackground.First();
            iar = iarBackground.transform.parent.gameObject;
        }
        instance = this;
    }

    // Use this to update member variables when system pause. 
    // Advice: avoid to update your families inside this function.
    protected override void onPause(int currentFrame)
    {
        GameObjectManager.setGameObjectState(HUD_A.First(), false); // hide HUD "A"
    }

    // Use this to update member variables when system resume.
    // Advice: avoid to update your families inside this function.
    protected override void onResume(int currentFrame)
    {
        if (openedAtLeastOnce)
            GameObjectManager.setGameObjectState(HUD_A.First(), true); // display HUD "A"
    }

    // Use to process your families.
    protected override void onProcess(int familiesUpdateCount)
    {
        // Open/Close IAR with Escape and A keys
        if (iar.activeInHierarchy && (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.Escape) || (Input.GetMouseButtonDown(0) && iarBackground.GetComponent<PointerOver>())))
            closeIar();
        else if (!iar.activeInHierarchy && (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.Escape)))
        {
            if (Input.GetKeyDown(KeyCode.A))
                openIar(0); // Open IAR on the first tab
            else
                openIar(tabs.Count - 1); // Open IAR on the last tab
        }
    }

    private void openIar(int tabId)
    {
        openedAtLeastOnce = true;
        GameObjectManager.setGameObjectState(HUD_A.First(), false); // hide HUD "A"
        GameObjectManager.setGameObjectState(iar, true); // open IAR
        SwitchTab(tabs.getAt(tabId)); // switch to the first tab
        MovingSystem.instance.Pause = true;
        DreamFragmentCollecting.instance.Pause = true;
        Highlighter.instance.Pause = true;
        ToggleObject.instance.Pause = true;
        CollectObject.instance.Pause = true;
        IARViewItem.instance.Pause = false;
        LockResolver.instance.Pause = true;
    }

    public void closeIar()
    {
        GameObjectManager.setGameObjectState(iar, false); // close IAR
        MovingSystem.instance.Pause = false;
        DreamFragmentCollecting.instance.Pause = false;
        Highlighter.instance.Pause = false;
        ToggleObject.instance.Pause = false;
        CollectObject.instance.Pause = false;
        IARViewItem.instance.Pause = true;
        LockResolver.instance.Pause = false;
        // display HUD "A"
        GameObjectManager.setGameObjectState(HUD_A.First(), true);
    }

    private void SwitchTab(GameObject newSelectedTab)
    {
        // reset all tabs (text and image) and disable all contents
        foreach (GameObject oldTab in tabs)
        {
            oldTab.GetComponent<Image>().sprite = defaultTabSprite;
            oldTab.GetComponentInChildren<Text>().fontStyle = FontStyle.Normal;
            GameObjectManager.setGameObjectState(oldTab.GetComponent<LinkedWith>().link, false);
        }
        // set new tab text and image
        newSelectedTab.GetComponent<Image>().sprite = selectedTabSprite;
        newSelectedTab.GetComponentInChildren<Text>().fontStyle = FontStyle.Bold;
        // enable new content
        GameObjectManager.setGameObjectState(newSelectedTab.GetComponent<LinkedWith>().link, true);
    }
}