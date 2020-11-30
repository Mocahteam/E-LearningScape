using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FYFY;
using FYFY_plugins.PointerManager;

public class IARDreamFragmentManager : FSystem {

	//Manage collected dream fragment in the IAR

	private Family f_buttonContainer = FamilyManager.getFamily(new AllOfComponents(typeof(PrefabContainer)), new AnyOfTags("DreamFragmentButtonContainer"));
	private Family f_contentContainer = FamilyManager.getFamily(new AllOfComponents(typeof(PrefabContainer)), new AnyOfTags("DreamFragmentContentContainer"));
	private Family f_contents = FamilyManager.getFamily(new AllOfComponents(typeof(PointerSensitive)), new AnyOfTags("DreamFragmentContent"));
	private Family f_canvas = FamilyManager.getFamily(new AllOfComponents(typeof(Canvas)));

	public static IARDreamFragmentManager instance;

	private RectTransform iarRectTransform;

	private GameObject draggedContent = null;
	//the offset is used to move the document from the point clicked and not the center
	private Vector2 offset;

	private DreamFragmentToggle tmpDFT;
	private RectTransform tmpRT;

	public IARDreamFragmentManager()
    {
        if (Application.isPlaying)
        {
			foreach(GameObject go in f_canvas)
			{
				if (go.name == "IAR")
				{
					iarRectTransform = go.GetComponent<RectTransform>();
					break;
				}
			}
        }
		instance = this;
    }

	public void OnClickDreamToggle(Toggle t)
    {
		tmpDFT = t.GetComponent<DreamFragmentToggle>();
		if (tmpDFT)
		{
			//change content's state depending on the value of the toggle
			GameObjectManager.setGameObjectState(tmpDFT.dreamFragmentContent, t.isOn);

			//change toggle's color depending on its value
			t.GetComponentInChildren<Image>().color = t.isOn ? tmpDFT.onColor : tmpDFT.offColor;
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
        if (Input.GetButtonDown("Fire1"))
        {
			//check if a document is clicked and set it as dragged
			foreach(GameObject go in f_contents)
            {
                if (go.GetComponent<PointerOver>())
                {
					draggedContent = go;
					tmpRT = go.GetComponent<RectTransform>();
					offset = new Vector2(tmpRT.position.x - Input.mousePosition.x - 111.2f, tmpRT.position.y - Input.mousePosition.y + 28.8f);
					Debug.Log(offset);
					//put the go above the others in hierarchy so that it is seen above the others
					GameObjectManager.setGameObjectParent(go, go.transform.parent.gameObject, true);
					break;
                }
            }
        }

        if (draggedContent)
        {
			//check if drag button is released to stop dragging
            if (Input.GetButtonUp("Fire1"))
            {
				draggedContent = null;
            }
            //move the dragged object
            else
			{
				// compute mouse position from screen center
				Vector3 pos = new Vector3((Input.mousePosition.x + offset.x) / Screen.width - 0.5f, (Input.mousePosition.y + offset.y) / Screen.height - 0.5f, 0);
				// correct position depending on canvas scale (0.6) and screen size comparing to reference size (800:500 => 16:10 screen ratio)
				draggedContent.transform.localPosition = Vector3.Scale(pos, iarRectTransform.sizeDelta / 1f);
			}
        }
	}
}