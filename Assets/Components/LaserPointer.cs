using UnityEngine;

public class LaserPointer : MonoBehaviour
{
    [HideInInspector]
    public SteamVR_TrackedObject trackedObj;

    // Laser
    // Laser prebab
    public GameObject laserPrefab;
    // Laser object
    [HideInInspector]
    public GameObject laser;
    // Laser transform
    [HideInInspector]
    public Transform laserTransform;
    // Point hit by the laser
    [HideInInspector]
    public Vector3 hitPoint;

    // Teleportation
    // Camera rig transform
    public Transform cameraRigTransform;
    // Reticle prefab
    public GameObject reticlePrefab;
    // Reticle object
    [HideInInspector]
    public GameObject reticle;
    // Reticle transform
    [HideInInspector]
    public Transform reticleTransform;
    // Head transform (camera)
    public Transform headTransform;
    // Reticle offset (from the floor)
    public Vector3 reticleOffset;
    // Layer mask (which layer can we teleport to ?)
    public LayerMask teleportMask;
    // Should the user teleport ?
    [HideInInspector]
    public bool shouldTeleport;
}