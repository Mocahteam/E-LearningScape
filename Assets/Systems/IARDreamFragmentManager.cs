using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FYFY;
using FYFY_plugins.PointerManager;

public class IARDreamFragmentManager : FSystem {

	//Manage collected dream fragment in the IAR

	private Family f_buttonContainer = FamilyManager.getFamily(new AnyOfTags("DreamFragmentButtonContainer"));
	private Family f_contentContainer = FamilyManager.getFamily(new AllOfComponents(typeof(PrefabContainer)), new AnyOfTags("DreamFragmentContentContainer"));
	private Family f_contents = FamilyManager.getFamily(new AnyOfTags("DreamFragmentContent"));
	private Family f_documents = FamilyManager.getFamily(new AllOfComponents(typeof(PointerSensitive)), new AnyOfTags("IARDocument"));
	private Family f_canvas = FamilyManager.getFamily(new AllOfComponents(typeof(Canvas)));

	public static IARDreamFragmentManager instance;

	private RectTransform iarRectTransform;
	private RectTransform contentContainerRT;

	private GameObject draggedDocument = null;
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

			if (f_contentContainer.Count > 0)
				contentContainerRT = f_contentContainer.First().GetComponent<RectTransform>();
            else
			{
				Debug.LogError("Missing right panel for the dream fragment tab in IAR.");
				File.AppendAllText("Data/UnityLogs.txt", string.Concat(System.Environment.NewLine, "[", DateTime.Now.ToString("yyyy.MM.dd.hh.mm"), "] Error - Missing right panel for the dream fragment tab in IAR"));
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
			foreach(GameObject go in f_documents)
            {
                if (go.GetComponent<PointerOver>())
                {
					draggedDocument = go;
					tmpRT = go.GetComponent<RectTransform>();
					float screenOffsetX = (Screen.width - contentContainerRT.sizeDelta.x) / 5;
					float screenOffsetY = (Screen.height - contentContainerRT.sizeDelta.y) / 5;
					offset = new Vector2(tmpRT.position.x - Input.mousePosition.x - screenOffsetX, tmpRT.position.y - Input.mousePosition.y + screenOffsetY);
					Debug.Log(screenOffsetX + " " + screenOffsetY);
					//put the go above the others in hierarchy so that it is seen above the others
					GameObjectManager.setGameObjectParent(go, go.transform.parent.gameObject, true);
					break;
                }
            }
        }

        if (draggedDocument)
        {
			//check if drag button is released to stop dragging
            if (Input.GetButtonUp("Fire1"))
            {
				draggedDocument = null;
            }
            //move the dragged object
            else
			{
				// compute mouse position from screen center
				Vector3 pos = new Vector3((Input.mousePosition.x + offset.x) / Screen.width - 0.5f, 
					(Input.mousePosition.y + offset.y) / Screen.height - 0.5f, 0);
				// correct position depending on canvas scale (0.6) and screen size comparing to reference size (800:500 => 16:10 screen ratio)
				draggedDocument.transform.localPosition = Vector3.Scale(pos, iarRectTransform.sizeDelta);
			}
        }
	}
}