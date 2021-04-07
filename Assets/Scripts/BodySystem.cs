using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;
using System.Xml;
using System.IO;

public class LimbUnity : MonoBehaviour
{
    [SerializeField]
    public int SortingOrder = 0;

    private int oldSO = 0;
    public int parentSortingOrder = 0;
    public LimbUnity parentData;
    public bool mirrored = false;

    public GameObject overParent;

    public List<Block> blockList = new List<Block>();
    public List<GameObject> blockobjlist = new List<GameObject>();

    public void Update()
    {
        if(oldSO != SortingOrder)
        {
            UpdateSortingOrder();
            //UpdateColor();
            oldSO = SortingOrder;
        }

    }

    public void UpdateSortingOrder(int update = 0)
    {

        SortingOrder += update;

        foreach (Transform child in gameObject.transform)
        {
            var rend = child.gameObject.GetComponent<SpriteRenderer>();
            if(rend != null)
                rend.sortingOrder = SortingOrder;
        }
    }

    public void UpdateColor()
    {
        foreach (Transform child in gameObject.transform)
        {
            var rend = child.gameObject.GetComponent<SpriteRenderer>();
            float mod = Mathf.Abs(3 / (float)SortingOrder);
            if (mod > 1)
                mod = 1f;
            else if (mod < 0.7f)
                mod = 0.7f;
            rend.color = new Color(1f * mod, 1f * mod, 1f * mod, rend.color.a);
        }
    }

}

public class BodyPart : DefinitionBase
{
    private static string blueprintPath = @"F:\untystuff\WORLD RECORD\Assets\Data\Blueprints\";

    [XmlArray("Blocks")]
    [XmlArrayItem("Block", typeof(Block))]
    public List<Block> blockList = new List<Block>();
    [XmlIgnore]
    public List<GameObject> blockobjlist = new List<GameObject>();

    public LimbControllerDefinition Controller;// = true;

    public int VisualLayer = 0;
    public bool Flippable = true;
    //public BodyPart Parent;

    public Vector2Int Max;
    public Vector2Int Min;

    private HashSet<MeshTriangle> alienTris = new HashSet<MeshTriangle>();
    private HashSet<HashSet<GameObject>> alienTrisMesh = new HashSet<HashSet<GameObject>>();

    public Vector2 jointLocation;

    private const float notAlienDampening = 100f;
    private const float notAlienSpringConstant = 100f;


