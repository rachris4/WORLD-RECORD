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

    public Chassis(ChassisDefinition init, string addstring = "")
    {
        def = init;
        def.SubTypeID += addstring;
        Initialize();
    }
    void Initialize()
    {


        foreach (var item in def.bodyParts)
        {
            GameObject limb = new GameObject(def.SubTypeID + "_" + item.limbName);
            var info = limb.AddComponent<BodyPartUnity>();
            GameObject limbSprite = new GameObject("sprite_"+limb.name);
            limbSprite.transform.parent = limb.transform;
            Vector3 pos = new Vector3(item.position.x, item.position.y, 0);//item.VisualLayer);
            //Debug.Log(item.limbName);
            limb.transform.position = pos;
            var bodyGrid = limb.AddComponent<Grid>();
            var rigidbody = limb.AddComponent<Rigidbody2D>();
            var limbdef = BodyPart.Load(item.limbName);
            rigidbody.useAutoMass = true;
            var currentRend = limbSprite.AddComponent<SpriteRenderer>();
            currentRend.sprite = Resources.Load<Sprite>("Sprites/BoundingBox");
            currentRend.sortingOrder = -item.VisualLayer;

            if (limb.name.Contains("Left"))
            {
                currentRend.color = new Color(1f, 0f, 0f, 1f);
                limb.layer = 22;
            }
            else if (limb.name.Contains("Right"))
            {
                currentRend.color = new Color(0f, 0f, 1f, 1f);
                limb.layer = 26;
            }
            else
            {
                limb.layer = 24;
            }

            Vector2Int min = Vector2Int.zero;
            Vector2Int max = Vector2Int.zero;
            int blockCount = 0;

            foreach (BodyPart bp in limbdef.blueprints)
            {
                blockCount = bp.blockList.Count;
                foreach (Block square in bp.blockList)
                {
                    GameObject newBlock = new GameObject(square.SubTypeID);
                    newBlock.transform.position = square.blockLocation.ToVector3();

                    BlockDefinition otherSquare;
                    DefinitionManager.definitions.blockDict.TryGetValue(square.SubTypeID, out otherSquare);
                    if (otherSquare == null)
                        otherSquare = square as BlockDefinition;
                    square.Weapon = otherSquare.Weapon;
                    square.InitializeByTypeID(newBlock);

                    if(newBlock.transform.position.x < min.x)
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


                    newBlock.transform.parent = limb.transform;
                    newBlock.transform.localScale = square.transformScale.ToVector3();
                    newBlock.layer = limb.layer;
                    newBlock.transform.rotation = Quaternion.Euler(0f, 0f, square.rotation);
                    SpriteRenderer newRend = newBlock.AddComponent<SpriteRenderer>();

                    


                    newRend.sprite = Resources.Load<Sprite>(otherSquare.pathSprite);
                    newRend.sortingOrder = currentRend.sortingOrder;
                    newRend.color = currentRend.color;
                    if (otherSquare.destructionProperties != null)
                    {
                        var healthobject = newBlock.AddComponent<Destroyable>();
                        //Debug.Log(otherSquare.destructionProperties.Threshold.ToString());
                        healthobject.Initialize(otherSquare.destructionProperties);
                    }
                    PolygonCollider2D shape;
                    switch (otherSquare.collider)
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



            //Vector3 scale = limb.transform.localScale / 10;
            //scale.x *= item.maxWidth;
            //scale.y *= item.maxLength;
            //bodyGrid.
            info.Max = max;
            info.Min = min;

            limbSprite.transform.localScale = new Vector3(max.x - min.x+1, max.y - min.y+1, 1f);
            currentRend.color = new Color(0f, 1f, 0f, 1f);
            Vector2 span = max - min;
            span = max - span / 2;
            limbSprite.transform.position = new Vector3(span.x, span.y, 0);
            //limb.transform.localScale = scale; IMPORTANT FIX LATER
            //limb.AddComponent<BoxCollider2D>();
            rigidbody.Sleep();
            if(item.Controller != null)
            {
                var limbController = limb.AddComponent<LimbController>();
                limbController.Initialize(item.Controller);
            }
            limbObjects.Add(limb);
            
        }
        foreach (var item in def.bodyParts)
        {
            GameObject temp = GameObject.Find(def.SubTypeID + "_" + item.limbName);
            var info = temp.GetComponent<BodyPartUnity>();
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
                var attachedLimb = temptemp.GetComponent<BodyPartUnity>();
                hinge.connectedBody = temptemp.GetComponent<Rigidbody2D>();
                //Debug.Log(joint.jointName + " Joint Def Anchor Pos : " +joint.pathsprite);
                Vector2 tempvector = new Vector2(0f, 0f);
                Vector2 span = info.Max - info.Min;
                Vector2 attachedSpan = attachedLimb.Max - attachedLimb.Min;
                tempvector.x = joint.anchorPosition.x*span.x;
                tempvector.y = joint.anchorPosition.y*span.y;
                span = info.Max - span / 2;
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

public class BodyPartUnity : MonoBehaviour
{
    [SerializeField]
    public Dictionary<Vector2Int, Block> blockDictionary = new Dictionary<Vector2Int, Block>();
    [SerializeField]
    public Vector2Int Max;
    [SerializeField]
    public Vector2Int Min;


    public void UpdateBlocks()
    {

    }

    void Update()
    {
        if (gameObject.transform.childCount <= 1)
            Destroy(gameObject);
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

    public void Save(string path)
    {
        path += ".xml";
        var def = new DefinitionSet();
        def.blueprints.Add(this);
        var serializer = new XmlSerializer(typeof(DefinitionSet));
        using (var stream = new FileStream(Path.Combine(blueprintPath,path), FileMode.Create))
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

public struct ChassisLimbDefinition
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

public class DestroyableDefinition
{
    [XmlAttribute("Health")]
    public float Health;
    [XmlAttribute("Threshold")]
    public float Threshold;
    [XmlAttribute("BreakPoints")]
    public int BreakPoints;
    [XmlAttribute("Hardness")]
    public float Hardness;
    [XmlAttribute("ShatterType")]
    public string shatterType;
    [XmlAttribute("FadeOut")]
    public float fadeOut;
    [XmlAttribute("Fuse")]
    public float Fuse;
}