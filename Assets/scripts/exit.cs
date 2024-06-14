using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class exit : MonoBehaviour
{
    public Button button;

    private void Start()
    {
        DontDestroyOnLoad(this);
        button.onClick.AddListener(exitApp);
    }

    public void exitApp()
    {
        Application.Quit();
    }
}
