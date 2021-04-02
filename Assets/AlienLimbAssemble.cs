using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlienLimbAssemble : MonoBehaviour
{
    [SerializeField]
    private string LoadedSubTypeID = "Blob";
    [SerializeField]
    private bool attachCamera = false;

    private bool initd = false;
    private GameObject cam;


    void Update()
    {
        if (!initd)
            Initialize();

    }

    void Initialize()
    {

        if (DefinitionManager.definitions.blueprints == null)
            return;

        foreach (var limbdef in DefinitionManager.definitions.blueprints)
        {
            if (limbdef.SubTypeID == LoadedSubTypeID)
            {
                GameObject limb = limbdef.CreateLimbUnity(limbdef);
            }
        }

        initd = true;
    }
}