    public GameObject CreateLimbUnity(GameObject entparent = null, int layerAddition = 0, bool wasMirrored = false, GameObject overParent = null)
    {

        string name = "";

        if(wasMirrored)
        {
            name += "mirrord_"; 
        }


        GameObject limb = new GameObject(name + SubTypeID);

        var limbobj = limb.AddComponent<LimbUnity>();

        if (overParent != null)
        {
            limb.transform.parent = overParent.transform;
            limbobj.overParent = overParent;
        }
            
        float controllerOffset = 0;

        if (entparent != null)
        {
            var parentlimbobj = entparent.GetComponent<LimbUnity>();
            limbobj.parentSortingOrder = parentlimbobj.SortingOrder;
            if (parentlimbobj.mirrored != wasMirrored)
            {

                if(Controller != null)
                {
                    controllerOffset = Controller.Offset + Controller.Wavelength;
                }
                limbobj.mirrored = true;

            }
            else
            {
                limbobj.mirrored = false;
            }
            if(parentlimbobj != null)
            {
                limb.transform.parent = parentlimbobj.overParent.transform;
                limbobj.overParent = parentlimbobj.overParent;
            }


            if (limbobj.mirrored)
                layerAddition *= -1;

            limbobj.SortingOrder = parentlimbobj.SortingOrder + layerAddition;
            limbobj.parentData = parentlimbobj;
        }

        var builder = limbobj.overParent.GetComponent<RobotAssemble>();
        builder.bpList.Add(this);
        builder.objList.Add(limb);

        GameObject limbSprite = new GameObject("sprite_" + limb.name);
        limbSprite.transform.parent = limb.transform;

        var bodyGrid = limb.AddComponent<Grid>();
        var rigidbody = limb.AddComponent<Rigidbody2D>();


        rigidbody.drag = 1f;

        rigidbody.useAutoMass = true;
        var currentRend = limbSprite.AddComponent<SpriteRenderer>();
        currentRend.sprite = Resources.Load<Sprite>("Sprites/BoundingBox");

        limb.layer = 24;

        if (Controller != null)
        {
            var limbController = limb.AddComponent<LimbController>();
            limbController.Initialize(Controller);
            limbController.offset = controllerOffset;

        }
        
        Vector2Int min = Vector2Int.zero;
        Vector2Int max = Vector2Int.zero;
        int blockCount = 0;

        blockCount = blockList.Count;

        foreach (Block square in blockList)
        {
            GameObject newBlock = square.CreateBlockUnity(limb, currentRend);

            if(square.HexBlock)
            {
                newBlock.AddComponent<AlienRotation>();
            }

            limbobj.blockobjlist.Add(newBlock);
            limbobj.blockList.Add(square);

            if (square.TypeID == "JointRotor")
                jointLocation = square.blockLocation.ToVector2();

            if (newBlock.transform.position.x < min.x)
            {
                min.x = (int)newBlock.transform.position.x;
            }
            if (newBlock.transform.position.y < min.y)
            {
                min.y = (int)newBlock.transform.position.y;
            }
            if (newBlock.transform.position.x > max.x)
            {
                max.x = (int)newBlock.transform.position.x;
            }
            if (newBlock.transform.position.y > max.y)
            {
                max.y = (int)newBlock.transform.position.y;
            }
        }


        

        Max = max;
        Min = min;

        limbSprite.transform.localScale = new Vector3(max.x - min.x + 1, max.y - min.y + 1, 1f);
        currentRend.color = new Color(0f, 1f, 0f, 0f);
        Vector2 span = max - min;
        span = max - span / 2;
        limbSprite.transform.position = new Vector3(span.x, span.y, 0);
        rigidbody.Sleep();

        

        return limb;
    }

