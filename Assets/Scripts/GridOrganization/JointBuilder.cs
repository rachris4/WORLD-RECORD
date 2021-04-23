using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JointBuilder : MonoBehaviour
{
    [SerializeField]
    public bool Mirror;
    [SerializeField]
    public string[] Bodyparts;
    [SerializeField]
    public float rotationMax;
    [SerializeField]
    public float rotationMin;

    public ControllerBuilder connectedJoint;
    public LineRenderer line;

    private bool connecting;
    private GameObject selectedObject;
    private BuildSystem BuildSystem;
    private GameObject panel;
    public GameObject gridButtons;
    public string presumptiveLimb;

    public void Start()
    {
        line = gameObject.AddComponent<LineRenderer>();
        line.enabled = false;
        Material newMat = Resources.Load<Material>("connection");
        line.material = newMat;
        line.textureMode = LineTextureMode.Tile;
        line.sortingOrder = 1000;
        BuildSystem = Utilities.FindGameManager().GetComponent<BuildSystem>();
        //gridButtons = BuildSystem.Canvas.transform.Find("GridButtons")?.gameObject;
        panel = BuildSystem.panel.gameObject;
    }
    public void ConnectJoint()
    {
        Debug.Log("butto worko");

        connecting = true;


    }
    public void Update()
    {
        if (connecting)
        {
            ConnectingLimb();
            return;
        }

        if (connectedJoint != null)
        {
            UpdateVisualConnections();
            if (Bodyparts == null || Bodyparts.Length == 0)
                Bodyparts = new string[1];
            Bodyparts[0] = connectedJoint.transform.parent.gameObject.name;
        }
        else if(presumptiveLimb != null)
        {
            SearchForLove();
            line.enabled = false;
        }
        else
        {
            line.enabled = false;
        }

    }

    private void DrawLine(Vector3 a, Vector3 b)
    {
        line.SetPosition(0, a);
        line.SetPosition(1, b);
      //  float dist = (a - b).magnitude;
       // line.material.mainTextureScale = new Vector2(dist*2,1f);
    }
    public void ConnectingLimb()
    {
        if (BuildSystem.buildModeOn)
            BuildSystem.buildModeOn = false;
        
        line.enabled = true;

        gridButtons.SetActive(false);
        panel.SetActive(false);


        Vector3 loc = Utilities.GetWorldPositionOnPlane(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f), 0f);

        DrawLine(gameObject.transform.position, loc);


        RaycastHit2D rayhit;

        rayhit = Physics2D.Raycast(loc, Vector2.zero, Mathf.Infinity, BuildSystem.allBlocksLayer);
        bool isValid = true;

        if (rayhit.collider?.gameObject == null)
        {
            ManageSelection(false);
            isValid = false;
        }
        else
        {
            if (rayhit.collider.gameObject.transform == gameObject.transform.parent)
                isValid = false;
            else if (rayhit.collider.gameObject.GetComponent<ControllerBuilder>() == null)
                isValid = false;
            else if (rayhit.collider.gameObject.GetComponent<ControllerBuilder>().connectedJoint != null)
                isValid = false;

            ManageSelection(isValid, rayhit.collider.gameObject);
        }

        if (Input.GetMouseButtonDown(0) && isValid)
        {
            var rotor = rayhit.collider.gameObject.GetComponent<ControllerBuilder>();
            connectedJoint = rotor;
            rotor.connectedJoint = this;
            connecting = false;
            ManageSelection(true);
            gridButtons.SetActive(true);
            panel.SetActive(true);

        }
        else if(Input.GetMouseButtonDown(0) && !isValid)
        {
            connecting = false;
            gridButtons.SetActive(true);
            panel.SetActive(true);
            ManageSelection(false);
        }
    }

    public void ManageSelection(bool isValid, GameObject obj = null)
    {
        if(selectedObject != null)
        {
            var rend = selectedObject.GetComponent<SpriteRenderer>();
            if (rend != null)
                rend.color = new Color(1f, 1f, 1f, 1f);
        }
        
        if(obj != null)
        {
            selectedObject = obj;
            var rend = selectedObject.GetComponent<SpriteRenderer>();
            if (rend != null && isValid)
                rend.color = new Color(0.6f, 0.6f, 1f, 1f);
            else if (rend != null && !isValid)
            {
                rend.color = new Color(1f, 0.6f, 0.6f, 1f);
            }
        }

    }

    private void SearchForLove()
    {
        var con = FindInEditor(presumptiveLimb);
        if (con != null)
            foreach (Transform child in con.transform)
            {
                if (child.gameObject.GetComponent<ControllerBuilder>() != null)
                {
                    var jnt = child.gameObject.GetComponent<ControllerBuilder>();
                    if(jnt.connectedJoint == null)
                        connectedJoint = child.gameObject.GetComponent<ControllerBuilder>();
                }
            }
    }

    private GameObject FindInEditor(string name)
    {
        GameObject result = GameObject.Find(name);
        if (result == null || result?.transform.parent != null)
            return null;
        else
            return result;
    }
    public void UpdateVisualConnections()
    {
        line.enabled = true;
        DrawLine(gameObject.transform.position, connectedJoint.transform.position);
    }
}

public class ControllerBuilder : MonoBehaviour
{
    [SerializeField]
    public string fwd;
    [SerializeField]
    public string bck;
    [SerializeField]
    public string Type;
    [SerializeField]
    public float StrengthMod;
    [SerializeField]
    public float SpeedMod;
    [SerializeField]
    public float P;
    [SerializeField]
    public float I;
    [SerializeField]
    public float D;
    [SerializeField]
    public float Rotation;
    [SerializeField]
    public float Border;
    [SerializeField]
    public float SpeedLimit;
    [SerializeField]
    public int Wavelength;
    [SerializeField]
    public float Offset;
    [SerializeField]
    public string ParentName;

    public JointBuilder connectedJoint;
}
