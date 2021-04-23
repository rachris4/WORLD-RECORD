using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using TMPro;

public class LoadBoard : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    GameObject loadBoard;
    [SerializeField]
    GameObject blueprintPrefab;
    [SerializeField]
    GameObject loadedBps;
    [SerializeField]
    BuildSystem buildSys;
    [SerializeField]
    private bool additive;

    private List<string> fileNames = new List<string>();

    private const int size = 100;
    private const int separation = 10;
    private bool isOpen = false;

    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void LoadBlueprints(string path)
    {
        fileNames.Clear();

        foreach (Transform child in loadedBps.transform)
        {
            Destroy(child.gameObject);
        }

        var dir = new DirectoryInfo(path);
        var folders = dir.GetDirectories();

        int count = 1;
        Vector3 screenAnchor = new Vector3(separation, -separation, 0f);
        var rect = loadBoard.GetComponent<RectTransform>();
        int interval = (int)Mathf.Floor(rect.rect.width/(separation+size));

        foreach (var fold in folders)
        {
            var info = fold.GetFiles("*.xml");
            if (info.Length == 0)
                continue;
            string subtypeID = info[0].Name;
            subtypeID = subtypeID.Replace(".xml", "");
            Debug.Log(subtypeID);

            if (!DefinitionManager.definitions.blueprintSubTypeIdList.Contains(subtypeID))
                continue;

            

            GameObject bp = Instantiate(blueprintPrefab);
            bp.name = subtypeID;
            bp.transform.parent = loadedBps.transform;

            var tfm = bp.GetComponent<RectTransform>();
            tfm.anchoredPosition = screenAnchor;


            var text = bp.transform.Find("Text").GetComponent<TextMeshProUGUI>();
            text.text = subtypeID;

            var button = bp.transform.Find("Thumb").GetComponent<Button>();
            if(!additive)
                button.onClick.AddListener(delegate { LoadBluePrint(bp.name); });
            else
                button.onClick.AddListener(delegate { LoadBluePrintAdditive(bp.name); });

            var thumb = bp.transform.Find("Thumb").GetComponent<Image>();

            screenAnchor.x += size + separation;
            if (count % interval == 0)
            {
                screenAnchor.y -= size + separation;
                screenAnchor.x -= interval * (size + separation);
            }
            count++;

            info = fold.GetFiles("*.png");
            if (info.Length == 0)
                continue;

            thumb.sprite = LoadImage(info[0].FullName);
        }
    }

    private void LoadBluePrint(string name)
    {
        buildSys.Load(name);
        ClickFolder();
    }

    private void LoadBluePrintAdditive(string name)
    {
        buildSys.LoadAdditive(name);
        ClickFolder();
    }

    public Sprite LoadImage(string path)
    {
        byte[] data = File.ReadAllBytes(path);
        Texture2D texture = new Texture2D(64, 64, TextureFormat.ARGB32, false);
        texture.LoadImage(data);
        texture.name = Path.GetFileNameWithoutExtension(path);
        var rect = new Rect(0f, 0f, texture.width, texture.height);
        return Sprite.Create(texture,rect,new Vector2(0.5f,0.5f));
    }

    public void ExitLoad()
    {
        foreach (Transform child in loadedBps.transform)
        {
            Destroy(child.gameObject);
        }

        loadBoard.SetActive(false); //= false;
    }

    public void StartLoad()
    {
        loadBoard.SetActive(true); //= false;
        LoadBlueprints(DefinitionSet.blueprintPath);
    }

    public void ClickFolder()
    {
        isOpen = !isOpen;
        if (isOpen)
        {
            StartLoad();
        }
        else
            ExitLoad();
    }

}
