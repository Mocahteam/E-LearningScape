using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class EndDoor : MonoBehaviour {

    private bool canReadEnding = true;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<FirstPersonController>())
        {
            Debug.Log("OnTriggerEnter");
            if (other.gameObject.transform.position.x - this.transform.position.x < 0 && canReadEnding)
            {
                StoryDisplaying.readingEnding = true;
                other.gameObject.GetComponent<FirstPersonController>().enabled = false;
                Cursor.visible = false;
            }
            else
            {
                canReadEnding = false;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.GetComponent<FirstPersonController>())
        {
            Debug.Log("OnTriggerExit");
            canReadEnding = true;
        }
    }
}
