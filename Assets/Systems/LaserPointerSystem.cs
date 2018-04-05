using UnityEngine;
using FYFY;

public class LaserPointerSystem : FSystem
{
    // Both Vive controllers (they also have TriggerSensitive3D)
    private Family controllers = FamilyManager.getFamily(new AllOfComponents(typeof(LaserPointer)));

    public LaserPointerSystem()
    {
        // For each controller
        foreach (GameObject c in controllers)
        {
            LaserPointer lp = c.GetComponent<LaserPointer>();
            // Get the tracked object (device)
            lp.trackedObj = lp.GetComponent<SteamVR_TrackedObject>();

            // Instantiate the laser
            lp.laser = GameObject.Instantiate(lp.laserPrefab);
            lp.laserTransform = lp.laser.transform;

            // Instantiate the reticle
            lp.reticle = GameObject.Instantiate(lp.reticlePrefab);
            lp.reticleTransform = lp.reticle.transform;
        }
    }

    // Use this to update member variables when system pause. 
    // Advice: avoid to update your families inside this function.
    protected override void onPause(int currentFrame)
    {
    }

    // Use this to update member variables when system resume.
    // Advice: avoid to update your families inside this function.
    protected override void onResume(int currentFrame)
    {
    }

    // Use to process your families.
    protected override void onProcess(int familiesUpdateCount)
    {
        // For eache controller
        foreach (GameObject c in controllers)
        {
            LaserPointer lp = c.GetComponent<LaserPointer>();
            SteamVR_Controller.Device controller = SteamVR_Controller.Input((int) lp.trackedObj.index);

            // If the controller is holding an item, stop
            Grabber g = c.GetComponent<Grabber>();
            if(g && g.objectInHand) { continue; }

            RaycastHit hit;
            
            // If the touchpad is pressed, show the teleportation laser
            if(controller.GetPress(SteamVR_Controller.ButtonMask.Touchpad))
            {
                // Can we teleport ?
                if(Physics.Raycast(lp.trackedObj.transform.position, lp.transform.forward, out hit, 100, lp.teleportMask))
                {
                    lp.hitPoint = hit.point;
                    ShowLaser(lp, hit);

                    // Show the reticle
                    lp.reticle.SetActive(true);
                    // Change the reticle position
                    lp.reticleTransform.position = lp.hitPoint + lp.reticleOffset;
                    // Indicate that we can teleport here
                    lp.shouldTeleport = true;
                } else // else hide the laser and the reticle
                {
                    lp.laser.SetActive(false);
                    lp.reticle.SetActive(false);
                    lp.shouldTeleport = false;
                }
            } else // else hide the laser and the reticle
            {
                lp.laser.SetActive(false);
                lp.reticle.SetActive(false);
            }

            // If touchpad is released, teleport
            if (controller.GetPressUp(SteamVR_Controller.ButtonMask.Touchpad) && lp.shouldTeleport)
            {
                Teleport(lp);
            }

            // Pointer on the canLaserPoint surfaces
            if(Physics.Raycast(lp.trackedObj.transform.position, lp.transform.forward, out hit, 100, lp.pointMask))
            {
                lp.hitPoint = hit.point;
                ShowLaser(lp, hit);
            }
        }
    }

    private void ShowLaser(LaserPointer lp, RaycastHit hit)
    {
        // Show the laser
        lp.laser.SetActive(true);
        // Position the laser between controller and raycast hit
        lp.laserTransform.position = Vector3.Lerp(lp.trackedObj.transform.position, lp.hitPoint, .5f);
        // Point the laser towards the raycast hit
        lp.laserTransform.LookAt(lp.hitPoint);
        // Scale the laser
        lp.laserTransform.localScale = new Vector3(lp.laserTransform.localScale.x,
            lp.laserTransform.localScale.y,
            hit.distance);
    }

    private void Teleport(LaserPointer lp)
    {
        // We cant teleport while we're teleporting
        lp.shouldTeleport = false;
        lp.reticle.SetActive(false);
        // Difference of pos between camera rig and head
        Vector3 difference = lp.cameraRigTransform.position - lp.headTransform.position;
        difference.y = 0;
        // Change our position
        lp.cameraRigTransform.position = lp.hitPoint + difference;
    }
}