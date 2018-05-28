using UnityEngine;
using FYFY;
using System;
using System.Linq;
using Valve.VR;
using TMPro;

public class TabletSystem : FSystem {

  private Family tablets = FamilyManager.getFamily(new AnyOfComponents(typeof(Tablet), typeof(TabletR3)));
  private Family tabletButtons = FamilyManager.getFamily(new AnyOfTags("Q-R1", "Q-R2", "Q-R3"), new AllOfComponents(typeof(Pointable))); //questions of the tablets

  private GameObject current;

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
        current = go.transform.parent.parent.gameObject;
        SteamVR_Events.System(EVREventType.VREvent_KeyboardClosed).Listen(OnKeyboardClosed);
      }
    }

    // Checking answered tablets
    foreach(GameObject go in tablets) {
      Tablet t = go.GetComponent<Tablet>();
      if(t && !t.isAnswered) {
        bool allAnswered = true;
        foreach(Question q in t.questions) allAnswered &= q.isAnswered;
        if(allAnswered)
        {
          foreach(Door d in t.opens) {
            d.isOpened = true;

            if(d.loadsOnOpen) d.loadsOnOpen.SetActive(true);

            AudioSource audio = d.gameObject.GetComponent<AudioSource>();
            audio.clip = d.openAudio;
            audio.Play();
          }

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
    string text = textBuilder.ToString().ToLower();
    Question que = current.GetComponent<Question>();
    if(!que.isAnswered)
    {
      if(current.tag == "Q-R3") // Questions on the third tablet are handled differently
      {
        TabletR3 t = current.transform.parent.parent.parent.gameObject.GetComponent<TabletR3>();
        if(t.answers.Contains(text))
        {
          t.answers = t.answers.Where(ans => ans != text).ToArray();
          que.source.clip = que.right;

          Char[] ca = text.ToCharArray();
          ca[0] = Char.ToUpper(ca[0]);
          que.answer.GetComponentInChildren<TextMeshProUGUI>().SetText(new string(ca));
          current.SetActive(false);
          que.answer.SetActive(true);
          que.isAnswered = true;
        } else {
          que.source.clip = que.wrong;
        }
        que.source.Play();
      } else {

        if(que.answers.Contains(text))
        {
          que.source.clip = que.right;

          current.SetActive(false);
          que.answer.SetActive(true);
          que.isAnswered = true;
        } else {
          que.source.clip = que.wrong;
        }
        que.source.Play();
      }
    }
  }
}
