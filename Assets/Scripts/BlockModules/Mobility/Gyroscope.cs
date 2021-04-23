using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gyroscope : MonoBehaviour
{
    [SerializeField]
    public string keybind = "r";
    [SerializeField]
    public string toggle = "z";
    [SerializeField]
    public float frontRotation = 0f;
    [SerializeField]
    public float force = 1000f;

    private PID pid;
    private Rigidbody2D rigid;

    void Start()
    {
        rigid = gameObject.transform.parent.GetComponent<Rigidbody2D>();
        pid = new PID(1f, 1f, 1f);

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (rigid != null && pid != null)
            ApplyForce();
        else
            Destroy(this);
    }

    public void ApplyForce()
    {
        float angle = gameObject.transform.parent.rotation.eulerAngles.z + frontRotation;
        float desiredAngle = 0f;
        if(rigid != null)
            rigid.AddTorque(force * pid.Update(desiredAngle, angle, Time.deltaTime));
    }
}
