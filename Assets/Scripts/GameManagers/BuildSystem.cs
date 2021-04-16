using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuildSystem : MonoBehaviour
{
    //ref for blocksystem script
    public BlockSystem blockSys;
    public GameObject gridObject;
    public Grid buildGrid;
    public BodyPart limb;
    private int saveWait = 0;
    //vars to hold data regarding current block type.
    private int currentBlockID = 0;
    private Block currentBlock;
    private int selectableBlocksTotal;

    // vars for the block template
    private GameObject blockTemplate;
    private SpriteRenderer currentRend;

    // float to adjust the size of blocks when placing in world;
    [SerializeField]
    private float blockSizeMod = 1f;

    // bools to control
    private bool buildModeOn = false;
    private bool buildBlocked = false;
    [SerializeField]
    public GameObject buttonPrefab;

    [SerializeField]
    private string blueprintName;
    [SerializeField]
    private string TypeID;
    //layer mask to control raycasting
    [SerializeField]
    private LayerMask solidNoBuildLayer;
    [SerializeField]
    private LayerMask fakeNoBuildLayer;
    [SerializeField]
    private LayerMask allBlocksLayer;
    [SerializeField]
    private LayerMask doNotBuild;
    [SerializeField]
    private TMP_InputField textMesh;


    private bool IsAlien = false;
    private int moveTick;
    private List<GameObject> buildLimbs = new List<GameObject>();

    private void Awake()
    {
        //ref for vlock sys
        //blockSys = GetComponent<BlockSystem>();


    }

    void Start()
    {
        //Debug.Log("I am alive!");
        gridObject = new GameObject("LimbName");
        buildLimbs.Add(gridObject);
        gridObject.AddComponent<BuildCamera>();
        buildGrid = gridObject.AddComponent<Grid>();
        if (TypeID == "Alien")
            IsAlien = true;
    }

    private void Update()
    {

        if (blockSys == null)
            return;
        // if E pressed, build mode

        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown("c") && saveWait < 0)
        {
            Debug.Log("SAVING!"); //huhhuhuuhuhuhu
            Save(true);
            saveWait = 1000;
        }
        else if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown("f") && saveWait < 0)
        {
            Debug.Log("SAVING AS VARIANT!"); //huhhuhuuhuhuhu
            Save(false);
            saveWait = 1000;
        }
        else if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown("v") && saveWait < 0)
        {
            Debug.Log("LOADING!");
            Load(gridObject.name);
            saveWait = 1000;
        }
        else
            saveWait--;









        if (Input.GetKeyDown("e"))
        {
            buildModeOn = !buildModeOn;


            // remove duplicate sprite for template.
            if(blockTemplate != null && !buildModeOn)
            {
                Destroy(blockTemplate);
            }

            // if we dont have a current block type selected
            /*
            if(currentBlock == null)
            {
                // ensure allblocks is ready
                if(blockSys.allBlocks[currentBlockID] != null)
                {
                    // get a new currentblockid
                    currentBlock = blockSys.allBlocks[currentBlockID];
                }
            }*/

            if(buildModeOn && blockTemplate == null)
            {
                //create a new obj for blocktemplate
                blockTemplate = new GameObject("CurrentBlockTemplate");
                // add and store reference toa  spriterenderer on the template
                currentRend = blockTemplate.AddComponent<SpriteRenderer>();
                // set sprite to match current block type
                currentRend.sprite = currentBlock.blockSprite;
                currentRend.sortingOrder = 1000;


            }
        }

        if (Input.mousePosition.x == 0 || Input.mousePosition.y <= 75 || Input.mousePosition.x == Screen.width - 1 || Input.mousePosition.y >= Screen.height - 75) return;


        if (buildModeOn && blockTemplate != null)
        {
            Vector3 scaler = blockTemplate.transform.localScale;
            float rotz = blockTemplate.transform.localEulerAngles.z;

            if (Input.GetKeyDown("r"))
            {
                moveTick = 1000;

                if (!IsAlien)
                    blockTemplate.transform.Rotate(0, 0, 90);
                else
                    blockTemplate.transform.Rotate(0, 0, 60);
            }
            if (Input.GetKeyDown("f"))
            {
                moveTick = 1000;

                if ((rotz > 85 && rotz < 95) || (rotz > 265 && rotz < 275))
                    scaler.y *= -1;
                else
                    scaler.x *= -1;
                blockTemplate.transform.localScale = scaler;
            }
            //float newPosX = Mathf.Round(Camera.main.ScreenToWorldPoint(Input.mousePosition).x / blockSizeMod) * blockSizeMod;
            //float newPosY = Mathf.Round(Camera.main.ScreenToWorldPoint(Input.mousePosition).y / blockSizeMod) * blockSizeMod;
            //Debug.Log((Mathf.Round(Input.mousePosition.y / blockSizeMod)*blockSizeMod).ToString());
            Vector3 loc = Utilities.GetWorldPositionOnPlane(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f), 0f);
            if (!IsAlien)
                blockTemplate.transform.position = new Vector3(Mathf.Round(loc.x / blockSizeMod) * blockSizeMod, Mathf.Round(loc.y / blockSizeMod) * blockSizeMod, 0f);
            else
                blockTemplate.transform.position = Utilities.RoundToHexCoordinates(loc, blockSizeMod);
            if (currentBlock == null)
                return;

            blockTemplate.transform.position += Quaternion.Euler(0f,0f,rotz)*new Vector3(currentBlock.spriteOffset.x*scaler.x, currentBlock.spriteOffset.y*scaler.y, 0f);
            RaycastHit2D rayhit;

            rayhit = Physics2D.Raycast(loc, Vector2.zero, Mathf.Infinity, allBlocksLayer);

            if (rayhit.collider != null)
            {
                buildBlocked = true;
            }
            else
            {
                buildBlocked = false;
            }
            rayhit = Physics2D.Raycast(blockTemplate.transform.position, Vector2.zero, Mathf.Infinity, allBlocksLayer);
            if (rayhit.collider != null)
            {
                buildBlocked = true;
            }
               
            
            if (buildBlocked)
            {
                currentRend.color = new Color(1f, 0f, 0f, 1f);
            }
            else
            {
                currentRend.color = new Color(1f, 1f, 1f, 1f);
            }

            float mouseWheel = Input.GetAxis("Mouse ScrollWheel");

            if(mouseWheel !=0)
            {
                selectableBlocksTotal = blockSys.allBlocks.Length - 1;

                if(mouseWheel > 0)
                {
                    currentBlockID--;

                    if(currentBlockID < 0)
                    {
                        currentBlockID = selectableBlocksTotal;
                    }

                } else
                {
                    currentBlockID++;

                    if (currentBlockID > selectableBlocksTotal)
                        currentBlockID = 0;

                }

                currentBlock = blockSys.allBlocks[currentBlockID];
                currentRend.sprite = currentBlock.blockSprite;
            }


            if (Input.GetMouseButtonDown(0))
            {
                moveTick = 1000;

                if(buildBlocked && rayhit.collider.gameObject != null)
                {
                    Destroy(rayhit.collider.gameObject);
                } else if(buildBlocked)
                {
                    rayhit = Physics2D.Raycast(loc, Vector2.zero, Mathf.Infinity, allBlocksLayer);
                    Destroy(rayhit.collider.gameObject);
                }

                GameObject newBlock = new GameObject(currentBlock.SubTypeID);
                newBlock.transform.position = blockTemplate.transform.position;
                newBlock.transform.parent = buildGrid.transform;
                newBlock.transform.localScale = blockTemplate.transform.localScale;
                newBlock.transform.rotation = blockTemplate.transform.rotation;
                SpriteRenderer newRend = newBlock.AddComponent<SpriteRenderer>();
                Debug.Log(Utilities.ConvertToHexagonalCoordinates(newBlock.transform.position, blockSizeMod).ToString());
                newRend.sprite = currentBlock.blockSprite;

                InitializeTypeIDBuilder(newBlock,currentBlock);

                PolygonCollider2D shape;
                switch (currentBlock.collider)
                {
                    case "box":
                        newBlock.AddComponent<BoxCollider2D>();
                        break;
                    case "triangle":
                        shape = newBlock.AddComponent<PolygonCollider2D>();
                        shape.pathCount = 1;
                        shape.points = new Vector2[] { new Vector2(-0.5f, 0.5f), new Vector2(-0.5f, -0.5f), new Vector2(0.5f, -0.5f) };
                        break;
                    case "toptwoslope":
                        shape = newBlock.AddComponent<PolygonCollider2D>();
                        shape.pathCount = 1;
                        shape.points = new Vector2[] { new Vector2(-0.5f, 0.5f), new Vector2(-0.5f, -0.5f), new Vector2(0.5f, -0.5f), new Vector2(0.5f, 0f) };
                        break;
                    case "bottwoslope":
                        shape = newBlock.AddComponent<PolygonCollider2D>();
                        shape.pathCount = 1;
                        shape.points = new Vector2[] { new Vector2(-0.5f, 0f), new Vector2(-0.5f, -0.5f), new Vector2(0.5f, -0.5f) };
                        break;
                    default:
                        newBlock.AddComponent<BoxCollider2D>();
                        break;
                }



                if(newBlock.layer == 1)
                {
                    if (currentBlock.isSolid)
                    {
                        newBlock.layer = 9;
                    }
                    else
                    {
                        newBlock.layer = 10;
                    }
                }

                newRend.sortingOrder = (-newBlock.layer + 7) * 10;

            }
            if (Input.GetMouseButtonDown(1) && blockTemplate != null)
            {
                moveTick = 1000;

                RaycastHit2D destroyHit = Physics2D.Raycast(blockTemplate.transform.position, Vector2.zero, Mathf.Infinity, allBlocksLayer);
                if (destroyHit.collider != null)
                {
                    Destroy(destroyHit.collider.gameObject);
                }
            }
        }

        moveTick--;
        var bcam = gridObject.GetComponent<BuildCamera>();

        if (moveTick < 0)
        {
            bcam.canMove = true;
        }
        else
            bcam.canMove = false;

    }

    public void InitializeTypeIDBuilder(GameObject obj, Block square)
    {
        switch (square.TypeID)
        {
            case "FixedWeaponBlock":
                //InitializeFixedWeapon(obj);
                break;
            case "TurretBlock":
                //InitializeTurret(obj);
                break;
            case "JointStator":
                InitializeJointStator(obj);
                break;
            case "JointRotor":
                InitializeJointRotor(obj);
                break;
            default:
                break;
        }
    }

    public void InitializeJointRotor(GameObject obj)
    {

    }

    public void InitializeJointStator(GameObject obj)
    {
        var jb = obj.AddComponent<JointBuilder>();
    }

    public void Save(bool overwrite = false)
    {
        limb = new BodyPart();
        gridObject.name = textMesh.text;
        limb.SubTypeID = gridObject.name;
        limb.TypeID = TypeID;
        Debug.Log(gridObject.transform.childCount.ToString());

        foreach (Transform child in gridObject.transform)
        {
            var item = child.gameObject;
            
            //Debug.Log("DEBUG: VECTOR CHILD: " + child.position.ToString());
            Block square = new Block();
            if (blockSys.blockLookup.TryGetValue(item.name, out square))
            {


               

                Block j = new Block();
                j.TypeID = square.TypeID;
                j.SubTypeID = square.SubTypeID;
                j.DisplayName = square.DisplayName;

                //j.isSolid = square.isSolid;
                //j.collider = square.collider;
                j.blockLocation = new SerializableVector2(new Vector2(child.position.x, child.position.y));
                j.rotation = child.rotation.eulerAngles.z;
                j.transformScale = new SerializableVector3(child.localScale);
                j.pathSprite = square.pathSprite;
                j.TurretProperties = square.TurretProperties;
                j.MeleeProperties = square.MeleeProperties;
                j.JointProperties = square.JointProperties;
                if (j.JointProperties == null)
                    j.JointProperties = new JointProperties();

                var ab = item.GetComponent<JointBuilder>();
                if(ab != null && ab.Bodyparts?.Length > 0)
                {
                    j.JointProperties.Bodyparts = new string[ab.Bodyparts.Length];
                    ab.Bodyparts.CopyTo(j.JointProperties.Bodyparts,0);
                    j.JointProperties.rotationMax = ab.rotationMax;
                    j.JointProperties.rotationMin = ab.rotationMin;
                    j.JointProperties.Mirror = ab.Mirror;
                }
                

                if(IsAlien)
                {
                    //Debug.Log()
                    j.HexVector = new SerializableVector3(Utilities.ConvertToHexagonalCoordinates(child.transform.position, blockSizeMod));
                    j.HexBlock = true;
                }

                limb.blockList.Add(j);
            }
            else
            {
                continue;
            }
            /*
            foreach(Block j in limb.blockList)
            {
                var vec = j.blockLocation.ToVector2();
                vec -= ogVector;
                j.blockLocation = new SerializableVector2(new Vector2(vec.x, vec.y));
                if (IsAlien)
                {
                    j.HexVector = new SerializableVector3(Utilities.ConvertToHexagonalCoordinates(vec, blockSizeMod));
                }
            }
            */
        }
        var jb = gridObject.GetComponent<ControllerBuilder>();

        if (limb.Controller == null)
            limb.Controller = new LimbControllerDefinition();

        if (jb != null)
        {
            limb.Controller.D = jb.D;
            limb.Controller.P = jb.P;
            limb.Controller.I = jb.I;
            limb.Controller.Wavelength = jb.Wavelength;
            limb.Controller.StrengthMod = jb.StrengthMod;
            limb.Controller.SpeedMod = jb.SpeedMod;
            limb.Controller.SpeedLimit = jb.SpeedLimit;
            limb.Controller.Type = jb.Type;
            limb.Controller.Offset = jb.Offset;
            //jb.Rotation
        }

        limb.Save(limb.SubTypeID,overwrite);
    }

    public void InitializeLimbControllerBuilder(GameObject obj, LimbControllerDefinition def = null)
    {
        var jb = obj.AddComponent<ControllerBuilder>();

        if(def == null)
            return;

        jb.D = def.D;
        jb.P = def.P;
        jb.I = def.I;
        jb.Wavelength = def.Wavelength;
        jb.StrengthMod = def.StrengthMod;
        jb.SpeedMod = def.SpeedMod;
        jb.SpeedLimit = def.SpeedLimit;
        jb.Type = def.Type;
        jb.Offset = def.Offset;
        jb.Rotation = def.Rotation;
    }

    public void LoadBlockCategory(GameObject obj)
    {
        string catName = obj.name;
        HashSet<BlockDefinition> blocks;
        DefinitionManager.definitions.blockCategories.TryGetValue(catName, out blocks);

        GameObject toolbar = GameObject.Find("BlockToolbar");
        if (toolbar == null)
        {
            toolbar = new GameObject("BlockToolbar");
            toolbar.transform.parent = obj.transform.parent;
        }
        else
            foreach (Transform child in toolbar.transform)
            {
                Destroy(child.gameObject);
            }

        if (blocks == null || blocks?.Count == 0)
            return;

        
        int disp = 0;

        foreach(BlockDefinition block in blocks)
        {
            GameObject blockbutton = Instantiate(buttonPrefab);
            blockbutton.name = block.SubTypeID;
            blockbutton.transform.parent = toolbar.transform;
            var img = blockbutton.GetComponent<Image>();
            img.sprite = Resources.Load<Sprite>(block.pathSprite);
            var tfm = blockbutton.GetComponent<RectTransform>();
            tfm.position = new Vector3(555+ 60 + disp,60f,0f);
            disp += 60;
            var button = blockbutton.GetComponent<Button>();
            button.onClick.AddListener(delegate { LoadBlockIntoHand(block); });
        }

    }

    public void LoadBlockIntoHand(BlockDefinition block)
    {
        ///Destroy(blockTemplate);
        currentBlock = BlockSystem.ConvertFromDefinition(block);
        if(currentRend != null)
            currentRend.sprite = currentBlock.blockSprite;
        else
        {
            if(blockTemplate == null)
                blockTemplate = new GameObject("CurrentBlockTemplate");

            buildModeOn = true;

            currentRend = blockTemplate.AddComponent<SpriteRenderer>();

                // set sprite to match current block type
            currentRend.sprite = currentBlock.blockSprite;
            currentRend.sortingOrder = 1000;
        }
            
        //DefinitionManager.definitions.blockDict.TryGetValue(block.SubTypeID, out currentBlock);
    }

    public void Trash()
    {
        if (gridObject == null)
            return;
        foreach (Transform child in gridObject.transform)
        {
            Destroy(child.gameObject);
        }
        gridObject.name = "New Grid";
        textMesh.text = "New Grid";
    }


    public void LoadAdditive(string name)
    {
        GameObject limb = new GameObject(name);
        ChangeCurrentGrid(limb);
        InstantiateBodyPart(limb);
        buildLimbs.Add(limb);
    }

    private void ChangeCurrentGrid(GameObject obj)
    {
        if (gridObject.transform.childCount == 0)
        {
            buildLimbs.Remove(gridObject);
            Destroy(gridObject);
        }
        else
        {
            Destroy(gridObject.GetComponent<BuildCamera>());
            textMesh.text = obj.name;
        }

        obj.AddComponent<BuildCamera>();
        gridObject = obj;

    }

    private GameObject ResetGrid(string objname = "")
    {
        buildLimbs.Remove(gridObject);
        Destroy(gridObject);
        var grid = new GameObject(objname);
        buildLimbs.Add(grid);
        grid.AddComponent<BuildCamera>();
        //buildGrid = grid.AddComponent<Grid>();
        textMesh.text = grid.name;
        return grid;
    }


    public void InstantiateBodyPart(GameObject obj)
    {

        var def = BodyPart.Load(obj.name);
        foreach (BodyPart bp in def.blueprints)
        {


            if (bp.Controller != null)
            {
                InitializeLimbControllerBuilder(obj, bp.Controller);
            }
            else
                InitializeLimbControllerBuilder(obj);

            foreach (Block square in bp.blockList)
            {
                GameObject newBlock = new GameObject(square.SubTypeID);
                newBlock.transform.position = square.blockLocation.ToVector3();
                newBlock.transform.parent = obj.transform;
                newBlock.transform.localScale = square.transformScale.ToVector3();
                newBlock.transform.rotation = Quaternion.Euler(0f, 0f, square.rotation);
                SpriteRenderer newRend = newBlock.AddComponent<SpriteRenderer>();
                newRend.sprite = Resources.Load<Sprite>(square.pathSprite);

                if (square.JointProperties != null && square.TypeID == "JointStator")
                {
                    JointBuilder jb = newBlock.AddComponent<JointBuilder>();
                    jb.Bodyparts = square.JointProperties.Bodyparts;
                    jb.Mirror = square.JointProperties.Mirror;
                    jb.rotationMin = square.JointProperties.rotationMin;
                    jb.rotationMax = square.JointProperties.rotationMax;
                }
                else if (square.TypeID == "JointStator")
                {
                    JointBuilder jb = newBlock.AddComponent<JointBuilder>();
                }

                PolygonCollider2D shape;
                switch (square.collider)
                {
                    case "box":
                        newBlock.AddComponent<BoxCollider2D>();
                        break;
                    case "triangle":
                        shape = newBlock.AddComponent<PolygonCollider2D>();
                        shape.pathCount = 1;
                        shape.points = new Vector2[] { new Vector2(-0.5f, 0.5f), new Vector2(-0.5f, -0.5f), new Vector2(0.5f, -0.5f) };
                        break;
                    case "toptwoslope":
                        shape = newBlock.AddComponent<PolygonCollider2D>();
                        shape.pathCount = 1;
                        shape.points = new Vector2[] { new Vector2(-0.5f, 0.5f), new Vector2(-0.5f, -0.5f), new Vector2(0.5f, -0.5f), new Vector2(0.5f, 0f) };
                        break;
                    case "bottwoslope":
                        shape = newBlock.AddComponent<PolygonCollider2D>();
                        shape.pathCount = 1;
                        shape.points = new Vector2[] { new Vector2(-0.5f, 0f), new Vector2(-0.5f, -0.5f), new Vector2(0.5f, -0.5f) };
                        break;
                    default:
                        newBlock.AddComponent<BoxCollider2D>();
                        break;
                }
            }
        }
    }

    public void Load(string objname) //jfc what a load drop
    {
        buildLimbs.Clear();
        gridObject = ResetGrid(objname);
        InstantiateBodyPart(gridObject);

    }
}


