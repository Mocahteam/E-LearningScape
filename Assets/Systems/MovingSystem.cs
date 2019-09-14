using UnityEngine;
using System.Collections.Generic;
using FYFY;
using UnityStandardAssets.Characters.FirstPerson;
using UnityEngine.UI;
using FYFY_plugins.TriggerManager;
using FYFY_plugins.PointerManager;
using FYFY_plugins.Monitoring;
using TMPro;

public class MovingSystem : FSystem
{
    // This system manage HUD on moving, walking speed and state of the FirstPersonController

    private Family f_player = FamilyManager.getFamily(new AllOfComponents(typeof(FirstPersonController), typeof(AudioBank)));
    private Family f_linkedHud = FamilyManager.getFamily(new AnyOfTags("DisableOnViewed", "EnableOnFirstCrouch", "HUD_H", "HUD_Menu"));
    private Family f_endRoom = FamilyManager.getFamily(new AnyOfTags("EndRoom"), new AnyOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));
    private Family f_cursor = FamilyManager.getFamily(new AnyOfTags("Cursor"));
    private Family f_waterWalking = FamilyManager.getFamily(new AnyOfLayers(12), new AllOfComponents(typeof(Triggered3D))); // Layer 12 <=> WaterCollider
    private Family f_HUDInputs = FamilyManager.getFamily(new AnyOfTags("DisableOnViewed", "EnableOnFirstCrouch", "HUD_A", "HUD_H", "HUD_Menu"));
    private Family f_CrouchHint = FamilyManager.getFamily(new AllOfComponents(typeof(AnimatedSprites), typeof(PointerOver), typeof(LinkedWith), typeof(BoxCollider)));
    private Family f_OutOfFirstRoom = FamilyManager.getFamily(new AllOfComponents(typeof(Triggered3D), typeof(LinkedWith)));

    public float traceMovementFrequency = 0;
    private bool crouching = false; // true when the player is crouching
    private bool changingPose = false;
    private float crouchingSpeed;
    private Vector3 targetScale;
    private Vector3 crouchingScale;
    private Vector3 standingScale = Vector3.one;
    private bool firstCrouchOccurs = false;
    private FirstPersonController playerController;
    private GameObject movableFragments;
    private Image tmpImage;
    private TMP_Text tmpTextMesh;
    private AudioBank audioBank;
    private Camera playerCamera;

    private bool playerIsWalking = false;
    private bool playerWasWalking = false;
    private bool showHUD;
    private bool hideHUD;
    private float hudShowingSpeed;
    private float hudHidingSpeed;
    private bool previousHUDState;
    private bool walkInWater = false;

    private float walkingTraceTimer = float.MaxValue;

    public static MovingSystem instance;

    public MovingSystem()
    {
        //when crouching, the scale of the player is changed (rather than its position)
        crouchingScale = new Vector3(standingScale.x * 0.2f, standingScale.y * 0.2f, standingScale.z * 0.2f);
        if (Application.isPlaying)
        {
            playerController = f_player.First().GetComponent<FirstPersonController>();
            movableFragments = playerController.transform.GetChild(3).gameObject;

            playerCamera = playerController.transform.GetChild(0).GetComponent<Camera>();
            audioBank = playerController.GetComponent<AudioBank>();
            f_waterWalking.addEntryCallback(onEnterWater);
            f_waterWalking.addExitCallback(onExitWater);

            f_CrouchHint.addEntryCallback(disableHUDWarning);
            f_OutOfFirstRoom.addEntryCallback(disableHUDWarning);
        }
        instance = this;
    }

    private void disableHUDWarning(GameObject go)
    {
        foreach (LinkedWith link in go.GetComponents<LinkedWith>())
            GameObjectManager.setGameObjectState(link.link, false);
    }

    private void onEnterWater(GameObject go)
    {
        walkInWater = true;
        playerController.m_FootstepSounds[0] = audioBank.audioBank[2];
        playerController.m_FootstepSounds[1] = audioBank.audioBank[3];
        if (!crouching)
        {
            playerController.m_WalkSpeed = playerController.m_WalkSpeed / 2;
            playerController.m_RunSpeed = playerController.m_RunSpeed / 2;
        }
    }

    private void onExitWater(int instanceId)
    {
        walkInWater = false;
        playerController.m_FootstepSounds[0] = audioBank.audioBank[0];
        playerController.m_FootstepSounds[1] = audioBank.audioBank[1];
        if (crouching)
        {
            playerController.m_FootstepSounds[0] = audioBank.audioBank[4];
            playerController.m_FootstepSounds[1] = audioBank.audioBank[5];
        }
        else
        {
            playerController.m_WalkSpeed = playerController.m_WalkSpeed * 2;
            playerController.m_RunSpeed = playerController.m_RunSpeed * 2;
            playerController.m_FootstepSounds[0] = audioBank.audioBank[0];
            playerController.m_FootstepSounds[1] = audioBank.audioBank[1];
        }
    }

    public void SetWalkSpeed(float speedW)
    {
        if (walkInWater || crouching)
            speedW = speedW / 2;
        playerController.m_WalkSpeed = speedW;
        playerController.m_RunSpeed = speedW;

    }

    // Use this to update member variables when system pause. 
    // Advice: avoid to update your families inside this function.
    protected override void onPause(int currentFrame)
    {
        playerController.enabled = false;
        f_player.First().GetComponentInChildren<ThirdPersonCameraControler>().enabled = false;
        SetHUD(false);
        // Show mouse cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
        // Hide fps cursor
        GameObjectManager.setGameObjectState(f_cursor.First(), false);
    }

    // Use this to update member variables when system resume.
    // Advice: avoid to update your families inside this function.
    protected override void onResume(int currentFrame)
    {
        playerController.m_MouseLook.m_CameraTargetRot = playerCamera.transform.localRotation;
        playerController.m_MouseLook.m_CharacterTargetRot = f_player.First().transform.localRotation;
        
        playerController.enabled = true;
        f_player.First().GetComponentInChildren<ThirdPersonCameraControler>().enabled = true;
        SetHUD(true);
        // hide mouse cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        // Show fps cursor
        GameObjectManager.setGameObjectState(f_cursor.First(), true);
    }

    private void SetHUD(bool state)
    {
        if (firstCrouchOccurs && previousHUDState != state)
        {
            foreach (GameObject hud in f_linkedHud)
                GameObjectManager.setGameObjectState(hud, state);
            previousHUDState = state;
        }
    }

    // Use to process your families.
    protected override void onProcess(int familiesUpdateCount)
    {
        SetHUD(f_endRoom.Count == 0);
        if (traceMovementFrequency > 0)
        {
            if (playerController.m_Input != Vector2.zero)
            {
                if (Time.time - walkingTraceTimer > traceMovementFrequency)
                {
                    GameObjectManager.addComponent<ActionPerformedForLRS>(playerController.gameObject, new
                    {
                        verb = "moved",
                        objectType = "avatar",
                        objectName = "player",
                        activityExtensions = new Dictionary<string, List<string>>() { { "position", new List<string>() { f_player.First().transform.position.ToString("G4") } } }
                    });
                    walkingTraceTimer = Time.time;
                }
            }
            else
                walkingTraceTimer = Time.time;
        }

        crouchingSpeed = 70f * Time.deltaTime;
        // when control button or right click is pressed then the player can alternatively crouch and standing
        if (Input.GetButtonDown("Fire2") && !changingPose)
        {
            changingPose = true; //true when the player is crouching or standing
            //change moving speed according to the stance
            if (crouching)
            {
                if (!walkInWater)
                {
                    playerController.m_WalkSpeed = playerController.m_WalkSpeed * 2;
                    playerController.m_RunSpeed = playerController.m_RunSpeed * 2;
                    playerController.m_FootstepSounds[0] = audioBank.audioBank[0];
                    playerController.m_FootstepSounds[1] = audioBank.audioBank[1];
                }
            }
            else
            { // standing and want to crouch
                if (!firstCrouchOccurs)
                    firstCrouchOccurs = true;
                if (!walkInWater)
                {
                    playerController.m_WalkSpeed = playerController.m_WalkSpeed / 2;
                    playerController.m_RunSpeed = playerController.m_RunSpeed / 2;
                    playerController.m_FootstepSounds[0] = audioBank.audioBank[4];
                    playerController.m_FootstepSounds[1] = audioBank.audioBank[5];
                }
            }
        }

        //if the player is changing stance
        if (changingPose)
        {
            if (crouching)
                targetScale = standingScale;
            else
                targetScale = crouchingScale;

            playerController.transform.localScale = Vector3.MoveTowards(playerController.transform.localScale, targetScale, crouchingSpeed / 10); //change stance gradually

            if (playerController.transform.localScale == targetScale) //when standing scale is reached
            {
                changingPose = false;
                crouching = !crouching; //true when the player is crouching

                if (crouching)
                {
                    GameObjectManager.addComponent<ActionPerformed>(playerController.gameObject, new { name = "turnOn", performedBy = "player" });
                    GameObjectManager.addComponent<ActionPerformedForLRS>(playerController.gameObject, new { verb = "crouched", objectType = "avatar", objectName = "player" });
                }
                else
                {
                    GameObjectManager.addComponent<ActionPerformed>(playerController.gameObject, new { name = "turnOff", performedBy = "player" });
                    GameObjectManager.addComponent<ActionPerformedForLRS>(playerController.gameObject, new { verb = "stood", objectType = "avatar", objectName = "player" });
                }
            }

        }

        // make HUD transparent on moving
        hudHidingSpeed = -3f * Time.deltaTime;
        hudShowingSpeed = 3f * Time.deltaTime;
        if (hideHUD)
        {
            float aCount = 1;
            foreach (GameObject hud in f_HUDInputs)
            {
                tmpImage = hud.GetComponentInChildren<Image>();
                if (tmpImage)
                {
                    tmpImage.color = new Color(tmpImage.color.r, tmpImage.color.g, tmpImage.color.b, tmpImage.color.a + hudHidingSpeed);
                    aCount = tmpImage.color.a;
                }
                tmpTextMesh = hud.GetComponentInChildren<TMP_Text>();
                if (tmpTextMesh)
                {
                    tmpTextMesh.color = new Color(tmpTextMesh.color.r, tmpTextMesh.color.g, tmpTextMesh.color.b, tmpTextMesh.color.a + hudHidingSpeed);
                    aCount = tmpTextMesh.color.a;
                }
            }
            if (aCount < 0.3f)
            {
                foreach (GameObject hud in f_HUDInputs)
                {
                    tmpImage = hud.GetComponentInChildren<Image>();
                    if (tmpImage)
                        tmpImage.color = new Color(tmpImage.color.r, tmpImage.color.g, tmpImage.color.b, 0.3f);
                    tmpTextMesh = hud.GetComponentInChildren<TMP_Text>();
                    if (tmpTextMesh)
                        tmpTextMesh.color = new Color(tmpTextMesh.color.r, tmpTextMesh.color.g, tmpTextMesh.color.b, 0.3f);
                }
                hideHUD = false;
            }
        }
        else if (showHUD)
        {
            float aCount = 0;
            foreach (GameObject hud in f_HUDInputs)
            {
                tmpImage = hud.GetComponentInChildren<Image>();
                if (tmpImage)
                {
                    tmpImage.color = new Color(tmpImage.color.r, tmpImage.color.g, tmpImage.color.b, tmpImage.color.a + hudShowingSpeed);
                    aCount = tmpImage.color.a;
                }
                tmpTextMesh = hud.GetComponentInChildren<TMP_Text>();
                if (tmpTextMesh)
                {
                    tmpTextMesh.color = new Color(tmpTextMesh.color.r, tmpTextMesh.color.g, tmpTextMesh.color.b, tmpTextMesh.color.a + hudShowingSpeed);
                    aCount = tmpTextMesh.color.a;
                }
            }
            if (aCount >= 1f)
            {
                foreach (GameObject hud in f_HUDInputs)
                {
                    tmpImage = hud.GetComponentInChildren<Image>();
                    if (tmpImage)
                        tmpImage.color = new Color(tmpImage.color.r, tmpImage.color.g, tmpImage.color.b, 1f);
                    tmpTextMesh = hud.GetComponentInChildren<TMP_Text>();
                    if (tmpTextMesh)
                        tmpTextMesh.color = new Color(tmpTextMesh.color.r, tmpTextMesh.color.g, tmpTextMesh.color.b, 1f);
                }
                showHUD = false;
            }
        }
        playerIsWalking = playerController.m_Input.x != 0 || playerController.m_Input.y != 0;
        if (playerIsWalking && !playerWasWalking)
        {
            hideHUD = true;
            showHUD = false;
        }
        else if (!playerIsWalking && playerWasWalking)
        {
            showHUD = true;
            hideHUD = false;
        }
        playerWasWalking = playerIsWalking;
        if (playerIsWalking)
        {
            if (playerController.m_Input.y != 0)
                movableFragments.transform.Rotate(new Vector3(1, 0, 0), (playerController.m_Input.y > 0 ? 1 : -1) * 100 * Time.deltaTime);
        }
    }
}