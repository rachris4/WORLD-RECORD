using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LimbController : MonoBehaviour
{
    [SerializeField]
    public string Type;
    [SerializeField]
    public float strengthMod = 300f;
    [SerializeField]
    public float speedMod = 1f;
    [SerializeField]
    public float speedLimit = 100f;
    [SerializeField]
    public float border = 0f;
    [SerializeField]
    public float P = 1f;
    [SerializeField]
    public float I = 1f;
    [SerializeField]
    public float D = 0f;
    [SerializeField]
    public float offset = 0f;
    [SerializeField]
    public int wavelength = 100;
    [SerializeField]
    private float integrityMod = 1f;
    [SerializeField]
    public float frontRotation = 0f;

    public GameObject parent;
    public HingeJoint2D joint;
    private Rigidbody2D rigidBody;
    private PhysicsMaterial2D mat;
    private PID pid;
    private PID apid;

    public int blockIntegrity = 1;
    public float currentIntegrity = 0;
    private int tick;
    public float max;
    public float min;
    private float storage;
    private bool grounded;
    private bool initd = false;
    private HashSet<Rigidbody2D> footRigids = new HashSet<Rigidbody2D>();
    string boi;

    public void Initialize(LimbControllerDefinition def)
    {
        Type = def.Type;
        strengthMod = def.StrengthMod;
        speedMod = def.SpeedMod;
        frontRotation = def.Rotation;
        P = def.P;
        I = def.I;
        D = def.D;
        offset = def.Offset;
        wavelength = def.Wavelength;
        border = def.Border;
        speedLimit = def.SpeedLimit;
        pid = new PID(P, I, D);
        apid = new PID(P, I, D);
        initd = true;
        if(Type == "Foot")
        {
            foreach(Transform child in gameObject.transform)
            {
                if (child.GetComponent<Rigidbody2D>())
                    footRigids.Add(child.GetComponent<Rigidbody2D>());
            }
        }
    }

    public void ResetTick()
    {
        if(wavelength != 0)
            tick = (int)offset % (wavelength*2);
    }

    void Start()
    {
        rigidBody = gameObject.GetComponent<Rigidbody2D>();
        if(rigidBody.sharedMaterial == null)
        {
            //mat = new PhysicsMaterial2D();
            //rigidBody.sharedMaterial = mat;
        }
        pid = new PID(P, I, D);
        footRigids.Add(rigidBody);
    }

    void FixedUpdate()
    {
        if (Type == null || !initd)
            return;

        if (gameObject.transform.parent == null)
            Destroy(this);

        currentIntegrity = 0;
        foreach(Transform child in gameObject.transform)
        {
            if(child.gameObject.GetComponent<Destroyable>() != null)
            {
                currentIntegrity++;
            }
        }
        currentIntegrity /= blockIntegrity;

        pid.pFactor = P;
        pid.dFactor = D;
        pid.iFactor = I;

        switch (Type)
        {
            case "GravityAlign":

                GravityAlign();
                break;

            case "MouseTrack":

                if (parent != null)
                    MouseTrack();
                break;

            case "Foot":
                if (parent != null)
                    Foot();
                break;

            case "Stable":
                if (parent != null)
                    Stable();
                break;
            case "Spin":

                Spin();
                break;

            case "Thigh":

                if (parent != null)
                    Thigh();
                break;

            default:
                Stable();
                break;
        }
    }

    public void Stable()
    {
        if (joint == null)
        {
            Destroy(this);
            return;
        }

        bool move = false;
        if (Input.GetKey("d"))
        {
            //move = true;
        }
        if (Input.GetKey("a"))
        {
            //move = true;
        }
        var motor = new JointMotor2D();

        if (move)
        {
            joint.useMotor = false;
            joint.motor = motor;
            return;
        }

        float displacement = parent.transform.rotation.eulerAngles.z + frontRotation;
        float angle = gameObject.transform.rotation.eulerAngles.z + frontRotation;
        //bool inbd = AngleInBounds(desiredAngle, displacement, joint.limits, border);
        joint.useMotor = true;
        motor.motorSpeed = pid.Update(displacement, angle, Time.deltaTime, speedLimit);
        motor.maxMotorTorque = 10 * strengthMod;
        joint.motor = motor;
    }

    public void Spin()
    {
        if (joint == null)
        {
            Destroy(this);
            return;
        }

        bool move = false;
        if (Input.GetKey("r"))
        {
            move = true;
        }
        var motor = new JointMotor2D();

        if (!move)
        {
            joint.useMotor = true;
            joint.useLimits = false;
            motor.motorSpeed = 0;
            motor.maxMotorTorque = 10 * strengthMod * currentIntegrity;
            joint.motor = motor;
        }

        joint.useMotor = true;
        joint.useLimits = false;
        motor.motorSpeed = speedLimit;
        motor.maxMotorTorque = 1000*currentIntegrity;
        joint.motor = motor;
    }

    public void Thigh()
    {
        if (joint == null)
        {
            Destroy(this);
            return;
        }

        int direction = 0;
        if (Input.GetKey("d"))
        {
            direction++;
        }
        if (Input.GetKey("a"))
        {
            direction--;

        }
        var motor = new JointMotor2D();
        if ((tick) % (wavelength*2) < wavelength)
        {
            direction *= 1;
        }
        else if ((tick) % (wavelength*2) < 2 * wavelength)
        {
            direction *= -1;
        }
        else
        {
            direction = 0;
            ResetTick();
        }

        if (direction != 0)
            tick++;

        float displacement = parent.transform.rotation.eulerAngles.z + frontRotation;
        float angle = gameObject.transform.rotation.eulerAngles.z + frontRotation;
        float desiredAngle = angle + direction * Time.deltaTime * speedMod*10;
        if (direction == 0)
        {
            desiredAngle = displacement;
        }

        desiredAngle = Utilities.RealRotationZFloat(gameObject, desiredAngle);


        //bool inbd = AngleInBounds(desiredAngle, displacement, joint.limits, border);
        //if (gameObject.name.Contains("Leg"))
        //    Debug.Log(gameObject.name + " : " + desiredAngle.ToString() + " / " + angle.ToString());
        joint.useMotor = true;
        motor.motorSpeed = pid.Update(desiredAngle, angle, Time.deltaTime, speedLimit);
        motor.maxMotorTorque = 10 * strengthMod* currentIntegrity;
        joint.motor = motor;
    }

    public void Foot()
    {
        //gravity align checkcheck
        float angle = gameObject.transform.rotation.eulerAngles.z + frontRotation;
        float desiredAngle = 0f;
        rigidBody.AddTorque(strengthMod* currentIntegrity * pid.Update(desiredAngle, angle, Time.deltaTime, speedLimit));

        int direction = 0;
        if (Input.GetKey("d"))
        {
            direction++;
        }
        if (Input.GetKey("a"))
        {
            direction--;
        }

        //do foot stuff
        if ((tick) % (wavelength * 2) < wavelength)
        {
            direction *= 1;
        }
        else if ((tick) % (wavelength * 2) < 2 *wavelength)
        {
            direction *= -1;
        }
        else
        {
            //rigidBody.sharedMaterial.friction = 1f;
            //rigidBody.sharedMaterial.bounciness = -1f;
            ResetTick();
            direction = 0;
        }

        //rigidBody.sharedMaterial.friction = 1f;
        //rigidBody.sharedMaterial.bounciness = -3000f;

        if ((rigidBody.velocity.x > 0 && Input.GetKey("a")) || (rigidBody.velocity.x < 0 && Input.GetKey("d")))
        {
            //rigidBody.sharedMaterial.friction = 1f;
            //rigidBody.sharedMaterial.bounciness = 0f;
            if(grounded)
                rigidBody.AddForce(new Vector2(-rigidBody.velocity.x * rigidBody.mass  * 300f, 0f));
            //rigidBody.sharedMaterial.bounciness = -1f;
        }
        else if(direction != 0)
        {
            //rigidBody.sharedMaterial.friction = 0.05f;
            //rigidBody.sharedMaterial.bounciness = 0.2f;
        }

        if (direction != 0)
            tick++;

        float pos = gameObject.transform.position.x;
        float despos = Time.deltaTime * speedMod * 5*direction;
        rigidBody.AddForce(new Vector2(strengthMod * currentIntegrity/20 * pid.Update(pos+despos, pos, Time.deltaTime, speedLimit), 0f));
    }

    public void GravityAlign()
    {
        float angle = gameObject.transform.rotation.eulerAngles.z + frontRotation;
        float desiredAngle = 0f;
        rigidBody.AddTorque(strengthMod * currentIntegrity * pid.Update(desiredAngle, angle, Time.deltaTime, speedLimit));
    }

    public void MouseTrack()
    {
        if (joint == null)
        {
            Destroy(this);
            return;
        }
        Vector3 loc = Utilities.GetWorldPositionOnPlane(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f), 0f);
        Vector3 dir = (loc - (gameObject.transform.position + gameObject.transform.rotation * joint.connectedAnchor));
        float displacement = parent.transform.rotation.eulerAngles.z + frontRotation;
        float desiredAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        desiredAngle = Utilities.RealRotationZFloat(gameObject, desiredAngle);
        float angle = gameObject.transform.rotation.eulerAngles.z + frontRotation;
        bool inbd = AngleInBounds(desiredAngle, displacement, joint.limits, border);
        if (dir.sqrMagnitude < 4)
            inbd = false;

        joint.useMotor = true;
        var motor = new JointMotor2D();
        if (inbd)
        {
            motor.motorSpeed = pid.Update(desiredAngle, angle, Time.deltaTime, speedLimit);
        }
        else
        {
            motor.motorSpeed = 0;
        }
        motor.maxMotorTorque = 10 * strengthMod * currentIntegrity;
        joint.motor = motor;
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        //Debug.Log("Ouch!");
        if (Type == "Foot") // && col.otherRigidbody.gameObject.layer == 8
        {
            grounded = true;
        }
    }

    void OnCollisionExit2D(Collision2D col)
    {
        //Debug.Log("Ouch!");
        if (Type == "Foot") // && col.otherRigidbody.gameObject.layer == 8
        {
            grounded = false;
        }
    }

    public static bool AngleInBounds(float angle, float displacement, JointAngleLimits2D limits, float border)
    {
        bool inbd = true;

        angle = UnityAngleSmooth(angle - displacement);
        float max = UnityAngleSmooth(limits.max + border);
        float min = UnityAngleSmooth(limits.min - border);

        if (angle > max || angle < min)
        {
            inbd = false;
        }

        return inbd;
    }

    public static float UnityAngleSmooth(float angle)
    {

        while(angle < -180 || angle > 180)
        {
            if (angle > 180)
                angle -= 360;
            if (angle < -180)
                angle += 360;
        }
        return angle;
    }

}
