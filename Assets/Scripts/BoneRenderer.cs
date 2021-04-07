using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoneRenderer : MonoBehaviour
{
    // Start is called before the first frame update
    private LineRenderer line;

    public GameObject boneTwo;


    void Start()
    {
        line = gameObject.AddComponent<LineRenderer>();
        line.enabled = true;
        Material newMat = Resources.Load<Material>("bone");
        line.material = newMat;
        line.startWidth = 1f;
        line.endWidth = 1f;
    }

    // Update is called once per frame
    void Update()
    {

        if (boneTwo == null || (gameObject.transform.position- boneTwo.transform.position).sqrMagnitude > 9)
            Destroy(this);

        UpdateLine();
    }

    void UpdateLine()
    {
        line.SetPosition(0, gameObject.transform.position);
        line.SetPosition(0, boneTwo.transform.position);
    }
}
