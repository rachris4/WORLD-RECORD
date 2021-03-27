using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;
using System.Xml;
using System.IO;

public class Projectile : MonoBehaviour
{
    [SerializeField]
    public int Fuse;
    [SerializeField]
    public int TimeAlive = 0;
    [SerializeField]
    public bool MouseGuided;
    [SerializeField]
    public Vector2 Acceleration;
    [SerializeField]
    public float rotationSpeed;

    private Rigidbody2D body;

    public void Initialize(ProjectileDefinition def, ProjectileWeaponDefinition wepdef, GameObject source)
    {

        body = gameObject.AddComponent<Rigidbody2D>();
        gameObject.AddComponent<BoxCollider2D>();
        gameObject.layer = 13;
        rotationSpeed = def.rotationSpeed;

        var rend = gameObject.AddComponent<SpriteRenderer>();
        rend.sprite = Resources.Load<Sprite>(def.pathSprite);

        gameObject.transform.parent = source.transform;
        Acceleration = def.Acceleration.ToVector2() * wepdef.SpeedMult;
        gameObject.transform.rotation = source.transform.rotation;
        body.velocity = gameObject.transform.rotation * def.Velocity.ToVector3() * wepdef.SpeedMult; //gameObject.transform.rotation * 
        gameObject.transform.localPosition = wepdef.barrelVector.ToVector3(); //gameObject.transform.rotation*
        gameObject.transform.localScale = source.transform.localScale;

        body.gravityScale = def.GravityScale;
        body.drag = def.LinearDrag;
        body.angularDrag = def.AngularDrag;
        MouseGuided = def.mouseGuidance;

        if (def.fuse > 0)
        {
            Fuse = (int)(def.fuse*wepdef.RangeMult);
        }

        if (def.destructionProperties != null)
        {
            var healthobject = gameObject.AddComponent<Destroyable>();
            //Debug.Log(otherSquare.destructionProperties.Threshold.ToString());
            healthobject.Initialize(def.destructionProperties);
        }

        gameObject.transform.parent = null;
    }

    private void FixedUpdate()
    {
        if (TimeAlive > Fuse && Fuse > 0)
        {
            var healthobject = gameObject.GetComponent<Destroyable>();
            if (healthobject != null)
                healthobject.explode();
        }

        if(MouseGuided)
        {
            Vector3 loc = Utilities.GetWorldPositionOnPlane(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f), 0f);
            Vector3 dir = (loc - gameObject.transform.position);
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            gameObject.transform.rotation = Quaternion.RotateTowards(gameObject.transform.rotation, Quaternion.AngleAxis(angle, Vector3.forward), rotationSpeed * 25 * Time.deltaTime);
        }

        if (Acceleration != Vector2.zero)
        {
            body.AddRelativeForce(Acceleration);
        }
        else if(!MouseGuided && body.velocity.magnitude > 0.3)
        {
            float angle = Mathf.Atan2(body.velocity.y, body.velocity.x) * Mathf.Rad2Deg;
            gameObject.transform.rotation = Quaternion.RotateTowards(gameObject.transform.rotation, Quaternion.AngleAxis(angle, Vector3.forward),rotationSpeed*25*Time.deltaTime);
        }

        TimeAlive++;
    }
}

public class ProjectileDefinition : DefinitionBase
{
    [XmlElement("pathSprite")]
    public string pathSprite;
    [XmlElement("isSolid")]
    public bool isSolid;
    [XmlElement("hitbox")]
    public string collider; //box, triangle, toptwoslope, bottwoslope
    [XmlElement("DestructionProperties")]
    public DestroyableDefinition destructionProperties;
    [XmlElement("Velocity")] //
    public SerializableVector2 Velocity; //vector 2 relative to barrel, x+ is barrel direction
    [XmlElement("RotationSpeed")] //
    public float rotationSpeed; //vector 2 relative to barrel, x+ is barrel direction
    [XmlElement("Acceleration")]
    public SerializableVector2 Acceleration; ///vector 2 relative to projectile, x+ is forward
    [XmlElement("Fuse")]
    public int fuse; //how many fixedupdates to boom
    [XmlArray("Submunitions")]
    [XmlArrayItem("Submunition", typeof(SubmunitionDefinition))]
    public SubmunitionDefinition[] SubmunitionDefinitions;
    [XmlElement("MouseGuidance")]
    public bool mouseGuidance; //how many fixedupdates to boom
    [XmlElement("LinearDrag")]
    public float LinearDrag; // drag
    [XmlElement("AngularDrag")]
    public float AngularDrag; // drag
    [XmlElement("GravityScale")]
    public float GravityScale; // gravity
}

public class SubmunitionDefinition
{
    [XmlAttribute("ProjectileSubTypeID")]
    public string SubTypeID; //how many fixedupdates to boom
}