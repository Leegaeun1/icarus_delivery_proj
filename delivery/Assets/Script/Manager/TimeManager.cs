using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
    public GameObject nextDayButtonObj;

    private int date = 1;
    private float gameTime = 0f;

    private float gameSpeed = 1000f;
    private const int secondsPerDay = 3 * 3600;

    // 하루가 끝났는지 체크하는 플래그
    private bool isDayEnded = false;

    private float limitTime = 650f;
    private bool isRunning = false;


    void Awake()
    {
        // UI 자동 연결 로직
        if (time == null)
        {
            time = GameObject.Find("game_time").GetComponent<TextMeshProUGUI>();
            //if (found != null && found.transform.childCount > 1)
            //    time = found.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        }

        if (day == null)
        {
            day = GameObject.Find("day_txt").GetComponent<TextMeshProUGUI>();
            //if (foundDay != null && foundDay.transform.childCount > 0)
            //    day = foundDay.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
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
        // 1. 일반 시간 흐름 (하루가 끝나지 않았을 때만 시간이 흐름)
        if (!isDayEnded)
        {
            gameTime += Time.deltaTime * gameSpeed;
            CheckEndOfDay(); // 시간이 다 되었는지 체크
        }
        UpdateDayNightUI();

        // 2. 타임어택 로직 (isRunning일 때만 작동)
        if (isRunning)
        {
            if (!timeattack_UI.activeSelf)
                timeattack_UI.SetActive(true);

            if (limitTime > 0)
            {
                limitTime -= Time.deltaTime * gameSpeed;
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
    // 시간이 3시(secondsPerDay)에 도달했는지 확인하는 함수
    void CheckEndOfDay()
    {
        // 현재까지 흐른 총 시간이 "오늘 끝날 시간(날짜 * 3시간)"을 넘었는지 확인
        float endOfToday = date * secondsPerDay;

        if (gameTime >= endOfToday)
        {
            // 시간을 딱 맞춰서 고정
            gameTime = endOfToday;

            // 하루 마감 상태로 전환
            isDayEnded = true;

            Debug.Log(date + "일차 업무 종료 (03:00). 시간 정지.");
        }
    }

    public void StartNextDay()
    {
        // 1. 날짜 증가
        date++;

        // 2. 상태 해제 (시간이 다시 흐르도록)
        isDayEnded = false;

        // 결제 로직이나 정산
        sheet.SaveDailyData();
        // + 애니메이션 넣기

        // 다시 메인화면으로 이동
        SceneManager.LoadScene("main_menu");

    }

    // 외부에서 호출하면 타이머를 시작하는 함수
    public void StartTimeAttack()
    {
        isRunning = true;
        timeattack_UI.SetActive(true);
    }

    // 타임 오버 시 처리
    private void EndTimer()
    {
        limitTime = 0;
        isRunning = false;
        timeattack_UI.SetActive(false);

        // --- 여기서 결제 로직 수행 ---
        if (sheet != null)
        {
            // 1. 결제를 시도합니다.
            // 성공하면(돈 충분) -> 돈이 차감되고 true 반환
            // 실패하면(돈 부족) -> ReadSpreadSheets 내부 로직에 의해 '마지막 재료'가 삭제되고 false 반환
            bool purchaseSuccess = sheet.ApplyPurchase();

            if (!purchaseSuccess)
            {
                Debug.Log("잔액 부족으로 특수 재료 제거됨. 남은 재료로 재결제 시도.");

                // 2. 실패했다면, 특수재료가 빠진 상태(가격이 내려감)로 다시 결제를 시도합니다.
                sheet.ApplyPurchase();
            }
            //sheet.SaveDailyData();
        }
        // ---------------------------

        // 다음 씬으로 이동
        if (uiManager != null)
        {
            uiManager.nextScene();
        }
    }

    void UpdateDayNightUI()
    {
        // UI에는 현재 gameTime을 기준으로 표시

        // 만약 하루가 끝난 상태라면 강제로 3:00 표시 (나머지 연산하면 0:00이 되기 때문)
        if (isDayEnded)
        {
            if (time != null) time.text = "03:00";
            if (day != null) day.text = date.ToString() + " 일차";
            return;
        }

        // 일반적인 시간 표시 계산
        int totalSeconds = Mathf.FloorToInt(gameTime); // 소수점 버림

        // 날짜 계산 (Update에서 date를 관리하므로 여기서는 표시용으로만 참고하거나 기존 date 사용)
        // int currentDate = (totalSeconds / secondsPerDay) + 1; 

        int secondsToday = totalSeconds % secondsPerDay;
        int hours = secondsToday / 3600;
        int minutes = (secondsToday % 3600) / 60;

        if (time != null) time.text = string.Format("{0:00}:{1:00}", hours, minutes);
        if (day != null) day.text = date.ToString() + " 일차";
    }
}