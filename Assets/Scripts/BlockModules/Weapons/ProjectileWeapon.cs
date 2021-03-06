using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;
using System.Xml;
using System.IO;

public class ProjectileWeapon : MonoBehaviour
{
    private ProjectileDefinition projectile;
    private ProjectileWeaponDefinition weapon;
    private Rigidbody2D rigid;
    private bool reloading;

    [SerializeField]
    private int Tick = 0;
    [SerializeField]
    private int ShotsQueued;

    [SerializeField]
    public string keybind = "g";

    [SerializeField]
    private bool reInit;
    [SerializeField]
    private string projectileSubTypeID;
    [SerializeField]
    private int rateOfFire = 1; //how many shots per 1000 fixed updates? fix later
    [SerializeField]
    private int reload = 0; //how many fixedupdates to reload
    [SerializeField]
    private int burstCount = 1; //how many shots shot on click, at rate of fire
    [SerializeField]
    private int projectileCount = 1; //old friend of mine, buckshot
    [SerializeField]
    private float deviation = 0; //degrees, buckshot's worst nightmare
    [SerializeField]
    private float DamageMult = 1f; //buckshot's worst nightmare
    [SerializeField]
    private float SpeedMult = 1f; //buckshot's worst nightmare
    [SerializeField]
    private float RangeMult = 1f; //buckshot's worst nightmare
    [SerializeField]
    private Vector2 barrelVector = new Vector2(1f,0f); //for aesthetics and cheese

    private ProjectileManager manager;

    public void Start()
    {
        manager = Utilities.FindGameManager().GetComponent<ProjectileManager>();
    }

    public void Initialize(ProjectileWeaponDefinition wepdef)
    {
        weapon = wepdef;
        projectileSubTypeID = wepdef.projectileSubTypeID;
        rateOfFire = wepdef.rateOfFire;
        reload = wepdef.reload;
        burstCount = wepdef.burstCount;
        projectileCount = wepdef.projectileCount;
        deviation = wepdef.deviation;
        DamageMult = wepdef.DamageMult;
        SpeedMult = wepdef.SpeedMult;
        RangeMult = wepdef.RangeMult;
        barrelVector = wepdef.barrelVector.ToVector2();
        DefinitionManager.definitions.projectileDict.TryGetValue(projectileSubTypeID, out projectile);
        rigid = Utilities.FindRigidbody(gameObject);
    }

    // Update is called once per physix
    private void FixedUpdate()
    {
        if (manager == null || projectile == null)
            return;

        ManageShooting();

    }

    public void ManageShooting()
    {
        if (reInit)
        {
            ProjectileWeaponDefinition def;
            if (DefinitionManager.definitions.projectileWeaponDict.TryGetValue(gameObject.name, out def))
            {
                Initialize(def);
                Debug.Log("Reloaded Definition!");
            }
            else
            {
                Debug.Log("failed to reload Definition!");
            }
            reInit = false;
        }

        if(!reloading)
        {
            if (burstCount > 1)
            {
                if (Input.GetKey(keybind) && ShotsQueued == 0)
                {
                    ShotsQueued = burstCount;
                    //Debug.Log("Shot Burst!");
                }
            }
            else
            {
                if (Input.GetKey(keybind) && ShotsQueued == 0)
                {
                    ShotsQueued = 1;
                    //Debug.Log("Shooting auto!!");
                }
            }
        }
        

        if (!reloading && ShotsQueued > 0)
        {

            float shotsPerTick = Mathf.Round(1 / (1000f / rateOfFire));
            Vector3 fwd = gameObject.transform.position + Utilities.RealRotation(gameObject) * weapon.barrelVector.ToVector3();
            if (shotsPerTick > 1)
            {
                for (int j = 0; j < weapon.projectileCount; j++)
                {
                    for (int i = 0; i < shotsPerTick; i++)
                    {
                        fwd += Utilities.RealRotation(gameObject) * projectile.Velocity.ToVector3() * SpeedMult * Time.deltaTime * (i + 1) / shotsPerTick;
                        Shoot(fwd);
                    }
                }
                ShotsQueued = 0;

            }
            else
            {
                for (int j = 0; j < weapon.projectileCount; j++)
                {
                    Shoot(fwd);
                }
                ShotsQueued--;
            }
            if (rigid != null)
            {
                rigid.AddForce(Utilities.RealRotation(gameObject) * new Vector2(-1f, 0f) * weapon.KnockBackForce);
            }
            reloading = true;
            //Debug.Log((Tick % (1000 / rateOfFire)).ToString());
        }
        else if(reloading)
        {
            Tick++;
            if (Tick > 1000 / rateOfFire)
            {
                Tick = 0;
                reloading = false;
            }
        }
    }

    public void Shoot(Vector3 forward)
    {
        manager.SpawnRaycasterProjectile(projectile,forward, Utilities.RealRotation(gameObject), gameObject.layer, weapon);
        /*
        var shot = new GameObject(projectile.SubTypeID);
        var proj = shot.AddComponent<PhysicsProjectile>();
        proj.Initialize(projectile, weapon, gameObject);*/
    }
}



public class ProjectileWeaponDefinition : DefinitionBase
{
    [XmlElement("Projectile")]
    public string projectileSubTypeID;
    [XmlElement("RateOfFire")]
    public int rateOfFire; //how many shots per 1000 fixed updates? fix later
    [XmlElement("Reload")]
    public int reload; //how many fixedupdates to reload
    [XmlElement("ShotsInBurst")]
    public int burstCount; //how many shots shot on click, at rate of fire
    [XmlElement("ProjectileCount")]
    public int projectileCount; //old friend of mine, buckshot
    [XmlElement("Deviation")]
    public float deviation; //degrees, buckshot's worst nightmare
    [XmlElement("DamageMult")]
    public float DamageMult; //buckshot's worst nightmare
    [XmlElement("SpeedMult")]
    public float SpeedMult; //buckshot's worst nightmare
    [XmlElement("RangeMult")]
    public float RangeMult; //buckshot's worst nightmare
    [XmlElement("BarrelDisplacement")]
    public SerializableVector2 barrelVector; //for aesthetics and cheese
    [XmlElement("KnockBackForce")]
    public float KnockBackForce; //for aesthetics and cheese
}

