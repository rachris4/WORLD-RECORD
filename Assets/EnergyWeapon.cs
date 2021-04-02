using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;
using System.Xml;
using System.IO;

public class EnergyWeapon : MonoBehaviour
{
    private EnergyWeaponDefinition weapon;
    private Rigidbody2D rigid;
    private LineRenderer line;

    [SerializeField]
    private int Tick = 0;
    [SerializeField]
    private int ShotsQueued;
    [SerializeField]
    private LayerMask mask;
    [SerializeField]
    public string keybind = "g";

    private ProjectileManager manager;


    public void Start()
    {
        line = gameObject.AddComponent<LineRenderer>();
        line.enabled = false;
        Material newMat = Resources.Load<Material>("mat");
        line.material = newMat;
        manager = Utilities.FindGameManager().GetComponent<ProjectileManager>();
    }

    public void Initialize(EnergyWeaponDefinition wepdef)
    {
        weapon = wepdef;
        rigid = Utilities.FindRigidbody(gameObject);

        if (rigid == null)
        {
            rigid = gameObject.transform.parent?.GetComponent<Rigidbody2D>();
        }
    }

    // Update is called once per physix
    private void FixedUpdate()
    {

        bool keypressed = false;

        if (Input.GetKey(keybind) && ShotsQueued == 0)
        {
            keypressed = true;
            ShotsQueued = weapon.BeamTime;
        }
        
        if (Tick < weapon.ChargeTime)
        {
            Tick++;
            line.enabled = false;
        }
        else if(ShotsQueued > 0)
        {

            Vector3 pos = gameObject.transform.position + Utilities.RealRotation(gameObject) * weapon.barrelVector.ToVector3();
            Shoot(pos);
            if (rigid != null)
            {
                rigid.AddForce(Utilities.RealRotation(gameObject) * new Vector2(-1f, 0f) * weapon.KnockBackForce);
            }
            ShotsQueued--;
            if (ShotsQueued==0)
            {
                Tick = 0;
            }
        }

    }

    public void Shoot(Vector3 pos)
    {

        line.enabled = true;
        line.SetPosition(0, pos);
        line.startWidth = weapon.LineWidth;
        line.endWidth = weapon.LineWidth;
        Vector3 dir = Utilities.RealRotation(gameObject) * new Vector3(1f, 0f, 0f);
        RaycastHit2D raycastHit = Physics2D.Raycast(pos, dir, weapon.Range, manager.mask);
        if (raycastHit)
        {
            line.SetPosition(1, raycastHit.point);
            Destroyable target = raycastHit.rigidbody?.GetComponent<Destroyable>();
            if (target != null)
            {
                target.health -= weapon.Damage;
            }
        }

        else
        {
            line.SetPosition(1, pos+ dir*weapon.Range);
        }
    }
}



public class EnergyWeaponDefinition : DefinitionBase
{
    [XmlElement("RateOfFire")]
    public int rateOfFire; //how many shots per 1000 fixed updates? fix later
    [XmlElement("ChargeTime")]
    public int ChargeTime; //how many fixedupdates to reload
    [XmlElement("BeamTime")]
    public int BeamTime; //how many fixedupdates to reload
    [XmlElement("DeviationCone")]
    public float DeviationCone; //degrees, buckshot's worst nightmare
    [XmlElement("Damage")]
    public float Damage; //buckshot's worst nightmare
    [XmlElement("Range")]
    public float Range; //buckshot's worst nightmare
    [XmlElement("BarrelDisplacement")]
    public SerializableVector2 barrelVector; //for aesthetics and cheese
    [XmlElement("KnockBackForce")]
    public float KnockBackForce; //for aesthetics and cheese
    [XmlElement("LineWidth")]
    public float LineWidth; //for aesthetics and cheese
}

