using UnityEngine;
using FYFY;
using UnityEngine.UI;

public class AnimateSprites : FSystem {

    private Family animatedSprites = FamilyManager.getFamily(new AllOfComponents(typeof(AnimatedSprites), typeof(Image)));

    private AnimatedSprites tmpAS;
    private GameObject tmpGo;
    private float lastChangeTime = -Mathf.Infinity;
    private AnimatedSprites solvedAnimation;

    public AnimateSprites()
    {
        if (Application.isPlaying)
        {
            int nb = animatedSprites.Count;
            for (int i = 0; i < nb; i++)
            {
                tmpGo = animatedSprites.getAt(i);
                tmpGo.GetComponent<AnimatedSprites>().usedSpriteID = 0;
                if (tmpGo.name == "Solved")
                {
                    solvedAnimation = tmpGo.GetComponent<AnimatedSprites>();
                }
            }
        }
    }

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {
        if(Time.time - lastChangeTime > 1f / 10)
        {
            lastChangeTime = Time.time;
            int nb = animatedSprites.Count;
            for (int i = 0; i < nb; i++)
            {
                tmpAS = animatedSprites.getAt(i).GetComponent<AnimatedSprites>();
                if (tmpAS.gameObject.activeSelf && tmpAS.animate)
                {
                    tmpAS.usedSpriteID++;
                    if (tmpAS.usedSpriteID == tmpAS.sprites.Length)
                    {
                        tmpAS.usedSpriteID = 0;
                        if (!tmpAS.loop)
                        {
                            tmpAS.animate = false;
                            if (tmpAS.disableWhenFinished)
                                GameObjectManager.setGameObjectState(tmpAS.gameObject, false);
                        }
                    }
                    tmpAS.GetComponent<Image>().sprite = tmpAS.sprites[tmpAS.usedSpriteID];
                    if (tmpAS.stopable && (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Return)))
                    {
                        tmpAS.animate = false;
                        tmpAS.usedSpriteID = 0;
                        tmpAS.GetComponent<Image>().sprite = tmpAS.sprites[0];
                        GameObjectManager.setGameObjectState(tmpAS.gameObject, false);
                    }
                }
            }
        }
	}
}