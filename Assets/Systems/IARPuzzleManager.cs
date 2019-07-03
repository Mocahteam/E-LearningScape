using UnityEngine;
using System.Collections.Generic;
using FYFY;
using FYFY_plugins.PointerManager;

public class IARPuzzleManager : FSystem {

    // Enable to interact Move and Rotate puzzle pieces inside IAR

    private Family f_puzzle = FamilyManager.getFamily(new AnyOfTags("PuzzleCanvas"), new AnyOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));
    private Family f_puzzleUI = FamilyManager.getFamily(new AnyOfTags("PuzzleUI"), new AllOfComponents(typeof(PointerOver), typeof(LinkedWith), typeof(puzzleDeltaPositions)), new AnyOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));

    private GameObject tmpGo;

    public IARPuzzleManager()
    {
        if (Application.isPlaying)
        {
            f_puzzle.addEntryCallback(onPuzzleEnabled);
            f_puzzle.addExitCallback(onPuzzleDisabled);
        }
    }

    private void onPuzzleEnabled(GameObject go)
    {
        this.Pause = false;
    }

    private void onPuzzleDisabled (int instanceId)
    {
        this.Pause = true;
    }

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {
        if (Input.GetMouseButtonDown(0) && f_puzzleUI.First())
        {
            tmpGo = f_puzzleUI.First();
            GameObjectManager.addComponent<ActionPerformedForLRS>(tmpGo, new { verb = "dragged", objectType = "draggable", objectName = tmpGo.name });
        }
        if (Input.GetMouseButtonUp(0) && tmpGo)
        {
            GameObjectManager.addComponent<ActionPerformedForLRS>(tmpGo, new
            {
                verb = "dropped",
                objectType = "draggable",
                objectName = tmpGo.name,
                activityExtensions = new Dictionary<string, List<string>>() { { "position", new List<string>() { tmpGo.GetComponent<RectTransform>().position.ToString("G4") } } }
            });
            // try to magnet puzzle piece
            int cpt = 0;
            // parse all neighbours
            foreach (LinkedWith neighbour in tmpGo.GetComponents<LinkedWith>())
            {
                // Check if neighbour exists and is active in hierarchy
                if (neighbour.link && neighbour.link.activeInHierarchy)
                {
                    // Compute distance between current piece and its neighbour (taking into account x and y deltas)
                    if (Mathf.Abs((tmpGo.transform.position.x - tmpGo.GetComponent<puzzleDeltaPositions>().xDelta[cpt]) - neighbour.link.transform.position.x) < 10 &&
                            Mathf.Abs((tmpGo.transform.position.y - tmpGo.GetComponent<puzzleDeltaPositions>().yDelta[cpt]) - neighbour.link.transform.position.y) < 10)
                        // magnets piece to this neighbour
                        tmpGo.transform.position = new Vector3(neighbour.link.transform.position.x + tmpGo.GetComponent<puzzleDeltaPositions>().xDelta[cpt], neighbour.link.transform.position.y + tmpGo.GetComponent<puzzleDeltaPositions>().yDelta[cpt], neighbour.link.transform.position.z);
                }
                cpt++;
            }
            tmpGo = null;
        }

        if (Input.GetMouseButton(0) && tmpGo)
        {
            tmpGo.transform.position = Input.mousePosition;
            float puzzleScale = tmpGo.GetComponent<RectTransform>().localScale.x;
            float puzzleHalfWidth = puzzleScale * tmpGo.GetComponent<RectTransform>().rect.width / 2;
            float puzzleHalfHeight = puzzleScale * tmpGo.GetComponent<RectTransform>().rect.height / 2;
            float canvasHalfWidth = tmpGo.transform.parent.GetComponent<RectTransform>().rect.width / 2;
            float canvasHalfHeight = tmpGo.transform.parent.GetComponent<RectTransform>().rect.height / 2;

            // avoid pieces to move outside the right panel
            if (tmpGo.transform.localPosition.x - puzzleHalfWidth < -canvasHalfWidth)
                tmpGo.transform.localPosition = new Vector3(-canvasHalfWidth + puzzleHalfWidth, tmpGo.transform.localPosition.y, tmpGo.transform.localPosition.z);

            if (tmpGo.transform.localPosition.x + puzzleHalfWidth > canvasHalfWidth)
                tmpGo.transform.localPosition = new Vector3(canvasHalfWidth - puzzleHalfWidth, tmpGo.transform.localPosition.y, tmpGo.transform.localPosition.z);

            if (tmpGo.transform.localPosition.y - puzzleHalfHeight < -canvasHalfHeight)
                tmpGo.transform.localPosition = new Vector3(tmpGo.transform.localPosition.x, -canvasHalfHeight + puzzleHalfHeight, tmpGo.transform.localPosition.z);

            if (tmpGo.transform.localPosition.y + puzzleHalfHeight  > canvasHalfHeight)
                tmpGo.transform.localPosition = new Vector3(tmpGo.transform.localPosition.x, canvasHalfHeight - puzzleHalfHeight, tmpGo.transform.localPosition.z);
        }
	}
}