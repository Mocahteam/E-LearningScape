using UnityEngine;
using FYFY;
using UnityEngine.UI;

public class SpritesAnimator : FSystem {

    // This system play Sprites animation

    private Family animatedSprites = FamilyManager.getFamily(new AllOfComponents(typeof(AnimatedSprites), typeof(Image)), new AnyOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));

    private AnimatedSprites tmpAS;
    private float lastChangeTime = -Mathf.Infinity;

    public static SpritesAnimator instance;

    public SpritesAnimator()
    {
        if (Application.isPlaying)
        {
            // Set each animated Sprite on its first frame
            int nb = animatedSprites.Count;
            for (int i = 0; i < nb; i++)
            {
                animatedSprites.getAt(i).GetComponent<AnimatedSprites>().usedSpriteID = 0;
            }
        }
        instance = this;
    }

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {
        if(Time.time - lastChangeTime > 1f / 10)
        {
            lastChangeTime = Time.time;
            // parse each animated sprite
            int nb = animatedSprites.Count;
            for (int i = 0; i < nb; i++)
            {
                tmpAS = animatedSprites.getAt(i).GetComponent<AnimatedSprites>();
                if (tmpAS.animate)
                {
                    tmpAS.usedSpriteID++;
                    // if last animation frame is reached
                    if (tmpAS.usedSpriteID == tmpAS.sprites.Length)
                    {
                        // restart to the first one
                        tmpAS.usedSpriteID = 0;
                        // if loop is disable => stop animation
                        if (!tmpAS.loop)
                        {
                            tmpAS.animate = false;
                            if (tmpAS.disableWhenFinished)
                                GameObjectManager.setGameObjectState(tmpAS.gameObject, false);
                        }
                    }
                    // Swicth to the current frame
                    tmpAS.GetComponent<Image>().sprite = tmpAS.sprites[tmpAS.usedSpriteID];
                    
                    // in case of animation is stoppable
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