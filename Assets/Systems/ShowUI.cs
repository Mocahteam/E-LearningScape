using UnityEngine;
using FYFY;
using FYFY_plugins.PointerManager;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;
using System.Collections.Generic;
using TMPro;

public class ShowUI : FSystem {

    private Family objects = FamilyManager.getFamily(new AnyOfTags("Object", "Plank", "Box", "Tablet", "TableE05"), new AllOfComponents(typeof(Selectable)));
    private Family buttons = FamilyManager.getFamily(new AllOfComponents(typeof(Button)));
    private Family ui = FamilyManager.getFamily(new AllOfComponents(typeof(Canvas)));
    private Family player = FamilyManager.getFamily(new AnyOfTags("Player"));
    private Family plank = FamilyManager.getFamily(new AnyOfTags("Plank"));
    private Family plankWords = FamilyManager.getFamily(new AnyOfTags("PlankText"), new AllOfComponents(typeof(PointerSensitive)));
    private Family box = FamilyManager.getFamily(new AnyOfTags("Box"));
    private Family boxTop = FamilyManager.getFamily(new AnyOfTags("BoxTop"));
    private Family balls = FamilyManager.getFamily(new AnyOfTags("Ball"));
    private Family tablet = FamilyManager.getFamily(new AnyOfTags("Tablet"));
    private Family screen1 = FamilyManager.getFamily(new AnyOfTags("Screen1"));
    
    private bool noSelection = true;
    private bool moveToPlank = false;
    private float speed = 0.1f;
    private float speedRotationX = 0;
    private float speedRotationX2 = 0;
    private Vector3 plankPos = Vector3.zero;
    private bool onPlank = false;
    private float dist = -1;
    private Vector3 plankTopRight;
    private bool pointerOverWord = false;
    private LineRenderer lr;
    private List<Vector3> lrPositions;
    private Vector3 lrFirstPosition = Vector3.zero;
    private Vector3 objectPos = Vector3.zero;
    private bool moveBox = false;
    private bool openBox = false;
    private bool takingBalls = false;
    private bool onBox = false;
    private Vector3 boxTopPos = Vector3.zero;
    private Vector3 boxTopIniPos;
    private bool ballsout = false;
    private int tmpCount = -1;
    private Vector3 ballPos = Vector3.zero;
    private bool ballFocused = false;
    private bool moveBall = false;
    private GameObject focusedBall = null;
    private Vector3 ballToCamera;
    private Vector3 boxTopRight = Vector3.zero;
    private bool onTablet = false;
    private bool moveTablet = false;
    private Vector3 tabletTopRight = Vector3.zero;
    private float aTabletDist = 1.42857143f;
    private float bTabletDist = -0.58571429f;
    private bool onTable = false;
    private bool moveToTable = false;
    private Vector3 posBeforeTable = Vector3.zero;
    private Quaternion rotBeforeTable;
    private Quaternion rot2BeforeTable;
    private Vector3 onTablePoint = Vector3.zero;
    private GameObject focusedTable = null;
    private Vector3 camNewDir;
    private Vector3 newDir;