    public void InitializeAlienParts(GameObject limb)
    {
        List<GameObject> AlienBlocks = new List<GameObject>();
        List<Block> AlienSquares = new List<Block>();
        Dictionary<Vector3, GameObject> HexDict = new Dictionary<Vector3, GameObject>();
        int kk = 0;

        var limbobj = limb.GetComponent<LimbUnity>();


        for (int p = 0; p < blockList.Count; p++)
        {
            GameObject newBlock = limbobj.blockobjlist[p];
            Block square = limbobj.blockList[p];


            BlockDefinition bloq;
            DefinitionManager.definitions.blockDict.TryGetValue(square.SubTypeID, out bloq);

            if (bloq == null)
                continue;

            if (square.HexBlock)
            {
                HexDict.Add(square.HexVector.ToVector3(), newBlock);
                AlienBlocks.Add(newBlock);
                AlienSquares.Add(square);
            }

            if (bloq.AlienProperties != null)
            {
                var rig = newBlock.GetComponent<Rigidbody2D>();

                if (rig != null)
                    continue;

                rig = newBlock.AddComponent<Rigidbody2D>();



                kk++;
                square.AlienProperties = bloq.AlienProperties;

                square.refIndex = kk;
                rig.useAutoMass = false;
                rig.constraints = RigidbodyConstraints2D.FreezeRotation;
                rig.mass = square.AlienProperties.Mass;
                var mat = new PhysicsMaterial2D();
                mat.friction = square.AlienProperties.MaterialFriction;
                mat.bounciness = square.AlienProperties.MaterialBounciness;
                rig.sharedMaterial = mat;
            }
        }

        MeshTriangle tri = new MeshTriangle();
        tri.Triangles = new GameObject[3];

        for (int i = 0; i < AlienSquares.Count; i++)
        {
            GameObject newBlock = AlienBlocks[i];
            Block square = AlienSquares[i];

            Rigidbody2D newRigid;

            newBlock.TryGetComponent<Rigidbody2D>(out newRigid);

            int springCount = 0;

            Vector3[] hexNeighbours = Utilities.HexNeighbours(square.HexVector.ToVector3());

            foreach (Vector3 hexPt in Utilities.HexNeighbours(square.HexVector.ToVector3()))
            {

                GameObject neighbourBlock;

                //Debug.Log("eeee!!");

                HexDict.TryGetValue(hexPt, out neighbourBlock);
                if (neighbourBlock == null)
                {
                    //Debug.Log("The hexagonal coordinate " + hexPt.ToString() + " did not exist in the dictionary.");
                    continue;

                }

                Block doubledef = AlienSquares[AlienBlocks.IndexOf(neighbourBlock)];

                tri = new MeshTriangle();
                tri.Triangles = new GameObject[3];

                HashSet<GameObject> triMesh = new HashSet<GameObject>();


                foreach (Vector3 doubleHex in Utilities.HexNeighbours(hexPt))
                {
                    GameObject doubleNeighbor;
                    HexDict.TryGetValue(doubleHex, out doubleNeighbor);
                    if (doubleNeighbor == null || doubleNeighbor == newBlock)
                    {
                        continue;
                    }

                    bool ogneighbor = false;

                    foreach (Vector3 tripleHex in Utilities.HexNeighbours(doubleHex))
                    {
                        if (tripleHex == square.HexVector.ToVector3())
                        {
                            ogneighbor = true;
                            break;
                        }
                    }

                    if (!ogneighbor)
                        continue;

                    triMesh.Clear();
                    triMesh.Add(newBlock);
                    triMesh.Add(neighbourBlock);
                    triMesh.Add(doubleNeighbor);

                    bool skip = false;

                    foreach (HashSet<GameObject> hash in alienTrisMesh)
                    {
                        if (hash.IsSupersetOf(triMesh))
                        {
                            skip = true;
                            break;
                        }
                    }

                    if (!skip)
                        alienTrisMesh.Add(triMesh);


                    /*
                    if (alienTris.Contains(tri))
                        continue;
                    else
                        alienTris.Add(tri);*/
                }


                //Debug.Log("oooo!!");


                Rigidbody2D otherRigid = neighbourBlock.GetComponent<Rigidbody2D>();

                if (square.AlienProperties != null && newRigid != null && doubledef.AlienProperties != null)
                {
                    bool makeSpring = true;


                    foreach (var spring in neighbourBlock.GetComponents<SpringJoint2D>())
                    {
                        if (spring.connectedBody == newRigid)
                        {
                            makeSpring = false;
                            //Debug.Log("the pairing " + newBlock.name + " / " + neighbourBlock.name + " failed because of the existing pair: " + neighbourBlock.name + " / " + spring.connectedBody.gameObject.name);
                            break;
                        }
                    }

                    if (!makeSpring)
                        continue;

                    if (square.AlienProperties.TypeID == "Bone" && "Bone" == doubledef.AlienProperties.TypeID)
                    {
                        var fix = newBlock.AddComponent<FixedJoint2D>();
                        fix.connectedBody = otherRigid;
                        fix.breakForce = (square.AlienProperties.SpringConstant + doubledef.AlienProperties.SpringConstant) * (square.AlienProperties.Plasticity + doubledef.AlienProperties.Plasticity) / 2f;
                        continue;
                    }
                    var newspring = newBlock.AddComponent<SpringJoint2D>();
                    newspring.autoConfigureDistance = false;
                    newspring.connectedBody = otherRigid;
                    newspring.distance = 1.05f;
                    newspring.dampingRatio = (square.AlienProperties.SpringDamping + doubledef.AlienProperties.SpringDamping) / 2f;
                    newspring.frequency = (square.AlienProperties.SpringConstant + doubledef.AlienProperties.SpringConstant) / 2f;
                    newspring.breakForce = newspring.frequency * (square.AlienProperties.Plasticity + doubledef.AlienProperties.Plasticity) / 2f;

                    /*
                    var newslide = newBlock.AddComponent<SliderJoint2D>();
                    newslide.connectedBody = otherRigid;
                    newslide.useLimits = true;
                    newslide.limits = new JointTranslationLimits2D { max = 90f, min = 1f };
                    newslide.breakForce = newspring.breakForce / 2;*/
                    //newslide.autoConfigureAngle = false;
                    //newslide.angle = 0f;

                }
                else if (square.AlienProperties != null)
                {


                    AlienProperties check = new AlienProperties();
                    Vector2 loc = Vector2.zero;


                    check = square.AlienProperties;
                    loc = doubledef.blockLocation.ToVector2();


                    if (square.AlienProperties?.TypeID == "Bone")
                    {
                        var fix = newBlock.AddComponent<FixedJoint2D>();
                        fix.connectedBody = Utilities.FindRigidbody(neighbourBlock);
                        fix.connectedAnchor = loc;
                        fix.breakForce = (square.AlienProperties.SpringConstant + notAlienSpringConstant) * (square.AlienProperties.Plasticity + notAlienDampening) / 2f;
                        continue;
                    }

                    var newspring = newBlock.AddComponent<SpringJoint2D>();
                    newspring.autoConfigureDistance = false;
                    newspring.connectedBody = Utilities.FindRigidbody(neighbourBlock);
                    newspring.connectedAnchor = loc;
                    newspring.distance = 1f;
                    newspring.dampingRatio = (square.AlienProperties.SpringDamping + notAlienDampening) / 2f;
                    newspring.frequency = (square.AlienProperties.SpringConstant + notAlienSpringConstant) / 2f;
                    newspring.breakForce = newspring.frequency * (square.AlienProperties.Plasticity + notAlienSpringConstant) / 2f;

                }


                //Debug.Log("AlienNeuighbours!!");


                springCount++;

            }

        }

        

        foreach (HashSet<GameObject> hash in alienTrisMesh)
        {
            int jj = 0;
            tri = new MeshTriangle();
            tri.Triangles = new GameObject[3];

            foreach (GameObject obj in hash)
            {

                tri.Triangles[jj] = obj;
                jj++;
            }
            alienTris.Add(tri);
        }

        if (AlienSquares.Count > 0 && alienTris.Count > 0)
        {
            var mesh = new GameObject(limb.name + "_fleshmesh");
            //mesh.transform.parent = limb.transform;
            var flesh = mesh.AddComponent<FleshMesh>();
            //Object.Destroy(limb.GetComponent<SpriteRenderer>());
            var filter = mesh.AddComponent<MeshFilter>();
            var render = mesh.AddComponent<MeshRenderer>();
            flesh.alienTris = alienTris;
        }
    }

