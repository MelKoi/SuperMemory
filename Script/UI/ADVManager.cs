using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ADVManager
{
    public string scene;
    public void ChangeScene(string scene)
    {
        SceneManager.LoadScene(scene);
    }
}
