using UnityEngine;
using FYFY;
using Valve.VR;
using System.Linq;

public class TabletSystem : FSystem {

  private Family tablets = FamilyManager.getFamily(new AllOfComponents(typeof(Tablet)));
  private Family tabletButtons = FamilyManager.getFamily(new AnyOfTags("Q-R1"), new AllOfComponents(typeof(Pointable))); //questions of the tablets

  private GameObject q;

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
    // Button input
    foreach(GameObject go in tabletButtons)
    {
      Pointable p = go.GetComponent<Pointable>();
      // Si le boutton est selectionné, on ouvre le clavier
      if(p.selected)
      {
        p.selected = false;
        SteamVR.instance.overlay.ShowKeyboard(0, 0, "Input", 50, "", false, 0);
        q = go.transform.parent.parent.gameObject;
        SteamVR_Events.System(EVREventType.VREvent_KeyboardClosed).Listen(OnKeyboardClosed);
      }
    }

    // Checking answered tablets
    foreach(GameObject go in tablets) {
      Tablet t = go.GetComponent<Tablet>();
      if(!t.isAnswered) {
        bool allAnswered = true;
        foreach(Question q in t.questions) allAnswered &= q.isAnswered;
        if(allAnswered)
        {
          Debug.Log("All answered !");
          Door d = t.opens;
          d.isOpened = true;
          d.loadsOnOpen.SetActive(true);
          d.gameObject.transform.position += d.translateOnOpen;

          AudioSource audio = go.GetComponent<AudioSource>();
          audio.clip = d.openAudio;
          audio.Play();

          t.isAnswered = true;
        }
      }
    }
  }

  // Fermeture du clavier
  public void OnKeyboardClosed(VREvent_t args)
  {
    System.Text.StringBuilder textBuilder = new System.Text.StringBuilder(1024);
    SteamVR.instance.overlay.GetKeyboardText(textBuilder, 1024);
    string text = textBuilder.ToString();
    Question que = q.GetComponent<Question>();
    if(que.answers.Contains(text))
    {
      que.source.clip = que.right;

      q.SetActive(false);
      que.answer.SetActive(true);
      que.isAnswered = true;
    } else {
      que.source.clip = que.wrong;
    }
    que.source.Play();
  }
}
