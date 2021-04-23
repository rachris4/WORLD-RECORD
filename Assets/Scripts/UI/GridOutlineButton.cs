using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;

public class GridOutlineButton : MonoBehaviour, IDragHandler, IBeginDragHandler
{
    // Start is called before the first frame update

    public GameObject gridObject;
    private bool dragging;
    Vector3 initialPos;
    BuildSystem bs;
    private void Start()
    {
        bs = Utilities.FindGameManager().GetComponent<BuildSystem>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        initialPos = Utilities.GetWorldPositionOnPlane(new Vector3(eventData.position.x, eventData.position.y), 0f);
        initialPos.x = Mathf.Round(initialPos.x);
        initialPos.y = Mathf.Round(initialPos.y);
    }
    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log(eventData.delta);

        Vector3 loc = Utilities.GetWorldPositionOnPlane(new Vector3(eventData.position.x, eventData.position.y), 0f);
        loc.x = Mathf.Round(loc.x);
        loc.y = Mathf.Round(loc.y);
        gridObject.transform.position += (loc-initialPos);

        initialPos = loc;

    }

}
