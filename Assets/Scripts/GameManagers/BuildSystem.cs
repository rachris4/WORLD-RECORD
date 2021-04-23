using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Xml.Serialization;
using System.Xml;
using System;
using System.Linq;
using System.IO;
using UnityEngine.EventSystems;
using System.Globalization;
using UnityEngine.SceneManagement;

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
    private GameObject selectedBlock;
    private SpriteRenderer currentRend;

    // float to adjust the size of blocks when placing in world;
    [SerializeField]
    private float blockSizeMod = 1f;

    // bools to control
    public bool buildModeOn = false;
    private bool buildBlocked = false;
    [SerializeField]
    public GameObject buttonPrefab;
    [SerializeField]
    public GameObject GridButtonPrefab;
    [SerializeField]
    public GameObject GridPrefab;
    [SerializeField]
    public GameObject TestPrefab;
    [SerializeField]
    public GameObject Canvas;

    [SerializeField]
    private string blueprintName;
    [SerializeField]
    private string TypeID;
    //layer mask to control raycasting
    [SerializeField]
    private LayerMask solidNoBuildLayer;
    [SerializeField]
    private LayerMask UILayerMask;
    [SerializeField]
    public LayerMask allBlocksLayer;
    [SerializeField]
    private LayerMask doNotBuild;
    [SerializeField]
    private TMP_InputField textMesh;
    [SerializeField]
    public BlockPanel panel;

    public bool IsAlien = false;
    private int moveTick;
    private List<GameObject> buildLimbs = new List<GameObject>();
    private Dictionary<GameObject, EditorProperties> editorDict = new Dictionary<GameObject, EditorProperties>();
    public GameObject prime;

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

        #region Legacy Save
        

        
        foreach(KeyValuePair<GameObject, EditorProperties> item in editorDict)
        {
            if (item.Key == null)
                continue;

            string log = "";

            foreach (GenericEditorInput thing in item.Value.Keybinds)
            {
                log += " [" + thing.Data + "]";
            }

            //Debug.Log("REEEEEEEEE" + item.Value.Keybinds.Count.ToString() + " / " + log);
        }
        
        // if E pressed, build mode



        /*
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown("c") && saveWait < 0)
        {
            Debug.Log("SAVING!"); //huhhuhuuhuhuhu
            SaveCurrentLimb(true);
            saveWait = 1000;
        }
        else if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown("f") && saveWait < 0)
        {
            Debug.Log("SAVING AS VARIANT!"); //huhhuhuuhuhuhu
            SaveCurrentLimb(false);
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
        */

        #endregion

        if (currentRend == null)
        {
            BlockDefinition bloq;
            DefinitionManager.definitions.blockDict.TryGetValue("armor_square", out bloq);
            currentBlock = BlockSystem.ConvertFromDefinition(bloq);
        }

        if (Input.mousePosition.x == 0 || Input.mousePosition.y <= 75 || Input.mousePosition.x == Screen.width - 1 || Input.mousePosition.y >= Screen.height - 75) return;

        if (IsPointerOverUIObject()) //as is
            return;

        if (currentBlock == null)
            return;

        if (Input.GetKeyDown("e"))
        {
            ToggleBuildMode();    
        }

        if (blockTemplate == null)
            panel.HidePanel();

        if (buildModeOn && blockTemplate != null)
        {

            Vector3 scaler = blockTemplate.transform.localScale;
            float rotz = blockTemplate.transform.localEulerAngles.z;

            if (Input.GetKeyDown("r"))
            {
                RotateBlockTemplate();
            }
            if (Input.GetKeyDown("f"))
            {
                FlipBlockTemplate(ref rotz, ref scaler);
            }

            Vector3 loc = Utilities.GetWorldPositionOnPlane(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f), 0f);

            AdjustBlockTemplate(rotz, scaler, loc);

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

            if (mouseWheel != 0) //largely deprecated
            {
                ScrollBlocks(mouseWheel);
            } //scroll through blockss

            

            if (Input.GetMouseButtonDown(0))
            {
                moveTick = 1000;

                if (buildBlocked && rayhit.collider.gameObject != null)
                {
                    Destroy(rayhit.collider.gameObject);
                } else if (buildBlocked)
                {
                    rayhit = Physics2D.Raycast(loc, Vector2.zero, Mathf.Infinity, allBlocksLayer);
                    Destroy(rayhit.collider.gameObject);
                }

                //rayhit = Physics2D.Raycast(blockTemplate.transform.position, Vector2.zero, Mathf.Infinity, UILayerMask);


                PlaceBlock();


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
        else
        {
            Vector3 loc = Utilities.GetWorldPositionOnPlane(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f), 0f);
            RaycastHit2D rayhit;

            rayhit = Physics2D.Raycast(loc, Vector2.zero, Mathf.Infinity, allBlocksLayer);
            //if (rayhit.collider?.gameObject != null)

            if (Input.GetMouseButtonDown(1))
                Debug.Log("custom click. \n" + panel.EditorProperties.DebugLine());

            if (Input.GetMouseButtonDown(0))
            {

                Debug.Log("Before Selection Change. \n" + panel.EditorProperties.DebugLine());

                if (rayhit.collider?.gameObject != null)
                {
                    ManageBlockSelection(rayhit.collider.gameObject);
                    //Debug.Log("selected : " + panel.EditorProperties.Keybinds.Count.ToString() + " / " + panel.EditorProperties.BlockData.Count.ToString());
                }
                else
                    ManageBlockSelection();
            }

            if (selectedBlock == null)
                panel.HidePanel();
            else
                panel.ShowPanel();
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

    private void AdjustBlockTemplate(float rotz, Vector3 scaler, Vector3 loc)
    {
        if (!IsAlien)
            blockTemplate.transform.position = new Vector3(Mathf.Round(loc.x / blockSizeMod) * blockSizeMod, Mathf.Round(loc.y / blockSizeMod) * blockSizeMod, 0f);
        else
            blockTemplate.transform.position = Utilities.RoundToHexCoordinates(loc, blockSizeMod);


        blockTemplate.transform.position += Quaternion.Euler(0f, 0f, rotz) * new Vector3(currentBlock.spriteOffset.x * scaler.x, currentBlock.spriteOffset.y * scaler.y, 0f);
    }

    private void FlipBlockTemplate(ref float rotz, ref Vector3 scaler)
    {
        moveTick = 1000;

        if ((rotz > 85 && rotz < 95) || (rotz > 265 && rotz < 275))
            scaler.y *= -1;
        else
            scaler.x *= -1;
        blockTemplate.transform.localScale = scaler;
    }
    private void RotateBlockTemplate()
    {
        moveTick = 1000;

        if (!IsAlien)
            blockTemplate.transform.Rotate(0, 0, 90);
        else
            blockTemplate.transform.Rotate(0, 0, 60);
    }

    private void ScrollBlocks(float mouseWheel)
    {
        selectableBlocksTotal = blockSys.allBlocks.Length - 1;

        if (mouseWheel > 0)
        {
            currentBlockID--;

            if (currentBlockID < 0)
            {
                currentBlockID = selectableBlocksTotal;
            }

        }
        else
        {
            currentBlockID++;

            if (currentBlockID > selectableBlocksTotal)
                currentBlockID = 0;

        }

        currentBlock = blockSys.allBlocks[currentBlockID];
        currentRend.sprite = currentBlock.blockSprite;
    }

    private void ToggleBuildMode()
    {
        buildModeOn = !buildModeOn;


        // remove duplicate sprite for template.
        if (blockTemplate != null && !buildModeOn)
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

        if (buildModeOn && blockTemplate == null)
        {
            //create a new obj for blocktemplate
            blockTemplate = new GameObject("CurrentBlockTemplate");
            // add and store reference toa  spriterenderer on the template
            currentRend = blockTemplate.AddComponent<SpriteRenderer>();




            // set sprite to match current block type
            currentRend.sprite = currentBlock.blockSprite;
            currentRend.sortingOrder = 1000;


        }

        ManageBlockSelection();
    }

    private void DeselectBlock()
    {
        if (selectedBlock != null)
        {
            var rrend = selectedBlock.GetComponent<SpriteRenderer>();
            if (rrend != null)
                rrend.color = new Color(1f, 1f, 1f, 1f);

            //var deput = selectedBlock.GetComponent<GenericInput>();
            //if(deput != null)
            //    deput.EditorProperties = UpdateEditorProperties(panel.EditorProperties, deput.EditorProperties);

            EditorProperties deput;
            editorDict.TryGetValue(selectedBlock, out deput);
            if (deput != null)
            {
                Debug.Log("Selection changing. Loading data from panel -> saving to deselected block \n" + panel.EditorProperties.DebugLine());
                deput = UpdateEditorProperties(panel.EditorProperties, deput);
                editorDict[selectedBlock] = deput;

                var jb = selectedBlock.GetComponent<JointBuilder>();
                if(jb != null)
                {
                    jb.rotationMax = HandleString(deput.BlockData[0].Data);
                    jb.rotationMin = HandleString(deput.BlockData[1].Data);
                    jb.Mirror = deput.BlockData[2].Data == "true";
                    //jb.Bodyparts = (int)double.Parse(deput.BlockData[3].Data);
                }

                var lc = selectedBlock.GetComponent<ControllerBuilder>(); //textnames = new string[6] { "Type", "Strength", "Snappiness", "Degree Offset", "Motion Time", "Time Offset" };
                if (lc != null)
                {

                    /*
                                    limb.Controller.D = jb.D;
                                    limb.Controller.P = jb.P;
                                    limb.Controller.I = jb.I;
                                    limb.Controller.Wavelength = jb.Wavelength;
                                    limb.Controller.StrengthMod = jb.StrengthMod;
                                    limb.Controller.SpeedMod = jb.SpeedMod;
                                    limb.Controller.SpeedLimit = jb.SpeedLimit;
                                    limb.Controller.Type = jb.Type;
                                    limb.Controller.Offset = jb.Offset;
                                    limb.Controller.fwd = jb.fwd;
                                    limb.Controller.bck = jb.bck;
                    */

                    lc.bck = deput.Keybinds[0].Data;
                    lc.fwd = deput.Keybinds[1].Data;
                    lc.Type = deput.BlockData[0].Data;
                    lc.StrengthMod = HandleString(deput.BlockData[1].Data);

                    float snap = HandleString(deput.BlockData[2].Data);
                    lc.SpeedMod = lc.StrengthMod * snap/10f;
                    lc.SpeedLimit = lc.SpeedMod * 2;

                    lc.P = snap;
                    lc.I = snap / 3f;
                    lc.D = snap / 30f;

                    lc.Rotation = HandleString(deput.BlockData[3].Data);
                    lc.Wavelength = (int)HandleString(deput.BlockData[4].Data);
                    lc.Offset = (int)HandleString(deput.BlockData[5].Data);
                }

            }
            else if(panel.hasInputs)
                Debug.Log("Deselected : " + selectedBlock.name + " has no value in the dictionary. Error?");

            panel.selectedBlock = null;
            selectedBlock = null;
        }
        //panel.HidePanel();
    }

    private float HandleString(string data)
    {
        double result = 0;

        double.TryParse(data, out result);

        return (float)result;

    }
    private void ManageBlockSelection(GameObject rayObj = null)
    {

        DeselectBlock();

        if (rayObj == null)
        {
            //panel.UpdatePanel();
            return;
        }

        selectedBlock = rayObj;
        panel.selectedBlock = selectedBlock;

        var rend = rayObj.GetComponent<SpriteRenderer>();
        if (rend != null)
            rend.color = new Color(0.6f, 1f, 0.6f, 1f);

        BlockDefinition bloq;
        DefinitionManager.definitions.blockDict.TryGetValue(rayObj.name, out bloq);

        if(bloq == null)
        {
            Debug.Log("weirdshit");
            return;
        }

        panel.block = BlockSystem.ConvertFromDefinition(bloq);
        //var input = rayObj.GetComponent<GenericInput>();
        EditorProperties input;
        editorDict.TryGetValue(rayObj, out input);
        if (input != null)
        {
            //panel.EditorProperties = input.EditorProperties;
            //panel.EditorProperties = UpdateEditorProperties(input.EditorProperties, panel.EditorProperties);
            panel.EditorProperties = UpdateEditorProperties(input, panel.EditorProperties);
        }
        else
            Debug.Log("!!!!!!");

        panel.UpdatePanel(false);



        Debug.Log("Selection changed. selected block -> panel \n" + panel.EditorProperties.DebugLine());


    }

    private void PlaceBlock()
    {
        GameObject newBlock = new GameObject(currentBlock.SubTypeID);
        newBlock.transform.position = blockTemplate.transform.position;
        newBlock.transform.parent = gridObject.transform;
        newBlock.transform.localScale = blockTemplate.transform.localScale;
        newBlock.transform.rotation = blockTemplate.transform.rotation;
        SpriteRenderer newRend = newBlock.AddComponent<SpriteRenderer>();
        //Debug.Log(Utilities.ConvertToHexagonalCoordinates(newBlock.transform.position, blockSizeMod).ToString());
        newRend.sprite = currentBlock.blockSprite;

        InitializeTypeIDBuilder(newBlock, currentBlock);

        if(panel.hasInputs)
        {
            //var inputs = newBlock.AddComponent<GenericInput>();
            //if (inputs != null)
            //    inputs.EditorProperties = UpdateEditorProperties(panel.EditorProperties, inputs.EditorProperties);
            //inputs.EditorProperties = panel.EditorProperties;


            EditorProperties inputs = new EditorProperties();
            inputs.Keybinds = new List<GenericEditorInput>();
            inputs.BlockData = new List<GenericEditorInput>();
            inputs = UpdateEditorProperties(panel.EditorProperties, inputs);
            editorDict.Add(newBlock, inputs);

          

        }

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



        if (newBlock.layer == 1)
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

    private EditorProperties UpdateEditorProperties(EditorProperties og, EditorProperties cc)
    {
        //if (obj.GetComponent<GenericInput>() == null)
        //    return;

        //var cc = obj.GetComponent<GenericInput>().EditorProperties;

       // EditorProperties cc;
       // editorDict.TryGetValue(obj, out cc);

        if (cc == null)
        {
            cc = new EditorProperties();
            cc.Keybinds = new List<GenericEditorInput>();
            cc.BlockData = new List<GenericEditorInput>();
        }
        //else
        //    editorDict.Remove(obj);

        cc.Keybinds.Clear();
        cc.BlockData.Clear();
        //cc.Keybinds.AddRange(og.Keybinds);
        //cc.BlockData.AddRange(og.BlockData);

        string log = "";


        foreach (GenericEditorInput bind in og.Keybinds)
        {
            cc.Keybinds.Add(new GenericEditorInput(bind.Name, bind.Data));
            log += " [" + bind.Name + "]";
        }

        foreach (GenericEditorInput text in og.BlockData)
        {
            cc.BlockData.Add(new GenericEditorInput(text.Name, text.Data));
        }

        //Debug.Log("HElp!" + log);

        //Debug.Break();
        return cc;
        //editorDict.Add(obj, cc);

    }

    private bool IsPointerOverUIObject()
    {
        // Referencing this code for GraphicRaycaster https://gist.github.com/stramit/ead7ca1f432f3c0f181f
        // the ray cast appears to require only eventData.position.
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }

    private void InitializeTypeIDBuilder(GameObject obj, Block square)
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

    private void InitializeJointRotor(GameObject obj)
    {

    }

    private void InitializeJointStator(GameObject obj)
    {
        var jb = obj.AddComponent<JointBuilder>();
    }

    public void SaveEnsemble(bool overwrite = false) //called by button listener
    {
        BodyEnsemble body = new BodyEnsemble();
        body.SubTypeID = textMesh.text;
        body.PrimeBodypart = prime.name;
        foreach (GameObject obj in buildLimbs)
        {

            if (obj == null)
                continue;

            BodyPart limb = EncodeBodyPart(obj);
            limb.BuildPosition = new SerializableVector3(obj.transform.position);
            body.bodyParts.Add(limb);
        
        }
        //Debug.Log(body.bodyParts.Count);

        string path = body.SubTypeID;
        string dir = Path.Combine(DefinitionSet.ensemblePath, path);
        //path = Path.Combine(blueprintPath, path);

        for (int j = 1; j < 20; j++)
        {
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
                break;
            }
            else if (!overwrite)
            {
                if (path.Where(n => Char.IsNumber(n)).Count() == 0)
                {
                    path += "_1";
                }
                else
                {
                    int i = Int32.Parse(new string(path.Reverse().TakeWhile(n => Char.IsNumber(n)).Reverse().ToArray()));
                    string text = path.Substring(0, path.Length - i.ToString().Length);
                    path = text + (i + 1);
                }
                dir = Path.Combine(DefinitionSet.blueprintPath, path);
            }
            else
                break;
        }

        body.SubTypeID = path;
        if(DefinitionManager.definitions.blueprintbodiesSubTypeIdList.Contains(path))
        {
            //Debug.Log("funky shit");
            //return;
            DefinitionManager.definitions.blueprintbodies.Remove(body);
            DefinitionManager.definitions.blueprintbodiesDict.Remove(path);
            DefinitionManager.definitions.blueprintbodiesSubTypeIdList.Remove(path);
        }

        DefinitionManager.definitions.blueprintbodies.Add(body);
        DefinitionManager.definitions.blueprintbodiesDict.Add(path, body);
        DefinitionManager.definitions.blueprintbodiesSubTypeIdList.Add(path);

        path += ".xml";
        var def = new DefinitionSet();
        def.blueprintbodies.Add(body);
        var serializer = new XmlSerializer(typeof(DefinitionSet));

        Debug.Log("Saving to " + Path.Combine(dir, path));

        using (var stream = new FileStream(Path.Combine(dir, path), FileMode.Create))
        {
            serializer.Serialize(stream, def);
        }

    }

    public void LoadEnsemble(string objname)
    {
        foreach(GameObject obj in buildLimbs)
        {
            Destroy(obj);
        }
        buildLimbs.Clear();
        gridObject = ResetGrid();

        BodyEnsemble body;
        DefinitionManager.definitions.blueprintbodiesDict.TryGetValue(objname, out body);

        if(body == null)
        {
            Debug.Log("sheesh!");
            return;
        }

        foreach(BodyPart bp in body.bodyParts)
        {
            LoadAdditive(bp.SubTypeID, bp);
        }

    }
    public BodyPart EncodeBodyPart(GameObject gridObject)
    {
        limb = new BodyPart();
        //gridObject.name = textMesh.text;
        limb.SubTypeID = gridObject.name;
        limb.TypeID = TypeID;

        foreach (Transform child in gridObject.transform)
        {
            var item = child.gameObject;

            //Debug.Log("DEBUG: VECTOR CHILD: " + child.position.ToString());
            Block square = new Block();
            Vector3 min = new Vector3(1000, 1000, 0);
            if (blockSys.blockLookup.TryGetValue(item.name, out square))
            {




                Block j = new Block();
                j.TypeID = square.TypeID;
                j.SubTypeID = square.SubTypeID;
                j.DisplayName = square.DisplayName;

                //j.isSolid = square.isSolid;
                //j.collider = square.collider;
                if (child.position.x < min.x)
                    min.x = child.position.x;
                if (child.position.y < min.y)
                    min.y = child.position.y;

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
                if (ab != null && ab.Bodyparts?.Length > 0)
                {
                    j.JointProperties.Bodyparts = new string[ab.Bodyparts.Length];
                    ab.Bodyparts.CopyTo(j.JointProperties.Bodyparts, 0);
                    j.JointProperties.rotationMax = ab.rotationMax;
                    j.JointProperties.rotationMin = ab.rotationMin;
                    j.JointProperties.Mirror = ab.Mirror;
                }


                if (IsAlien)
                {
                    //Debug.Log()
                    j.HexVector = new SerializableVector3(Utilities.ConvertToHexagonalCoordinates(child.transform.position, blockSizeMod));
                    j.HexBlock = true;
                }

                EditorProperties ep;

                editorDict.TryGetValue(item, out ep);

                if(ep != null)
                {
                    j.EditorProperties = ep;
                }


                limb.blockList.Add(j);
            }
            else
            {
                continue;
            }

            foreach(Block b in limb.blockList)
            {
                //b.blockLocation = new SerializableVector2(b.blockLocation.ToVector3() - min);
            }

            var jb = item.GetComponent<ControllerBuilder>();

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
                limb.Controller.fwd = jb.fwd;
                limb.Controller.bck = jb.bck;
                limb.Controller.Rotation = jb.Rotation;
                limb.Controller.ParentName = jb.ParentName;
            }
        }
        

        return limb;
    }
    
    public void SaveGridObject(bool overwrite = false)
    {
        limb = EncodeBodyPart(gridObject);
        //Debug.Log(gridObject.transform.childCount.ToString());

        limb.Save(limb.SubTypeID, overwrite);
    }
    public void SaveCurrentLimb(GameObject obj = null, bool overwrite = false)
    {

        if (obj == null)
            obj = gridObject;

        limb = EncodeBodyPart(obj);
        //Debug.Log(gridObject.transform.childCount.ToString());

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
        jb.ParentName = def.ParentName;
        jb.fwd = def.fwd;
        jb.bck = def.bck;
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

        panel.ShowPanel();
        panel.block = currentBlock;
        panel.selectedBlock = null;
        panel.UpdatePanel();
        //Debug.Log(panel.hasInputs);
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
        gridObject = ResetGrid();
        gridObject.name = "New Grid";
        //textMesh.text = "New Grid";
    }

    public void Test()
    {
        SaveEnsemble(true);
        string sub = textMesh.text;

        GameObject fab = Instantiate(TestPrefab);
        fab.layer = 13;
        var assembly = fab.GetComponent<GridAssembly>();
        DefinitionManager.definitions.blueprintbodiesDict.TryGetValue(sub, out assembly.body);
        Utilities.DontDestroyOnLoad(fab);
        //fab.transform.position = new Vector3(-70f, 0f, 0f);
        SceneManager.LoadScene(2);

    }

    public void LoadAdditive(string name, BodyPart bp = null)
    {
        GameObject outline = Instantiate(GridPrefab);
        GameObject limb = new GameObject(name);
        var button = outline.GetComponent<GridOutlineSprite>();
        button.canvasElement = Canvas;
        button.prefab = GridButtonPrefab;
        button.gridObject = limb;
        button.name = name + "_buttonthing";
        button.buildSystem = this;
        limb.name = name;
        //ChangeCurrentGrid(limb);
        if (bp == null)
            LoadBodyPartFromManager(limb);
        else
            LoadBodyPart(limb, bp);
        buildLimbs.Add(limb);
    }

    public void ChangeCurrentGrid(GameObject obj)
    {
        if (gridObject.transform.childCount == 0)
        {
            buildLimbs.Remove(gridObject);
            Destroy(gridObject);
        }
        else
        {
            Destroy(gridObject.GetComponent<BuildCamera>());
        }

        obj.AddComponent<BuildCamera>();
        gridObject = obj;
        //textMesh.text = obj.name;


    }

    private GameObject ResetGrid(string objname = "")
    {
        buildLimbs.Remove(gridObject);
        Destroy(gridObject);
        var grid = new GameObject(objname);
        buildLimbs.Add(grid);
        grid.AddComponent<BuildCamera>();
        //buildGrid = grid.AddComponent<Grid>();
        //textMesh.text = grid.name;
        return grid;
    }

    
    public void LoadBodyPart(GameObject obj, BodyPart bp)
    {

        obj.transform.position = bp.BuildPosition.ToVector3();

        foreach (Block square in bp.blockList)
        {
            GameObject newBlock = new GameObject(square.SubTypeID);
            newBlock.transform.localPosition = square.blockLocation.ToVector3();
            newBlock.transform.parent = obj.transform;
            newBlock.transform.localScale = square.transformScale.ToVector3();
            newBlock.transform.rotation = Quaternion.Euler(0f, 0f, square.rotation);
            SpriteRenderer newRend = newBlock.AddComponent<SpriteRenderer>();
            newRend.sprite = Resources.Load<Sprite>(square.pathSprite);

            if (square.EditorProperties != null)
            {
                editorDict.Add(newBlock, square.EditorProperties);
            }

            if (square.JointProperties != null && square.TypeID == "JointStator")
            {
                JointBuilder jb = newBlock.AddComponent<JointBuilder>();
                jb.Bodyparts = square.JointProperties.Bodyparts;
                jb.Mirror = square.JointProperties.Mirror;
                jb.rotationMin = square.JointProperties.rotationMin;
                jb.rotationMax = square.JointProperties.rotationMax;
                if(jb.Bodyparts != null && jb.Bodyparts.Length != 0)
                    jb.presumptiveLimb = square.JointProperties.Bodyparts[0];

            }
            else if (square.TypeID == "JointStator")
            {
                JointBuilder jb = newBlock.AddComponent<JointBuilder>();
            }

            if(square.TypeID == "JointRotor")
            {
                if (bp.Controller != null)
                {
                    InitializeLimbControllerBuilder(newBlock, bp.Controller);
                }
                else
                    InitializeLimbControllerBuilder(newBlock);
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
    public void LoadBodyPartFromManager(GameObject obj)
    {

        var def = BodyPart.Load(obj.name);
        foreach (BodyPart bp in def.blueprints)
        {

            LoadBodyPart(obj, bp);
            
        }
    }

    public void Load(string objname) //jfc what a load drop
    {
        buildLimbs.Clear();
        gridObject = ResetGrid(objname);
        LoadBodyPartFromManager(gridObject);

    }
}

public class BodyEnsemble
{
    [XmlAttribute("SubTypeID")]
    public string SubTypeID;

    [XmlArray("Bodyparts")]
    [XmlArrayItem("Bodypart", typeof(BodyPart))]
    public List<BodyPart> bodyParts = new List<BodyPart>();

    [XmlElement("PrimeBodypart")]
    public string PrimeBodypart;

}


