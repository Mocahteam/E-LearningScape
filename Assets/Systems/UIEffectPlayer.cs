using UnityEngine;
using FYFY;

public class UIEffectPlayer : FSystem {

    // Display UI Effect based on PlayUIEffect component

    private Family f_playUiEffect = FamilyManager.getFamily(new AllOfComponents(typeof(PlayUIEffect)));
    private Family f_uiEffect = FamilyManager.getFamily(new AnyOfTags("UIEffect"));

    private GameObject rightBG;
    private GameObject wrongBG;
    private AnimatedSprites solvedAnimation;

    private GameObject workingBG;

    private bool blinkCorrect = false;
    private bool blinkWrong = false;

    private float startTime;

    public static UIEffectPlayer instance;

    public UIEffectPlayer()
    {
        if (Application.isPlaying)
        {
            foreach (GameObject go in f_uiEffect)
            {
                if (go.name == "Right")
                    rightBG = go;
                else if (go.name == "Wrong")
                    wrongBG = go;
                else if (go.name == "Solved")
                    solvedAnimation = go.GetComponent<AnimatedSprites>();
            }

            // add callback on families
            f_playUiEffect.addEntryCallback(onNewEffect);
        }
        instance = this;
    }

    // if new gameobject enter inside f_uiEffect we play associated effect
    private void onNewEffect(GameObject go)
    {
        PlayUIEffect uiEffect = go.GetComponent<PlayUIEffect>();
        if (uiEffect.effectCode == 0 || uiEffect.effectCode == 2)
        {
            // play right sound
            GameObjectManager.addComponent<PlaySound>(go, new { id = 0 }); // id refer to FPSController AudioBank
            if (uiEffect.effectCode == 0)
            {
                blinkCorrect = true;
                startTime = Time.time;
            }
            if (uiEffect.effectCode == 2)
            {
                solvedAnimation.animate = true;
                GameObjectManager.setGameObjectState(solvedAnimation.gameObject, true);
            }
        }
        else if (uiEffect.effectCode == 1)
        {
            // play wrong sound
            GameObjectManager.addComponent<PlaySound>(go, new { id = 1 }); // id refer to FPSController AudioBank
            blinkWrong = true;
            startTime = Time.time;
        }

        // Remove UI Effect
        GameObjectManager.removeComponent<PlayUIEffect>(go);
    }

    // Use to process your families.
    protected override void onProcess(int familiesUpdateCount)
    {
        workingBG = null;
        if (blinkCorrect)
            workingBG = rightBG;
        else if (blinkWrong)
            workingBG = wrongBG;
        if (workingBG)
        {
            float delta = Time.time - startTime;
            if (delta < 0.1 || (delta > 0.3 && delta < 0.4))
                GameObjectManager.setGameObjectState(workingBG, true);
            else {
                GameObjectManager.setGameObjectState(workingBG, false);
                if (delta >= 0.4)
                {
                    blinkCorrect = false;
                    blinkWrong = false;
                }
            }
        }
    }
}