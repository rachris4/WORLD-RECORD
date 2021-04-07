using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixedThruster : MonoBehaviour
{
    [SerializeField]
    public string keybind = "r";
    [SerializeField]
    public string toggle = "z";

    private float fuel = 1000;
    private Rigidbody2D rigid;
    [SerializeField]
    public float force = 1000f;
    private SpriteRenderer flare;
    private GameObject obj;
    private bool damp  =false;
    private PID pid;

    void Start()
    {
        rigid = Utilities.FindRigidbody(gameObject);
        obj = new GameObject(gameObject.name + "_thruster");
        obj.transform.parent = gameObject.transform;
        flare = obj.AddComponent<SpriteRenderer>();
        flare.sprite = Resources.Load<Sprite>("Sprites/flare");
        if(gameObject.GetComponent<LimbUnity>() != null)
            flare.sortingOrder = 1+gameObject.GetComponent<LimbUnity>().SortingOrder;
        else if (gameObject.transform.parent.GetComponent<LimbUnity>() != null)
            flare.sortingOrder = 1+gameObject.transform.parent.GetComponent<LimbUnity>().SortingOrder;
        flare.color = new Color(1f, 1f, 1f, 0f);
        obj.transform.localPosition = Vector3.zero + new Vector3(-1f, 0f,0f) * obj.transform.localScale.x;
        pid = new PID(20f, 1f, 1f);

    }

    public void UpdateRenderOrder()
    {

        var parentRend = gameObject.transform.parent.GetComponent<SpriteRenderer>();
        if (parentRend == null)
            return;
        var childRend = obj.GetComponent<SpriteRenderer>();
        childRend.sortingOrder = parentRend.sortingOrder + 1;
    }

    void Update()
    {
        UpdateRenderOrder();
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        obj.transform.localRotation = Quaternion.Euler(new Vector3(0f,0f,0f));

        if (Input.GetKeyDown(toggle))
        {
            damp = !damp;
            pid = new PID(20f, 1f, 1f);
        }

        if (Input.GetKey(keybind) && fuel > 0)
        {
            ApplyForce();
            fuel--;
        }
        else if(damp && fuel > 0)
        {
            ApplyDampingForce();
        }
        else if (fuel < 0)
        {
            Destroy(this);
            Destroy(obj);
        }
        else
            flare.color = new Color(1f, 1f, 1f, 0f);
    }

    public void ApplyForce()
    {
        rigid.AddForce(Utilities.RealRotation(gameObject) * new Vector2(1f, 0f)*force);
        flare.color = new Color(1f, 1f, 1f, Mathf.Sin(fuel+Random.value)+1f);
    }

    public void ApplyDampingForce()
    {
        
        Vector2 dir = Utilities.RealRotation(gameObject) * new Vector2(1f, 0f);
        float newforce = Vector2.Dot(rigid.velocity, dir);
        newforce = pid.Update(0f, newforce, Time.deltaTime)*rigid.mass;
        if (newforce > force)
            newforce = force;
        else if (newforce < 0)
            newforce = 0;

        Debug.Log(newforce);

        rigid.AddForce(Utilities.RealRotation(gameObject) * new Vector2(1f, 0f) * newforce);
        flare.color = new Color(1f, 1f, 1f, (Mathf.Sin(fuel + Random.value) + 1f)*newforce/force);

        fuel -= newforce / force;

    }

}
