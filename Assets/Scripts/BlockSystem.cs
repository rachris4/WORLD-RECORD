﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;
using System.Xml;
using System.IO;

public class BlockSystem : MonoBehaviour
{

    // Array we expose to inspector / editor, use this instead of the old arrays to define block types.
    [SerializeField]
    public BlockDefinition[] allBlockTypes;

    // Array to store all blocks created in Awake()
    [HideInInspector]
    public Block[] allBlocks;
    public Dictionary<string, Block> blockLookup = new Dictionary<string, Block>();

    public BlockSystem()
    {
    }

    public void Init()
    {
        // Initialise allBlocks array.
        
        allBlocks = new Block[allBlockTypes.Length];

        //SerializeToXml(instance, @"F:\untystuff\WORLD RECORD\Assets\Data"); 

        // For loops to populate main allBlocks array.
        for (int i = 0; i < allBlockTypes.Length; i++)
        {
            // Instead of referencing multiple arrays, we just create a new BlockType object and get values from that.
            BlockDefinition newBlockDef = allBlockTypes[i];
            Block j = new Block();
            j.TypeID = newBlockDef.TypeID;
            j.SubTypeID = newBlockDef.SubTypeID;
            j.DisplayName = newBlockDef.DisplayName;
            j.blockSprite = Resources.Load<Sprite>(newBlockDef.pathSprite);
            j.isSolid = newBlockDef.isSolid;
            j.collider = newBlockDef.collider;
            j.pathSprite = newBlockDef.pathSprite;
            j.destructionProperties = newBlockDef.destructionProperties;
            j.Weapon = newBlockDef.Weapon;
            j.spriteOffset = newBlockDef.spriteOffset;
            j.AlienProperties = newBlockDef.AlienProperties;
            j.TurretProperties = newBlockDef.TurretProperties;
            //j.JointProperties = newBlockDef.JointProperties;
            allBlocks[i] = j;//new Block(newBlockDef.TypeID, newBlockDef.SubTypeID, newBlockDef.DisplayName, Resources.Load<Sprite>(newBlockDef.pathSprite), newBlockDef.isSolid, newBlockDef.collider);
            blockLookup.Add(newBlockDef.SubTypeID, allBlocks[i]);
            //Debug.Log("Solid block: allBlocks[" + i + "] = " + newBlockDef.blockName + newBlockDef.collider);
        }

    }
}

// We still use the Block class to store the final Block type data.
public class Block : BlockDefinition
{
    public Sprite blockSprite;
    public long entityID;
    [XmlElement("blockLocation")]
    public SerializableVector2 blockLocation;
    [XmlElement("rotation")]
    public float rotation;
    [XmlElement("transformScale")]
    public SerializableVector3 transformScale;
    [XmlElement("HexVector")]
    public SerializableVector3 HexVector;
    [XmlElement("HexBlock")]
    public bool HexBlock;

    private BodyPart limbdef;
    public int refIndex;

    public void InitializeFixedWeapon(GameObject obj)
    {
        ProjectileWeaponDefinition def;
        if (DefinitionManager.definitions.projectileWeaponDict.TryGetValue(Weapon, out def))
        {
            //Debug.Log(TypeID);
            var wep = obj.AddComponent<ProjectileWeapon>();
            wep.Initialize(def);
            return;
        }
        EnergyWeaponDefinition laserdef;
        if (DefinitionManager.definitions.energyWeaponDict.TryGetValue(Weapon, out laserdef))
        {
            //Debug.Log(TypeID);
            var wep = obj.AddComponent<EnergyWeapon>();
            wep.Initialize(laserdef);
            return;
        }
    }

    public void InitializeTurret(GameObject obj)
    {
        var barrelobj = new GameObject("barrel");
        var turret = barrelobj.AddComponent<TurretController>();
        barrelobj.transform.parent = obj.transform;
        barrelobj.transform.localPosition = Vector3.zero;
        InitializeFixedWeapon(barrelobj);
        turret.Initialize(TurretProperties);
        SpriteRenderer oldrend = obj.GetComponent<SpriteRenderer>();
        SpriteRenderer turretrend = barrelobj.AddComponent<SpriteRenderer>();
        turretrend.sprite = Resources.Load<Sprite>(TurretProperties.TurretPathSprite);
        turretrend.sortingOrder = oldrend.sortingOrder+2;
        turretrend.color = oldrend.color;
    }

