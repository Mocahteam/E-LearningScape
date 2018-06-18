using UnityEngine;
using FYFY;
using UnityEngine.UI;

public class TimerSystem : FSystem {

    private Family texts = FamilyManager.getFamily(new AllOfComponents(typeof(Text)));
    private Family timings = FamilyManager.getFamily(new AnyOfTags("Timings"));

    private Text timer; //text ui displaying the timer
    private float initialTime;  //time at the beginning of the game

	private GameObject forGO;

    public TimerSystem()
    {
        if (Application.isPlaying)
        {
            initialTime = Time.time;
            timer = null;
            int nbText = texts.Count;
            for (int i = 0; i < nbText; i++)
            {
                forGO = texts.getAt(i);
                if (forGO.name == "CurrentTime")
                {
                    timer = forGO.GetComponent<Text>();
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
        float time = Time.time - initialTime;
        int hours = (int)time / 3600;
        int minutes = (int)(time % 3600)/60;
        int seconds = (int)(time % 3600) % 60;
        string t = string.Concat(hours.ToString("D2"),":",minutes.ToString("D2"),":",seconds.ToString("D2")); //time text in the format "00:00:00"
        timer.text = t;
        if (Timer.addTimer) //when true, the current time is saved and displayed on screen
        {
			int nbTimings = timings.Count;
			for(int i = 0; i < nbTimings; i++)
            {
				forGO = timings.getAt (i);
				if (!forGO.activeSelf) //find an unused timing ui text
                {
                    GameObjectManager.setGameObjectState(forGO,true);
					forGO.GetComponent<Text>().text = t;
                    break;
                }
            }
            Timer.addTimer = false;
        }
    }
}