    public void Save(string path)
    {
        path += ".xml";
        var def = new DefinitionSet();
        def.blueprints.Add(this);
        var serializer = new XmlSerializer(typeof(DefinitionSet));
        using (var stream = new FileStream(Path.Combine(blueprintPath, path), FileMode.Create))
        {
            serializer.Serialize(stream, def);
        }
    }

    public static DefinitionSet Load(string path)
    {
        path += ".xml";
        var serializer = new XmlSerializer(typeof(DefinitionSet));
        using (var stream = new FileStream(Path.Combine(blueprintPath, path), FileMode.Open))
        {
            return serializer.Deserialize(stream) as DefinitionSet;
        }
    }

}

public struct MeshTriangle
{
    public GameObject[] Triangles;
}

public class LimbControllerDefinition
{

    [XmlAttribute("ControllerType")]
    public string Type;
    [XmlElement("StrengthMod")]
    public float StrengthMod;
    [XmlElement("SpeedMod")]
    public float SpeedMod;
    [XmlElement("P")]
    public float P;
    [XmlElement("I")]
    public float I;
    [XmlElement("D")]
    public float D;
    [XmlElement("Rotation")]
    public float Rotation;
    [XmlElement("Border")]
    public float Border;
    [XmlElement("SpeedLimit")]
    public float SpeedLimit;
    [XmlElement("Wavelength")]
    public int Wavelength;
    [XmlElement("Offset")]
    public float Offset;
}


