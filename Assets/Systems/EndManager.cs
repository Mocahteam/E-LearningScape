using UnityEngine;
using UnityEngine.UI;
using FYFY;
using FYFY_plugins.Monitoring;
using System.Collections.Generic;

public class EndManager : FSystem {

    // this system manage the epilog

    private Family f_answer = FamilyManager.getFamily(new AnyOfTags("A-R3"), new NoneOfProperties(PropertyMatcher.PROPERTY.ACTIVE_SELF)); // answers not already displayed of the third room
    // Will contain a game object when IAR is openned
    private Family f_iarBackground = FamilyManager.getFamily(new AnyOfTags("UIBackground"), new AnyOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));

    private Family f_onEnigma = FamilyManager.getFamily(new AllOfComponents(typeof(ReadyToWork)));

    public GameObject player;
    public GameObject gameRooms;
    public GameObject waterFloor;
    public UnlockedRoom unlockedRoom;
    public StoryText storyText;

    public Image fadingBackground;
    private float fadingTimer = 2;
    private float fadingStart;
    private bool alphaToWhite = false;
    private bool whiteToAlpha = false;

    private bool switchToEndRoom = false;
    private bool readEndText = false;

    private bool useEndRoom = false;

    public static EndManager instance;

    public EndManager()
    {
        instance = this;
    }

    protected override void onStart()
    {
        f_answer.addExitCallback(onNewAnswerDisplayed);
        f_iarBackground.addExitCallback(onIARClosed);

        useEndRoom = LoadGameContent.internalGameContent.useEndRoom;
    }

    private void onNewAnswerDisplayed(int instanceId)
    {
        // When all answer was displayed => ask to teleport in the end room or display end story
        if (f_answer.Count == 0)
        {
            if (useEndRoom)
            {
                switchToEndRoom = true;

                GameObjectManager.addComponent<ActionPerformed>(player, new { overrideName = "teleportToFinalScene", performedBy = "system" });
                unlockedRoom.roomNumber = 4;
            }
            else
            {
                readEndText = true;
            }
        }
    }

    private void onIARClosed(int instanceId)
    {
        if (switchToEndRoom)
        {
            fadingStart = Time.time;
            switchToEndRoom = false;
            alphaToWhite = true;
            this.Pause = false;
            IARTabNavigation.instance.Pause = true;
            GameObjectManager.setGameObjectState(fadingBackground.gameObject, true);
            foreach (GameObject go in f_onEnigma)
                GameObjectManager.removeComponent<ReadyToWork>(go);
        }
        else if (readEndText)
        {
            IARTabNavigation.instance.Pause = true;
            foreach (GameObject go in f_onEnigma)
                GameObjectManager.removeComponent<ReadyToWork>(go);

            // show story
            storyText.storyProgression++;
            StoryDisplaying.instance.Pause = false;
            GameObjectManager.addComponent<PlaySound>(storyText.gameObject, new { id = 8 }); // id refer to FPSController AudioBank
        }
    }

    // Use to process your families.
    protected override void onProcess(int familiesUpdateCount)
    {
        if (alphaToWhite && Time.time - fadingStart < fadingTimer)
        {
            fadingBackground.color = new Color(1, 1, 1, (Time.time - fadingStart) / fadingTimer);
        }

        if (alphaToWhite && Time.time - fadingStart >= fadingTimer)
        {
            fadingBackground.color = new Color(1, 1, 1, 1);
            alphaToWhite = false;
            whiteToAlpha = true;
            fadingStart = Time.time;
            // switch to end room
            player.transform.position = new Vector3(74, 0.33f, -3);
            player.transform.localRotation = Quaternion.Euler(0, -90, 0);
            Camera.main.transform.localRotation = Quaternion.Euler(Vector3.zero);
            foreach (Transform child in gameRooms.transform)
                GameObjectManager.setGameObjectState(child.gameObject, false);
            GameObject endRoom = gameRooms.transform.GetChild(4).gameObject;
            GameObjectManager.setGameObjectState(endRoom, true);
            if (player.GetComponentInChildren<Light>())
                GameObjectManager.setGameObjectState(player.GetComponentInChildren<Light>().gameObject, false);
            RenderSettings.fogDensity = 0; // to view far fragment inside the last scene
            Camera.main.farClipPlane = 300;
            foreach (Transform child in endRoom.transform)
                if (child.gameObject.GetComponent<MeshRenderer>())
                    child.gameObject.GetComponent<MeshRenderer>().allowOcclusionWhenDynamic = false;
            foreach (Transform child in waterFloor.transform)
                if (child.gameObject.GetComponent<MeshRenderer>())
                    child.gameObject.GetComponent<MeshRenderer>().allowOcclusionWhenDynamic = false;

            // disable all systems except this, DreamFragmentCollect, MovingSystem, SendStatements and ActionsManager
            List<FSystem> allSystems = new List<FSystem>(FSystemManager.fixedUpdateSystems());
            allSystems.AddRange(FSystemManager.updateSystems());
            allSystems.AddRange(FSystemManager.lateUpdateSystems());
            foreach (FSystem syst in allSystems)
                if (syst != this && syst != DreamFragmentCollecting.instance && syst != MovingSystem_FPSMode.instance && syst != MovingSystem_TeleportMode.instance && syst != MovingSystem_UIMode.instance && syst != SendStatements.instance && syst != ActionsManager.instance)
                    syst.Pause = true;
        }

        if (whiteToAlpha && Time.time - fadingStart < fadingTimer)
            fadingBackground.color = new Color(1, 1, 1, 1 - (Time.time - fadingStart) / fadingTimer);

        if (whiteToAlpha && Time.time - fadingStart >= fadingTimer)
        {
            whiteToAlpha = false;
            GameObjectManager.setGameObjectState(fadingBackground.gameObject, false);
        }
    }
}