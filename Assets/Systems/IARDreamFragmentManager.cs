using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FYFY;
using FYFY_plugins.PointerManager;
using TMPro;
using UnityEngine.EventSystems;

public class IARDreamFragmentManager : FSystem {

	//Manage collected dream fragment in the IAR

	private Family f_contentContainer = FamilyManager.getFamily(new AllOfComponents(typeof(PrefabContainer)), new AnyOfTags("DreamFragmentContentContainer"));
	private Family f_documents = FamilyManager.getFamily(new AllOfComponents(typeof(PointerSensitive)), new AnyOfTags("IARDocument"));
	private Family f_buttons = FamilyManager.getFamily(new AnyOfTags("DreamFragmentButtons"));
	private Family f_canvas = FamilyManager.getFamily(new AllOfComponents(typeof(Canvas)));
	private Family f_dreamFragments = FamilyManager.getFamily(new AllOfComponents(typeof(DreamFragment)));
	private Family f_focusedToggles = FamilyManager.getFamily(new AllOfComponents(typeof(DreamFragmentToggle), typeof(Toggle), typeof(PointerSensitive), typeof(PointerOver)));
    private Family f_togglableFragments = FamilyManager.getFamily(new AllOfComponents(typeof(DreamFragmentToggle)));

    public static IARDreamFragmentManager instance;
	public static bool virtualDreamFragment = true;

	private RectTransform iarRectTransform;
	private RectTransform contentContainerRT;
	//button to open the link of a dream fragment
	private GameObject onlineButton;

	// selectedIARFragment is either the gameobject of a dream fragment toggle or a dream fragment content
	private GameObject selectedIARFragment;
	private DreamFragment selectedDreamFragment;
	private GameObject draggedDocument = null;
	private GameObject selectedDocument = null;
	private GameObject mouseOverToggle = null;
	//the offset is used to move the document from the point clicked and not the center
	private Vector2 offset;

	private DreamFragmentToggle tmpDFToggle;
	private RectTransform tmpRT;
	private DreamFragment tmpDreamFragment;
	private GameObject tmpGO;
	private Image tmpImage;

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
				Debug.LogError("Missing right panel for the dream fragment tab in IAR.");

			foreach(GameObject go in f_buttons)
            {
				if (go.name == "SeeMore")
					onlineButton = go;
            }

			// Add callbacks for mouse over toggle
			f_focusedToggles.addEntryCallback(OnMouseEnterToggle);
			f_focusedToggles.addExitCallback(OnMouseExitToggle);
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

			//set game object as last sibling in hierarchy to see it above the others
			//(do it even when the object is disabled to prevent a bug happening when it is activated again)
			tmpDFToggle.dreamFragmentContent.transform.SetAsLastSibling();

			if (!t.isOn && t.gameObject != selectedIARFragment)
				t.isOn = true;

			//change content's state depending on the value of the toggle
			GameObjectManager.setGameObjectState(tmpDFToggle.dreamFragmentContent, t.isOn);

            //unhighligh in left panel previous controller
            if (selectedDocument != null)
            {
				GameObject togglableFragment = selectedDocument.GetComponentInParent<LinkedWith>().link;
				DreamFragmentToggle dft = togglableFragment.GetComponent<DreamFragmentToggle>();
				togglableFragment.GetComponentInChildren<Image>().sprite = dft.onState;
				dft.currentState = dft.onState;
            }
            selectedDocument = null;
			if (t.isOn)
			{
				//highligh in left panel new controller
				t.GetComponentInChildren<Image>().sprite = tmpDFToggle.selectedState;
				tmpDFToggle.currentState = tmpDFToggle.selectedState;
				selectedIARFragment = t.gameObject;
                GameObjectManager.addComponent<PlaySound>(selectedIARFragment, new { id = 13 }); // id refer to FPSController AudioBank

                if (tmpDFToggle.dreamFragmentContent.transform.childCount > 0)
					selectedDocument = tmpDFToggle.dreamFragmentContent.transform.GetChild(0).gameObject;
			}
            else
			{
				t.GetComponentInChildren<Image>().sprite = tmpDFToggle.offState;
				tmpDFToggle.currentState = tmpDFToggle.offState;
                GameObjectManager.addComponent<PlaySound>(t.gameObject, new { id = 14 }); // id refer to FPSController AudioBank
                selectedIARFragment = null;
            }

