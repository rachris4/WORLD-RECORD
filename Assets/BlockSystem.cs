using System;
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

    public void InitializeByTypeID(GameObject obj)
    {
        //Debug.Log(TypeID);
        if(TypeID == "FixedWeaponBlock")
        {
            ProjectileWeaponDefinition def;
            if(DefinitionManager.definitions.projectileWeaponDict.TryGetValue(Weapon, out def))
            {
                //Debug.Log(TypeID);
                var wep = obj.AddComponent<ProjectileWeapon>();
                wep.Initialize(def);
            }

        }
    }

}


public class BlockDefinition : DefinitionBase
{
    [XmlElement("pathSprite")]
    public string pathSprite;
    [XmlElement("isSolid")]
    public bool isSolid;
    [XmlElement("collider")]
    public string collider; //box, triangle, toptwoslope, bottwoslope
    [XmlElement("DestructionProperties")]
    public DestroyableDefinition destructionProperties;
    [XmlElement("weapon")]
    public string Weapon = "";
}
