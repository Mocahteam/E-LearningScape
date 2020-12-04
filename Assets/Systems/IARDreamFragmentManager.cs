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
	private Family f_buttons = FamilyManager.getFamily(new AnyOfTags("DreamFragmentButtons"));
	private Family f_canvas = FamilyManager.getFamily(new AllOfComponents(typeof(Canvas)));
	private Family f_dreamFragments = FamilyManager.getFamily(new AllOfComponents(typeof(DreamFragment)));

	public static IARDreamFragmentManager instance;

	private RectTransform iarRectTransform;
	private RectTransform contentContainerRT;
	//button to open the link of a dream fragment
	private GameObject onlineButton;

	// selectedIARFragment is either the gameobject of a dream fragment toggle or a dream fragment content
	private GameObject selectedIARFragment;
	private DreamFragment selectedDreamFragment;
	private GameObject draggedDocument = null;
	//the offset is used to move the document from the point clicked and not the center
	private Vector2 offset;

	private DreamFragmentToggle tmpDFToggle;
	private RectTransform tmpRT;
	private DreamFragment tmpDreamFragment;

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

			foreach(GameObject go in f_buttons)
            {
				if (go.name == "SeeMore")
					onlineButton = go;
            }
        }
		instance = this;
    }

	public void OnClickDreamToggle(Toggle t)
    {
		tmpDFToggle = t.GetComponent<DreamFragmentToggle>();
		if (tmpDFToggle)
		{
			//remove "new"
			if (t.GetComponent<NewDreamFragment>())
				GameObjectManager.removeComponent<NewDreamFragment>(t.gameObject);

			//change content's state depending on the value of the toggle
			GameObjectManager.setGameObjectState(tmpDFToggle.dreamFragmentContent, t.isOn);

			if (t.isOn)
			{
				t.GetComponentInChildren<Image>().color = tmpDFToggle.onColor;
				selectedIARFragment = t.gameObject;
				//set game object as last sibling in hierarchy to see it above the others
				tmpDFToggle.dreamFragmentContent.transform.SetAsLastSibling();
			}
            else
			{
				t.GetComponentInChildren<Image>().color = tmpDFToggle.offColor;
				selectedIARFragment = null;
			}

			SetOnlineButtonState();
		}
	}

	public void OpenLink()
    {
		if (selectedDreamFragment)
		{
			try
			{
				Application.OpenURL(selectedDreamFragment.urlLink);
			}
			catch (Exception)
			{
				Debug.LogError(string.Concat("Invalid dream fragment link: ", selectedDreamFragment.urlLink));
				File.AppendAllText("Data/UnityLogs.txt", string.Concat(System.Environment.NewLine, "[", DateTime.Now.ToString("yyyy.MM.dd.hh.mm"), "] Error - Invalid dream fragment link: ", selectedDreamFragment.urlLink));
			}
			GameObjectManager.addComponent<ActionPerformedForLRS>(selectedDreamFragment.gameObject, new
			{
				verb = "accessed",
				objectType = "viewable",
				objectName = string.Concat(selectedDreamFragment.gameObject.name, "_Link"),
				activityExtensions = new Dictionary<string, List<string>>() { { "link", new List<string>() { selectedDreamFragment.urlLink } } }
			});
		}
		else
			GameObjectManager.setGameObjectState(onlineButton, false);
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
					selectedIARFragment = go.transform.parent.gameObject;
					tmpRT = go.GetComponent<RectTransform>();
					float screenOffsetX = (Screen.width - contentContainerRT.sizeDelta.x) / 11.5f;
					float screenOffsetY = (Screen.height - contentContainerRT.sizeDelta.y) / 14;
					offset = new Vector2(tmpRT.position.x - Input.mousePosition.x - screenOffsetX, tmpRT.position.y - Input.mousePosition.y + screenOffsetY);
					//put the go above the others in hierarchy so that it is seen above the others
					GameObjectManager.setGameObjectParent(go, go.transform.parent.gameObject, true);
					//set game object as last sibling in hierarchy to see it above the others
					go.transform.SetAsLastSibling();
					go.transform.parent.SetAsLastSibling();
					SetOnlineButtonState();
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

	/// <summary>
	/// This function takes a dream fragment toggle or a dream fragment content and
	/// returns the dream fragment corresponding to the id in the last 2 characters of the object's name
	/// </summary>
	/// <param name="go">The gameobject of a dream fragment toggle or content</param>
	/// <returns></returns>
	private DreamFragment GetDreamFragment(GameObject go)
	{
		int id = -1;
        if (go)
        {
            try
			{
				id = int.Parse(go.name.Substring(go.name.Length - 2, 2));
			}
            catch (Exception) { }
        }

		if(id > -1)
        {
			foreach(GameObject fragment in f_dreamFragments)
            {
				tmpDreamFragment = fragment.GetComponent<DreamFragment>();
				if (tmpDreamFragment.type == 0 && tmpDreamFragment.id == id)
					return tmpDreamFragment;
            }
        }

		return null;
	}

	/// <summary>
	/// Checks in the dream fragment corresponding to the selected toggle if there is a link
	/// and set the state accordingly
	/// </summary>
	private void SetOnlineButtonState()
	{
		selectedDreamFragment = null;

        if (selectedIARFragment)
			selectedDreamFragment = GetDreamFragment(selectedIARFragment);

		// activate link button if there is a link
		GameObjectManager.setGameObjectState(onlineButton, selectedDreamFragment && 
			selectedDreamFragment.urlLink != null && selectedDreamFragment.urlLink != "");
	}
}