    public ShowUI()
    {
        ballToCamera = Camera.main.transform.position + Camera.main.transform.forward;
        boxTopIniPos = new Vector3(0, 0.2625f, 0);
        plankTopRight = plank.First().transform.position + plank.First().transform.localScale/2;
        lr = plank.First().GetComponent<LineRenderer>();
        lrPositions = new List<Vector3>();
        foreach (GameObject button in buttons)
        {
            if(button.name == "Close")
            {
                button.GetComponent<Button>().onClick.AddListener(CloseWindow);
            }
        }
        foreach (GameObject c in ui)
        {
            if (c.name == "UI")
            {
                c.SetActive(false);
            }
            else if (c.name == "Cursor")
            {
                c.SetActive(true);
            }
            else if (c.name == "Timer")
            {
                c.SetActive(true);
            }
        }
        Ball b = null;
        int i = 0;
        foreach(GameObject go in balls)
        {
            b = go.GetComponent<Ball>();
            b.initialPosition = go.transform.localPosition;
            b.id = i;
            i++;
            go.GetComponent<Renderer>().material.color = Random.ColorHSV() + Color.white/2.5f;
            b.color = go.GetComponent<Renderer>().material.color;
            //init text and number
        }
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
        if (moveToPlank)
        {
            player.First().transform.position = Vector3.MoveTowards(player.First().transform.position, plankPos, speed);
            camNewDir = Vector3.left;
            newDir = Vector3.RotateTowards(Camera.main.transform.forward, camNewDir, Mathf.Deg2Rad * speedRotationX, 0);
            Camera.main.transform.rotation = Quaternion.LookRotation(newDir);
            if (Vector3.Angle(Camera.main.transform.forward, camNewDir) < 0.5f && player.First().transform.position == plankPos)
            {
                newDir = Vector3.RotateTowards(player.First().transform.forward, Vector3.left, 360, 0);
                player.First().transform.rotation = Quaternion.LookRotation(newDir);
                newDir = Vector3.RotateTowards(Camera.main.transform.forward, camNewDir, 360, 0);
                Camera.main.transform.rotation = Quaternion.LookRotation(newDir);
                foreach (GameObject c in ui)
                {
                    if (c.name == "UI")
                    {
                        c.SetActive(true);
                        foreach (Transform child in c.transform)
                        {
                            if (child.gameObject.name == "Close")
                            {
                                child.gameObject.GetComponent<RectTransform>().localPosition = Camera.main.WorldToScreenPoint(plankTopRight);
                                child.gameObject.GetComponent<RectTransform>().localPosition = child.gameObject.GetComponent<RectTransform>().localPosition -new Vector3(child.gameObject.GetComponent<RectTransform>().rect.width + Camera.main.pixelWidth, child.gameObject.GetComponent<RectTransform>().rect.height + Camera.main.pixelHeight, 0)/2;
                            }
                        }
                    }
                }
                moveToPlank = false;
            }
        }
        else if (moveBox)
        {
            if (openBox)
            {
                boxTop.First().transform.position = Vector3.MoveTowards(boxTop.First().transform.position, boxTopPos, speed / 10);
                if(boxTop.First().transform.position == boxTopPos)
                {
                    openBox = false;
                    takingBalls = true;
                    tmpCount = 1;
                }
            }
            else if (takingBalls)
            {
                Ball b = null;
                foreach(GameObject ball in balls)
                {
                    b = ball.GetComponent<Ball>();
                    ball.GetComponent<Rigidbody>().isKinematic = true;
                    if (b.id < tmpCount)
                    {
                        if (b.outOfBox)
                        {
                            ballPos = Vector3.up * ((float)balls.Count/10 - (float)(b.id/5)/3) + Vector3.right * ((float)(b.id%5) * -2f / 4+1f)+Vector3.forward*0.2f;
                            ball.transform.localPosition = Vector3.MoveTowards(ball.transform.localPosition, ballPos, speed);
                            if (ball.transform.localPosition == ballPos && b.id == balls.Count - 1)
                            {
                                takingBalls = false;
                                moveBox = false;
                                ballsout = true;
                                foreach (GameObject c in ui)
                                {
                                    if (c.name == "UI")
                                    {
                                        c.SetActive(true);
                                        boxTopRight = Vector3.up * ((float)balls.Count / 10 + 0.3f) + Vector3.right * -1.3f + Vector3.forward * 0.2f;
                                        foreach (Transform child in c.transform)
                                        {
                                            if (child.gameObject.name == "Close")
                                            {
                                                child.gameObject.GetComponent<RectTransform>().localPosition = Camera.main.WorldToScreenPoint(box.First().transform.TransformPoint(boxTopRight));
                                                child.gameObject.GetComponent<RectTransform>().localPosition = child.gameObject.GetComponent<RectTransform>().localPosition - new Vector3(child.gameObject.GetComponent<RectTransform>().rect.width + Camera.main.pixelWidth, child.gameObject.GetComponent<RectTransform>().rect.height + Camera.main.pixelHeight, 0) / 2;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            ball.transform.localPosition = Vector3.MoveTowards(ball.transform.localPosition, Vector3.up, speed);
                            ball.transform.localRotation = Quaternion.Euler(Vector3.up*-90);
                            if(ball.transform.localPosition == Vector3.up)
                            {
                                b.outOfBox = true;
                                tmpCount++;
                            }
                        }
                    }
                }
            }
            else
            {
                box.First().transform.position = Vector3.MoveTowards(box.First().transform.position, objectPos, speed);
                if(player.First().transform.localScale.x < 0.9f)
                {
                    player.First().transform.position = Vector3.MoveTowards(player.First().transform.position, player.First().transform.position + Vector3.up * (1.6f - player.First().transform.position.y), (1.6f - 0.26f) * speed / dist);
                }
                newDir = Vector3.RotateTowards(box.First().transform.forward, -player.First().transform.forward, Mathf.Deg2Rad * speedRotationX, 0);
                box.First().transform.rotation = Quaternion.LookRotation(newDir);
                Camera.main.transform.localRotation = Quaternion.Euler(Vector3.MoveTowards(Camera.main.transform.localRotation.eulerAngles, Vector3.zero, speedRotationX2));
                if(box.First().transform.position == objectPos && box.First().transform.forward == -player.First().transform.forward)
                {
                    openBox = true;
                    boxTopPos = boxTop.First().transform.position + box.First().transform.right * boxTop.First().transform.localScale.x;
                    ballToCamera = Camera.main.ScreenToWorldPoint(new Vector3(Camera.main.pixelWidth / 2, Camera.main.pixelHeight / 2, 0.5f));
                }
            }
        }
        else if (moveBall)
        {
            if (ballFocused)
            {
                focusedBall.transform.position = Vector3.MoveTowards(focusedBall.transform.position, ballToCamera, speed / 2);
                focusedBall.transform.localRotation = Quaternion.RotateTowards(focusedBall.transform.localRotation, Quaternion.Euler(0, 90, 0), speedRotationX);
                if (focusedBall.transform.position == ballToCamera)
                {
                    moveBall = false;
                    foreach (Transform child in focusedBall.transform)
                    {
                        if (child.gameObject.name == "Text")
                        {
                            child.gameObject.SetActive(false);
                        }
                    }
                }
            }
            else
            {
                ballPos = Vector3.up * ((float)balls.Count / 10 - (float)(focusedBall.GetComponent<Ball>().id / 5) / 3) + Vector3.right * ((float)(focusedBall.GetComponent<Ball>().id % 5) * -2f / 4 + 1f) + Vector3.forward * 0.2f;
                focusedBall.transform.localPosition = Vector3.MoveTowards(focusedBall.transform.localPosition, ballPos, speed / 2);
                focusedBall.transform.localRotation = Quaternion.RotateTowards(focusedBall.transform.localRotation, Quaternion.Euler(0, -90, 0), speedRotationX);
                if (focusedBall.transform.localPosition == ballPos)
                {
                    moveBall = false;
                    foreach (Transform child in focusedBall.transform)
                    {
                        if (child.gameObject.name == "Number")
                        {
                            child.gameObject.SetActive(false);
                        }
                    }
                }
            }
        }
        else if (moveTablet)
        {
            float d = 0.33f;
            if ((float)Camera.main.pixelWidth / 900 < (float)Camera.main.pixelHeight / 600)
            {
                d = aTabletDist * (float)Camera.main.pixelHeight / (float)Camera.main.pixelWidth + bTabletDist;
            }
            else
            {
                d = 0.56f;
            }
            tablet.First().GetComponent<Rigidbody>().isKinematic = true;
            Vector3 vec = new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z);
            vec.Normalize();
            objectPos = new Vector3(player.First().transform.position.x, 1.78f, player.First().transform.position.z) + vec * (d);
            tablet.First().transform.position = Vector3.MoveTowards(tablet.First().transform.position, objectPos, speed);
            newDir = Vector3.RotateTowards(tablet.First().transform.forward, player.First().transform.forward, Mathf.Deg2Rad * speedRotationX, 0);
            tablet.First().transform.rotation = Quaternion.LookRotation(newDir);
            Camera.main.transform.localRotation = Quaternion.Euler(Vector3.MoveTowards(Camera.main.transform.localRotation.eulerAngles, Vector3.zero, speedRotationX2));
            if (tablet.First().transform.position == objectPos)
            {
                moveTablet = false;
                foreach (GameObject c in ui)
                {
                    if (c.name == "UI")
                    {
                        c.SetActive(true);
                        tabletTopRight = Vector3.up * Camera.main.pixelHeight + Vector3.right * Camera.main.pixelWidth;
                        foreach (Transform child in c.transform)
                        {
                            if (child.gameObject.name == "Close")
                            {
                                child.gameObject.GetComponent<RectTransform>().localPosition = tabletTopRight;
                                child.gameObject.GetComponent<RectTransform>().localPosition = child.gameObject.GetComponent<RectTransform>().localPosition - new Vector3(child.gameObject.GetComponent<RectTransform>().rect.width + Camera.main.pixelWidth, child.gameObject.GetComponent<RectTransform>().rect.height + Camera.main.pixelHeight, 0) / 2;
                            }
                        }
                    }
                }
                screen1.First().GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceCamera;
            }
        }
        else if (moveToTable)
        {
            player.First().GetComponent<Rigidbody>().detectCollisions = false;
            if (onTable)
            {
                player.First().transform.position = Vector3.MoveTowards(player.First().transform.position, onTablePoint, speed);
                Camera.main.transform.localRotation = Quaternion.RotateTowards(Camera.main.transform.localRotation, Quaternion.Euler(90,0,0), speedRotationX);
                player.First().transform.rotation = Quaternion.RotateTowards(player.First().transform.rotation, Quaternion.Euler(0, focusedTable.transform.rotation.eulerAngles.y, 0), speedRotationX2);
                if (player.First().transform.position == onTablePoint)
                {
                    foreach (GameObject c in ui)
                    {
                        if (c.name == "UI")
                        {
                            c.SetActive(true);
                            foreach (Transform child in c.transform)
                            {
                                if (child.gameObject.name == "Close")
                                {
                                    child.gameObject.GetComponent<RectTransform>().localPosition = Vector3.up * Camera.main.pixelHeight + Vector3.right * Camera.main.pixelWidth;
                                    child.gameObject.GetComponent<RectTransform>().localPosition = child.gameObject.GetComponent<RectTransform>().localPosition - new Vector3(child.gameObject.GetComponent<RectTransform>().rect.width + Camera.main.pixelWidth, child.gameObject.GetComponent<RectTransform>().rect.height + Camera.main.pixelHeight, 0) / 2;
                                }
                            }
                        }
                    }
                    moveToTable = false;
                }
            }
            else
            {
                player.First().transform.position = Vector3.MoveTowards(player.First().transform.position, posBeforeTable, speed);
                Camera.main.transform.localRotation = Quaternion.RotateTowards(Camera.main.transform.localRotation, rotBeforeTable, speedRotationX);
                player.First().transform.rotation = Quaternion.RotateTowards(player.First().transform.rotation, rot2BeforeTable, speedRotationX2);
                if (player.First().transform.position == posBeforeTable)
                {
                    moveToTable = false;
                    player.First().GetComponent<FirstPersonController>().enabled = true;
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                    player.First().GetComponent<Rigidbody>().detectCollisions = true;
                }
            }
        }

        if (noSelection)
        {
            foreach (GameObject go in objects)
            {
                if (go.GetComponent<Selectable>().isSelected)
                {
                    noSelection = false;
                    //show ui
                    foreach(GameObject c in ui)
                    {
                        if(c.name == "UI")
                        {
                            c.SetActive(true);
                            foreach (Transform child in c.transform)
                            {
                                if (child.gameObject.name == "Window")
                                {
                                    child.gameObject.SetActive(false);
                                    if (go.tag == "Object")
                                    {
                                        child.gameObject.SetActive(true);
                                    }
                                    else if(go.tag == "Plank")
                                    {
                                        if(player.First().transform.localScale.x < 0.9f)
                                        {
                                            plankPos = new Vector3(plank.First().transform.position.x + 1.5f, 1.6f, plank.First().transform.position.z);
                                        }
                                        else
                                        {
                                            plankPos = new Vector3(plank.First().transform.position.x + 1.5f, 0.98f, plank.First().transform.position.z);
                                        }
                                        dist = (plankPos - player.First().transform.position).magnitude;
                                        foreach (Transform t in player.First().transform)
                                        {
                                            if (t.gameObject.tag == "MainCamera")
                                            {
                                                speedRotationX = Vector3.Angle(t.gameObject.transform.forward, Vector3.left) * speed / dist;
                                            }
                                        }
                                        moveToPlank = true;
                                        c.SetActive(false);
                                        onPlank = true;
                                    }
                                    else if(go.tag == "Box")
                                    {
                                        go.GetComponent<Rigidbody>().isKinematic = true;
                                        Vector3 v = new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z);
                                        v.Normalize();
                                        objectPos = new Vector3(player.First().transform.position.x, 1.78f-1, player.First().transform.position.z) + v * (go.transform.localScale.y + 1.5f);
                                        dist = (objectPos - go.transform.position).magnitude;
                                        speedRotationX = Vector3.Angle(box.First().transform.forward, -player.First().transform.forward) * speed / dist;
                                        speedRotationX2 = Camera.main.transform.localRotation.eulerAngles.magnitude * speed / dist;
                                        moveBox = true;
                                        c.SetActive(false);
                                        onBox = true;
                                    }
                                    else if(go.tag == "Tablet")
                                    {
                                        go.GetComponent<Rigidbody>().isKinematic = true;
                                        float d = 0.33f;
                                        if ((float)Camera.main.pixelWidth / 900 < (float)Camera.main.pixelHeight / 600)
                                        {
                                            d = aTabletDist * (float)Camera.main.pixelHeight / (float)Camera.main.pixelWidth + bTabletDist;
                                        }
                                        else
                                        {
                                            d = 0.56f;
                                        }
                                        tablet.First().GetComponent<Rigidbody>().isKinematic = true;
                                        Vector3 v = new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z);
                                        v.Normalize();
                                        objectPos = new Vector3(player.First().transform.position.x, 1.78f, player.First().transform.position.z) + v * (d);
                                        dist = (objectPos - go.transform.position).magnitude;
                                        speedRotationX = Vector3.Angle(tablet.First().transform.forward, player.First().transform.forward) * speed / dist;
                                        speedRotationX2 = Camera.main.transform.localRotation.eulerAngles.magnitude * speed / dist;
                                        c.SetActive(false);
                                        onTablet = true;
                                        moveTablet = true;
                                    }
                                    else if (go.tag == "TableE05")
                                    {
                                        if (go.name.Contains(1.ToString()))
                                        {
                                            onTablePoint = go.transform.position - go.transform.right * 0.75f + go.transform.forward * 0.75f + Vector3.up * (go.transform.localScale.y * 0.55f + 1);
                                            dist = (onTablePoint - player.First().transform.position).magnitude;
                                        }
                                        else if (go.name.Contains(2.ToString()))
                                        {
                                            onTablePoint = go.transform.position + go.transform.right * 0.75f + go.transform.forward * 0.75f + Vector3.up * (go.transform.localScale.y * 0.55f + 1);
                                            dist = (onTablePoint - player.First().transform.position).magnitude;
                                        }
                                        else if (go.name.Contains(3.ToString()))
                                        {
                                            onTablePoint = go.transform.position - go.transform.forward * 0.75f + Vector3.up * (go.transform.localScale.y * 0.55f + 1);
                                            dist = (onTablePoint - player.First().transform.position).magnitude;
                                        }
                                        speedRotationX = Vector3.Angle(Camera.main.transform.forward, Vector3.down) * speed / dist;
                                        speedRotationX2 = Vector3.Angle(player.First().transform.forward, go.transform.forward) * speed / dist;
                                        posBeforeTable = player.First().transform.position;
                                        rotBeforeTable = Camera.main.transform.localRotation;
                                        rot2BeforeTable = player.First().transform.rotation;
                                        focusedTable = go;
                                        c.SetActive(false);
                                        onTable = true;
                                        moveToTable = true;
                                    }
                                }
                            }
                        }
                        else if (c.name == "Cursor")
                        {
                            c.SetActive(false);
                        }
                    }
                    //disable player moves
                    player.First().GetComponent<FirstPersonController>().enabled = false;
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.lockState = CursorLockMode.Confined;
                    Cursor.visible = true;
                    break;
                }
            }
        }
        else
        {
            foreach(GameObject go in objects)
            {
                if (go.GetComponent<Selectable>().isSelected)
                {
                    if(go.tag == "Plank")
                    {
                        pointerOverWord = false;
                        foreach(GameObject word in plankWords)
                        {
                            if (word.GetComponent<PointerOver>())
                            {
                                pointerOverWord = true;
                                if (Input.GetMouseButtonDown(0))
                                {
                                    if (word.GetComponent<TextMeshPro>().color == Color.red)
                                    {
                                        word.GetComponent<TextMeshPro>().color = Color.black;
                                        lr.positionCount--;
                                        if (lr.positionCount == 3)
                                        {
                                            lr.positionCount--;
                                            lrPositions.RemoveAt(lrPositions.LastIndexOf(lrFirstPosition));
                                        }
                                        if(word.transform.position == lrFirstPosition)
                                        {
                                            lrPositions.Remove(word.transform.position);
                                            lrFirstPosition = lrPositions.Find(ReturnFirst);
                                        }
                                        else
                                        {
                                            lrPositions.Remove(word.transform.position);
                                        }
                                        lr.SetPositions(lrPositions.ToArray());
                                    }
                                    else
                                    {
                                        if (lr.positionCount > 2)
                                        {
                                            foreach (GameObject w in plankWords)
                                            {
                                                w.GetComponent<TextMeshPro>().color = Color.black;
                                            }
                                            lr.positionCount = 0;
                                            lrPositions.Clear();
                                        }
                                        word.GetComponent<TextMeshPro>().color = Color.red;
                                        lr.positionCount++;
                                        lrPositions.Add(word.transform.position);
                                        if(lr.positionCount == 1)
                                        {
                                            lrFirstPosition = word.transform.position;
                                        }
                                        if (lr.positionCount == 3)
                                        {
                                            lr.positionCount++;
                                            lrPositions.Add(lrFirstPosition);
                                        }
                                        lr.SetPositions(lrPositions.ToArray());
                                    }
                                }
                                else
                                {
                                    if(word.GetComponent<TextMeshPro>().color != Color.red)
                                    {
                                        word.GetComponent<TextMeshPro>().color = Color.yellow;
                                    }
                                }
                            }
                            else
                            {
                                if (word.GetComponent<TextMeshPro>().color != Color.red)
                                {
                                    word.GetComponent<TextMeshPro>().color = Color.black;
                                }
                            }
                        }
                        if (!pointerOverWord && Input.GetMouseButtonDown(0))
                        {
                            foreach (GameObject word in plankWords)
                            {
                                word.GetComponent<TextMeshPro>().color = Color.black;
                                lr.positionCount = 0;
                                lrPositions.Clear();
                            }
                        }
                    }
                    else if (go.tag == "Box")
                    {
                        if (ballsout)
                        {
                            Ball b = null;
                            if (ballFocused)
                            {
                                if(Input.GetMouseButtonDown(0) && !moveBall)
                                {
                                    ballFocused = false;
                                    moveBall = true;
                                    foreach (Transform child in focusedBall.transform)
                                    {
                                        if (child.gameObject.name == "Text")
                                        {
                                            child.gameObject.SetActive(true);
                                        }
                                    }
                                    ballPos = Vector3.up * ((float)balls.Count / 10 - (float)(focusedBall.GetComponent<Ball>().id / 5) / 3) + Vector3.right * ((float)(focusedBall.GetComponent<Ball>().id % 5) * -2f / 4 + 1f) + Vector3.forward * 0.2f;
                                    dist = (box.First().transform.TransformPoint(ballPos) - ballToCamera).magnitude;
                                    speedRotationX = 180 * speed / dist/2;
                                }
                            }
                            else if(!moveBall)
                            {
                                foreach (GameObject ballGO in balls)
                                {
                                    b = ballGO.GetComponent<Ball>();
                                    if (ballGO.GetComponent<PointerOver>() && !ballFocused)
                                    {
                                        ballGO.GetComponent<Renderer>().material.color = Color.yellow + Color.white / 4;
                                        if (Input.GetMouseButtonDown(0))
                                        {
                                            ballFocused = true;
                                            moveBall = true;
                                            focusedBall = ballGO;
                                            foreach(Transform child in focusedBall.transform)
                                            {
                                                if(child.gameObject.name == "Number")
                                                {
                                                    child.gameObject.SetActive(true);
                                                }
                                            }
                                            ballGO.GetComponent<Renderer>().material.color = b.color;
                                            ballPos = Vector3.up * ((float)balls.Count / 10 - (float)(focusedBall.GetComponent<Ball>().id / 5) / 3) + Vector3.right * ((float)(focusedBall.GetComponent<Ball>().id % 5) * -2f / 4 + 1f) + Vector3.forward * 0.2f;
                                            dist = (box.First().transform.TransformPoint(ballPos) - ballToCamera).magnitude;
                                            speedRotationX = 180 * speed / dist/2;
                                        }
                                    }
                                    else
                                    {
                                        ballGO.GetComponent<Renderer>().material.color = b.color;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
	}

    private void CloseWindow()
    {
        if (!onTable)
        {
            player.First().GetComponent<FirstPersonController>().enabled = true;
            Cursor.lockState = CursorLockMode.None;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        foreach (GameObject c in ui)
        {
            if (c.name == "UI")
            {
                if (onPlank)
                {
                    foreach (Transform child in c.transform)
                    {
                        if (child.gameObject.name == "Close")
                        {
                            child.gameObject.GetComponent<RectTransform>().localPosition = new Vector3(105, 90, 0);
                        }
                    }
                }
                else if(onBox)
                {
                    foreach (Transform child in c.transform)
                    {
                        if (child.gameObject.name == "Close")
                        {
                            child.gameObject.GetComponent<RectTransform>().localPosition = new Vector3(105, 90, 0);
                        }
                    }
                    boxTop.First().transform.localPosition = boxTopIniPos;
                    box.First().GetComponent<Rigidbody>().isKinematic = false;
                    foreach(GameObject go in balls)
                    {
                        go.transform.localPosition = go.GetComponent<Ball>().initialPosition;
                        go.GetComponent<Ball>().outOfBox = false;
                        go.GetComponent<Rigidbody>().isKinematic = false;
                    }
                    tmpCount = 1;
                }
                else if (onTablet)
                {
                    foreach (Transform child in c.transform)
                    {
                        if (child.gameObject.name == "Close")
                        {
                            child.gameObject.GetComponent<RectTransform>().localPosition = new Vector3(105, 90, 0);
                            tablet.First().GetComponent<Rigidbody>().isKinematic = false;
                        }
                    }
                    screen1.First().GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
                    screen1.First().GetComponent<RectTransform>().localPosition = Vector3.forward * -0.026f;
                }
                else if (onTable)
                {
                    foreach (Transform child in c.transform)
                    {
                        if (child.gameObject.name == "Close")
                        {
                            child.gameObject.GetComponent<RectTransform>().localPosition = new Vector3(105, 90, 0);
                        }
                    }
                    moveToTable = true;
                }
                c.SetActive(false);
            }
            else if (c.name == "Cursor")
            {
                c.SetActive(true);
            }
        }
        foreach (GameObject go in objects)
        {
            if (go.GetComponent<Selectable>().isSelected)
            {
                go.GetComponent<Selectable>().isSelected = false;
                Selectable.selected = false;
                noSelection = true;
                break;
            }
        }
        onPlank = false;
        onBox = false;
        onTablet = false;
        onTable = false;
    }

    private bool ReturnFirst(Vector3 v)
    {
        return true;
    }
}