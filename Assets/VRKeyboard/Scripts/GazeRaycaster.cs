/***
 * Author: Yunhan Li 
 * Any issue please contact yunhn.lee@gmail.com
 ***/

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace VRKeyboard.Utils {
    public class GazeRaycaster : MonoBehaviour {
        #region Public Variables
        public float loadingTime;
        public Image circle;
        #endregion

        #region Private Variables
        private string lastTargetName = "";
        #endregion

        #region MonoBehaviour Callbacks
        void FixedUpdate() {
            RaycastHit hit;

            Vector3 fwd = transform.TransformDirection(Vector3.forward);

            if (Physics.Raycast(transform.position, fwd, out hit)) {

                // Trigger events only if we hit the keys or operation button
                if (hit.transform.tag == "VRGazeInteractable") {
                    // Check if we have already gazed over the object.
                    if (lastTargetName == hit.transform.name) {
                        return;
                    }

                    // Set the last hit if last targer is empty 
                    if (lastTargetName == "") {
                        lastTargetName = hit.transform.name;
                    }

                    // Check if current hit is same with last one;
                    if (hit.transform.name != lastTargetName) {
                        circle.fillAmount = 0f;
                        lastTargetName = hit.transform.name;
                        return;
                    }

                    StartCoroutine(FillCircle(hit.transform));
                    return;
                } else {
                    ResetGazer();
                }
            } else {
                ResetGazer();
            }
        }
        #endregion

        #region Private Methods
        private IEnumerator FillCircle(Transform target) {
            // When the circle starts to fill, reset the timer.
            float timer = 0f;
            circle.fillAmount = 0f;

            while (timer < loadingTime) {
                if (target.name != lastTargetName) {
                    yield break;
                }

                timer += Time.deltaTime;
                circle.fillAmount = timer / loadingTime;
                yield return null;
            }

            circle.fillAmount = 1f;

            if (target.GetComponent<Button>()) {
                target.GetComponent<Button>().onClick.Invoke();
            }

            ResetGazer();
        }

        // Reset the loading circle to initial, and clear last detected target.
        private void ResetGazer() {
            if (circle == null) {
                Debug.LogError("Please assign target loading image, (ie. circle image)");
                return;
            }

            circle.fillAmount = 0f;
            lastTargetName = "";
        }
        #endregion
    }
}