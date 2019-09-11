using FYFY;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EventWrapper : MonoBehaviour {

	public void CloseFragmentUI()
    {
        DreamFragmentCollecting.instance.CloseFragmentUI();
    }

    public void OpenFragmentLink()
    {
        DreamFragmentCollecting.instance.OpenFragmentLink();
    }

    public void OnPlayerAskHelp()
    {
        HelpSystem.instance.OnPlayerAskHelp();
    }

    public void OnClickHintLinkButton()
    {
        IARHintManager.instance.OnClickHintLinkButton();
    }

    public void IarResumeButton()
    {
        IARTabNavigation.instance.closeIar();
    }

    public void IarRestartButton()
    {
        GameObjectManager.loadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void StartGame()
    {
        MenuSystem.instance.StartGame();
    }

    public void IarSwitchTab(GameObject tab)
    {
        IARTabNavigation.instance.SwitchTab(tab);
    }

    public void moveWheelUp()
    {
        LockResolver.instance.moveWheelUp();
    }

    public void moveWheelDown()
    {
        LockResolver.instance.moveWheelDown();
    }

    public void selectLeftWheel()
    {
        LockResolver.instance.SelectLeftWheel();
    }

    public void selectRightWheel()
    {
        LockResolver.instance.SelectRightWheel();
    }

    public void CheckMastermindAnswer()
    {
        LoginManager.instance.CheckMastermindAnswer();
    }

    public void OnEndEditMastermindAnswer()
    {
        if (Input.GetButtonDown("Submit"))
            LoginManager.instance.CheckMastermindAnswer();
    }

    public void IarCheckAnswer(GameObject query)
    {
        IARQueryEvaluator.instance.IarCheckAnswer(query);
    }

    public void IarOnEndEditAnswer(GameObject query)
    {
        IARQueryEvaluator.instance.IarOnEndEditAnswer(query);
    }

    public void SwitchFont(bool accessibleFont)
    {
        SettingsManager.instance.SwitchFont(accessibleFont);
    }

    public void UpdateCursorSize(float newSize)
    {
        SettingsManager.instance.UpdateCursorSize(newSize);
    }

    public void UpdateAlpha(float newAlpha)
    {
        SettingsManager.instance.UpdateAlpha(newAlpha);
    }

    public void ToggleTextAnimation(bool newState)
    {
        SettingsManager.instance.ToggleTextAnimation(newState);
    }

    public void SetWalkSpeed (float newSpeed)
    {
        MovingSystem.instance.SetWalkSpeed(newSpeed);
    }
}
