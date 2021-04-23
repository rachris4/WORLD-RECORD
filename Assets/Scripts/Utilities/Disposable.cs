using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Disposable : MonoBehaviour
{
    public float deathTime = 1000f;
    public float fadeoutTime = 1000f;
    private long tick = 0;
    private Color spriteColor;
    private bool mesh = false;
    // Start is called before the first frame update
    void Start()
    {
        if(gameObject.GetComponent<SpriteRenderer>() != null)
        {
            spriteColor = gameObject.GetComponent<SpriteRenderer>().color;
            mesh = false;
        } else if (gameObject.GetComponent<MeshRenderer>() != null)
        {
            mesh = true;
            spriteColor = gameObject.GetComponent<MeshRenderer>().material.color;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Delete()
    {
        ExplosionScript.scraps.Remove(this);
        if (gameObject != null)
            Destroy(gameObject);
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (tick < deathTime)
        {
            tick = (long)deathTime;
        }
    }
    // Update is called once per frame
    void Update()
    {
        tick++;
        if(tick > deathTime && !mesh)
        {
            float mod = (tick - deathTime) / (fadeoutTime);
            gameObject.GetComponent<SpriteRenderer>().color = new Color(spriteColor.r, spriteColor.g, spriteColor.b, spriteColor.a *(1f-mod));
            gameObject.transform.localScale = gameObject.transform.localScale*(1f - mod);
            Vector3 fun = gameObject.transform.rotation.eulerAngles;
            fun.x = mod * 90f;
            gameObject.transform.rotation = Quaternion.Euler(fun);
        } else if (tick > deathTime && mesh)
        {
            float mod = (tick - deathTime) / (fadeoutTime);
            gameObject.GetComponent<MeshRenderer>().material.color = new Color(spriteColor.r, spriteColor.g, spriteColor.b, spriteColor.a *(1f-mod));
            //gameObject.transform.localScale = gameObject.transform.localScale * (1f - mod);
            Vector3 fun = gameObject.transform.rotation.eulerAngles;
            fun.x = mod * 100f;
            gameObject.transform.rotation = Quaternion.Euler(fun);
        }
        if (tick > deathTime + fadeoutTime)
            Delete();
    }
}
