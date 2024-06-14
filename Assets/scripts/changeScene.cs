using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class changeScene : MonoBehaviour
{
    public Button button;

    private void Start()
    {
        DontDestroyOnLoad(this);
        button.onClick.AddListener(change);
    }

    public void change()
    {
        SceneManager.LoadScene("condition2");
    }
}
