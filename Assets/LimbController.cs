using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LimbController : MonoBehaviour
{
    [SerializeField]
    public string Type;
    [SerializeField]
    public float strengthMod = 1f;
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
    private HingeJoint2D joint;
    private Rigidbody2D rigidBody;
    private PhysicsMaterial2D mat;
    private PID pid;
    private PID apid;

    public int blockIntegrity = 1;
    private int tick;
    private float max;
    private float min;
    private float storage;
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

    }

    void Start()
    {
        rigidBody = gameObject.GetComponent<Rigidbody2D>();
        if(rigidBody.sharedMaterial == null)
        {
            mat = new PhysicsMaterial2D();
            rigidBody.sharedMaterial = mat;
        }
        pid = new PID(P, I, D);

    }
    void FixedUpdate()
    {
        if (Type == null)
            return;

        if (parent != null && joint == null)
        {
            foreach (HingeJoint2D j in parent.GetComponents<HingeJoint2D>())
            {
                if (j.connectedBody.gameObject.name == gameObject.name)
                {
                    joint = j;
                }
            }
        }

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

                Foot();
                break;

            case "Stable":

                Stable();
                break;

            case "Thigh":

                if (parent != null)
                    Thigh();
                break;

            default:

                break;
        }
    }

    public void Stable()
    {
        bool move = false;
        if (Input.GetKey("d"))
        {
            move = true;
        }
        if (Input.GetKey("a"))
        {
            move = true;
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

    public void Thigh()
    {
        int direction = 0;
        int oldtick = tick;
        if (Input.GetKey("d"))
        {
            direction++;
            tick++;
        }
        if (Input.GetKey("a"))
        {
            direction--;
            if(tick == oldtick)
                tick++;
        }
        var motor = new JointMotor2D();
        if (tick + offset < wavelength)
        {
            if(tick + offset - wavelength == -1)
            {
                motor.motorSpeed = 0;
                motor.maxMotorTorque = 10 * strengthMod;
                joint.motor = motor;
                return;
            }
            direction *= 1;
        }
        else if (tick + offset < 2 * wavelength)
        {
            if (tick + offset - 2*wavelength == -1)
            {
                motor.motorSpeed = 0;
                motor.maxMotorTorque = 10 * strengthMod;
                joint.motor = motor;
                return;
            }
            direction *= -1;
        }
        else
        {
            tick = -(int)offset;
        }            

        float displacement = parent.transform.rotation.eulerAngles.z + frontRotation;
        float angle = gameObject.transform.rotation.eulerAngles.z + frontRotation;
        float desiredAngle = angle + direction * Time.deltaTime * speedMod*10;
        if (direction == 0)
            desiredAngle = displacement;
        //bool inbd = AngleInBounds(desiredAngle, displacement, joint.limits, border);

        joint.useMotor = true;
        motor.motorSpeed = pid.Update(desiredAngle, angle, Time.deltaTime, speedLimit);
        motor.maxMotorTorque = 10 * strengthMod;
        joint.motor = motor;
    }

    public void Foot()
    {
        //gravity align checkcheck
        float angle = gameObject.transform.rotation.eulerAngles.z + frontRotation;
        float desiredAngle = 0f;
        rigidBody.AddTorque(strengthMod * pid.Update(desiredAngle, angle, Time.deltaTime, speedLimit));

        int direction = 0;
        int oldtick = tick;
        if (Input.GetKey("d"))
        {
            direction++;
            tick++;
        }
        if (Input.GetKey("a"))
        {
            direction--;
            if (tick == oldtick)
                tick++;
        }

        //do foot stuff
        if (tick+offset < wavelength)
        {
            direction *= 1;
        }
        else if (tick+offset < 2*wavelength)
        {
            direction *= -1;
        }
        else
        {
            rigidBody.sharedMaterial.friction = 1f;
            rigidBody.sharedMaterial.bounciness = -1f;
            tick = -(int)offset;
            direction = 0;
        }

        if ((direction > 0 && Input.GetKey("a")) || (direction < 0 && Input.GetKey("d")))
        {
            rigidBody.sharedMaterial.friction = 1000f;
            rigidBody.sharedMaterial.bounciness = -3000f;
        }
        else
        {
            rigidBody.sharedMaterial.friction = 0.05f;
            rigidBody.sharedMaterial.bounciness = 0.2f;
        }

        if(direction != 0)
            tick++;

        float pos = gameObject.transform.position.x;
        float despos = Time.deltaTime * speedMod * 5;
        despos *= direction;

        rigidBody.AddForce(new Vector2(direction * strengthMod/10 * pid.Update(pos+despos, pos, Time.deltaTime, speedLimit), 0f));
    }

    public void GravityAlign()
    {
        float angle = gameObject.transform.rotation.eulerAngles.z + frontRotation;
        float desiredAngle = 0f;
        rigidBody.AddTorque(strengthMod * pid.Update(desiredAngle, angle, Time.deltaTime, speedLimit));
    }

    public void MouseTrack()
    {
        Vector3 loc = Utilities.GetWorldPositionOnPlane(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f), 0f);
        Vector3 dir = (loc - (gameObject.transform.position + gameObject.transform.rotation * joint.connectedAnchor));
        float displacement = parent.transform.rotation.eulerAngles.z + frontRotation;
        float desiredAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
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
        motor.maxMotorTorque = 10 * strengthMod;
        joint.motor = motor;
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
