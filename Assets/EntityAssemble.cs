using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityAssemble : MonoBehaviour
{
    [SerializeField]
    private string LoadedSubTypeID = "";
    [SerializeField]
    private string playerTag = "";
    [SerializeField]
    private bool attachCamera = false;

    private bool initd = false;
    private GameObject cam;

    void Start()
    {
        if (attachCamera)
        {
            

        }
    }

    void Update()
    {
        if (!initd)
            Initialize();

    }

    void Initialize()
    {

        if (DefinitionManager.definitions.chassisDefinitions == null)
            return;

        foreach (var def in DefinitionManager.definitions.chassisDefinitions)
        {
            if (def.SubTypeID == LoadedSubTypeID)
            {
                Chassis spawn = new Chassis(def, playerTag, gameObject);
            }
        }

        initd = true;
    }
}
