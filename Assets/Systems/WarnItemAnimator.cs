using UnityEngine;
using FYFY;
using UnityEngine.UI;

public class WarnItemAnimator : FSystem {

    // This system play Sprites animation

    private Family f_warnableItems = FamilyManager.getFamily(new AllOfComponents(typeof(WarnItem), typeof(Image)), new AnyOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));

    private Image img;
    private WarnItem wi;

    public WarnItemAnimator()
    {
        if (Application.isPlaying)
        {
            f_warnableItems.addEntryCallback(onItemToWarn);
            f_warnableItems.addExitCallback(onItemNotWarnable);
        }
    }

    private void onItemToWarn(GameObject go)
    {
        this.Pause = false;
    }

    private void onItemNotWarnable (int uniqueInstanceId)
    {
        if (f_warnableItems.Count <= 0)
            this.Pause = true;
    }

    // Use to process your families.
    protected override void onProcess(int familiesUpdateCount)
    {
        // parse each animated sprite
        int nb = f_warnableItems.Count;
        for (int i = 0; i < nb; i++)
        {
            img = f_warnableItems.getAt(i).GetComponent<Image>();
            wi = f_warnableItems.getAt(i).GetComponent<WarnItem>();

            wi.alpha += wi.way * wi.speed * Time.deltaTime;
            if (wi.alpha < 0)
            {
                wi.alpha = 0;
                wi.way = 1;
            }
            else if (wi.alpha > wi.maxAlpha)
            {
                wi.alpha = wi.maxAlpha;
                wi.way = -1;
            }
            img.color = new Color(img.color.r, img.color.g, img.color.b, wi.alpha);
        }
    }
}