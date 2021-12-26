using UnityEngine;
using UnityEngine.UI;
using FYFY;
using FYFY_plugins.Monitoring;
using System.Collections.Generic;

public class EndManager : FSystem {

    // this system manage the epilog

    private Family f_answer = FamilyManager.getFamily(new AnyOfTags("A-R3"), new NoneOfProperties(PropertyMatcher.PROPERTY.ACTIVE_SELF)); // answers not already displayed of the third room
    private Family f_questionR3 = FamilyManager.getFamily(new AnyOfTags("Q-R3"));
    // Will contain a game object when IAR is openned
    private Family f_iarBackground = FamilyManager.getFamily(new AnyOfTags("UIBackground"), new AnyOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));

    private Family f_player = FamilyManager.getFamily(new AnyOfTags("Player"));
    private Family f_gameRooms = FamilyManager.getFamily(new AnyOfTags("GameRooms"));
    private Family f_waterFloor = FamilyManager.getFamily(new AnyOfTags("WaterFloor"));
    private Family f_unlockedRoom = FamilyManager.getFamily(new AllOfComponents(typeof(UnlockedRoom)));
    private Family f_onEnigma = FamilyManager.getFamily(new AllOfComponents(typeof(ReadyToWork)));

    private Family f_storyDisplayer = FamilyManager.getFamily(new AllOfComponents(typeof(StoryText)));

    private Image fadingBackground;
    private float fadingTimer = 2;
    private float fadingStart;
    private bool alphaToWhite = false;
    private bool whiteToAlpha = false;

    private bool switchToEndRoom = false;
    private bool readEndText = false;

    private bool useEndRoom = false;

    public EndManager()
    {
        if (Application.isPlaying)
        {
            f_answer.addExitCallback(onNewAnswerDisplayed);
            f_iarBackground.addExitCallback(onIARClosed);

            // Get singleton fading screen
            fadingBackground = GameObject.Find("MenuFadingBackground").GetComponent<Image>();

            useEndRoom = LoadGameContent.gameContent.useEndRoom;
        }
    }

    private void onNewAnswerDisplayed(int instanceId)
    {
        // When all answer was displayed => ask to teleport in the end room or display end story
        if (f_answer.Count == 0)
        {
            GameObjectManager.addComponent<ActionPerformedForLRS>(f_questionR3.First().transform.parent.parent.gameObject, new
            {
                verb = "completed",
                objectType = "menu",
                objectName = f_questionR3.First().transform.parent.parent.gameObject.name
            });
            if (useEndRoom)
            {
                switchToEndRoom = true;

                GameObjectManager.addComponent<ActionPerformed>(f_player.First(), new { overrideName = "teleportToFinalScene", performedBy = "system" });
                f_unlockedRoom.First().GetComponent<UnlockedRoom>().roomNumber = 4;
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
            f_storyDisplayer.First().GetComponent<StoryText>().storyProgression++;
            StoryDisplaying.instance.Pause = false;
            GameObjectManager.addComponent<PlaySound>(f_storyDisplayer.First(), new { id = 8 }); // id refer to FPSController AudioBank}
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
            f_player.First().transform.position = new Vector3(74, 0.33f, -3);
            f_player.First().transform.localRotation = Quaternion.Euler(0, -90, 0);
            Camera.main.transform.localRotation = Quaternion.Euler(Vector3.zero);
            foreach (Transform child in f_gameRooms.First().transform)
                GameObjectManager.setGameObjectState(child.gameObject, false);
            GameObject endRoom = f_gameRooms.First().transform.GetChild(4).gameObject;
            GameObjectManager.setGameObjectState(endRoom, true);
            if (f_player.First().GetComponentInChildren<Light>())
                GameObjectManager.setGameObjectState(f_player.First().GetComponentInChildren<Light>().gameObject, false);
            RenderSettings.fogDensity = 0; // to view far fragment inside the last scene
            Camera.main.farClipPlane = 300;
            foreach (Transform child in endRoom.transform)
                if (child.gameObject.GetComponent<MeshRenderer>())
                    child.gameObject.GetComponent<MeshRenderer>().allowOcclusionWhenDynamic = false;
            foreach (Transform child in f_waterFloor.First().transform)
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