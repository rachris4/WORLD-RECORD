using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JointManager : MonoBehaviour
{

    public GameObject Stator;
    public GameObject Rotor;
    public HingeJoint2D Joint;
    // Start is called before the first frame update
    bool initd = false;
    // Update is called once per frame
    void FixedUpdate()
    {
        if(!initd)
        {
            initd = Stator != null && Rotor != null && Joint != null;
            return;
        }

        if(Stator == null || Rotor == null)
        {
            gameObject.transform.parent = null;
            Destroy(Joint);
            Destroy(this);
            return;
        }

    }
}
