using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameQuit : MonoBehaviour
{
    public Button exitButton;
    // Start is called before the first frame update
    void Start()
    {
        exitButton.onClick.AddListener(QuitGame);
    }

    // Update is called once per frame
    void Update()
    {
    }


    void QuitGame()
    {

        Application.Quit();
    }
}
