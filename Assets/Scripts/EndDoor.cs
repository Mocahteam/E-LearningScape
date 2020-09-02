using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;
using FYFY;

public class EndDoor : MonoBehaviour {

    public GameObject storyDisplayer;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<FirstPersonController>())
        {
            // show story
            storyDisplayer.GetComponent<StoryText>().storyProgression++;
            StoryDisplaying.instance.Pause = false;
            GameObjectManager.addComponent<PlaySound>(storyDisplayer, new { id = 8 }); // id refer to FPSController AudioBank
        }
    }
}
