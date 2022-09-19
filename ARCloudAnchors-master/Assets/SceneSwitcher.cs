using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GoogleScene()
    {
        SceneManager.LoadScene("PersistentCloudAnchors");
    }
    public void MyScene()
    {
        SceneManager.LoadScene("ARCloudAnchor");
    }

    public void ReloadScene()
    {
        Scene scene = SceneManager.GetActiveScene();

        Debug.Log("Active scene is '" + scene.name + "'.");
        SceneManager.LoadScene(scene.name);
    }

    public void HomeScene()
    {
        SceneManager.LoadScene("Home");
    }
}
