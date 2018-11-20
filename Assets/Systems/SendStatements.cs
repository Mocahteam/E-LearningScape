using UnityEngine;
using FYFY;
using DIG.GBLXAPI;
using System;

public class SendStatements : FSystem {

    private Family f_actionForLRS = FamilyManager.getFamily(new AllOfComponents(typeof(ActionPerformedForLRS)));

    public static FSystem instance;

    public SendStatements()
    {
        if (Application.isPlaying)
        {
            if(!GBLXAPI.Instance.IsInit())
                GBLXAPI.Instance.init(GBL_Interface.lrsURL, GBL_Interface.lrsUser, GBL_Interface.lrsPassword, GBL_Interface.standardsConfigDefault, GBL_Interface.standardsConfigUser);

            GBLXAPI.Instance.debugStatement = false;

            //Generate player name unique to each playing session (computer name + date + hour)
            GBL_Interface.playerName = string.Concat(Environment.MachineName, "-", DateTime.Now.ToString("yyyy.MM.dd.hh.mm"));
            //Generate a UUID from the player name
            GBL_Interface.userUUID = GBLXAPI.Instance.GenerateActorUUID(string.Concat(Environment.MachineName, "-", DateTime.Now.ToString("yyyy.MM.dd.hh.mm")));

            f_actionForLRS.addEntryCallback(ActionProcessing);
        }
        instance = this;
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
	}

    private void ActionProcessing(GameObject go)
    {
        if (!this.Pause)
        {
            ActionPerformedForLRS[] listAP = go.GetComponents<ActionPerformedForLRS>();
            int nb = listAP.Length;
            ActionPerformedForLRS ap;
            for(int i = nb-1; i > -1; i--)
            {
                ap = listAP[i];
                //If no result info filled
                if (!ap.result)
                    GBL_Interface.SendStatement(ap.verb, ap.objectType, ap.objectName);
                else
                    GBL_Interface.SendStatementWithResult(ap.verb, ap.objectType, ap.objectName, ap.completed, ap.success, ap.response, ap.score, ap.duration);
                GameObjectManager.removeComponent(ap);
            }
        }
    }
}