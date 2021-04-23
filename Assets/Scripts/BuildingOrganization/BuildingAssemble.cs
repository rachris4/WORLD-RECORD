using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingAssemble : MonoBehaviour
{
    [SerializeField]
    private string LoadedSubTypeID = "";
    private bool initd = false;
    private GameObject cam;
    private HashSet<GameObject> blocks = new HashSet<GameObject>();
    private int blockCountOG;
    private int falltick;
    private Vector2 min;
    private Vector2 max;
    private GameObject rubble;

    void FixedUpdate()
    {
        if (!initd)
            Initialize();

        bool awake = false;

        foreach (GameObject child in blocks)
        {
            if (child == null)
            {
                blocks.Remove(child);
                return;
            }
            Rigidbody2D rigidBody = child.GetComponent<Rigidbody2D>();
            if (!rigidBody.IsSleeping())
                awake = true;
            else if (awake)
                rigidBody.WakeUp();
            if (blocks.Count < (float)blockCountOG / 2f)
            {
                child.GetComponent<Destroyable>().health -= 10f;
            }
        }
        if(blocks.Count < (float)blockCountOG/2f)
        {
            if(rubble == null)
            {
                rubble = new GameObject("rubble");
                Vector2 span = max - min;
                span = max - span / 2;
                rubble.transform.position = new Vector3(gameObject.transform.position.x+span.x/4, gameObject.transform.position.y + min.y+9, 0f);
                Vector3 scale = rubble.transform.localScale;
                scale.x = (max.x - min.x+1)/8;
                rubble.transform.localScale = scale;

                /*

                SpriteRenderer blockRend = rubble.AddComponent<SpriteRenderer>();
                blockRend.sprite = Resources.Load<Sprite>("Sprites/Rubble");
                blockRend.sortingOrder = -301;

                */


                //blockRend.color = new Color(0.6f, 0.6f, 0.6f, 1f);
                /*
                gameObject.layer = 8;
                foreach (Transform child in gameObject.transform)
                {
                    if (child == null)
                    {
                        continue;
                    }
                    Rigidbody2D rigidBody = child.GetComponent<Rigidbody2D>();
                    if(rigidBody == null)
                    {

                        child.gameObject.layer = 8;
                        child.gameObject.AddComponent<Rigidbody2D>();
                        child.gameObject.AddComponent<BoxCollider2D>();
                        var des = child.gameObject.AddComponent<Destroyable>();
                        des.shatterType = Destroyable.ShatterType.Voronoi;
                        des.extraPoints = 2;
                    }
                }*/
            }
            falltick++;

            rubble.transform.position += new Vector3(0f, 0.002f, 0f)*2;
            gameObject.transform.position -= new Vector3(0f, 0.005f + Mathf.Abs(Mathf.Sin(falltick/60))*falltick*0.0001f + falltick*0.0001f, 0f)*2;

            if(falltick > 350)
            {

                Destroy(gameObject);
            }
        }

    }

    void Initialize()
    {

        if (DefinitionManager.definitions.blueprints == null)
            return;

        var currentRend = gameObject.GetComponent<SpriteRenderer>();


        foreach (var def in DefinitionManager.definitions.blueprints)
        {
            if (def.SubTypeID == LoadedSubTypeID)
            {
                foreach (Block square in def.blockList)
                {
                    GameObject newBlock = square.CreateBlockUnity(gameObject, currentRend);
                    blocks.Add(newBlock);
                    Rigidbody2D rigidBody = newBlock.AddComponent<Rigidbody2D>();
                    newBlock.AddComponent<BuildingPart>();
                    BlockDefinition otherSquare;
                    DefinitionManager.definitions.blockDict.TryGetValue(square.SubTypeID, out otherSquare);

                    GameObject sprite = new GameObject("bg_sprite_" + square.SubTypeID);
                    Utilities.CopyTransform(newBlock.transform, ref sprite);

                    SpriteRenderer blockRend = sprite.AddComponent<SpriteRenderer>();
                    blockRend.sprite = Resources.Load<Sprite>(otherSquare.pathSprite);
                    blockRend.sortingOrder = currentRend.sortingOrder-305;
                    blockRend.color = new Color(0.6f, 0.6f, 0.6f, 1f);


                    rigidBody.mass = otherSquare.mass;
                    rigidBody.gravityScale = 0.3f;
                    rigidBody.angularDrag = 1f;
                    rigidBody.sharedMaterial = new PhysicsMaterial2D();
                    rigidBody.sharedMaterial.friction = 1000f;
                    rigidBody.sharedMaterial.bounciness = -3000f;
                    rigidBody.Sleep();

                    if (newBlock.transform.position.x < min.x)
                    {
                        min.x = (int)newBlock.transform.position.x;
                    }
                    if (newBlock.transform.position.y < min.y)
                    {
                        min.y = (int)newBlock.transform.position.y;
                    }
                    if (newBlock.transform.position.x > max.x)
                    {
                        max.x = (int)newBlock.transform.position.x;
                    }
                    if (newBlock.transform.position.y > max.y)
                    {
                        max.y = (int)newBlock.transform.position.y;
                    }
                    

                }
            }
        }
        blockCountOG = blocks.Count;
        initd = true;
    }
}
