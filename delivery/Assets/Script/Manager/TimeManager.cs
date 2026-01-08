using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public TextMeshProUGUI time;
    public TextMeshProUGUI day;
    public GameManager gameManager;
    public ReadSpreadSheets sheet;

    [Header("타임어택 설정")]
    public TextMeshProUGUI timeattack;
    public GameObject timeattack_UI;

    public UIManager uiManager;

    private int date = 1;
    private float gameTime = 0f;
    private const float timeScale = 100f;
    private const int secondsPerDay = 3 * 3600;

    private float limitTime = 650f;
    private bool isRunning = false;

    void Awake()
    {
        // UI 자동 연결 로직
        if (time == null)
        {
            GameObject found = GameObject.Find("DayUI");
            if (found != null && found.transform.childCount > 1)
                time = found.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        }

        if (day == null)
        {
            GameObject foundDay = GameObject.Find("DayUI");
            if (foundDay != null && foundDay.transform.childCount > 0)
                day = foundDay.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        }
        if (sheet == null)
            sheet = FindObjectOfType<ReadSpreadSheets>();

        // UIManager 자동 찾기
        if (uiManager == null)
        {
            uiManager = FindObjectOfType<UIManager>();
        }
        timeattack_UI.SetActive(false);

    }

    void Update()
    {
        // 1. 일반 시간 흐름
        gameTime += Time.deltaTime * timeScale;
        UpdateDayNightUI();

        // 2. 타임어택 로직 (isRunning일 때만 작동)
        if (isRunning)
        {
            if (!timeattack_UI.activeSelf)
                timeattack_UI.SetActive(true);

            if (limitTime > 0)
            {
                limitTime -= Time.deltaTime * timeScale;
                if (limitTime < 0) limitTime = 0;
            }
            else
            {
                // 시간 종료 시
                EndTimer();
            }

            // UI 갱신
            if (timeattack != null)
            {
                // 650초(약 10분) 기준이면 로직 수정 필요할 수 있음. 현재는 분:초 표시
                int min = Mathf.FloorToInt(limitTime / 60f); // 초 단위라면 60으로 나눔

                // 원본 코드의 h 로직이 0으로 고정되어 있어서 그대로 둠
                int h = 0;
                timeattack.text = string.Format("{0:00}:{1:00}", h, min);
            }
        }
    }

    // 외부에서 호출하면 타이머를 시작하는 함수
    public void StartTimeAttack()
    {
        isRunning = true;
        timeattack_UI.SetActive(true);
        Debug.Log("타임어택 시작!");
    }

    // 타임 오버 시 처리
    private void EndTimer()
    {
        limitTime = 0;
        isRunning = false;
        timeattack_UI.SetActive(false);
        if (sheet != null)
        {
            sheet.LoadSavedDeck();
            Debug.Log("게임 종료: 저장된 덱을 다시 불러왔습니다.");
        }
        // 다음 씬으로 이동
        if (uiManager != null)
        {
            uiManager.nextScene();
        }
    }

    void UpdateDayNightUI()
    {
        // 기존 로직 유지
        int totalSeconds = Mathf.FloorToInt(gameTime / 600f) * 600;
        date = (totalSeconds / secondsPerDay) + 1;

        int secondsToday = totalSeconds % secondsPerDay;
        int hours = secondsToday / 3600;
        int minutes = (secondsToday % 3600) / 60;

        if (time != null) time.text = string.Format("{0:00}:{1:00}", hours, minutes);
        if (day != null) day.text = date.ToString() + " 일차";
    }
}