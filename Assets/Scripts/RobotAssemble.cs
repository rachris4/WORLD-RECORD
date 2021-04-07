using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotAssemble : MonoBehaviour
{
    [SerializeField]
    private string LoadedSubTypeID = "HumanTorso";
    [SerializeField]
    private bool attachCamera = false;

    private bool initd = false;
    private GameObject cam;

    private int tick = 0;

    [HideInInspector]
    public List<BodyPart> bpList = new List<BodyPart>();
    public List<GameObject> objList = new List<GameObject>();
    private bool alienUpdated = false;

    void Update()
    {
        if (!initd)
            Initialize();
        
        if(tick > 10 && bpList.Count > 0 && !alienUpdated)
        {
            for(int i = 0; i < bpList.Count; i++)
            {
                bpList[i].InitializeAlienParts(objList[i]);
            }

            alienUpdated = true;
        }

        if(Input.GetKeyDown("f"))
        {
            FlipChassis();
        }

        tick++;
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


    void Initialize()
    {

        if (DefinitionManager.definitions.blueprints == null)
            return;

        BodyPart limbdef;
        DefinitionManager.definitions.blueprintDict.TryGetValue(LoadedSubTypeID, out limbdef);
        if (limbdef != null)
        {
            GameObject limb = limbdef.CreateLimbUnity(null,0,false,gameObject);
        }

        HashSet<Rigidbody2D> rigids = new HashSet<Rigidbody2D>();
        Utilities.FindChildRigidBodies(gameObject, ref rigids);
        FixOverParentTranslation(rigids);

        initd = true;
    }
}
