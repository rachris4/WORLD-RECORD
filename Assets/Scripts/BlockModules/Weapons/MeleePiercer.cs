using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleePiercer : MonoBehaviour
{

    [SerializeField]
    public float maxDamage = 100f;
    [SerializeField]
    public int maxHits = 2;
    [SerializeField]
    public float forcePerCut = 40f;
    [SerializeField]
    public string Type = "blade";

    private Rigidbody2D rigid;
    private Collider2D box;
    private HashSet<Collider2D> cutList = new HashSet<Collider2D>();
    private HashSet<Vector3> resistList = new HashSet<Vector3>();

    private float dmgPool;
    private float cutCount = 0;
    private int hardnesses = 0;

    public void Initialize(MeleeProperties meleeDef)
    {
        maxDamage = meleeDef.TotalDamage;
        maxHits = (int)meleeDef.TotalHits;
        forcePerCut = meleeDef.ForcePerImpale;
        Type = meleeDef.Type;
    }

    private void Start()
    {
        rigid = Utilities.FindRigidbody(gameObject);
        box = gameObject.GetComponent<BoxCollider2D>() as Collider2D;
        box.isTrigger = true;
        dmgPool = maxDamage;
    }

    void ResetMeleeCollider()
    {
        box.isTrigger = true;
        foreach(Collider2D col in cutList)
        {
            if (col == null)
                continue;
            Physics2D.IgnoreCollision(col, box, false);
        }
        cutList.Clear();
        dmgPool = maxDamage;
        cutCount = 0;
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        var destroy = col.gameObject.GetComponent<Destroyable>();
        if (destroy == null)
            return;
        resistList.Add(col.gameObject.transform.position);
        hardnesses += (int)destroy.hardness;
    }

    void OnTriggerEnter2D(Collider2D col)
    {

        if (cutList.Count >= maxHits || dmgPool <= 1)
        {
            box.isTrigger = false;
            return;
        }

        if (cutList.Contains(col))
            return;
        else
            cutList.Add(col);

        Physics2D.IgnoreCollision(col, box, true);

        Destroyable target = col.gameObject.GetComponent<Destroyable>();
        if (target != null)
        {
            
            float mod = 1f;
            if (Type == "spike")
            {
                mod += Mathf.Abs((rigid.angularVelocity-col.attachedRigidbody.angularVelocity)) / 50;
                var dir = Utilities.RealRotation(gameObject) * new Vector3(1f, 0f, 0f);
                var comp = Vector3.Dot(dir, Vector3.Normalize(rigid.velocity));
                var noSpin = (2 - comp)*1.5f-1f;
                if (noSpin > 5)
                    noSpin = 5;
                Debug.Log(noSpin.ToString());
                mod *= (maxHits / (cutCount * 2));
                cutCount += noSpin;
            }

            target.health -= maxDamage*target.hardness / (maxHits*mod);
            dmgPool -= maxDamage * target.hardness / (maxHits*mod);
            cutCount += target.hardness;
        }

    }

    void FixedUpdate()
    {

       // Debug.Log(cutList.Count.ToString() + " / " + dmgPool.ToString());

        if (box.isTrigger && (cutCount >= maxHits || dmgPool <= 0))
        {
            box.isTrigger = false;
        }

        if (cutCount == 0)
        {
            return;
        }

        ContactFilter2D filter = new ContactFilter2D();
        filter.useTriggers = false;
        filter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
        filter.useLayerMask = true;
        List<Collider2D> collisions = new List<Collider2D>();

        Physics2D.OverlapCollider(box, filter, collisions);


        foreach (Collider2D col in collisions)
        {
            if(cutList.Contains(col))
            {
                resistList.Add(col.gameObject.transform.position);
            }
        }

        if (resistList.Count == 0)
        {
            ResetMeleeCollider();
            return;

        }

        Vector3 force = Vector2.zero;
        hardnesses /= resistList.Count;
        int kk = 0;
        foreach (Vector3 vec in resistList)
        {
            if (kk > 10)
                break;
            force += Vector3.Normalize(gameObject.transform.position - vec) * forcePerCut;
            kk++;
        }
        rigid.AddForce(force*hardnesses);

        rigid.AddTorque(-rigid.angularVelocity * force.magnitude*hardnesses * forcePerCut / 500);

        resistList.Clear();
    }
}
