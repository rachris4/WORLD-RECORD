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
    }

    // Update is called once per physix
    private void FixedUpdate()
    {

        if(reInit)
        {
            ProjectileWeaponDefinition def;
            if(DefinitionManager.definitions.projectileWeaponDict.TryGetValue(gameObject.name, out def))
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

        bool keypressed = false;

        if (burstCount > 1)
        {
            if (Input.GetKeyDown(keybind) && ShotsQueued == 0)
            {
                keypressed = true;
                ShotsQueued = burstCount;
                Debug.Log("Shot Burst!");
            }
        }
        else
        {
            if (Input.GetKey(keybind) && ShotsQueued == 0)
            {
                keypressed = true;
                ShotsQueued = 1;
                Debug.Log("Shooting auto!!");
            }
        }

        if (ShotsQueued > 0)
        {
            Tick++;
            Debug.Log("Tick!");
        }

        if (Tick > 0 && Tick > 1000/rateOfFire && ShotsQueued > 0)
        {
            Shoot();
            ShotsQueued--;
            Tick = 0;
            Debug.Log((Tick % (1000 / rateOfFire)).ToString());
        }

    }

    public void Shoot()
    {
        var shot = new GameObject(projectile.SubTypeID);
        var proj = shot.AddComponent<Projectile>();
        proj.Initialize(projectile, weapon, gameObject);
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
}

