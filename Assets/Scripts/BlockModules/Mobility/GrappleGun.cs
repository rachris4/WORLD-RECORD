using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrappleGun : MonoBehaviour
{


    [SerializeField]
    public string keybind = "x";
    [SerializeField]
    public int delay = 100;
    [SerializeField]
    public float force = 3000f;
    [SerializeField]
    public float maxlength = 40f;

    public int Tick = 0;
    private bool canShoot = true;
    private GameObject hook;
    private Rigidbody2D hookbody;
    private DistanceJoint2D joint;
    private Rigidbody2D rigid;

    private void Start()
    {
        rigid = Utilities.FindRigidbody(gameObject);
    }

    private void Update()
    {
        if(hook != null && hookbody == null)
            hook.transform.localRotation = Quaternion.Euler(new Vector3(0f, 0f, 0f));
    }

    private void FixedUpdate()
    {

        if (hook == null)
        {
            InitializeFakeHook();
            canShoot = true;
            Tick = 0;
        }
        else if(hookbody != null)
        {
            Tick++;

            if(Tick > delay)
            {
                var dir = (gameObject.transform.position - hook.transform.position);
                var loc = dir;
                dir = Vector3.Normalize(dir);
                //hookbody.AddForce(dir * force/10f);
                joint.distance = joint.distance - 0.1f;
                if(loc.magnitude<1f || Tick > delay*10)
                {
                    Destroy(hook);
                    return;
                }
            }

            canShoot = false;

        }


        if (Input.GetKey(keybind) && canShoot)
        {
            ShootGrapple();
        }
    }

    public void InitializeFakeHook()
    {
        hook = new GameObject(gameObject.name + "_grapplehook");
        hook.transform.parent = gameObject.transform;
        var rend = hook.AddComponent<SpriteRenderer>();
        rend.sprite = Resources.Load<Sprite>("Sprites/grapple");
        if (gameObject.GetComponent<LimbUnity>() != null)
            rend.sortingOrder = 1 + gameObject.GetComponent<LimbUnity>().SortingOrder;
        else if (gameObject.transform.parent.GetComponent<LimbUnity>() != null)
            rend.sortingOrder = 1 + gameObject.transform.parent.GetComponent<LimbUnity>().SortingOrder;
        hook.transform.localPosition = Vector3.zero + new Vector3(1f, 0f, 0f) * gameObject.transform.localScale.x;
    }

    public void ShootGrapple()
    {
        hookbody = hook.AddComponent<Rigidbody2D>();
        hook.layer = gameObject.layer;
        hook.transform.parent = null;
        var gh = hook.AddComponent<GrappleHook>();
        hook.AddComponent<BoxCollider2D>();
        hookbody.AddForce(Utilities.RealRotation(hook) * new Vector2(1f, 0f) * force);
        joint = hook.AddComponent<DistanceJoint2D>();
        joint.maxDistanceOnly = true;
        joint.autoConfigureDistance = false;
        joint.distance = maxlength;
        joint.connectedBody = rigid;
        joint.anchor = new Vector2(-0.5f, 0f);
        joint.connectedAnchor = gameObject.transform.localPosition+Utilities.RealRotation(gameObject)*new Vector2(0.5f,0f);
        gh.rope = joint;
        gh.shooter = this;

        var rope = hook.AddComponent<RopeRendererBezier>();
        rope.Initialize(gameObject, hook);
        rope.steps = 9f;
        rope.maxLength = maxlength;

    }

}

public class GrappleHook : MonoBehaviour
{
    private Rigidbody2D rigid;
    public DistanceJoint2D rope;
    public GrappleGun shooter;
    private Rigidbody2D connectedRigid;

    private void Start()
    {
        rigid = Utilities.FindRigidbody(gameObject);
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        //Debug.Log("Ouch!");
        if(connectedRigid == null)
        {
            var joint = gameObject.AddComponent<FixedJoint2D>();
            joint.connectedBody = col.rigidbody;
            connectedRigid = col.rigidbody;
            rope.distance = 0;
            rope.autoConfigureDistance = true;
            if(shooter.Tick < shooter.delay)
                shooter.Tick = shooter.delay;
        }
        
    }

}
