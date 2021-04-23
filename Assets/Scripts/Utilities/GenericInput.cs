using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericInput : MonoBehaviour
{
    // Start is called before the first frame update
    public string TypeID;
    public EditorProperties EditorProperties;


    [SerializeField]
    private List<string> binds = new List<string>();
    [SerializeField]
    private List<string> texts = new List<string>();

    public void Start()
    {
        //Initialize();
    }

    void Initialize()
    {
        EditorProperties = new EditorProperties();
        EditorProperties.BlockData = new List<GenericEditorInput>();
        EditorProperties.Keybinds = new List<GenericEditorInput>();


        EditorProperties.BlockData.Add(new GenericEditorInput("help!", "iwantdeath!"));
    }

    // Update is called once per frame
    void Update()
    {
        /*
        binds.Clear();
        texts.Clear();

        if (EditorProperties != null)
        {
            for (int i = 0; i > EditorProperties.Keybinds.Count; i++)
            {
                binds[i] = EditorProperties.Keybinds[i].Name;
            }

            for (int i = 0; i > EditorProperties.BlockData.Count; i++)
            {
                texts[i] = EditorProperties.BlockData[i].Name;
            }

        }
        */
        //Debug.Log(binds.Count.ToString() + " / " + texts.Count.ToString());

    }


}

