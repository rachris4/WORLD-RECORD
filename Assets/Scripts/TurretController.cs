using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretController : MonoBehaviour
{
    public TurretProperties definition;

    public SpriteRenderer parentRend;
    public SpriteRenderer childRend;

    bool fixedRotation = false;
    private float rotz;

    public void Initialize(TurretProperties def, bool fix = false, float rotation = 0f)
    {
        definition = def;
        if (fix)
        {
            rotz = rotation;
            fixedRotation = fix;
        }
    }

    public void FixedUpdate()
    {
        if (definition == null)
            return;

        if (fixedRotation)
        {
            FixTurretAngle();
        }
        else
        {
            Vector3 loc = Utilities.GetWorldPositionOnPlane(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f), 0f);
            UpdateTurretAngle(loc);
        }
        
        UpdateRenderOrder();

    }
    public void FixTurretAngle()
    {
        gameObject.transform.rotation = Quaternion.RotateTowards(gameObject.transform.rotation, Quaternion.AngleAxis(Utilities.RealRotationZFloat(gameObject, rotz), Vector3.forward), definition.RotationSpeed * 25 * Time.deltaTime);
    }

    public void UpdateTurretAngle(Vector3 loc)
    {
        Vector3 dir = (loc - gameObject.transform.position);
        if (dir.magnitude > definition.AIRange)
            return;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        gameObject.transform.rotation = Quaternion.RotateTowards(gameObject.transform.rotation, Quaternion.AngleAxis(Utilities.RealRotationZFloat(gameObject, angle), Vector3.forward), definition.RotationSpeed * 25 * Time.deltaTime);
    }
    public void UpdateRenderOrder()
    {

        parentRend = gameObject.transform.parent.GetComponent<SpriteRenderer>();
        childRend = gameObject.GetComponent<SpriteRenderer>();
        childRend.sortingOrder = parentRend.sortingOrder + 2;
    }
}
