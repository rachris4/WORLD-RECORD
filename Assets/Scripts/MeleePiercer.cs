using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleePiercer : MonoBehaviour
{

    [SerializeField]
    public float maxDamage = 10000f;
    [SerializeField]
    public int maxHits = 5;
    [SerializeField]
    public float forcePerCut = 20f;

    private Rigidbody2D rigid;
    private Collider2D box;
    private HashSet<Collider2D> cutList = new HashSet<Collider2D>();
    private HashSet<Vector3> resistList = new HashSet<Vector3>();

    private float dmgPool;
    private int cutCount = 0;

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
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        //if (box.isTrigger == true)
        //    return;

        //resistList.Add(col.gameObject.transform.position);

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
            target.health -= maxDamage/maxHits;
            dmgPool -= maxDamage / maxHits;
        }

    }

    void FixedUpdate()
    {

       // Debug.Log(cutList.Count.ToString() + " / " + dmgPool.ToString());

        if (box.isTrigger && (cutList.Count >= maxHits || dmgPool <= 0))
        {
            box.isTrigger = false;
        }

        if (cutList.Count == 0)
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

        foreach (Vector3 vec in resistList)
        {
            force += Vector3.Normalize(gameObject.transform.position - vec) * forcePerCut;
        }
        rigid.AddForce(force);

        resistList.Clear();
    }
}
