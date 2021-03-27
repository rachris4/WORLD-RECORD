using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    private string blueprintName;

    //layer mask to control raycasting
    [SerializeField]
    private LayerMask solidNoBuildLayer;
    [SerializeField]
    private LayerMask fakeNoBuildLayer;
    [SerializeField]
    private LayerMask allBlocksLayer;

    private void Awake()
    {
        //ref for vlock sys
        //blockSys = GetComponent<BlockSystem>();


    }

    void Start()
    {
        //Debug.Log("I am alive!");
        gridObject = new GameObject("LimbName");
        buildGrid = gridObject.AddComponent<Grid>();

    }

    private void Update()
    {






        if (blockSys == null)
            return;
        // if E pressed, build mode

        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey("c") && saveWait < 0)
        {
            Debug.Log("SAVING!"); //huhhuhuuhuhuhu
            Save();
            saveWait = 200;
        } else if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey("v") && saveWait < 0)
        {
            Debug.Log("LOADING!");
            Load();
            saveWait = 200;
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
            if(currentBlock == null)
            {
                // ensure allblocks is ready
                if(blockSys.allBlocks[currentBlockID] != null)
                {
                    // get a new currentblockid
                    currentBlock = blockSys.allBlocks[currentBlockID];
                }
            }

            if(buildModeOn && blockTemplate == null)
            {
                //create a new obj for blocktemplate
                blockTemplate = new GameObject("CurrentBlockTemplate");
                // add and store reference toa  spriterenderer on the template
                currentRend = blockTemplate.AddComponent<SpriteRenderer>();
                // set sprite to match current block type
                currentRend.sprite = currentBlock.blockSprite;

            }
        }

        if (buildModeOn && blockTemplate != null)
        {
            if (Input.GetKeyDown("r"))
            {
                //oldrotation = rotation;
                blockTemplate.transform.Rotate(0, 0, 90);
            }
            if (Input.GetKeyDown("f"))
            {
                Vector3 scaler = blockTemplate.transform.localScale;
                float rotz = blockTemplate.transform.localEulerAngles.z;
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
            blockTemplate.transform.position = new Vector3(Mathf.Round(loc.x / blockSizeMod) * blockSizeMod, Mathf.Round(loc.y / blockSizeMod) * blockSizeMod, 0f);


RaycastHit2D rayhit;

            if (currentBlock.isSolid == true)
            {
                rayhit = Physics2D.Raycast(blockTemplate.transform.position, Vector2.zero, Mathf.Infinity, solidNoBuildLayer);
            }
            else
            {
                rayhit = Physics2D.Raycast(blockTemplate.transform.position, Vector2.zero, Mathf.Infinity, fakeNoBuildLayer);
            }

            if (rayhit.collider != null)
            {
                buildBlocked = true;
            }
            else
            {
                buildBlocked = false;
            }
            if (buildBlocked)
            {
                currentRend.color = new Color(1f, 0f, 0f, 1f);
                blockTemplate.layer = 1;
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


            if (Input.GetMouseButtonDown(0) && !buildBlocked)
            {
                GameObject newBlock = new GameObject(currentBlock.SubTypeID);
                newBlock.transform.position = blockTemplate.transform.position;
                newBlock.transform.parent = buildGrid.transform;
                newBlock.transform.localScale = blockTemplate.transform.localScale;
                newBlock.transform.rotation = blockTemplate.transform.rotation;
                SpriteRenderer newRend = newBlock.AddComponent<SpriteRenderer>();
                newRend.sprite = currentBlock.blockSprite;
                //Debug.Log(currentBlock.collider);
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
                RaycastHit2D destroyHit = Physics2D.Raycast(blockTemplate.transform.position, Vector2.zero, Mathf.Infinity, allBlocksLayer);
                if (destroyHit.collider != null)
                {
                    Destroy(destroyHit.collider.gameObject);
                }
            }
        }
    }

    private void Save()
    {
        limb = new BodyPart();
        limb.SubTypeID = gridObject.name;
        limb.TypeID = "Limb";
        Debug.Log(gridObject.transform.childCount.ToString());
        Vector2 ogVector = Vector2Int.zero;
        bool gotit = false;
        foreach (Transform child in gridObject.transform)
        {
            var item = child.gameObject;
            if(!gotit)
            {
                ogVector = new Vector2(item.transform.position.x, item.transform.position.y);
                gotit = true;
            }
            //Debug.Log("DEBUG: VECTOR CHILD: " + child.position.ToString());
            Block square = new Block();
            if (blockSys.blockLookup.TryGetValue(item.name, out square))
            {
                Block j = new Block();
                j.TypeID = square.TypeID;
                j.SubTypeID = square.SubTypeID;
                j.DisplayName = square.DisplayName;
                //j.blockSprite = Resources.Load<Sprite>(square.pathSprite);
                //j.isSolid = square.isSolid;
                //j.collider = square.collider;
                j.blockLocation = new SerializableVector2(new Vector2(child.position.x, child.position.y));
                j.blockLocation = new SerializableVector2(j.blockLocation.ToVector2() - ogVector);
                j.rotation = child.rotation.eulerAngles.z;
                j.transformScale = new SerializableVector3(child.localScale);
                j.pathSprite = square.pathSprite;
                limb.blockList.Add(j);
            }
            else
            {
                continue;
            }

            //child is your child transform
        }
        /// something foreach hinge do thing
        limb.Save(limb.SubTypeID);
    }

    private void Load() //jfc what a load drop
    {
        string name = gridObject.name;
        Destroy(gridObject);
        gridObject = new GameObject(name);
        buildGrid = gridObject.AddComponent<Grid>();
        var def = BodyPart.Load(name);
        foreach(BodyPart bp in def.blueprints)
        {
            foreach(Block square in bp.blockList)
            {
                GameObject newBlock = new GameObject(square.SubTypeID);
                newBlock.transform.position = square.blockLocation.ToVector3();
                newBlock.transform.parent = buildGrid.transform;
                newBlock.transform.localScale = square.transformScale.ToVector3();
                newBlock.transform.rotation = Quaternion.Euler(0f, 0f, square.rotation);
                SpriteRenderer newRend = newBlock.AddComponent<SpriteRenderer>();
                newRend.sprite = Resources.Load<Sprite>(square.pathSprite);
                //Debug.Log(currentBlock.collider);
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
}
