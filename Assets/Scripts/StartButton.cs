using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class StartButton : MonoBehaviour
{

    public Button gameStartButton;


    // Start is called before the first frame update
    void Start()
    {
        gameStartButton.onClick.AddListener(OnGameStartButtonClicked);
    }

    private void OnGameStartButtonClicked()
    {

        SceneManager.LoadScene("New Scene");

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
