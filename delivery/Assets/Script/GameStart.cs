using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStart : MonoBehaviour
{
    public void startNewGame()
    {

        // 초기화 하기 !
        SceneManager.LoadScene("main_menu");
    }

    public void continueGame()
    {

        SceneManager.LoadScene("main_menu");

    }

    public void exitGame()
    {
        Application.Quit();
    }
}
