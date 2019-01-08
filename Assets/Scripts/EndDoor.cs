using System.Collections;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class EndDoor : MonoBehaviour {

    public GameObject storyDisplayer;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<FirstPersonController>())
        {
            // show story
            storyDisplayer.GetComponent<StoryText>().storyProgression++;
            StoryDisplaying.instance.Pause = false;
        }
    }
}
