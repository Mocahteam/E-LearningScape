using UnityEngine;
using System.Collections.Generic;
using FYFY;
using FYFY_plugins.PointerManager;

public class IARPuzzleManager : FSystem {

    // Enable to interact Move and Rotate puzzle pieces inside IAR

    private Family f_puzzle = FamilyManager.getFamily(new AnyOfTags("PuzzleCanvas"), new AnyOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));
    private Family f_focusedPiece = FamilyManager.getFamily(new AnyOfTags("PuzzleUI"), new AllOfComponents(typeof(PointerOver), typeof(LinkedWith), typeof(puzzleDeltaPositions), typeof(MagnetizedPieces)), new AnyOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));
    private Family f_pieces = FamilyManager.getFamily(new AnyOfTags("PuzzleUI"), new AllOfComponents(typeof(puzzleDeltaPositions), typeof(MagnetizedPieces)));

    private GameObject tmpGo;

    private bool enableTraceInteraction = true;

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
        enableTraceInteraction = true;
        this.Pause = false;
        synchronizeConnections();
    }

    private void onPuzzleDisabled (int instanceId)
    {
        this.Pause = true;
    }

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {
        if (Input.GetButtonDown("Fire1") && f_focusedPiece.First())
        {
            tmpGo = f_focusedPiece.First();
            // just trace the player interact with the puzzle
            if (enableTraceInteraction)
            {
                GameObjectManager.addComponent<ActionPerformedForLRS>(tmpGo, new { verb = "interacted", objectType = "interactable", objectName = "Puzzle" });
                enableTraceInteraction = false;
            }
        }
        if (Input.GetButtonUp("Fire1") && tmpGo)
        {
            // try to magnet puzzle piece
            int cpt = 0;
            // parse all neighbours
            foreach (LinkedWith neighbourCandidate in tmpGo.GetComponents<LinkedWith>())
            {
                // Check if neighbour exists and is active in hierarchy
                if (neighbourCandidate.link && neighbourCandidate.link.activeInHierarchy)
                {
                    // Compute distance between current piece and its neighbour (taking into account x and y deltas)
                    if (Mathf.Abs((tmpGo.transform.position.x - (tmpGo.GetComponent<puzzleDeltaPositions>().xDelta[cpt] * Screen.width / 1280)) - neighbourCandidate.link.transform.position.x) < 10 &&
                            Mathf.Abs((tmpGo.transform.position.y - (tmpGo.GetComponent<puzzleDeltaPositions>().yDelta[cpt] * Screen.width / 1280)) - neighbourCandidate.link.transform.position.y) < 10)
                    {
                        // magnets piece to this neighbour
                        tmpGo.transform.position = new Vector3(neighbourCandidate.link.transform.position.x + (tmpGo.GetComponent<puzzleDeltaPositions>().xDelta[cpt] * Screen.width / 1280), neighbourCandidate.link.transform.position.y + (tmpGo.GetComponent<puzzleDeltaPositions>().yDelta[cpt] * Screen.width / 1280), neighbourCandidate.link.transform.position.z);
                        GameObjectManager.addComponent<PlaySound>(tmpGo, new { id = 17 });
                        if (!tmpGo.GetComponent<MagnetizedPieces>().connectedWith.Contains(neighbourCandidate.link))
                            tmpGo.GetComponent<MagnetizedPieces>().connectedWith.Add(neighbourCandidate.link);
                        if (!neighbourCandidate.link.GetComponent<MagnetizedPieces>().connectedWith.Contains(tmpGo))
                            neighbourCandidate.link.GetComponent<MagnetizedPieces>().connectedWith.Add(tmpGo);
                    }

                }
                cpt++;
            }
            tmpGo = null;
        }

        if (Input.GetButton("Fire1") && tmpGo)
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

            // move neighbours
            moveConnectedPieces(tmpGo, new List<GameObject>{ tmpGo });
        }
    }

    private List<GameObject> moveConnectedPieces (GameObject movedPiece, List<GameObject> alreadyMoved)
    {
        List<GameObject> connectedPieces = movedPiece.GetComponent<MagnetizedPieces>().connectedWith;
        foreach (GameObject connectedPiece in connectedPieces)
        {
            // Look for delta with this connected Piece
            int cpt = 0;
            // parse all neighbours
            foreach (LinkedWith neighbourCandidate in movedPiece.GetComponents<LinkedWith>())
            {
                if (neighbourCandidate.link == connectedPiece)
                {
                    connectedPiece.transform.position = new Vector3(movedPiece.transform.position.x - (movedPiece.GetComponent<puzzleDeltaPositions>().xDelta[cpt] * Screen.width / 1280), movedPiece.transform.position.y - (movedPiece.GetComponent<puzzleDeltaPositions>().yDelta[cpt] * Screen.width / 1280), movedPiece.transform.position.z);
                    if (!alreadyMoved.Contains(connectedPiece))
                    {
                        alreadyMoved.Add(connectedPiece);
                        alreadyMoved = moveConnectedPieces(connectedPiece, alreadyMoved);
                    }
                }
                cpt++;
            }
                
        }
        return alreadyMoved;
    }

    private void synchronizeConnections()
    {
        foreach (GameObject piece in f_pieces)
            piece.GetComponent<MagnetizedPieces>().connectedWith.Clear();
        foreach (GameObject piece in f_pieces)
        {
            // parse all neighbours
            int cpt = 0;
            foreach (LinkedWith neighbourCandidate in piece.GetComponents<LinkedWith>())
            {
                // Check if neighbour exists and is active in hierarchy
                if (neighbourCandidate.link && neighbourCandidate.link.activeInHierarchy)
                {
                    // Compute distance between current piece and its neighbour (taking into account x and y deltas)
                    if (Mathf.Abs((piece.transform.position.x - (piece.GetComponent<puzzleDeltaPositions>().xDelta[cpt] * Screen.width / 1280)) - neighbourCandidate.link.transform.position.x) < 5 &&
                            Mathf.Abs((piece.transform.position.y - (piece.GetComponent<puzzleDeltaPositions>().yDelta[cpt] * Screen.width / 1280)) - neighbourCandidate.link.transform.position.y) < 5)
                    {
                        // connect pieces
                        if (!piece.GetComponent<MagnetizedPieces>().connectedWith.Contains(neighbourCandidate.link))
                            piece.GetComponent<MagnetizedPieces>().connectedWith.Add(neighbourCandidate.link);
                        if (!neighbourCandidate.link.GetComponent<MagnetizedPieces>().connectedWith.Contains(piece))
                            neighbourCandidate.link.GetComponent<MagnetizedPieces>().connectedWith.Add(piece);
                    }

                }
                cpt++;
            }
        }
    }
}