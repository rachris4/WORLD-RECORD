﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;
using System.Xml;
using System;
using System.IO;

public class DefinitionManager : MonoBehaviour
{
    // Start is called before the first frame update
    public static DefinitionSet definitions;

    //public int GetNewEntityId()

    void Start()
    {
        //Debug.Log(Application.persistentDataPath);
        //BlockDefinitions blockDefs = BlockDefinitions.Initialize();
        definitions = new DefinitionSet();
        definitions.Initialize();
        BlockSystem sys = gameObject.AddComponent<BlockSystem>();
        sys.allBlockTypes = new BlockDefinition[definitions.blockDefinitions.Count];
        definitions.blockDefinitions.CopyTo(sys.allBlockTypes);
        //sys.allBlockTypes = blockDefs.blockDefinitions;
        sys.Init();
        BuildSystem build = GetComponent<BuildSystem>();
        build.blockSys = sys;
//return;
        foreach(var def in definitions.chassisDefinitions)
        {
            Chassis god = new Chassis(def);
            Chassis eve = new Chassis(def, "yeb");
        }


        //BuildSystem build = gameObject.AddComponent<BuildSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

[XmlRoot(ElementName = "Definitions")]
public class DefinitionSet
{
    private static string definitionPath = @"F:\untystuff\WORLD RECORD\Assets\Data\Definitions";
    private static string blueprintPath = @"F:\untystuff\WORLD RECORD\Assets\Data\Blueprints";







    [XmlArray("BlockDefinitions")]
    [XmlArrayItem("BlockDefinition", typeof(BlockDefinition))]
    public HashSet<BlockDefinition> blockDefinitions = new HashSet<BlockDefinition>();
    [XmlIgnore]
    public Dictionary<string, BlockDefinition> blockDict = new Dictionary<string, BlockDefinition>();
    public HashSet<string> SubTypeIdList = new HashSet<string>();


    [XmlArray("ChassisDefinitions")]
    [XmlArrayItem("ChassisDefinition", typeof(ChassisDefinition))]
    public HashSet<ChassisDefinition> chassisDefinitions = new HashSet<ChassisDefinition>();

    [XmlArray("ProjectileDefinitions")]
    [XmlArrayItem("ProjectileDefinition", typeof(ProjectileDefinition))]
    public HashSet<ProjectileDefinition> projectileDefinitions = new HashSet<ProjectileDefinition>();
    [XmlIgnore]
    public Dictionary<string, ProjectileDefinition> projectileDict = new Dictionary<string, ProjectileDefinition>();
    public HashSet<string> projectileSubTypeList = new HashSet<string>();


    [XmlArray("ProjectileWeaponDefinitions")]
    [XmlArrayItem("ProjectileWeaponDefinition", typeof(ProjectileWeaponDefinition))]
    public HashSet<ProjectileWeaponDefinition> projectileWeaponDefinitions = new HashSet<ProjectileWeaponDefinition>();
    [XmlIgnore]
    public Dictionary<string, ProjectileWeaponDefinition> projectileWeaponDict = new Dictionary<string, ProjectileWeaponDefinition>();
    public HashSet<string> projectileWeaponSubTypeList = new HashSet<string>();


    [XmlArray("Blueprints")]
    [XmlArrayItem("Blueprint", typeof(BodyPart))]
    public HashSet<BodyPart> blueprints = new HashSet<BodyPart>();
    public HashSet<string> blueprintSubTypeIdList = new HashSet<string>();
    //public Dictionary<string, BodyPart> blueprintDict = new Dictionary<string, BodyPart>();

    public void Initialize()
    {
        DirectoryInfo dir = new DirectoryInfo(definitionPath);
        FileInfo[] info = dir.GetFiles("*.xml");
        foreach(FileInfo f in info)
        {
            Debug.Log(f.FullName);
            DefinitionSet temp = DefinitionSet.Load(f.FullName);
            if (temp == null)
                continue;
            foreach (BlockDefinition item in temp.blockDefinitions)
            {
                if (SubTypeIdList.Contains(item.SubTypeID))
                {
                    Debug.Log("SKREECH! Subtypeids clashed. Idiot.");
                    continue;
                }

                SubTypeIdList.Add(item.SubTypeID);
                blockDefinitions.Add(item);
                blockDict.Add(item.SubTypeID, item);
            }
            foreach (ChassisDefinition item in temp.chassisDefinitions)
            {
                if (SubTypeIdList.Contains(item.SubTypeID))
                {
                    Debug.Log("SKREECH! Subtypeids clashed. Idiot.");
                    continue;
                }

                SubTypeIdList.Add(item.SubTypeID);
                chassisDefinitions.Add(item);
            }
            foreach (ProjectileWeaponDefinition item in temp.projectileWeaponDefinitions) // WEAPONS
            {
                if (projectileWeaponSubTypeList.Contains(item.SubTypeID))
                {
                    Debug.Log("SKREECH! Subtypeids clashed. Idiot.");
                    continue;
                }

                projectileWeaponSubTypeList.Add(item.SubTypeID);
                projectileWeaponDefinitions.Add(item);
                projectileWeaponDict.Add(item.SubTypeID, item);
            }
            foreach (ProjectileDefinition item in temp.projectileDefinitions) // PROJECTILES
            {
                if (projectileSubTypeList.Contains(item.SubTypeID))
                {
                    Debug.Log("SKREECH! Subtypeids clashed. Idiot.");
                    continue;
                }

                projectileSubTypeList.Add(item.SubTypeID);
                projectileDefinitions.Add(item);
                projectileDict.Add(item.SubTypeID, item);
            }
        }

        Debug.Log("block definitions loaded : " + blockDefinitions.Count.ToString());
        Debug.Log("chassis definitions loaded : " + chassisDefinitions.Count.ToString());
        Debug.Log("projectile definitions loaded : " + projectileDefinitions.Count.ToString());
        Debug.Log("weapon definitions loaded : " + projectileWeaponDefinitions.Count.ToString());

        /// BLUEPRINTS BLUEPRINTS BLUEPRINTS BLUEPRINTS BLUEPRINTSBLUEPRINTSBLUEPRINTSBLUEPRINTSBLUEPRINTSBLUEPRINTSBLUEPRINTSBLUEPRINTSBLUEPRINTSBLUEPRINTSBLUEPRINTSBLUEPRINTS
        /// BLUEPRINTSBLUEPRINTSBLUEPRINTSBLUEPRINTSBLUEPRINTSBLUEPRINTSBLUEPRINTSBLUEPRINTSBLUEPRINTSBLUEPRINTSBLUEPRINTSBLUEPRINTSBLUEPRINTSBLUEPRINTSBLUEPRINTSBLUEPRINTS
        dir = new DirectoryInfo(blueprintPath);
        info = dir.GetFiles("*.xml");
        foreach (FileInfo f in info)
        {
            //Debug.Log(f.FullName);
            DefinitionSet temp = DefinitionSet.Load(f.FullName);
            if (temp == null)
                continue;
            foreach (BodyPart item in temp.blueprints)
            {
                if (blueprintSubTypeIdList.Contains(item.SubTypeID))
                {
                    Debug.Log("SKREECH! Subtypeids clashed. Idiot.");
                    continue;
                }
                //blueprintDict.Add(item.SubTypeID, item);
                blueprintSubTypeIdList.Add(item.SubTypeID);
                blueprints.Add(item);
            }
        }


    }




    public void Save(string path)
    {
        var serializer = new XmlSerializer(typeof(DefinitionSet));
        using (var stream = new FileStream(path, FileMode.Create))
        {
            serializer.Serialize(stream, this);
        }
    }

    public static DefinitionSet Load(string path)
    {
        var serializer = new XmlSerializer(typeof(DefinitionSet));
        using (var stream = new FileStream(path, FileMode.Open))
        {
            try
            {
                var serial = serializer.Deserialize(stream) as DefinitionSet;
                return serial;
            }
            catch(Exception E)
            {
                Debug.Log("Error loading file located at " + path + "\n" + E.ToString());
                return null;
            }
        }
    }
}

public class DefinitionBase
{
    [XmlAttribute("TypeID")]
    public string TypeID;
    [XmlElement("SubTypeID")]
    public string SubTypeID;
    [XmlElement("DisplayName")]
    public string DisplayName;
}

public struct SerializableVector2
{
    [XmlAttribute("x")]
    public float x;
    [XmlAttribute("y")]
    public float y;

    public SerializableVector2(Vector2 original)
    {
        x = original.x;
        y = original.y;
    }

    public Vector2 ToVector2()
    {
        return new Vector2(x, y);
    }

    public Vector3 ToVector3()
    {
        return new Vector3(x, y, 0f);
    }
}

public struct SerializableVector3
{
    [XmlAttribute("x")]
    public float x;
    [XmlAttribute("y")]
    public float y;
    [XmlAttribute("z")]
    public float z;

    public SerializableVector3(Vector3 original)
    {
        x = original.x;
        y = original.y;
        z = original.z;
    }

    public Vector3 ToVector3()
    {
        return new Vector3(x, y, z);
    }
}