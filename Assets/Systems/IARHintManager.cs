using UnityEngine;
using FYFY;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;
using UnityEngine.SceneManagement;
using FYFY_plugins.Monitoring;
using System.Collections.Generic;
using TMPro;
using System;
using System.IO;

public class IARHintManager : FSystem {

    // manage IAR menu (last-1 tab)

    private Family f_scrollView = FamilyManager.getFamily(new AllOfComponents(typeof(ScrollRect), typeof(PrefabContainer)));
    private Family f_helpTabContent = FamilyManager.getFamily(new AnyOfTags("HelpTabContent"), new AllOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));
    private Family f_visibleHintsIAR = FamilyManager.getFamily(new AllOfComponents(typeof(HintContent)), new AllOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));
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
    /// color of a hint button
    /// </summary>
    private ColorBlock colorHint;
    /// <summary>
    /// color of a new hint button
    /// </summary>
    private ColorBlock colorNewHint;
    /// <summary>
    /// color of the selected hint button
    /// </summary>
    private ColorBlock colorSelectedHint;

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

    private bool needRefresh = true;

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
            hintLinkButton.onClick.AddListener(OnClickHintLinkButton);

            //set hint button colors values
            colorHint = new ColorBlock();
            colorHint.normalColor = new Color(200, 200, 200, 255) / 256;
            colorHint.highlightedColor = new Color(200, 200, 235, 255) / 256;
            colorHint.pressedColor = new Color(150, 150, 150, 255) / 256;
            colorHint.disabledColor = new Color(130, 130, 130, 130) / 256;
            colorHint.colorMultiplier = 1;
            colorNewHint = ColorBlock.defaultColorBlock;
            /*colorNewHint.normalColor = new Color(254, 255, 189, 255) / 256;
            colorNewHint.highlightedColor = new Color(248, 255, 137, 255) / 256;
            colorNewHint.pressedColor = new Color(199, 192, 98, 255) / 256;
            colorNewHint.disabledColor = new Color(253, 255, 137, 128) / 256;
            colorNewHint.colorMultiplier = 1;*/
            colorSelectedHint = ColorBlock.defaultColorBlock;
            colorSelectedHint = new ColorBlock();
            colorSelectedHint.normalColor = new Color(254, 255, 189, 255) / 256;
            colorSelectedHint.highlightedColor = new Color(248, 255, 137, 255) / 256;
            colorSelectedHint.pressedColor = new Color(199, 192, 98, 255) / 256;
            colorSelectedHint.disabledColor = new Color(253, 255, 137, 128) / 256;
            colorSelectedHint.colorMultiplier = 1;
        }
    }

    protected override void onProcess(int familiesUpdateCount)
    {
        // Remove IAR Hints if associated action is not reachable or if the enigma is resolved
        if (needRefresh)
        {
            foreach (GameObject hint in f_visibleHintsIAR)
            {
                HintContent hc = hint.GetComponent<HintContent>();
                if (hc.monitor)
                {
                    bool endActionReachable = MonitoringManager.getNextActionsToReachPlayerObjective(MonitoringManager.Instance.PetriNetsName[hc.monitor.fullPnSelected], int.MaxValue).Count > 0;

                    bool stillReachable = false;
                    if (endActionReachable)
                        stillReachable = hc.monitor.isStillReachable(hc.actionName);

                    if (!endActionReachable || !stillReachable)
                    {
                        // remove the button
                        GameObjectManager.unbind(hint);
                        GameObject.Destroy(hint);
                    }
                }
            }
            needRefresh = false;
        }

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

    private void onHelpTabExit (int uniqueId)
    {
        this.Pause = true;
    }

    private void onHelpTabSelected(GameObject go)
    {
        needRefresh = true; // ask to refresh list of hints
        this.Pause = false;
    }

    private void onNewButtonAvailable(GameObject newHint)
    {
        Button hintButton = newHint.GetComponent<Button>();
        hintButton.onClick.AddListener(delegate { OnClickHint(hintButton); });
        hintButton.colors = colorNewHint;
        hintCounter++;
        newHint.transform.GetChild(0).GetComponent<TMP_Text>().text += " " + hintCounter;

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
        if (selectedHint)
            //change the color of the previousliy selected button
            selectedHint.colors = colorHint;

        selectedHint = b;
        selectedHint.colors = colorSelectedHint;
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
            activityExtensions = new Dictionary<string, List<string>>() {
                { "type", new List<string>() { "hint" } },
                { "content", new List<string>() { b.GetComponent<HintContent>().text } }
            }
        });
    }

    /// <summary>
    /// Called when the link button of an hint on the right part of help tab in IAR is pressed
    /// and open the link of the hint
    /// </summary>
    private void OnClickHintLinkButton()
    {
        try
        {
            Application.OpenURL(hintLink);

            GameObjectManager.addComponent<ActionPerformedForLRS>(hintLinkButton.gameObject, new
            {
                verb = "read",
                objectType = "viewable",
                objectName = "hintLink",
                activityExtensions = new Dictionary<string, List<string>>() {
                    { "link", new List<string>() { hintLink } }
                }
            });
        }
        catch (Exception)
        {
            Debug.LogError(string.Concat("Invalid hint link: \"", hintLink, "\""));
            File.AppendAllText("Data/UnityLogs.txt", string.Concat(System.Environment.NewLine, "[", DateTime.Now.ToString("yyyy.MM.dd.hh.mm"), "] Error - Invalid hint link: \"", hintLink, "\"."));
        }
    }
}