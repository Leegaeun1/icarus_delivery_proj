using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStart : MonoBehaviour
{
    public TimeManager timeManager;

    private void Awake()
    {
        timeManager = GameObject.Find("TimeManager").GetComponent<TimeManager>();
    }
    public void startNewGame()
    {
        // 싱글톤 인스턴스를 통해 초기화 함수 호출
        if (ReadSpreadSheets.Instance != null)
        {
            ReadSpreadSheets.Instance.ResetGameData();
            timeManager.ResetTimeData();
        }
        else
        {
            // 혹시라도 인스턴스가 없다면 PlayerPrefs만이라도 지움
            PlayerPrefs.DeleteAll();
        }

        // 초기화 후 씬 이동
        SceneManager.LoadScene("main_menu");
    }

    public void continueGame()
    {
        // 해당 스테이지의 첫날부터 시작
        SceneManager.LoadScene("main_menu");


    }

    public void exitGame()
    {
        Application.Quit();
    }
}
