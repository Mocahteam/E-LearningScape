using UnityEngine;
using FYFY;
using System;
using System.Linq;
using Valve.VR;
using TMPro;

public class DoorSystem : FSystem {

  private Family doors = FamilyManager.getFamily(new AnyOfComponents(typeof(Door)));

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
    foreach(GameObject go in doors) {
      Door d = go.GetComponent<Door>();
      if(d.isOpened) {
        float step = d.speed * Time.deltaTime;
        go.transform.localPosition = Vector3.MoveTowards(go.transform.localPosition, d.openedPos, step);
      }
    }
  }
}
