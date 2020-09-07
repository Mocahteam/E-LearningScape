using UnityEngine;
using UnityEngine.UI;
using FYFY;
using System.Collections.Generic;

public class EndManager : FSystem {

    // this system manage the epilog

    private Family f_player = FamilyManager.getFamily(new AnyOfTags("Player"));
    private Family f_gameRooms = FamilyManager.getFamily(new AnyOfTags("GameRooms"));
    private Family f_waterFloor = FamilyManager.getFamily(new AnyOfTags("WaterFloor"));
    private Family f_unlockedRoom = FamilyManager.getFamily(new AllOfComponents(typeof(UnlockedRoom)));

    private Image fadingBackground;
    private float fadingTimer = 2;
    private float fadingStart;
    private bool alphaToWhite = false;
    private bool whiteToAlpha = false;

    public static EndManager instance = null;

    public EndManager()
    {
        instance = this;
        if (Application.isPlaying)
        {
            // Get singleton fading screen
            fadingBackground = GameObject.Find("MenuFadingBackground").GetComponent<Image>();
        }
    }

    public void startEpilog()
    {
        f_unlockedRoom.First().GetComponent<UnlockedRoom>().roomNumber = 4;
        fadingStart = Time.time;
        alphaToWhite = true;
        this.Pause = false;
        IARTabNavigation.instance.Pause = true;
        GameObjectManager.setGameObjectState(fadingBackground.gameObject, true);
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

            // disable all systems except this, DreamFragmentCollect, MovingSystem, and ActionsManager
            List<FSystem> allSystems = new List<FSystem>(FSystemManager.fixedUpdateSystems());
            allSystems.AddRange(FSystemManager.updateSystems());
            allSystems.AddRange(FSystemManager.lateUpdateSystems());
            foreach (FSystem syst in allSystems)
                if (syst != this && syst != DreamFragmentCollecting.instance && syst != MovingSystem.instance && syst != JumpingSystem.instance)
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