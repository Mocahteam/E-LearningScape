using UnityEngine;
using FYFY;
using UnityEngine.UI;

public class AnimateSprites : FSystem {

    private Family animatedSprites = FamilyManager.getFamily(new AllOfComponents(typeof(AnimatedSprites)));
    private Family test = FamilyManager.getFamily(new AllOfComponents(typeof(PointerEnterName)));
    private Family images = FamilyManager.getFamily(new AllOfComponents(typeof(Image)));

    private AnimatedSprites tmpAS;
    private float lastChangeTime = -Mathf.Infinity;
    private AnimatedSprites solvedAnimation;

    public AnimateSprites()
    {
        if (Application.isPlaying)
        {
            int nb = animatedSprites.Count;
            for (int i = 0; i < nb; i++)
            {
                animatedSprites.getAt(i).GetComponent<AnimatedSprites>().usedSpriteID = 0;
            }
            nb = images.Count;
            for (int i = 0; i < nb; i++)
            {
                if (images.getAt(i).name == "Solved")
                {
                    solvedAnimation = images.getAt(i).GetComponent<AnimatedSprites>();
                }
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
                            GameObjectManager.setGameObjectState(tmpAS.gameObject, !tmpAS.disableWhenFinished);
                        }
                    }
                    tmpAS.GetComponent<Image>().sprite = tmpAS.sprites[tmpAS.usedSpriteID];
                }
            }
        }
        if (solvedAnimation.gameObject.activeSelf && Input.GetMouseButtonDown(0))
        {
            solvedAnimation.animate = false;
            solvedAnimation.usedSpriteID = 0;
            solvedAnimation.GetComponent<Image>().sprite = solvedAnimation.sprites[0];
            GameObjectManager.setGameObjectState(solvedAnimation.gameObject, false);
        }
	}
}