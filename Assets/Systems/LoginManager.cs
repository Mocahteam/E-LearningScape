using UnityEngine;
using FYFY;
using FYFY_plugins.PointerManager;

public class LoginManager : FSystem {

    // this system manage the login panel

    private Family focusedLogin = FamilyManager.getFamily(new AnyOfTags("Login"), new AllOfComponents(typeof(ReadyToWork)));
    private Family closeLogin = FamilyManager.getFamily(new AnyOfTags("Login", "InventoryElements"), new AllOfComponents(typeof(PointerOver)));
    private Family f_iarBackground = FamilyManager.getFamily(new AnyOfTags("UIBackground"), new AnyOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));

    // Selectable Component is dynamically added by IARGearsEnigma when this enigma is solved => this is a sure condition to know that login is unlocked
    private Family f_loginUnlocked = FamilyManager.getFamily(new AnyOfTags("Login"), new AllOfComponents(typeof(Selectable)));

    private GameObject selectedLoginPanel;
    private GameObject loginCover;
    private float bonnetSpeedRotation = 200f;
    private float tmpRotationCount = 0;
    private Vector3 tmpTarget;
    private float speed;

    private bool coverAnimate = false;
    
    public static LoginManager instance;

    public LoginManager()
    {
        if (Application.isPlaying)
        {
            f_loginUnlocked.addEntryCallback(onLoginUnlocked);
            focusedLogin.addEntryCallback(onReadyToWorkOnLogin);
        }
        instance = this;
    }

    private void onLoginUnlocked(GameObject go)
    {
        // launch animation of login protection
        loginCover = go.transform.GetChild(0).gameObject; // the first child is the cover
        tmpTarget = go.transform.position - (Vector3.up); 
        coverAnimate = true;
    }

    private void onReadyToWorkOnLogin(GameObject go)
    {
        selectedLoginPanel = go;
    }

    private void onWordMouseEnter(GameObject go)
    {
        
    }

    private void onWordMouseExit(int instanceId)
    {
        
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
	protected override void onProcess(int familiesUpdateCount)
    {
        if (selectedLoginPanel)
        {
            // "close" ui (give back control to the player) when clicking on nothing or Escape is pressed and IAR is closed (because Escape close IAR)
            if (((closeLogin.Count == 0 && Input.GetMouseButtonDown(0)) || (Input.GetKeyDown(KeyCode.Escape) && f_iarBackground.Count == 0)))
                ExitLogin();
            else
            {
                
            }
        }

        speed = Time.deltaTime;
        if (coverAnimate)
        {
            // open the cover of the box
            loginCover.transform.position = Vector3.MoveTowards(loginCover.transform.position, tmpTarget, speed);
            if (loginCover.transform.position == tmpTarget)
                coverAnimate = false;
        }
	}

    private void ExitLogin()
    {
        // remove ReadyToWork component to release selected GameObject
        GameObjectManager.removeComponent<ReadyToWork>(selectedLoginPanel);

        selectedLoginPanel = null;
    }
}