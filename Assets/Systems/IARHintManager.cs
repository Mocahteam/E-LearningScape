using UnityEngine;
using FYFY;
using UnityEngine.UI;
using FYFY_plugins.Monitoring;
using System.Collections.Generic;
using TMPro;
using System;
using System.IO;
using System.Collections;

public class IARHintManager : FSystem {

    // manage IAR menu (last-1 tab)

    private Family f_scrollView = FamilyManager.getFamily(new AllOfComponents(typeof(ScrollRect), typeof(PrefabContainer)));
    private Family f_helpTabContent = FamilyManager.getFamily(new AnyOfTags("HelpTabContent"), new AllOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));
    private Family f_enabledHintsIAR = FamilyManager.getFamily(new AllOfComponents(typeof(HintContent)), new AllOfProperties(PropertyMatcher.PROPERTY.ACTIVE_SELF));
    private Family f_description = FamilyManager.getFamily(new AnyOfTags("HelpDescriptionUI"));

    /// <summary>
    /// The content of the scroll view containing received hint buttons (left part of help tab in IAR)
    /// </summary>
    private RectTransform scrollViewContent;
    /// <summary>
    /// prefab used to instantiate hint buttons
    /// </summary>
    private GameObject hintButtonPrefab;

    private Button selectedHint = null;

    /// <summary>
    /// Description of the selected hint in IAR (right part of help tab in IAR)
    /// </summary>
    private TextMeshProUGUI hintTitle;
    private TextMeshProUGUI hintText;

    /// <summary>
    /// Count the number of hint given
    /// </summary>
    private int hintCounter = 0;

    /// <summary>
    /// used to open a link to get more info about a hint (disabled if link is empty)
    /// </summary>
    private Button hintLinkButton;
    /// <summary>
    /// contains the link of the last selected hint and used to open the link when the hintLinkButton is clicked
    /// </summary>
    private string hintLink;

    public static IARHintManager instance;

    public IARHintManager()
    {
        if (Application.isPlaying)
        {
            scrollViewContent = f_scrollView.First().transform.GetChild(0).GetChild(0).GetComponent<RectTransform>();
            hintButtonPrefab = f_scrollView.First().GetComponent<PrefabContainer>().prefab;

            f_helpTabContent.addEntryCallback(onHelpTabSelected);
            f_helpTabContent.addExitCallback(onHelpTabExit);

            f_enabledHintsIAR.addEntryCallback(onNewButtonAvailable);
            f_enabledHintsIAR.addExitCallback(onButtonRemoved);

            hintTitle = f_description.First().transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            hintText = f_description.First().transform.GetChild(1).GetComponent<TextMeshProUGUI>();
            hintLinkButton = f_description.First().transform.GetChild(2).GetComponent<Button>();

            // start coroutine that refresh list of hints each second
            MainLoop.instance.StartCoroutine(refreshHints());
        }
        instance = this;
    }

    protected override void onProcess(int familiesUpdateCount)
    {
        // reset hint positions
        int nbActivatedHint = scrollViewContent.GetComponentsInChildren<Button>().Length;
        scrollViewContent.sizeDelta = new Vector2(scrollViewContent.sizeDelta.x, (nbActivatedHint + 1) * hintButtonPrefab.GetComponent<RectTransform>().sizeDelta.y);
        float hintCpt = 0.5f;
        RectTransform[] tmpRTArray = scrollViewContent.GetComponentsInChildren<RectTransform>();
        for (int i = tmpRTArray.Length - 1; i >= 0; i--)
            if (tmpRTArray[i].GetComponent<Button>())
            {
                //tmpRTArray[i].anchoredPosition = Vector2.down * hintCpt * hintButtonPrefab.GetComponent<RectTransform>().sizeDelta.y;
                float targetPos = -1 * hintCpt * hintButtonPrefab.GetComponent<RectTransform>().sizeDelta.y;
                if (targetPos - tmpRTArray[i].anchoredPosition.y > -2 && targetPos - tmpRTArray[i].anchoredPosition.y < 2)
                    tmpRTArray[i].anchoredPosition = new Vector2(0, targetPos);
                else
                {
                    int way = 1;
                    if (targetPos < tmpRTArray[i].anchoredPosition.y)
                        way = -1;
                    tmpRTArray[i].anchoredPosition = new Vector2(0, tmpRTArray[i].anchoredPosition.y + 60 * way * Time.deltaTime);
                }
                hintCpt++;
            }
    }


    // Remove IAR Hints if associated action is not reachable or if the enigma is resolved or if another hint with higher level is displayed or if another hint with same content is already displayed
    private IEnumerator refreshHints()
    {
        yield return new WaitForSeconds(1); // wait one second to synchronize hints
        List<HintContent> checkCopy = new List<HintContent>();
        foreach (GameObject hint in f_enabledHintsIAR)
            checkCopy.Add(hint.GetComponent<HintContent>());
        foreach (GameObject hint in f_enabledHintsIAR)
        {
            HintContent hc = hint.GetComponent<HintContent>();
            if (hc.monitor)
            {
                bool endActionReachable = MonitoringManager.getNextActionsToReachPlayerObjective(MonitoringManager.Instance.PetriNetsName[hc.monitor.fullPnSelected], int.MaxValue).Count > 0;

                try
                {
                    bool stillReachable = false;
                    if (endActionReachable)
                        stillReachable = hc.monitor.isStillReachable(hc.actionName);

                    bool higherHint = false;
                    bool sameContent = false;
                    foreach (HintContent hc2 in checkCopy)
                    {
                        if (hc != hc2 && hc.monitor == hc2.monitor && hc.actionName == hc2.actionName && hc.level.CompareTo(hc2.level) < 0)
                        {
                            higherHint = true;
                            break;
                        }
                        if (hc != hc2 && hc.text == hc2.text)
                        {
                            sameContent = true;
                            break;
                        }
                    }


                    if (!endActionReachable || !stillReachable || higherHint || sameContent)
                    {
                        // remove the button
                        if (hint.GetComponent<Button>() == selectedHint)
                            selectedHint = null;
                        GameObjectManager.unbind(hint);
                        GameObject.Destroy(hint);
                        checkCopy.Remove(hc);
                    }
                }
                catch (TraceAborted ta)
                {
                    Debug.Log(ta.Message);
                }
            }
        }
        MainLoop.instance.StartCoroutine(refreshHints());
    }

    private void onHelpTabExit (int uniqueId)
    {
        this.Pause = true;
    }

    private void onHelpTabSelected(GameObject go)
    {
        this.Pause = false;
    }

    private void onNewButtonAvailable(GameObject newHint)
    {
        Button hintButton = newHint.GetComponent<Button>();
        hintButton.onClick.AddListener(delegate { OnClickHint(hintButton); });
        hintCounter++;
        newHint.transform.GetChild(0).GetComponent<TMP_Text>().text = LoadGameContent.gameContent.hintButtonText + " " + hintCounter;

        RectTransform tmpRT = newHint.GetComponent<RectTransform>();
        tmpRT.localScale = Vector3.one;
        tmpRT.offsetMin = new Vector2(0, tmpRT.offsetMin.y);
        tmpRT.offsetMax = new Vector2(0, tmpRT.offsetMax.y);
        tmpRT.anchoredPosition = new Vector2(0, -0.5f * hintButton.GetComponent<RectTransform>().sizeDelta.y);
    }

    private void onButtonRemoved(int uniqueInstanceId)
    {
        if (!selectedHint) {
            hintTitle.text = "";
            hintText.text = "";
            GameObjectManager.setGameObjectState(hintLinkButton.gameObject, false);
        }
    }

    /// <summary>
    /// Called when the player click on a hint button in the hint list in help tab of IAR
    /// </summary>
    /// <param name="b">The clicked button</param>
    private void OnClickHint(Button b)
    {
        SetNormalColor(b);

        selectedHint = b;
        HintContent tmpHC = selectedHint.GetComponent<HintContent>();
        //display hint info on the right part of the help tab in IAR
        hintTitle.text = b.GetComponentInChildren<TMP_Text>().text;
        hintText.text = tmpHC.text;
        if (tmpHC.link != "")
        {
            //if link filled, display link button
            hintLink = tmpHC.link;
            GameObjectManager.setGameObjectState(hintLinkButton.gameObject, true);
        }
        else
            GameObjectManager.setGameObjectState(hintLinkButton.gameObject, false);

        if (selectedHint.GetComponent<NewHint>())
            GameObjectManager.removeComponent<NewHint>(selectedHint.gameObject);

        GameObjectManager.addComponent<ActionPerformedForLRS>(b.gameObject, new
        {
            verb = "read",
            objectType = "feedback",
            objectName = string.Concat("hint_", b.transform.GetChild(0).GetComponent<TMP_Text>().text),
            activityExtensions = new Dictionary<string, string>() {
                { "type", "hint" },
                { "reference", tmpHC.monitor.id+"."+tmpHC.actionName },
                { "content", b.GetComponent<HintContent>().text }
            }
        });
    }

    public void SetNormalColor(Button b)
    {
        ColorBlock colorSelectedHint = new ColorBlock();
        colorSelectedHint.highlightedColor = b.colors.highlightedColor;
        colorSelectedHint.pressedColor = b.colors.pressedColor;
        colorSelectedHint.selectedColor = b.colors.selectedColor;
        colorSelectedHint.disabledColor = b.colors.disabledColor;
        colorSelectedHint.colorMultiplier = b.colors.colorMultiplier;
        colorSelectedHint.normalColor = new Color(114, 114, 114, 255) / 256;
        b.colors = colorSelectedHint;
    }

    /// <summary>
    /// Called when the link button of an hint on the right part of help tab in IAR is pressed
    /// and open the link of the hint
    /// </summary>
    public void OnClickHintLinkButton()
    {
        try
        {
            Application.OpenURL(hintLink);

            int monitorId = -1;
            string actionName = "";
            if (selectedHint)
            {
                HintContent tmpHC = selectedHint.GetComponent<HintContent>();
                monitorId = tmpHC.monitor.id;
                actionName = tmpHC.actionName;
            }

            GameObjectManager.addComponent<ActionPerformedForLRS>(hintLinkButton.gameObject, new
            {
                verb = "read",
                objectType = "viewable",
                objectName = "hintLink",
                activityExtensions = new Dictionary<string, string>() {
                    { "reference", monitorId + "." + actionName },
                    { "link", hintLink }
                }
            });
        }
        catch (Exception)
        {
            Debug.LogError(string.Concat("Invalid hint link: \"", hintLink, "\""));
        }
    }
}