    public void InitializeJointStator(GameObject obj, GameObject parent)
    {

        if (JointProperties == null)
            return;

        if (JointProperties.Bodyparts== null)
        {
            JointProperties.Bodyparts = new string[0];
            return;
        }
        /*
        BodyPart oglimbdef;
        DefinitionManager.definitions.blueprintDict.TryGetValue(parent.name, out oglimbdef);
        if (oglimbdef == null)
            return;*/
        int iterator = 1;
        if (JointProperties.Mirror)
            iterator = 2;
        for(int i = 0; i < iterator; i++)
        {
            foreach (string part in JointProperties.Bodyparts)
            {
                BodyPart limbdef;

                DefinitionManager.definitions.blueprintDict.TryGetValue(part, out limbdef);
                if (limbdef == null)
                    continue;
                //else
                //    Debug.Log("Joint Properties failed.");

                int layerAddition = 2;

                GameObject limb = limbdef.CreateLimbUnity(parent, layerAddition, i > 0);
                //limb.transform.parent = parent.transform;

                var limbobj = limb.GetComponent<LimbUnity>();

                //Debug.Log(parent.name + " : " + parentlimbobj.SortingOrder.ToString() + " / " + parentlimbobj.parentSortingOrder.ToString() + "\n" + limb.name + " : " + limbobj.SortingOrder.ToString() + " / " + limbobj.parentSortingOrder.ToString());


                GameObject limbjoint = new GameObject(limb.name + "_joint");
                limbjoint.transform.parent = limb.transform;
                limbjoint.transform.localPosition = limbdef.jointLocation;
                var rend = limbjoint.AddComponent<SpriteRenderer>();
                rend.sprite = Resources.Load<Sprite>(JointProperties.JointPathSprite);
                rend.sortingOrder = limbobj.SortingOrder + layerAddition / 2;

                var hinge = parent.AddComponent<HingeJoint2D>();
                hinge.autoConfigureConnectedAnchor = false;
                hinge.connectedBody = limb.GetComponent<Rigidbody2D>();
                hinge.anchor = obj.transform.localPosition;
                hinge.connectedAnchor = limbdef.jointLocation;
                hinge.useLimits = true;
                hinge.limits = new JointAngleLimits2D { max = JointProperties.rotationMax, min = JointProperties.rotationMin };

                var controller = limb.GetComponent<LimbController>();
                if (controller != null)
                {
                    controller.max = JointProperties.rotationMax;
                    controller.min = JointProperties.rotationMin;
                    controller.joint = hinge;
                    controller.parent = parent;
                }
            }
        }
    }

    public void InitializeJointRotor(GameObject obj)
    {

    }

    public void InitializeGrappleGun(GameObject obj)
    {
        obj.AddComponent<GrappleGun>();
    }

    public void InitializeThrusterRotator(GameObject obj)
    {
        var thrusterobj = new GameObject("thruster");
        var turret = thrusterobj.AddComponent<TurretController>();
        turret.Initialize(TurretProperties, true, 90f);
        thrusterobj.transform.parent = obj.transform;
        thrusterobj.transform.localPosition = Vector3.zero;
        InitializeThruster(thrusterobj);       
        SpriteRenderer oldrend = obj.GetComponent<SpriteRenderer>();
        SpriteRenderer turretrend = thrusterobj.AddComponent<SpriteRenderer>();
        turretrend.sprite = Resources.Load<Sprite>("Sprites/rotatingthruster");
        turretrend.sortingOrder = oldrend.sortingOrder + 2;
        turretrend.color = oldrend.color;
    }

    public void InitializeThruster(GameObject obj)
    {
        obj.AddComponent<FixedThruster>();
    }

    public void InitializeMeleePiercer(GameObject obj)
    {
        obj.AddComponent<MeleePiercer>();
    }

    public void InitializeByTypeID(GameObject obj, GameObject parent = null)
    {
        //Debug.Log(TypeID);
        switch(TypeID)
        {
            case "FixedWeaponBlock":
                InitializeFixedWeapon(obj);
                break;
            case "TurretBlock":
                InitializeTurret(obj);
                break;
            case "JointStator":
                InitializeJointStator(obj,parent);
                break;
            case "JointRotor":
                InitializeJointRotor(obj);
                break;
            case "FixedThruster":
                InitializeThruster(obj);
                break;
            case "RotatingThruster":
                InitializeThrusterRotator(obj);
                break;
            case "GrappleGun":
                InitializeGrappleGun(obj);
                break;
            case "MeleePiercer":
                InitializeMeleePiercer(obj);
                break;
            default:
                break;
        }

    }

