using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;
using TMPro;

public class GridOutlineSprite : MonoBehaviour
{
    // Start is called before the first frame update

    public GameObject canvasElement;
    public GameObject gridObject;
    public GameObject prefab;
    public BuildSystem buildSystem;
    private SpriteRenderer outline;
    private GameObject button;
    private TMP_InputField input;
    private Image primeButton;

    void Start()
    {
        outline = gameObject.GetComponent<SpriteRenderer>();
        button = Instantiate(prefab);
        button.name = gameObject.name + "_button";
        var move = button.transform.GetComponentInChildren<GridOutlineButton>();
        input = button.transform.Find("translate").GetComponentInChildren<TMP_InputField>();
        primeButton = button.transform.Find("PrimeButton").GetComponent<Image>();
        button.transform.Find("PrimeButton").GetComponent<Button>().onClick.AddListener(MakePrime);
        input.text = gridObject.name;
        move.gridObject = gridObject;
        button.transform.parent = canvasElement.transform;
        button.GetComponent<Button>().onClick.AddListener(delegate { ChangeGridObject(); });
        ChangeGridObject();
    }

    // Update is called once per frame
    void Update()
    {
        if (gridObject == null)
        {
            Destroy(button);
            Destroy(gameObject);
        }
        else
            gridObject.name = input.text;

        if(buildSystem.prime != gridObject)
        {
            UpdateNotPrime();
        }

        UpdateOutline();    
    }

    private void UpdateNotPrime()
    {
        primeButton.color = new Color(0.6f, 0.6f, 0.6f, 0.6f);
    }

    public void MakePrime()
    {
        buildSystem.prime = gridObject;
        primeButton.color = new Color(1f, 1f, 0.8f, 1f);
    }


    void UpdateOutline()
    {
        if (gridObject == null)
            return;

        Vector3 min = new Vector3(10000,10000,0);
        Vector3 max = new Vector3(-10000, -10000, 0);
        Vector3 loc = Vector3.zero;

        foreach (Transform child in gridObject.transform)
        {

            loc = child.position;

            if (loc.x < min.x)
            {
                min.x = (int)loc.x;
            }
            if (loc.y < min.y)
            {
                min.y = (int)loc.y;
            }
            if (loc.x > max.x)
            {
                max.x = (int)loc.x;
            }
            if (loc.y > max.y)
            {
                max.y = (int)loc.y;
            }

        }
        float width = max.x - min.x;
        float height = max.y - min.y;

        Vector3 mid = (max - min) / 2 + min;

        //Debug.Log(mid.y.ToString() + " / " + height.ToString());

        if ((mid.x+width/2) % 1f == 0)
        {
            width += 5;
        }
        else
            width += 4;

        if ((mid.y + height / 2) % 1f == 0)
        {
            height += 5;
        }
        else
            height += 4;


        if (width <= 0 || height <= 0)
            return;

        outline.size = new Vector2(width, height);

        gameObject.transform.position = mid;
        //Debug.Log(min.ToString() + " / " + max.ToString());

        var rect = button.GetComponent<RectTransform>();
        rect.position = Camera.main.WorldToScreenPoint(mid);
        var mins = Camera.main.WorldToScreenPoint(mid-new Vector3(width/2,height/2));
        var maxs = Camera.main.WorldToScreenPoint(mid + new Vector3(width / 2, height / 2));

        rect.sizeDelta = new Vector2(maxs.x - mins.x, maxs.y - mins.y);
        

    }
    public void ChangeGridObject()
    {
        button.GetComponent<Button>().enabled = false;
        button.GetComponent<Image>().enabled = false;

        buildSystem.ChangeCurrentGrid(gridObject);

        //Transform[] children = canvasElement.GetComponentsInChildren<Transform>(true);
        //Debug.Log(children.Length);

        foreach (Transform child in canvasElement.transform)
        {
            if(child.gameObject != button)
            {
                var doubleButton = child.gameObject.GetComponent<Button>();
                doubleButton.GetComponent<Button>().enabled = true;
                doubleButton.GetComponent<Image>().enabled = true;
            }
        }
    }

    private void OnDestroy()
    {
        Destroy(button);
    }
}
