using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;
using System.Xml;
using System.IO;

public class Chassis
{
    public ChassisDefinition def;

    public List<BodyConnector> Joints = new List<BodyConnector>();

    public BodyPart[] bodyParts;

    public List<GameObject> jointObjects = new List<GameObject>();

    public List<GameObject> limbObjects = new List<GameObject>();

    public Vector2 position;

    public Vector2 rotation;

    private GameObject entparent;

    public Chassis(ChassisDefinition init, string addstring = "", GameObject obj = null)
    {
        def = init;
        def.SubTypeID += addstring;
        entparent = obj;
        Initialize();
    }
    void Initialize()
    {


        foreach (var item in def.bodyParts)
        {

            BodyPart limbdef;
            DefinitionManager.definitions.blueprintDict.TryGetValue(item.limbName, out limbdef);
            if (limbdef == null)
            {
                continue;
            }

            GameObject limb = limbdef.CreateLimbUnity(limbdef, def, item, entparent);
            limbObjects.Add(limb);
            
        }

        foreach (var item in def.bodyParts)
        {
            GameObject temp = GameObject.Find(def.SubTypeID + "_" + item.limbName);

            BodyPart limbdef;
            DefinitionManager.definitions.blueprintDict.TryGetValue(item.limbName, out limbdef);
            if (limbdef == null)
            {
                continue;
            }

            if (temp == null)
            {
                Debug.Log("Tired of this shit!");
                    continue;
            }
            //Debug.Log(item.limbName + " has this many joints: " +item.Joints.Length.ToString());
            if (item.Joints == null)
                continue;

            foreach (var joint in item.Joints)
            {
                var hinge = temp.AddComponent<HingeJoint2D>();
                hinge.autoConfigureConnectedAnchor = false;
                var temptemp = GameObject.Find(def.SubTypeID + "_" + joint.childName);

                BodyPart attachedLimb;
                DefinitionManager.definitions.blueprintDict.TryGetValue(joint.childName, out attachedLimb);
                if (attachedLimb == null)
                {
                    continue;
                }


                hinge.connectedBody = temptemp.GetComponent<Rigidbody2D>();
                //Debug.Log(joint.jointName + " Joint Def Anchor Pos : " +joint.pathsprite);
                Vector2 tempvector = new Vector2(0f, 0f);
                Vector2 span = limbdef.Max - limbdef.Min;
                Vector2 attachedSpan = attachedLimb.Max - attachedLimb.Min;
                tempvector.x = joint.anchorPosition.x*span.x;
                tempvector.y = joint.anchorPosition.y*span.y;
                span = limbdef.Max - span / 2;
                hinge.anchor = tempvector + span;
                tempvector.x = joint.hookPosition.x*attachedSpan.x;
                tempvector.y = joint.hookPosition.y * attachedSpan.y;
                attachedSpan = attachedLimb.Max - attachedSpan / 2;
                hinge.connectedAnchor = tempvector + attachedSpan;
                hinge.useLimits = true;
                hinge.limits = new JointAngleLimits2D { max = joint.rotationMax, min = joint.rotationMin };
                var controller = temptemp.GetComponent<LimbController>();
                if(controller != null)
                {
                    controller.parent = temp;
                }
               // hinge.breakForce = 100000f;
              //  hinge.breakTorque = 1000f;
                /*
                continue;

                GameObject jntobj = new GameObject(joint.jointName);
                var rend = jntobj.AddComponent<SpriteRenderer>();
                rend.sprite = Resources.Load<Sprite>("Sprites/triangle");
                jointObjects.Add(jntobj);
                Joints.Add(joint);*/
            }

        }
        foreach(var item in limbObjects)
        {
            var rigidbody = item.GetComponent<Rigidbody2D>();
            rigidbody.WakeUp();
        }

    }

}

public class ChassisDefinition : DefinitionBase
{

    [XmlArray("BodyParts")]
    [XmlArrayItem("BodyPart", typeof(ChassisLimbDefinition))]
    public ChassisLimbDefinition[] bodyParts;



}


public class BodyPart : DefinitionBase
{
    private static string blueprintPath = @"F:\untystuff\WORLD RECORD\Assets\Data\Blueprints\";

    [XmlArray("Blocks")]
    [XmlArrayItem("Block", typeof(Block))]
    public HashSet<Block> blockList = new HashSet<Block>();

    public int VisualLayer = 0;
    public bool Flippable = true;
    //public BodyPart Parent;

    [XmlArray("BodyConnectors")]
    [XmlArrayItem("BodyConnector", typeof(BodyConnector))]
    public BodyConnector[] Joints;

    public Vector2Int Max;
    public Vector2Int Min;

