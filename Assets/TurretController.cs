using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretController : MonoBehaviour
{
    public TurretProperties definition;

    public void Initialize(TurretProperties def, string weaponSubTypeID)
    {
        definition = def;
        
    }

    public void FixedUpdate()
    {
        if (definition == null)
            return;
        Vector3 loc = Utilities.GetWorldPositionOnPlane(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f), 0f);
        Vector3 dir = (loc - gameObject.transform.position);
        if (dir.magnitude > definition.AIRange)
            return;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        gameObject.transform.rotation = Quaternion.RotateTowards(gameObject.transform.rotation, Quaternion.AngleAxis(angle, Vector3.forward), definition.RotationSpeed * 25 * Time.deltaTime);
    }
}
