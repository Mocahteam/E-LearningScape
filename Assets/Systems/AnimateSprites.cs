using UnityEngine;
using FYFY;
using UnityEngine.UI;

public class AnimateSprites : FSystem {

    private Family animatedSprites = FamilyManager.getFamily(new AllOfComponents(typeof(AnimatedSprites)));

    private AnimatedSprites tmpAS;

    public AnimateSprites()
    {
        int nb = animatedSprites.Count;
        for (int i = 0; i < nb; i++)
        {
            animatedSprites.getAt(i).GetComponent<AnimatedSprites>().usedSpriteID = 0;
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
        int nb = animatedSprites.Count;
        for(int i = 0; i < nb; i++)
        {
            tmpAS = animatedSprites.getAt(i).GetComponent<AnimatedSprites>();
            if (tmpAS.gameObject.activeSelf && tmpAS.animate)
            {
                tmpAS.usedSpriteID++;
                if (tmpAS.usedSpriteID == tmpAS.sprites.Length)
                {
                    tmpAS.usedSpriteID = 0;
                }
                tmpAS.GetComponent<Image>().sprite = tmpAS.sprites[tmpAS.usedSpriteID];
            }
        }
	}
}