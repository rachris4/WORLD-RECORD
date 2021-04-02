using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutlineRenderer : MonoBehaviour
{
    // Reference to the shader material defined in the next section
    public float outlineSize = 5f;

    private List<Material> attachedMaterials = new List<Material>();

    void Start()
    {
        foreach (var s in GetComponentsInChildren<SpriteRenderer>())
        {
            AddOutline(s);
        }
    }


    private void AddOutline(SpriteRenderer sprite)
    {
        var width = sprite.bounds.size.x;
        var height = sprite.bounds.size.x;

        var widthScale = 1 / width;
        var heightScale = 1 / height;

        // Add child object with sprite renderer
        var outline = new GameObject("Outline");
        outline.transform.parent = sprite.gameObject.transform;
        outline.transform.localScale = gameObject.transform.localScale*1.3f;
        outline.transform.localPosition = new Vector3(0f, 0f, 0f);
        outline.transform.localRotation = Quaternion.identity;
        var outlineSprite = outline.AddComponent<SpriteRenderer>();
        outlineSprite.sprite = sprite.sprite;
        outlineSprite.color = new Color(0f, 0f, 0f, 0.4f);
        outlineSprite.sortingOrder = sprite.sortingOrder-1;
        // The UV coordinates of the texture is always from 0..1 no matter
        // what the aspect ratio is so we need to specify both the
        // horizontal and vertical size of the outline
        /*
        outlineSprite.material.SetFloat(
            "_HSize", 0.1f * widthScale * outlineSize);
        outlineSprite.material.SetFloat(
            "_VSize", 0.1f * heightScale * outlineSize);
        attachedMaterials.Add(outlineSprite.material);*/
    }
}