    public GameObject CreateBlockUnity(GameObject parent = null, SpriteRenderer currentRend = null, BodyPart limb = null)
    {
        GameObject newBlock = new GameObject(SubTypeID);
        newBlock.transform.parent = parent.transform;

        newBlock.transform.position = blockLocation.ToVector3()+parent.transform.position;

        BlockDefinition otherSquare;
        DefinitionManager.definitions.blockDict.TryGetValue(SubTypeID, out otherSquare);
        if (otherSquare == null)
            otherSquare = this as BlockDefinition;

        if (otherSquare.TurretProperties != null)
            TurretProperties = otherSquare.TurretProperties;
        if (otherSquare.AlienProperties != null)
            AlienProperties = otherSquare.AlienProperties;


        Weapon = otherSquare.Weapon;


        if (limb != null)
            limbdef = limb;

        newBlock.transform.localScale = transformScale.ToVector3();
        newBlock.layer = parent.layer;
        newBlock.transform.rotation = Quaternion.Euler(0f, 0f, rotation);
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
            case "circle":
                newBlock.AddComponent<CircleCollider2D>();
                break;
            default:
                newBlock.AddComponent<CircleCollider2D>();
                break;

        }

        InitializeByTypeID(newBlock,parent);

        return newBlock;

    }

}

public class AlienRotation : MonoBehaviour
{

    Quaternion iniRot;

    void Start()
    {
        iniRot = gameObject.transform.localRotation;
    }

    void FixedUpdate()
    {
        UpdateRotationAlien();
    }
    
    private void UpdateRotationAlien()
    {
        gameObject.transform.localRotation = iniRot;
    }
}

public class BlockDefinition : DefinitionBase
{

    [XmlElement("pathSprite")]
    public string pathSprite;
    [XmlElement("flippedPathSprite")]
    public string flippedPathSprite;
    [XmlElement("isSolid")]
    public bool isSolid;
    [XmlElement("SpriteOffset")]
    public SerializableVector2 spriteOffset = new SerializableVector2(Vector2.zero);
    [XmlElement("collider")]
    public string collider; //box, triangle, toptwoslope, bottwoslope
    [XmlElement("DestructionProperties")]
    public DestroyableDefinition destructionProperties;
    [XmlElement("weapon")]
    public string Weapon = "";
    [XmlElement("Mass")]
    public float mass;
    public AlienProperties AlienProperties;
    public TurretProperties TurretProperties;
    public JointProperties JointProperties;
}

public class TurretProperties
{

    [XmlElement("TurretPathSprite")]
    public string TurretPathSprite;
    [XmlElement("AI")]
    public bool AI;
    [XmlElement("AIRange")]
    public float AIRange;
    [XmlElement("RotationSpeed")]
    public float RotationSpeed;
    [XmlElement("TurretOffset")]
    public SerializableVector2 TurretOffset;
}

public class JointProperties
{
    [XmlAttribute("TypeID")]
    public string TypeID;
    [XmlElement("JointPathSprite")]
    public string JointPathSprite;
    [XmlElement("Mirror")]
    public bool Mirror;
    [XmlElement("rotationMax")]
    public float rotationMax;
    [XmlElement("rotationMin")]
    public float rotationMin;
    [XmlElement("angularDampening")]
    public float angularDampening;
    [XmlElement("positionDampening")]
    public float positionDampening;
    [XmlArray("BodyParts")]
    [XmlArrayItem("BodyPart", typeof(string))]
    public string[] Bodyparts;
}

public class AlienProperties
{

    [XmlAttribute("TypeID")]
    public string TypeID;
    [XmlElement("Mass")]
    public float Mass;
    [XmlElement("MaterialBounciness")]
    public float MaterialBounciness;
    [XmlElement("MaterialFriction")]
    public float MaterialFriction;
    [XmlElement("SpringDamping")]
    public float SpringDamping;
    [XmlElement("SpringConstant")]
    public float SpringConstant;
    [XmlElement("Plasticity")]
    public float Plasticity;
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
