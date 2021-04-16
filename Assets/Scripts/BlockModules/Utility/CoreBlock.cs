using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoreBlock : MonoBehaviour
{
    GameObject OverParent;
    GridAssembly assembly;

    // Start is called before the first frame update
    void Start()
    {
        OverParent = Utilities.FindOverParent(gameObject);
        assembly = OverParent.GetComponent<GridAssembly>();
    }

    // Update is called once per frame
    void Update()
    {
        if (OverParent == null)
            return;

        if(gameObject.transform.parent?.parent?.gameObject != OverParent)
        {
            assembly.KillAllControllers();
        }
    }

    void OnDestroy()
    {
        assembly.KillAllControllers();
    }
}
