using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Random = System.Random;

public class ProjectileTypeManager : MonoBehaviour
{
    [Header("Data")]
    //public ConquestBattleConfiguration Config;
    public Mesh mesh;
    public Material material;
    public ProjectileDefinition def;

    [Header("Instances")]
    public List<ProjectileData> projectiles = new List<ProjectileData>();

    //Working values
    private RaycastHit2D[] rayHitBuffer = new RaycastHit2D[1];
    private Vector3 worldPoint;
    private Vector3 transPoint;
    private List<Matrix4x4[]> bufferedData = new List<Matrix4x4[]>();
    private bool initialized;

    private void Initialize()
    {
        material = new Material(material); //Shader.Find("Standard")
        material.enableInstancing = true;
        material.mainTexture = Resources.Load(def.pathSprite) as Texture;
        mesh = new Mesh();
        Utilities.CreatePrimitiveQuad(ref mesh);
        initialized = true;
    }

    private void FixedUpdate()
    {
        if (!initialized && def != null)
        {
            Initialize();
            Debug.Log("initititit!");
        }
        else if (!initialized)
            return;
        UpdateProjectiles(Time.deltaTime); //Time.deltaTime
        //UpdatePhysicsProjectiles(Time.deltaTime);
    }

    private void Update()
    {
        if (!initialized)
            return;
        BatchAndRender();
    }

    private void BatchAndRender()
    {
        //If we dont have projectiles to render then just get out
        if (projectiles.Count <= 0)
            return;

        //Clear the batch buffer
        bufferedData.Clear();

        //If we can fit all in 1 batch then do so
        if (projectiles.Count < 1023)
            bufferedData.Add(projectiles.Select(p => p.renderData).ToArray());
        else
        {
            //We need multiple batches
            int count = projectiles.Count;
            for (int i = 0; i < count; i += 1023)
            {
                if (i + 1023 < count)
                {
                    Matrix4x4[] tBuffer = new Matrix4x4[1023];
                    for (int ii = 0; ii < 1023; ii++)
                    {
                        tBuffer[ii] = projectiles[i + ii].renderData;
                    }
                    bufferedData.Add(tBuffer);
                }
                else
                {
                    //last batch
                    Matrix4x4[] tBuffer = new Matrix4x4[count - i];
                    for (int ii = 0; ii < count - i; ii++)
                    {
                        tBuffer[ii] = projectiles[i + ii].renderData;
                    }
                    bufferedData.Add(tBuffer);
                }
            }
        }

        //Draw each batch
        foreach (var batch in bufferedData)
            Graphics.DrawMeshInstanced(mesh, 0, material, batch, batch.Length,null,UnityEngine.Rendering.ShadowCastingMode.Off,false,31);
    }

    private void UpdateProjectiles(float tick)
    {
        foreach (var projectile in projectiles)
        {
            projectile.expiration -= tick;

            if (projectile.expiration > 0)
            {
                //Sort out the projectiles 'forward' direction
                transPoint = projectile.rot * new Vector3(1f, 0f, 0f);
                //See if its going to hit something and if so handle that
                if (Physics2D.RaycastNonAlloc(projectile.pos, transPoint, rayHitBuffer, projectile.speed.magnitude * tick, projectile.team) > 0)
                {
                    worldPoint = rayHitBuffer[0].point;
                    SpawnSplash(worldPoint);
                    rayHitBuffer[0].rigidbody?.AddForce(Vector3.Normalize(projectile.speed) * def.Impulse);
                    Destroyable target = rayHitBuffer[0].collider.GetComponent<Destroyable>();
                    if (target != null)
                    {
                        if (projectile.damage > target.health)
                        {
                            int hits = 1;
                            float health = target.health;
                            float damage = projectile.damage;

                            projectile.damage -= target.health;
                            target.health = -1;

                            var rca = Physics2D.RaycastAll(worldPoint, transPoint, projectile.speed.magnitude * tick, projectile.team);
                            foreach (RaycastHit2D hit in rca)
                            {
                                hits++;
                                target = hit.collider.GetComponent<Destroyable>();
                                hit.rigidbody?.AddForce(Vector3.Normalize(projectile.speed) * def.Impulse);
                                if (target == null)
                                    continue;
                                if (projectile.damage > target.health)
                                {
                                    health += target.health;
                                    projectile.damage -= target.health;
                                    target.health = -1;

                                }
                                else
                                {
                                    target.health -= projectile.damage;
                                    projectile.expiration = -1;
                                    break;
                                }
                            }
                            projectile.speed -= Vector3.Normalize(projectile.speed) * def.Impulse * hits * health / damage / 500f;
                        }
                        else
                        {
                            target.health -= projectile.damage;
                            projectile.expiration = -1;
                        }
                    }
                }
                //This project wont be hitting anything this tick so just move it forward
                if(projectile.GravityScale != 0 || projectile.accel != Vector3.zero)
                {
                    projectile.speed += new Vector3(0f, -1f, 0f)*projectile.GravityScale*9.81f*tick;
                    projectile.speed += projectile.rot * projectile.accel*tick;
                    projectile.pos += projectile.speed * tick;
                    float angle = Mathf.Atan2(projectile.speed.y, projectile.speed.x) * Mathf.Rad2Deg;
                    projectile.rot = Quaternion.RotateTowards(projectile.rot, Quaternion.AngleAxis(angle, Vector3.forward), 25 * Time.deltaTime);
                }
                else
                    projectile.pos += projectile.speed * tick;
                
            }
        }
        //Remove all the projectiles that have hit there expiration, can happen due to time or impact
        projectiles.RemoveAll(p => p.expiration <= 0);
    }

