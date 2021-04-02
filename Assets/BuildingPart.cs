using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingPart : MonoBehaviour
{
    int tick = 0;
    Vector3 ogLocation;
    Rigidbody2D rigidBody;
    Destroyable destroyable;

    // Start is called before the first frame update
    void Start()
    {
        rigidBody = gameObject.GetComponent<Rigidbody2D>();
        destroyable = gameObject.GetComponent<Destroyable>();
        ogLocation = gameObject.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (destroyable == null || rigidBody == null)
            return;

        if ((ogLocation - gameObject.transform.position).sqrMagnitude > 0.36)
        {
            destroyable.health -= 1;
        }
        if (rigidBody.velocity.sqrMagnitude < 0.1)
        {
            tick++;
            if(tick > 1000)
                rigidBody.Sleep();
        }
        else
            tick = 0;
    }
}
