using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;
using System.Xml;
using System.IO;
using System;
using System.Linq;
using Unity.Collections;

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

        return;

        if (overParent != null && gameObject.transform.parent != overParent)
        {
            var cont = gameObject.GetComponent<LimbController>();
            if (cont != null)
                Destroy(cont);
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
    public static string blueprintPath = @"F:\untystuff\WORLD RECORD\Assets\Data\Blueprints\";

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

    private List<MeshTriangle> alienTris = new List<MeshTriangle>();
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
        else
        {
            var jm = limb.AddComponent<JointManager>();
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
                limb.layer = limbobj.overParent.layer;
                limb.transform.position += limbobj.overParent.transform.position;
            }


            if (limbobj.mirrored)
                layerAddition *= -1;

            limbobj.SortingOrder = parentlimbobj.SortingOrder + layerAddition;
            limbobj.parentData = parentlimbobj;
        }

        var builder = limbobj.overParent.GetComponent<GridAssembly>();
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

        if (Controller != null)
        {
            var limbController = limb.AddComponent<LimbController>();
            limbController.Initialize(Controller);
            limbController.blockIntegrity = blockList.Count;
            limbController.offset = controllerOffset;
            limbController.ResetTick();

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
            {
                jointLocation = square.blockLocation.ToVector2();
                var jm = limb.GetComponent<JointManager>();
                jm.Rotor = newBlock;
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
            if (newBlock == null)
                continue;
            Block square = limbobj.blockList[p];
            newBlock.layer = limb.layer;

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
                rig.drag = 0.6f;
                var mat = new PhysicsMaterial2D();
                mat.friction = square.AlienProperties.MaterialFriction;
                mat.bounciness = square.AlienProperties.MaterialBounciness;
                rig.sharedMaterial = mat;
            }
        }

        MeshTriangle tri = new MeshTriangle();
        tri.Hexes = new Transform[3];

        for (int i = 0; i < AlienSquares.Count; i++)
        {
            GameObject newBlock = AlienBlocks[i];
            Block square = AlienSquares[i];

            Rigidbody2D newRigid;

            newBlock.TryGetComponent<Rigidbody2D>(out newRigid);

            Vector3[] hexNeighbours = Utilities.HexNeighbours(square.HexVector.ToVector3());
            int help = 0;

            foreach (Vector3 hexPt in hexNeighbours)
            {

                GameObject neighbourBlock;

                //Debug.Log("eeee!!");

                HexDict.TryGetValue(hexPt, out neighbourBlock);
                if (neighbourBlock == null)
                {
                    
                    continue;

                }

                Block doubledef = AlienSquares[AlienBlocks.IndexOf(neighbourBlock)];

                MakeSprings(newBlock, neighbourBlock, square, doubledef);


                tri = new MeshTriangle();
                tri.Hexes = new Transform[3];

                HashSet<GameObject> triMesh = new HashSet<GameObject>();

                Vector3 doubleHex = hexNeighbours[(help + 1) % 6];
                help++;


                GameObject doubleNeighbor;
                HexDict.TryGetValue(doubleHex, out doubleNeighbor);
                if (doubleNeighbor == null)
                {
                    continue;
                }

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



            }

        }

        foreach (HashSet<GameObject> hash in alienTrisMesh)
        {
            int jj = 0;
            tri = new MeshTriangle();
            tri.Hexes = new Transform[3];

            foreach (GameObject obj in hash)
            {

                tri.Hexes[jj] = obj.transform;
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
            flesh.InitializeTransforms(alienTris);
            flesh.SortingOrder = limbobj.SortingOrder - 1;
        }
    }

    public void MakeSprings(GameObject newBlock, GameObject neighbourBlock, Block square, Block doubledef)
    {
        Rigidbody2D otherRigid = neighbourBlock.GetComponent<Rigidbody2D>();
        Rigidbody2D newRigid = newBlock.GetComponent<Rigidbody2D>();

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
                return;

            if (square.AlienProperties.TypeID == "Bone" && "Bone" == doubledef.AlienProperties.TypeID)
            {
                var fix = newBlock.AddComponent<FixedJoint2D>();
                fix.connectedBody = otherRigid;
                fix.breakForce = (square.AlienProperties.SpringConstant + doubledef.AlienProperties.SpringConstant) * (square.AlienProperties.Plasticity + doubledef.AlienProperties.Plasticity) / 2f;
                return;
            }
            var newspring = newBlock.AddComponent<SpringJoint2D>();
            newspring.autoConfigureDistance = false;
            newspring.connectedBody = otherRigid;
            newspring.anchor = -square.spriteOffset.ToVector2();
            newspring.connectedAnchor = -doubledef.spriteOffset.ToVector2();
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
                return;
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
    }

    public void Save(string path, bool overwrite = false)
    {

        Vector3 min = Vector3.zero;
        Vector3 max = Vector3.zero;
        Vector3 loc = Vector3.zero;

        foreach (Block bloq in blockList)
        {
            loc = bloq.blockLocation.ToVector3();

            if (loc.x < min.x)
            {
                min.x = (int)loc.x;
            }
            if (loc.y < min.y)
            {
                min.y = (int)loc.y;
            }
            if (loc.x > max.x)
            {
                max.x = (int)loc.x;
            }
            if (loc.y > max.y)
            {
                max.y = (int)loc.y;
            }
        }
        int bord = 2;

        min.x -= bord;
        min.y -= bord;
        max.x += bord;
        max.y += bord;

        foreach (Block bloq in blockList)
        {
            loc = bloq.blockLocation.ToVector3();
            loc -= min;
            bloq.blockLocation = new SerializableVector2(loc);
        }

            string dir = Path.Combine(blueprintPath, path);
        //path = Path.Combine(blueprintPath, path);

        for(int j = 1; j < 20; j++)
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
                dir = Path.Combine(blueprintPath, path);
            }
            else
                break;
        }

        //dir = Path.Combine(dir, @"\");
        //Debug.Log(Path.Combine(dir, path).ToString());
        var screenshit = Utilities.FindMainCamera();//new GameObject("screenshotter");
        var comp = screenshit.AddComponent<Screenshotter>();
        comp.Initialize(min, max, dir, path, false);

        path += ".xml";
        var def = new DefinitionSet();
        def.blueprints.Add(this);
        var serializer = new XmlSerializer(typeof(DefinitionSet));
        using (var stream = new FileStream(Path.Combine(dir,path), FileMode.Create))
        {
            serializer.Serialize(stream, def);
        }
    }
    
    public static DefinitionSet Load(string path)
    {
        //path += ".xml";
        var serializer = new XmlSerializer(typeof(DefinitionSet));
        using (var stream = new FileStream(Path.Combine(Path.Combine(blueprintPath, path), path + ".xml"), FileMode.Open))
        {
            return serializer.Deserialize(stream) as DefinitionSet;
        }
    }

}

public struct MeshTriangle
{
    public Transform[] Hexes;
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


