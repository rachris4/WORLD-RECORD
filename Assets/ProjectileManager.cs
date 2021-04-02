using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Random = System.Random;

public class ProjectileManager : MonoBehaviour
{
    [Header("Data")]
    //public ConquestBattleConfiguration Config;
    public Mesh mesh;
    public Material material;
    public float life;
    public float speed;
    public float damage;
    public LayerMask mask;

    [Header("Instances")]
    public List<ProjectileData> projectiles = new List<ProjectileData>();
    public List<PhysicsProjectile> physicsProjectiles = new List<PhysicsProjectile>();
    public List<GameObject> splashPool = new List<GameObject>();

    //Working values
    private RaycastHit2D[] rayHitBuffer = new RaycastHit2D[1];
    private Vector3 worldPoint;
    private Vector3 transPoint;
    private List<Matrix4x4[]> bufferedData = new List<Matrix4x4[]>();

    private sbyte index = 0;
    private const int Seed = 5366354;
    private static float[] RandomSet;

    public static float ApplySpeedDeviation(float speed, float deviation, sbyte index)
    {


        speed *= (1+(RandomSet[index] - 0.5f) * deviation);

        return speed;
    }

    public static Quaternion ApplyDeviation(Quaternion direction, float maxAngle, ref sbyte index)
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

        rotz += (RandomSet[index]-0.5f) * maxAngle;

        return Quaternion.Euler(eulerAngs.x, eulerAngs.y, rotz);
    }

    public void SpawnRaycasterProjectile(ProjectileWeaponDefinition wep, ProjectileDefinition def, Vector3 position, Quaternion rotation, int team, float damageScale)
    {
        ProjectileData n = new ProjectileData();
        //n = def as ProjectileData;

        n.rot = ApplyDeviation(rotation,wep.deviation,ref index);
        n.scale = Vector3.one;
        n.speed = n.rot* Vector3.Normalize(def.Velocity.ToVector3())* ApplySpeedDeviation(def.Velocity.ToVector3().magnitude, 0.5f, index);
        n.scale.x += speed/100;
        n.pos = position + n.speed*Time.deltaTime;
        n.expiration = def.fuse;
        n.team = team;
        n.damage = damage;
        n.GravityScale = def.GravityScale;
        n.damageScale = damageScale;
        n.accel = def.Acceleration.ToVector3();
        projectiles.Add(n);
    }

    public void SpawnUnityProjectile(Vector3 position, Quaternion rotation, int team, float damageScale)
    {

    }

    private void FixedUpdate()
    {
        UpdateProjectiles(Time.deltaTime); //Time.deltaTime
        //UpdatePhysicsProjectiles(Time.deltaTime);
    }

    private void Update()
    {
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
                if (Physics2D.RaycastNonAlloc(projectile.pos, transPoint, rayHitBuffer, speed * tick, mask) > 0)
                {
                    projectile.expiration = -1;
                    worldPoint = rayHitBuffer[0].point;
                    SpawnSplash(worldPoint);
                    Destroyable target = rayHitBuffer[0].rigidbody?.GetComponent<Destroyable>();
                    if (target != null)
                    {
                        target.health -= projectile.damage * projectile.damageScale;
                    }
                }
                else
                {
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
        }
        //Remove all the projectiles that have hit there expiration, can happen due to time or impact
        projectiles.RemoveAll(p => p.expiration <= 0);
    }
    /*
    private void UpdatePhysicsProjectiles(float tick)
    {
        foreach (var projectile in projectiles)
        {
            projectile.expiration -= tick;

            if (projectile.expiration > 0)
            {
                //Sort out the projectiles 'forward' direction
                transPoint = projectile.rot * new Vector3(1f,0f,0f);
                //See if its going to hit something and if so handle that
                if (Physics2D.RaycastNonAlloc(projectile.pos, transPoint, rayHitBuffer, speed * tick, mask) > 0)
                {
                    projectile.expiration = -1;
                    worldPoint = rayHitBuffer[0].point;
                    SpawnSplash(worldPoint);
                    Destroyable target = rayHitBuffer[0].rigidbody.GetComponent<Destroyable>();
                    if (target != null)
                    {
                        target.health -= projectile.damage * projectile.damageScale;
                    }
                }
                else
                {
                    //This project wont be hitting anything this tick so just move it forward
                    projectile.pos += transPoint * (speed * tick);
                }
            }
        }
        //Remove all the projectiles that have hit there expiration, can happen due to time or impact
        projectiles.RemoveAll(p => p.expiration <= 0);
    }
    */
    private void SpawnSplash(Vector3 worldPoint)
    {
        //TODO: implament spawning of your splash effect e.g. the visual effect of a projectile hitting something
    }
}

public class ProjectileData
{
    public Vector3 pos;
    public Quaternion rot;
    public Vector3 scale;
    public float expiration;
    public int team;
    public float damage;
    public float damageScale;
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