            SetButtonsState();

			GameObjectManager.addComponent<ActionPerformedForLRS>(t.gameObject, new
			{
					verb = t.isOn ? "activated": "deactivated",
					objectType = "dreamFragment",
					activityExtensions = new Dictionary<string, string>() {
						{ "value", tmpDFToggle.dreamFragmentSrc.name }
					}
			});
		}
	}

	// Changes toggle sprite to focused state on mouse enter
	public void OnMouseEnterToggle(GameObject go)
    {
		mouseOverToggle = go;
		tmpDFToggle = go.GetComponent<DreamFragmentToggle>();
		tmpImage = go.GetComponentInChildren<Image>();

		tmpDFToggle.currentState = tmpImage.sprite;
		tmpImage.sprite = tmpDFToggle.focusedState;

		//put content in first plane
		tmpDFToggle.dfContentSibling = tmpDFToggle.dreamFragmentContent.transform.GetSiblingIndex();
		tmpDFToggle.dreamFragmentContent.transform.SetAsLastSibling();
	}

	// Changes toggle sprite back to the state before mouse over
	public void OnMouseExitToggle(int instanceID)
	{
        if (mouseOverToggle)
		{
			tmpDFToggle = mouseOverToggle.GetComponent<DreamFragmentToggle>();
			tmpImage = mouseOverToggle.GetComponentInChildren<Image>();

			tmpImage.sprite = tmpDFToggle.currentState;
			tmpDFToggle.currentState = null;
			mouseOverToggle = null;
			tmpDFToggle.dreamFragmentContent.transform.SetSiblingIndex(tmpDFToggle.dfContentSibling);
		}

		// put selected content on first plane if it is defined
		if (selectedIARFragment != null)
			selectedIARFragment.GetComponent<DreamFragmentToggle>().dreamFragmentContent.transform.SetAsLastSibling();
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
			}
			GameObjectManager.addComponent<ActionPerformedForLRS>(selectedDreamFragment.gameObject, new
			{
				verb = "accessed",
				objectType = "link",
				activityExtensions = new Dictionary<string, string>() {
					{ "type", "dreamFragment" },
					{ "value", selectedDreamFragment.gameObject.name },
					{ "link", selectedDreamFragment.urlLink }
				}
			});
		}
		else
			GameObjectManager.setGameObjectState(onlineButton, false);
    }

	public void RotateDocument(float angle)
    {
        if (selectedDocument)
		{
			selectedDocument.GetComponent<RectTransform>().Rotate(0, 0, angle);
		}
	}

	public void ZoomDocument(float value)
    {
        if (selectedDocument)
		{
			tmpRT = selectedDocument.GetComponent<RectTransform>();
			tmpRT.localScale += Vector3.one * value / 100;
			//check minimum and maximum values
			if (tmpRT.localScale.x < 0.1f)
				tmpRT.localScale = Vector3.one * 0.1f;
			else if (tmpRT.localScale.x > 3f)
				tmpRT.localScale = Vector3.one * 3f;
		}
	}

	/// <summary>
	/// Resets the selected dream fragment to the state it was when it was created (zoom, rotation and documents positions).
	/// </summary>
	public void ResetFragment()
	{
        if (selectedIARFragment)
        {
			//get the dream fragment content
			tmpDFToggle = selectedIARFragment.GetComponent<DreamFragmentToggle>();
			if (tmpDFToggle)
				tmpGO = tmpDFToggle.dreamFragmentContent;
			else
				tmpGO = selectedIARFragment;

			int posID;
			int l = tmpGO.transform.childCount;
			float gap = 30;
			for (int i = 0; i < l; i++)
			{
				//if there are several document for one dream fragment, give them different position to make them visible
				//(here we put a gap of 30 between each, alternating left and right)
				tmpRT = tmpGO.transform.GetChild(i).GetComponent<RectTransform>();
				posID = l - i - 1;
				tmpRT.anchoredPosition = new Vector2((l % 2 == 0 ? gap / 2 : 0) + gap * (posID / 2 + posID % 2) * (posID % 2 == 0 ? 1 : -1), 0);
				//reset rotation and scale
				tmpRT.localRotation = Quaternion.Euler(0, 0, 0);
				tmpRT.localScale = Vector3.one;
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
        if (Input.GetButtonDown("Fire1"))
        {
			//check if a document is clicked and set it as dragged
			foreach (GameObject go in f_documents)
            {
                if (go.GetComponent<PointerOver>())
				{
					GameObject togglableFragment;
					DreamFragmentToggle dft;
					//unhighligh in left panel previous controller
					if (selectedDocument != null)
					{
						togglableFragment = selectedDocument.GetComponentInParent<LinkedWith>().link;
						dft = togglableFragment.GetComponent<DreamFragmentToggle>();
						togglableFragment.GetComponentInChildren<Image>().sprite = dft.onState;
						dft.currentState = dft.onState;
                    }
                    draggedDocument = go;
					selectedDocument = go;
					selectedIARFragment = selectedDocument.transform.parent.GetComponent<LinkedWith>().link;
					tmpRT = go.GetComponent<RectTransform>();
					float screenOffsetX = (Screen.width - contentContainerRT.sizeDelta.x) / 11.5f;
					float screenOffsetY = (Screen.height - contentContainerRT.sizeDelta.y) / 14;
					offset = new Vector2(tmpRT.position.x - Input.mousePosition.x - screenOffsetX, tmpRT.position.y - Input.mousePosition.y + screenOffsetY);
					//put the go above the others in hierarchy so that it is seen above the others
					GameObjectManager.setGameObjectParent(go, go.transform.parent.gameObject, true);
					//set game object as last sibling in hierarchy to see it above the others
					go.transform.SetAsLastSibling();
					go.transform.parent.SetAsLastSibling();
					SetButtonsState();
					//highligh in left panel new controller
					togglableFragment = selectedDocument.GetComponentInParent<LinkedWith>().link;
					dft = togglableFragment.GetComponent<DreamFragmentToggle>();
					togglableFragment.GetComponentInChildren<Image>().sprite = dft.selectedState;
					dft.currentState = dft.selectedState;
					break;
                }
            }
        }

        if (draggedDocument)
        {
			//check if drag button is released to stop dragging
            if (Input.GetButtonUp("Fire1"))
				draggedDocument = null;
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

        if (selectedDocument)
			// zoom documents with mouse wheel
			ZoomDocument(Input.mouseScrollDelta.y * 3f);
	}

	/// <summary>
	/// This function takes a dream fragment toggle or a dream fragment content and
	/// returns the dream fragment corresponding to the id in the last 2 characters of the object's name
	/// </summary>
	/// <param name="go">The gameobject of a dream fragment toggle or content</param>
	/// <returns></returns>
	private DreamFragment GetDreamFragment(GameObject go)
	{
		int id = GetDreamFragmentID(go);

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
	/// This function takes a dream fragment toggle or a dream fragment content and
	/// returns the dream fragment id (last 2 characters of the object's name)
	/// </summary>
	/// <param name="go">The gameobject of a dream fragment toggle or content</param>
	/// <returns></returns>
	private int GetDreamFragmentID(GameObject go)
	{
		if (go)
		{
			try
			{
				return int.Parse(go.name.Substring(go.name.Length - 2, 2));
			}
			catch (Exception) { }
		}
		return -1;
	}

	/// <summary>
	/// Checks in the dream fragment corresponding to the selected toggle if there is a link
	/// and set the state accordingly.
	/// </summary>
	private void SetOnlineButtonState()
	{
		selectedDreamFragment = null;

        if (selectedIARFragment)
			selectedDreamFragment = GetDreamFragment(selectedIARFragment);

		// activate link button if there is a link
		if(selectedDreamFragment && selectedDreamFragment.urlLink != null && selectedDreamFragment.urlLink != "")
		{
			GameObjectManager.setGameObjectState(onlineButton, true);
			onlineButton.GetComponent<TextMeshProUGUI>().text = selectedDreamFragment.linkButtonText;
		}
		else
			GameObjectManager.setGameObjectState(onlineButton, false);
	}

	/// <summary>
	/// Enable or disable buttons used to manipulate documents depending on whether there is a document selected.
	/// </summary>
	private void SetButtonsState()
    {
		foreach (GameObject go in f_buttons)
			GameObjectManager.setGameObjectState(go, selectedDocument && selectedDocument.activeSelf);

		SetOnlineButtonState();
    }
}