using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BlockPanel : MonoBehaviour
{

    [Header("Presets")]
    [SerializeField]
    private GameObject obj_disp;
    [SerializeField]
    private GameObject obj_desc;
    [SerializeField]
    private GameObject obj_TypeID;
    [SerializeField]
    private GameObject obj_health;
    [SerializeField]
    private GameObject obj_hardness;
    [SerializeField]
    private GameObject obj_action;
    [SerializeField]
    private GameObject obj_text_1;
    [SerializeField]
    private GameObject obj_text_2;
    [SerializeField]
    private GameObject obj_text_3;
    [SerializeField]
    private GameObject obj_text_4;
    [SerializeField]
    private GameObject obj_text_5;
    [SerializeField]
    private GameObject obj_text_6;
    [SerializeField]
    private GameObject obj_key_1;
    [SerializeField]
    private GameObject obj_key_2;

    [Header("Sets")]
    [SerializeField]
    public GameObject selectedBlock;
    public Block block;

    private GameObject[] texts;
    private GameObject[] binds;
    private string[] keynames;
    private string[] textnames;
    public EditorProperties EditorProperties;

    public bool hasInputs = false;
    private bool isNew = true;

    // Start is called before the first frame update
    void Start()
    {
        texts = new GameObject[6] { obj_text_1, obj_text_2, obj_text_3, obj_text_4, obj_text_5, obj_text_6 };
        binds = new GameObject[2] { obj_key_1, obj_key_2 };
        EditorProperties = new EditorProperties();
        EditorProperties.Keybinds = new List<GenericEditorInput>();
        EditorProperties.BlockData = new List<GenericEditorInput>();
    }

    // Update is called once per frame
    void Update()
    {
        if(hasInputs)
            UpdateEditorProperties();

    }

    public void HidePanel()
    {
        this.gameObject.SetActive(false);
    }

    public void ShowPanel()
    {
        this.gameObject.SetActive(true);
    }
    public void UpdateEditorProperties()
    {

        //Debug.Log("texts" + textnames.Length.ToString());
        //Debug.Log("binds" + keynames.Length.ToString());

        if (textnames != null && EditorProperties.BlockData.Count > 0)
        {
            for (int j = 0; j < EditorProperties.BlockData.Count; j++)
            {
                GenericEditorInput input = EditorProperties.BlockData[j];
                if (input.Name == null)
                    continue;
                for (int i = 0; i < textnames.Length; i++)
                {
                    if (input.Name == textnames[i])
                    {
                        input.Data = texts[i].GetComponentInChildren<TMP_InputField>().text;
                        EditorProperties.BlockData[j] = input;
                        //Debug.Log(textnames[i] + input.Data);
                    }
                }
            }
        }

        if (keynames != null && EditorProperties.Keybinds.Count > 0)
        {
            for (int j = 0; j < EditorProperties.Keybinds.Count; j++)
            {
                GenericEditorInput input = EditorProperties.Keybinds[j];

                for (int i = 0; i < keynames.Length; i++)
                {
                    if (input.Name == keynames[i])
                    {
                        input.Data = binds[i].GetComponentInChildren<TMP_InputField>().text;
                        EditorProperties.Keybinds[j] = input;
                    }
                }
            }
        }


    }

    public void LoadPanel(GameObject rayObj)
    {

    }

    public void UpdatePanel(bool fromScratch = true)
    {
        if (block == null)
            return;

        isNew = fromScratch;

        if(isNew)
        {
            foreach (GameObject obj_bind in binds)
            {
                obj_bind.GetComponentInChildren<TMP_InputField>().text = "?";
            }

            foreach (GameObject obj_text in texts)
            {
                obj_text.GetComponentInChildren<TMP_InputField>().text = "[...]";
            }
        }
        

        if (block.destructionProperties != null)
            obj_health.GetComponent<TextMeshProUGUI>().text = "Health : " + block.destructionProperties.Health.ToString();
        if (block.destructionProperties != null)
            obj_hardness.GetComponent<TextMeshProUGUI>().text = "Hardness : " + block.destructionProperties.Hardness.ToString();

        obj_TypeID.GetComponent<TextMeshProUGUI>().text = "TypeID : " + block.TypeID;
        obj_disp.GetComponent<TextMeshProUGUI>().text = block.DisplayName;

        if (block.Description != null)
        {
            obj_desc.GetComponent<TextMeshProUGUI>().text = block.Description;
        }
        else
            obj_desc.GetComponent<TextMeshProUGUI>().text = "";

        //if(obj == null)
        //    obj = blockObject;

        var button = obj_action.GetComponent<Button>();
        button.onClick.RemoveAllListeners();

        switch (block.TypeID)
        {
            case "FixedWeaponBlock":
                keynames = new string[1] { "Shoot" };
                textnames = new string[0];
                CreatePanelInputs();
                break;
            case "TurretBlock":
                keynames = new string[1] { "Shoot" };
                textnames = new string[0];
                CreatePanelInputs();
                break;
            case "JointStator":
                textnames = new string[3] { "Rotation Min", "Rotation Max", "Mirror Limb" };
                keynames = new string[0];
                if (selectedBlock != null)
                {
                    button.onClick.AddListener(selectedBlock.GetComponent<JointBuilder>().ConnectJoint);
                    selectedBlock.GetComponent<JointBuilder>().gridButtons = gameObject.transform.parent.gameObject;
                }
                //else
                    //Debug.Log("t'were null, m'lord");

                CreatePanelInputs("Attach Joint", true);
                break;
            case "JointRotor":
                textnames = new string[6] { "Type", "Strength", "Snappiness", "Degree Offset", "Motion Time", "Time Offset" };
                keynames = new string[2] { "Backward", "Forward" };
                CreatePanelInputs();
                break;
            case "FixedThruster":
                keynames = new string[2] { "Thrust" , "Toggle Damps"};
                textnames = new string[0];
                CreatePanelInputs();
                break;
            case "RotatingThruster":
                keynames = new string[2] { "Thrust", "Toggle Damps" };
                textnames = new string[0];
                CreatePanelInputs();
                break;
            case "GrappleGun":
                keynames = new string[1] { "Shoot Grapple" };
                textnames = new string[0];
                CreatePanelInputs();
                break;
            default:
                textnames = new string[0];
                keynames = new string[0];
                CreatePanelInputs();
                break;
        }

    }
    /*
    public void Kkvbbsd(GameObject sdsd);
    {

    }
    */
    private void CreatePanelInputs(string actionName = null, bool hasAction = false)
    {

        //var inputs = obj.AddComponent<GenericInput>();
        //inputs.texts = new GameObject[textInputs];
        //inputs.binds = new GameObject[keyInputs];
        if(isNew)
        {
            EditorProperties.Keybinds.Clear();
            EditorProperties.BlockData.Clear();
        }
            

        if (keynames.Length == 0 && textnames.Length == 0)
        {
            hasInputs = false;
        }
        else
            hasInputs = true;

        for (int i = 0; i < texts.Length; i++)
        {
            if (i + 1 > textnames.Length)
            {
                texts[i].GetComponentInChildren<TextMeshProUGUI>().text = "~";
                texts[i].SetActive(false);
            }
            else
            {
                texts[i].SetActive(true);
                if (isNew)
                    EditorProperties.BlockData.Add(new GenericEditorInput(textnames[i], "[...]"));
                else
                    texts[i].GetComponentInChildren<TMP_InputField>().text = EditorProperties.BlockData[i].Data;

                texts[i].GetComponentInChildren<TextMeshProUGUI>().text = textnames[i];
                //inputs.texts[i] = texts[i];
            }
        }

        for (int i = 0; i < binds.Length; i++)
        {
            if (i + 1 > keynames.Length)
            {
                binds[i].SetActive(false);
                binds[i].GetComponentInChildren<TextMeshProUGUI>().text = "~";
            }
            else
            {
                //inputs.binds[i] = binds[i];
                if (isNew)
                    EditorProperties.Keybinds.Add(new GenericEditorInput(keynames[i], "?"));
                else
                    binds[i].GetComponentInChildren<TMP_InputField>().text = EditorProperties.Keybinds[i].Data;

                binds[i].SetActive(true);
                binds[i].GetComponentInChildren<TextMeshProUGUI>().text = keynames[i];
            }
        }

        //Debug.Log("binds / " + EditorProperties.Keybinds.Count.ToString());
       // Debug.Log("texts / " + EditorProperties.BlockData.Count.ToString());

        if (hasAction && !isNew)
        {
            obj_action.SetActive(true);
            obj_action.GetComponentInChildren<TextMeshProUGUI>().text = actionName;
        }
        else
            obj_action.SetActive(false);

    }
}

