using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class Screenshotter : MonoBehaviour
{
    [SerializeField]
    private Vector3 min = Vector3.zero;
    [SerializeField]
    private Vector3 max = Vector3.zero;
    [SerializeField]
    private string path = "";
    [SerializeField]
    private string name = "";
    private bool delete = false;
    // Update is called once per frame

    public void Initialize(Vector3 mi, Vector3 ma, string pa, string na = "", bool del = false)
    {
        min = mi;
        max = ma;
        path = pa;
        name = na;
        delete = del;
    }

    public static void TakeScreenshotWorld(string path, Vector3 min, Vector3 max, string name = "")
    {

        min = Camera.main.WorldToScreenPoint(min);
        max = Camera.main.WorldToScreenPoint(max);

        int width = (int)(max.x - min.x);
        int height = (int)(max.y - min.y);

        if (width > height)
            height = width;
        else
            width = height;

        Vector3 mid = min + (max - min) / 2;

        var tex = new Texture2D(width, height, TextureFormat.RGB24, false);

        Rect rex = new Rect(mid, new Vector2(width,height));
        rex.center = mid;

        tex.ReadPixels(rex, 0, 0);
        tex.Apply();

        // Encode texture into PNG
        var bytes = tex.EncodeToPNG();
        UnityEngine.Object.Destroy(tex);
        if (name != "")
            path = Path.Combine(path, name);
        else
            path = Path.Combine(path, DateTime.UtcNow.ToLongDateString());

        Debug.Log("Saved screenshot as " + path + "_screenshot.png");


        System.IO.File.WriteAllBytes(path + "_screenshot.png", bytes);
    }

    void OnPostRender()
    {
        if (path == "")
            return;
        TakeScreenshotWorld(path, min, max, name);

        if (delete)
            Destroy(gameObject);
        else
            Destroy(this);
    }
}