    private HashSet<MeshTriangle> alienTris = new HashSet<MeshTriangle>();

    public GameObject CreateLimbUnity(BodyPart limbdef, ChassisDefinition def = null, ChassisLimbDefinition item = null,  GameObject entparent = null)
    {

        string name = "";
        if(def != null)
        {
            name = def.SubTypeID + "_";
        }

        GameObject limb = new GameObject(name + limbdef.SubTypeID);
        
        if (entparent != null)
            limb.transform.parent = entparent.transform;
        GameObject limbSprite = new GameObject("sprite_" + limb.name);
        limbSprite.transform.parent = limb.transform;

        var bodyGrid = limb.AddComponent<Grid>();
        var rigidbody = limb.AddComponent<Rigidbody2D>();

        


        rigidbody.useAutoMass = true;
        var currentRend = limbSprite.AddComponent<SpriteRenderer>();
        currentRend.sprite = Resources.Load<Sprite>("Sprites/BoundingBox");

        if (limb.name.Contains("Left"))
        {
            currentRend.color = new Color(0.5f, 0.5f, 0.5f, 1f);
            limb.layer = 22;
        }
        else if (limb.name.Contains("Right"))
        {
            currentRend.color = new Color(1f, 1f, 1f, 1f);
            limb.layer = 26;
        }
        else
        {
            currentRend.color = new Color(1f, 1f, 1f, 1f);
            limb.layer = 24;
        }

        if (item != null)
        {
            Vector3 pos = new Vector3(item.position.x, item.position.y, 0);//item.VisualLayer);
            currentRend.sortingOrder = -item.VisualLayer;
            limb.transform.position = pos;
            if (item.Controller != null)
            {
                var limbController = limb.AddComponent<LimbController>();
                limbController.Initialize(item.Controller);
            }
        }

        Vector2Int min = Vector2Int.zero;
        Vector2Int max = Vector2Int.zero;
        int blockCount = 0;

        blockCount = limbdef.blockList.Count;

        List<GameObject> AlienBlocks = new List<GameObject>();
        List<Block> AlienSquares = new List<Block>();
        Dictionary<Vector3, GameObject> HexDict = new Dictionary<Vector3, GameObject>();

        int kk = 0;

        foreach (Block square in limbdef.blockList)
        {
            GameObject newBlock = square.CreateBlockUnity(limb, currentRend);

            BlockDefinition bloq;
            DefinitionManager.definitions.blockDict.TryGetValue(square.SubTypeID, out bloq);

            if (bloq == null)
                continue;

            if (bloq.AlienProperties != null)
            {
                kk++;

                square.AlienProperties = bloq.AlienProperties;

                square.refIndex = kk;
                AlienBlocks.Add(newBlock);
                AlienSquares.Add(square);
                var rig = newBlock.AddComponent<Rigidbody2D>();
                rig.useAutoMass = false;
                rig.mass = square.AlienProperties.Mass;
                var mat = new PhysicsMaterial2D();
                mat.friction = square.AlienProperties.MaterialFriction;
                mat.bounciness = square.AlienProperties.MaterialBounciness;
                rig.sharedMaterial = mat;
                HexDict.Add(square.HexVector.ToVector3(), newBlock);
            }

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

        int fixes = 0;

        for(int i = 0; i < AlienSquares.Count; i++)
        {
            GameObject newBlock = AlienBlocks[i];
            Block square = AlienSquares[i];
            Rigidbody2D newRigid = newBlock.GetComponent<Rigidbody2D>();
            if(square.AlienProperties.TypeID == "Bone" && fixes < 2)
            {
                var fix = limb.AddComponent<FixedJoint2D>();
                fix.connectedBody = newRigid;
                fixes++;
            }
            int springCount = 0;

            foreach(Vector3 hexPt in Utilities.HexNeighbours(square.HexVector.ToVector3()))
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

                MeshTriangle tri = new MeshTriangle();
                tri.Triangles = new GameObject[3];

                foreach (Vector3 doubleHex in Utilities.HexNeighbours(hexPt))
                {
                    GameObject doubleNeighbor;
                    HexDict.TryGetValue(doubleHex, out doubleNeighbor);
                    if (doubleNeighbor == null || doubleNeighbor == newBlock)
                    {
                        continue;
                    }
                    Debug.ClearDeveloperConsole();
                    bool ogneighbor = false;

                    foreach (Vector3 tripleHex in Utilities.HexNeighbours(doubleHex))
                    {
                        if(tripleHex == square.HexVector.ToVector3())
                        {
                            ogneighbor = true;
                            break;
                        }
                    }

                    if (!ogneighbor)
                        continue;
                    /*
                    Block tripledef = AlienSquares[AlienBlocks.IndexOf(doubleNeighbor)];
                    Block[] set = new Block[3];
                    set[0] = square;
                    set[1] = doubledef;
                    set[2] = tripledef;

                    if (set[0].refIndex > set[1].refIndex) //ew
                    {
                        set[0] = doubledef;
                        set[1] = square;
                    }
                    if(set[1].refIndex > set[2].refIndex)
                    {
                        var temp = set[1];
                        set[1] = set[2];
                        set[2] = temp;
                    }
                    if (set[0].refIndex > set[1].refIndex)
                    {
                        var temp = set[0];
                        set[0] = set[1];
                        set[1] = temp;
                    }

                    for(int l = 0; l < 3; l++)
                    {
                        var sq = set[l];

                        if(sq == square)
                            tri.Triangles[l] = newBlock;
                        else if (sq == doubledef)
                            tri.Triangles[l] = neighbourBlock;
                        else if(sq == tripledef)
                            tri.Triangles[l] = doubleNeighbor;
                    }

                    */

                    tri.Triangles[0] = newBlock;
                    tri.Triangles[1] = neighbourBlock;
                    tri.Triangles[2] = doubleNeighbor;


                    if (alienTris.Contains(tri))
                        continue;
                    else
                        alienTris.Add(tri);
                }

                bool makeSpring = true;

                //Debug.Log("oooo!!");

                
                Rigidbody2D otherRigid = neighbourBlock.GetComponent<Rigidbody2D>();
                foreach(var spring in neighbourBlock.GetComponents<SpringJoint2D>())
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
                //Debug.Log("AlienNeuighbours!!");
                if(square.AlienProperties.TypeID == "Bone" && "Bone" == doubledef.AlienProperties.TypeID)
                {
                    var fix = newBlock.AddComponent<FixedJoint2D>();
                    fix.connectedBody = otherRigid;
                    fix.breakForce = (square.AlienProperties.SpringConstant + doubledef.AlienProperties.SpringConstant) * (square.AlienProperties.Plasticity + doubledef.AlienProperties.Plasticity) / 2f;
                    continue;
                }
                var newspring = newBlock.AddComponent<SpringJoint2D>();
                newspring.autoConfigureDistance = false;
                newspring.connectedBody = otherRigid;
                newspring.distance = 2.1f;
                newspring.dampingRatio = (square.AlienProperties.SpringDamping+ doubledef.AlienProperties.SpringDamping)/2f;
                newspring.frequency = (square.AlienProperties.SpringConstant + doubledef.AlienProperties.SpringConstant)/2f;
                newspring.breakForce = newspring.frequency * (square.AlienProperties.Plasticity + doubledef.AlienProperties.Plasticity) / 2f;

                springCount++;

            }

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

        limbdef.Max = max;
        limbdef.Min = min;

        limbSprite.transform.localScale = new Vector3(max.x - min.x + 1, max.y - min.y + 1, 1f);
        currentRend.color = new Color(0f, 1f, 0f, 0f);
        Vector2 span = max - min;
        span = max - span / 2;
        limbSprite.transform.position = new Vector3(span.x, span.y, 0);
        rigidbody.Sleep();

        

        return limb;
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

public class BodyConnector
{

    [XmlAttribute("jointName")]
    public string jointName;
    [XmlAttribute("childName")]
    public string childName;
    [XmlElement("rotationMax")]
    public float rotationMax;
    [XmlElement("rotationMin")]
    public float rotationMin;
    [XmlElement("angularDampening")]
    float angularDampening;
    [XmlElement("positionDampening")]
    float positionDampening;
    [XmlElement("pathsprite")]
    public string pathsprite;
    [XmlElement("anchorPosition")]
    public SerializableVector2 anchorPosition;
    [XmlElement("hookPosition")]
    public SerializableVector2 hookPosition;
    [XmlElement("rotation")]
    float rotation;

    public BodyPart parent;
    public BodyPart child;
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

public class ChassisLimbDefinition
{
    [XmlAttribute("limbName")]
    public string limbName;
    [XmlElement("maxWidth")]
    public int maxWidth;
    [XmlElement("maxLength")]
    public int maxLength;
    [XmlElement("position")]
    public Vector2 position;
    [XmlArray("BodyConnectors")]
    [XmlArrayItem("BodyConnector", typeof(BodyConnector))]
    public BodyConnector[] Joints;
    [XmlElement("VisualLayer")]
    public int VisualLayer;// = 0;
    [XmlElement("Flippable")]
    public bool Flippable;// = true;
    public LimbControllerDefinition Controller;// = true;
}

