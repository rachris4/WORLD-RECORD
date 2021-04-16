using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridAssembly : MonoBehaviour
{
    [SerializeField]
    private string LoadedSubTypeID = "HumanTorso";
    [SerializeField]
    private bool flipOnInit = false;
    [SerializeField]
    private bool isPlayer = false;

    private bool initd = false;
    private GameObject cam;

    private int tick = 0;

    [HideInInspector]
    public List<BodyPart> bpList = new List<BodyPart>();
    public List<GameObject> objList = new List<GameObject>();
    public List<LimbController> controllers = new List<LimbController>();

    private bool alienUpdated = false;

    void Update()
    {
        if (!initd)
            Initialize();
        
        if(isPlayer)
        {
            UpdateCamera();
        }

        if(tick > 10 && bpList.Count > 0 && !alienUpdated)
        {
            for(int i = 0; i < bpList.Count; i++)
            {
                bpList[i].InitializeAlienParts(objList[i]);
            }

            alienUpdated = true;

            if (flipOnInit)
                FlipChassis();

        }

        if((tick+1) % 100 == 0) //sinful. fix later
        {
        }

        if(Input.GetKeyDown("f") && isPlayer)
        {
            FlipChassis();
        }

        tick++;
    }

    public void KillAllControllers()
    {
        foreach(GameObject obj in objList)
        {
            if (obj == null)
                return;
            var limbcon = obj.GetComponent<LimbController>();
            if(limbcon != null)
            {
                if(limbcon.joint != null)
                {
                    //limbcon.joint.limits = new JointAngleLimits2D { max = limbcon.joint.limits.max + 25, min = limbcon.joint.limits.min - 25 };
                    limbcon.joint.useMotor = false;
                }
                
                Destroy(limbcon);
                //limbcon.strengthMod = 0;
                //limbcon.speedMod = 0;
                //limbcon.speedLimit = 0;
            }
        }
    }

    void FixOverParentTranslation(HashSet<Rigidbody2D> rigids)
    {
        //gameObject.transform.position -= new Vector3(meanVec.x, 0f, 0f)/2f;
        

        //gameObject.transform.position -= new Vector3(meanVec.x, 0f, 0f) * 2 * gameObject.transform.localScale.x;

        var vec = gameObject.transform.position;
        gameObject.transform.position = Vector3.zero;

        foreach(Rigidbody2D rigid in rigids)
        {
            rigid.gameObject.transform.position += vec;
        }

    }

    void FlipChassis() // oh jesus
    {

        DynamicCamera dyn = cam.GetComponent<DynamicCamera>();
        dyn.isFlipped = !dyn.isFlipped;

        HashSet<Rigidbody2D> rigids = new HashSet<Rigidbody2D>();
        Utilities.FindChildRigidBodies(gameObject, ref rigids,99);

        FixOverParentTranslation(rigids);

        HashSet<Rigidbody2D> justLimbs = new HashSet<Rigidbody2D>();
        Utilities.FindChildRigidBodies(gameObject, ref justLimbs, 1);

        Vector3 meanVec = Vector3.zero;
        foreach(Rigidbody2D rigid in justLimbs)
        {
            foreach(HingeJoint2D joint in rigid.gameObject.GetComponents<HingeJoint2D>())
            {
                joint.limits = new JointAngleLimits2D { max = -joint.limits.min, min = -joint.limits.max };
            }
            meanVec += gameObject.transform.position-rigid.gameObject.transform.position;
        }

        meanVec /= rigids.Count;

        foreach (Rigidbody2D rigid in rigids)
        {
            rigid.bodyType = RigidbodyType2D.Static;

        }

        foreach (Rigidbody2D rigid in justLimbs)
        {
            rigid.gameObject.transform.localPosition += new Vector3(meanVec.x, 0f, 0f) * 2 * gameObject.transform.localScale.x;

        }

        var scale = gameObject.transform.localScale;
        scale.x *= -1f;
        gameObject.transform.localScale = scale;

        foreach (Rigidbody2D rigid in rigids)
        {
            rigid.bodyType = RigidbodyType2D.Dynamic;
        }

    }

    void UpdateCamera()
    {
        DynamicCamera dyn = cam.GetComponent<DynamicCamera>();
        if (dyn == null)
            return;

        Vector3 avg = Vector3.zero;
        int count = 0;
        foreach(var obj in objList)
        {
            if (obj == null)
                continue;
            avg += obj.transform.position;
            count++;
        }
        avg /= count;
        dyn.target = avg;
    }

    void Initialize()
    {

        if (DefinitionManager.definitions.blueprints == null)
            return;

        cam = Utilities.FindMainCamera();

        BodyPart limbdef;
        DefinitionManager.definitions.blueprintDict.TryGetValue(LoadedSubTypeID, out limbdef);
        if (limbdef != null)
        {
            GameObject limb = limbdef.CreateLimbUnity(null,-100,false,gameObject);
            limb.layer = gameObject.layer;
        }

        HashSet<Rigidbody2D> rigids = new HashSet<Rigidbody2D>();
        Utilities.FindChildRigidBodies(gameObject, ref rigids,1);

        foreach(Transform child in gameObject.transform)
        {
            child.localPosition = Vector3.zero;
        }

        FixOverParentTranslation(rigids);
        
        initd = true;
    }
}
