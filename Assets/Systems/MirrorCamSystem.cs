using UnityEngine;
using FYFY;

public class MirrorCamSystem : FSystem {

    // This system manage cylinder mirrors

    private Family f_mirrors = FamilyManager.getFamily(new AllOfComponents(typeof(MirrorScript)));
    private Family f_activeMirrors = FamilyManager.getFamily(new AllOfComponents(typeof(MirrorScript)), new AnyOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));

    public GameObject player;

    public static MirrorCamSystem instance;

    public MirrorCamSystem()
    {
        instance = this;
    }

    protected override void onStart()
    {
        foreach (GameObject mirror in f_mirrors)
        {
            // On récupère la caméra de chaque mirroir
            Camera cam = mirror.GetComponentInChildren<Camera>(true);
            // On modifie la matrice de projection pour inverser l'axe x (effet mirroir)
            Matrix4x4 mat = cam.projectionMatrix;
            mat *= Matrix4x4.Scale(new Vector3(-1, 1, 1));
            // On réapplique la matrice de projection à la caméra
            cam.projectionMatrix = mat;

        }
    }

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {
        // Pour faire l'effet mirroir l'idée est de maintenir en permanence le cyclindre orienté vers le joueur et que la caméra ait un angle inverse par rapport à l'axe de vision du joueur (exemple si le joueur regarde le mirroir vers le bas, si on fait pointer la caméra vers le joueur le reflet ne sera pas bon car la caméra pointera vers le haut, il faut donc quelle soit orienté vers le joueur mais sur la verticalité il faut qu'elle soit dans le même sens)

        // On veut que le cylindre représentant les mirroirs soient toujours orientés vers le joueur
        foreach (GameObject mirror in f_activeMirrors)
        {
            // On oriente le miroir vers le joueur
            mirror.transform.LookAt(player.transform, Vector3.up);
            // On s'assure que le miroir soit bien mis à plat
            Vector3 dir = mirror.transform.forward;
            dir.y = 0;
            mirror.transform.rotation = Quaternion.LookRotation(dir);
        }
        // Maintenant on oriente la caméra dans le bon sens, l'astuce est de la faire pointer vers le joueur puis simplement d'inverser y
        foreach (GameObject mirror in f_activeMirrors)
        {
            // On oriente la caméra vers le joueur
            Camera cam = mirror.GetComponentInChildren<Camera>(true);
            cam.transform.LookAt(player.transform, Vector3.up);
            // On s'assure que la caméra donne une angle de reflet opposé à l'angle incident du joueur
            Vector3 dir = mirror.transform.forward;
            dir.y = -dir.y;
            cam.transform.rotation = Quaternion.LookRotation(dir);
        }
    }
}