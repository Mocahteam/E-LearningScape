using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class EndDoor : MonoBehaviour {

    public GameObject storyDisplayer;

    private bool canReadEnding = true;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<FirstPersonController>())
        {
            if (other.gameObject.transform.position.x - this.transform.position.x < 0 && canReadEnding)
            {
                // show story
                storyDisplayer.GetComponent<StoryText>().storyProgression++;
                StoryDisplaying.instance.Pause = false;
            }
            else
                canReadEnding = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.GetComponent<FirstPersonController>())
            canReadEnding = true;
    }
}
