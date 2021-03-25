using UnityEngine;
using FYFY;
using DIG.GBLXAPI;
using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;

public class SendStatements : FSystem {

    private Family f_actionForLRS = FamilyManager.getFamily(new AllOfComponents(typeof(ActionPerformedForLRS)));

    public static SendStatements instance;

    public static bool shouldPause = true;

    public SendStatements()
    {
        if (Application.isPlaying)
        {
            // Add code to constructor here
        }
        instance = this;
    }

    public void initGBLXAPI()
    {
        if (!GBLXAPI.IsInit)
            GBLXAPI.Init(GBL_Interface.lrsAddresses);

        GBLXAPI.debugMode = false;

        //Generate player name unique to each playing session (computer name + date + hour)
        GBL_Interface.playerName = LoadGameContent.sessionID;
        //Generate a UUID from the player name
        GBL_Interface.userUUID = GBLUtils.GenerateActorUUID(LoadGameContent.sessionID);
    }

	// Use this to update member variables when system resume.
	// Advice: avoid to update your families inside this function.
	protected override void onResume(int currentFrame){
        this.Pause = shouldPause;
	}

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {
        // Do not use callbacks because in case in the same frame actions are removed on a GO and another component is added in another system, family will not trigger again callback because component will not be processed
        foreach (GameObject go in f_actionForLRS)
        {
            ActionPerformedForLRS[] listAP = go.GetComponents<ActionPerformedForLRS>();
            int nb = listAP.Length;
            ActionPerformedForLRS ap;
            if (!this.Pause)
            {
                for (int i = 0; i < nb; i++)
                {
                    ap = listAP[i];
                    //If no result info filled
                    if (!ap.result)
                    {
                        GBL_Interface.SendStatement(ap.verb, ap.objectType, ap.objectName, ap.activityExtensions);
                    }
                    else
                    {
                        bool? completed = null, success = null;

                        if (ap.completed > 0)
                            completed = true;
                        else if (ap.completed < 0)
                            completed = false;

                        if (ap.success > 0)
                            success = true;
                        else if (ap.success < 0)
                            success = false;

                        GBL_Interface.SendStatementWithResult(ap.verb, ap.objectType, ap.objectName, ap.activityExtensions, ap.resultExtensions, completed, success, ap.response, ap.score, ap.duration);
                    }
                }
            }
            for (int i = nb - 1; i > -1; i--)
            {
                GameObjectManager.removeComponent(listAP[i]);
            }
        }
	}
}