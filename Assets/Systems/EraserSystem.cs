using UnityEngine;
using FYFY;
using FYFY_plugins.TriggerManager;

public class EraserSystem : FSystem {
  private Family erasers = FamilyManager.getFamily(new AllOfComponents(typeof(Eraser)));
  private Family erasables = FamilyManager.getFamily(new AllOfComponents(typeof(Erasable)));

  protected override void onPause(int currentFrame) {
	}

	// Use this to update member variables when system resume.
	// Advice: avoid to update your families inside this function.
	protected override void onResume(int currentFrame){
	}

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {
    foreach (GameObject g in erasers)
    {
      Triggered3D t3d = g.GetComponent<Triggered3D>();
      if(!t3d) continue;
      foreach (GameObject target in t3d.Targets)
      {
        Erasable e = target.GetComponent<Erasable>();
        if (!e) continue;
        GameObject disc = g.GetComponent<Eraser>().disc;
        float x = g.transform.position.x;
        float y = g.transform.position.y;
        GameObject.Instantiate(disc, new Vector3(x, y, disc.transform.position.z), disc.transform.rotation).SetActive(true);
      }
    }
  }

}