    private void SpawnSplash(Vector3 worldPoint)
    {
        //TODO: implament spawning of your splash effect e.g. the visual effect of a projectile hitting something
    }

}

public class ProjectileManager : MonoBehaviour
{
    [Header("Data")]
    //public ConquestBattleConfiguration Config;
    public LayerMask attackerMask;
    public LayerMask defenderMask;
    public Material seedMaterial;

    [Header("Instances")]
    private Dictionary<string, ProjectileTypeManager> ammoDict = new Dictionary<string, ProjectileTypeManager>();

    [Header("RNG-Determinism")]
    public static sbyte index = 0;
    public const int Seed = 5366354;
    public static float[] RandomSet;

    private bool initializedAmmos;

    private void InitializeAmmoTypes()
    {
        foreach(KeyValuePair<string, ProjectileDefinition> pair in DefinitionManager.definitions.projectileDict)
        {
            GameObject obj = new GameObject(pair.Key + "_manager");
            obj.transform.parent = gameObject.transform;
            ProjectileTypeManager manager = obj.AddComponent<ProjectileTypeManager>();
            manager.def = pair.Value;
            manager.material = seedMaterial;
            ammoDict.Add(pair.Key, manager);
        }
        initializedAmmos = true;
    }

    private void FixedUpdate()
    {
        if (!initializedAmmos && DefinitionManager.definitions.projectileDefinitions.Count != 0)
        {
            InitializeAmmoTypes();
        }
        else if (!initializedAmmos)
            return;
    }

    public void SpawnRaycasterProjectile(ProjectileDefinition def, Vector3 position, Quaternion rotation, int layer, ProjectileWeaponDefinition wep = null)
    {
        ProjectileData n = new ProjectileData();
        //n = def as ProjectileData;


        if (wep != null)
            n.rot = ApplyDeviation(rotation, wep.deviation);
        else
            n.rot = rotation;

        n.scale = Vector3.one;
        n.speed = n.rot * Vector3.Normalize(def.Velocity.ToVector3()) * ApplySpeedDeviation(def.Velocity.ToVector3().magnitude, 0.5f);
        n.scale.x += n.speed.magnitude / 100;
        n.pos = position+new Vector3(0f,0f,0.01f) + n.speed * Time.deltaTime;
        n.expiration = def.fuse;

        if (attackerMask == (attackerMask | (1 << layer)))
            n.team = defenderMask;
        else
            n.team = attackerMask;

        n.damage = def.Damage;
        n.GravityScale = def.GravityScale;
        n.accel = def.Acceleration.ToVector3();

        if (wep != null)
        {
            n.damage *= wep.DamageMult;
            n.expiration *= wep.RangeMult;
            n.speed *= wep.SpeedMult;
        }

        ProjectileTypeManager manager;
        ammoDict.TryGetValue(def.SubTypeID, out manager);

        if(manager != null)
            manager.projectiles.Add(n);
    }

    public void SpawnUnityProjectile(Vector3 position, Quaternion rotation, int team, float damageScale)
    {

    }

    public static float ApplySpeedDeviation(float speed, float deviation)
    {


        speed *= (1 + (RandomSet[index] - 0.5f) * deviation);

        return speed;
    }

    public static Quaternion ApplyDeviation(Quaternion direction, float maxAngle)
    {
        if (maxAngle == 0)
            return direction;

        if (RandomSet == null)
        {
            RandomSet = new float[128];

            Random rand = new Random(Seed);

            for (int i = 0; i < 128; i++)
            {
                RandomSet[i] = (float)(rand.NextDouble());
            }
        }

        if (index == 127)
        {
            index = 0;
        }
        else
        {
            index++;
        }

        Vector3 eulerAngs = direction.eulerAngles;
        float rotz = eulerAngs.z;

        rotz += (RandomSet[index] - 0.5f) * maxAngle;

        return Quaternion.Euler(eulerAngs.x, eulerAngs.y, rotz);
    }
}

public class ProjectileData
{
    public Vector3 pos;
    public Quaternion rot;
    public Vector3 scale;
    public float expiration;
    public LayerMask team;
    public float damage;
    public Vector3 speed;
    public float GravityScale;
    public Vector3 accel;

    public Matrix4x4 renderData
    {
        get
        {
            return Matrix4x4.TRS(pos, rot, scale);
        }
    }
    /*
    public void UpdateRenderData()
    {

    }*/
}
