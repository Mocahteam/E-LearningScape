using UnityEngine;
using FYFY;
using DIG.GBLXAPI;

public class SendStatements : FSystem {

    public static FSystem instance;

    public SendStatements()
    {
        if (Application.isPlaying)
        {
            if(!GBLXAPI.Instance.IsInit())
                GBLXAPI.Instance.init(GBL_Interface.lrsURL, GBL_Interface.lrsUser, GBL_Interface.lrsPassword, GBL_Interface.standardsConfigDefault, GBL_Interface.standardsConfigUser);
            GBLXAPI.Instance.debugStatement = false;

            GBL_Interface.SendStatement("collected","item","wire");
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
}