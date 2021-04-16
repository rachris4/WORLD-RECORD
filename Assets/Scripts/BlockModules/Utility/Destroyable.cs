using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destroyable : MonoBehaviour
{
    public System.Action<List<GameObject>> OnFragmentsGenerated;
    [SerializeField]
    public float health = 100;
    [SerializeField]
    public float threshold = 5f;
    public float hardness = 100f;
    public bool allowRuntimeFragmentation = true;
    public int extraPoints = 2;
    public int subshatterSteps = 0;
    [SerializeField]
    private bool boom = false;
    private Vector2 oldVelocity = Vector2.zero;

    public string fragmentLayer = "Scrap";
    public string sortingLayerName = "Scrap";
    public int orderInLayer = 10;
    public float fadeOut = 100;
    public float fuse = 1000;
    public enum ShatterType
    {
        Triangle,
        Voronoi
    };
    public ShatterType shatterType = ShatterType.Voronoi;
    public List<GameObject> fragments = new List<GameObject>();
    private List<List<Vector2>> polygons = new List<List<Vector2>>();
   
    public void Initialize(DestroyableDefinition def)
    {
        health = def.Health;
        extraPoints = def.BreakPoints;
        if (def.shatterType == "Voronoi")
            shatterType = ShatterType.Voronoi;
        else
            shatterType = ShatterType.Triangle;
        threshold = def.Threshold;
        hardness = def.Hardness;
        fuse = def.Fuse;
        fadeOut = def.fadeOut;
        generateFragments();

    }

    public void explode()
    {
        Vector2 vel = oldVelocity;
        if (gameObject.transform.parent != null)
        {
            if (gameObject.transform.parent.gameObject.GetComponent<Rigidbody2D>() != null)
                vel = gameObject.transform.parent.gameObject.GetComponent<Rigidbody2D>().velocity;
            gameObject.transform.parent = null;
        }
        if(gameObject.GetComponent<Rigidbody2D>() == null)
        {
            var body = gameObject.AddComponent<Rigidbody2D>();
            body.velocity = vel;
        }
        //if fragments were not created before runtime then create them now
        if (fragments.Count == 0 && allowRuntimeFragmentation)
        {
            generateFragments();
        }
        //otherwise unparent and activate them
        else
        {
            foreach (GameObject frag in fragments)
            {
                if (frag == null)
                    continue;
                frag.transform.parent = null;
                frag.SetActive(true);
            }
        }
        //if fragments exist destroy the original
        if (fragments.Count > 0)
        {
            Destroy(gameObject);
        }
    }
    /// <summary>
    /// Creates fragments and then disables them
    /// </summary>
    public void fragmentInEditor()
    {
        if (fragments.Count > 0)
        {
            deleteFragments();
        }
        generateFragments();
        setPolygonsForDrawing();
        foreach (GameObject frag in fragments)
        {
            frag.transform.parent = transform;
            frag.SetActive(false);
        }
    }
    public void deleteFragments()
    {
        foreach (GameObject frag in fragments)
        {
            if (Application.isEditor)
            {
                DestroyImmediate(frag);
            }
            else
            {
                Destroy(frag);
            }
        }
        fragments.Clear();
        polygons.Clear();
    }
    /// <summary>
    /// Turns Gameobject into multiple fragments
    /// </summary>
    private void generateFragments()
    {
        fragments = new List<GameObject>();
        switch (shatterType)
        {
            case ShatterType.Triangle:
                fragments = ExplosionScript.GenerateTriangularPieces(gameObject, extraPoints, subshatterSteps);
                break;
            case ShatterType.Voronoi:
                fragments = ExplosionScript.GenerateVoronoiPieces(gameObject, extraPoints, subshatterSteps);
                break;
            default:
                Debug.Log("invalid choice");
                break;
        }
        //sets additional aspects of the fragments
        foreach (GameObject p in fragments)
        {
            if (p != null)
            {
                p.layer = 8;//LayerMask.NameToLayer(fragmentLayer);
                //p.GetComponent<Renderer>().sortingLayerName = sortingLayerName;
                p.GetComponent<Renderer>().sortingOrder = orderInLayer;
            }
        }
    }
    private void setPolygonsForDrawing()
    {
        polygons.Clear();
        List<Vector2> polygon;

        foreach (GameObject frag in fragments)
        {
            polygon = new List<Vector2>();
            foreach (Vector2 point in frag.GetComponent<PolygonCollider2D>().points)
            {
                Vector2 offset = rotateAroundPivot((Vector2)frag.transform.position, (Vector2)transform.position, Quaternion.Inverse(transform.rotation)) - (Vector2)transform.position;
                offset.x /= transform.localScale.x;
                offset.y /= transform.localScale.y;
                polygon.Add(point + offset);
            }
            polygons.Add(polygon);
        }
    }
    private Vector2 rotateAroundPivot(Vector2 point, Vector2 pivot, Quaternion angle)
    {
        Vector2 dir = point - pivot;
        dir = angle * dir;
        point = dir + pivot;
        return point;
    }

     void OnCollisionEnter2D(Collision2D col)
    {
        //Debug.Log("Ouch!");
        if(col.relativeVelocity.magnitude > threshold)
        {
            health -= col.relativeVelocity.magnitude;
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        if (boom || health < 0)
            explode();

    }
}
