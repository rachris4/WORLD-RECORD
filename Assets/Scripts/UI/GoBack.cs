using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GoBack : MonoBehaviour
{
    // Start is called before the first frame update
    public void Return()
    {
        Utilities.DestroyAll();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }

    public void Restart()
    {
        if (Utilities._ddolObjects.Count == 0)
            return;

        GameObject body = Utilities._ddolObjects[0];

        foreach(Transform child in body.transform)
        {
            Destroy(child.gameObject);
        }

        GridAssembly assembly = body.GetComponent<GridAssembly>();
        assembly.controllers.Clear();
        assembly.objList.Clear();
        assembly.initd = false;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);


    }

}
