using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrameRateManager : MonoBehaviour
{

    public int frameRate = 60;

    void Start()
    {
        if (Application.isEditor)
        {
            Application.targetFrameRate = frameRate;
        }
        else
        {
            QualitySettings.vSyncCount = 1;

        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F11))
        {
            if(Application.isEditor)
            {
                Application.targetFrameRate = Application.targetFrameRate == 0 ? frameRate : 0;

            }
            else
            {
                QualitySettings.vSyncCount = QualitySettings.vSyncCount == 0 ? 1 : 0;

            }
        }
    }

    IEnumerator changeFramerate()
    {
        yield return new WaitForSeconds(1);
        Application.targetFrameRate = frameRate;
    }
}
