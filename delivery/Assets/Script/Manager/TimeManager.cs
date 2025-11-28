using TMPro;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public TextMeshProUGUI time;
    public TextMeshProUGUI day;
    public GameManager gameManager;
    public TextMeshProUGUI timeattack; // 타임어택 UI
    public GameObject timeattack_UI;
    public TimeManager ui;

    private int date = 1;
    private float gameTime = 0f;
    private const float timeScale = 100f;
    private const int secondsPerDay = 3 * 3600;

    private float limitTime = 650f;

    private bool isclick = false;

    void Awake()
    {
        // UI 자동 연결 (안전장치)
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
    }

    void Update()
    {
        // 1. 시간 흐름 계산
        gameTime += Time.deltaTime * timeScale;

        // 2. 날짜 UI 갱신 (함수로 분리됨)
        UpdateDayNightUI();
        if (isclick)
        {
            timeattack_UI.SetActive(true);
            // 3. 타임어택 (5분 카운트다운) 로직
            if (limitTime > 0)
            {
                limitTime -= Time.deltaTime*timeScale;
                if (limitTime < 0) limitTime = 0;
            }
            else
            {
                limitTime = 0;
                timeattack_UI.SetActive(false);
                ui.GetComponent<UIManager>().nextScene();
                isclick = false;
            }

            // UI가 진짜로 존재하는지 확인 후 텍스트 변경
            if (timeattack != null)
            {
                int min = Mathf.FloorToInt(limitTime / 60f);
                int h = 0;
                timeattack.text = string.Format("{0:00}:{1:00}", h, min);
            }
        }
        else
        {
            isclick = false;
            timeattack_UI.SetActive(false);
        }
    }

    void UpdateDayNightUI()
    {
        int totalSeconds = Mathf.FloorToInt(gameTime / 600f) * 600;
        date = (totalSeconds / secondsPerDay) + 1;

        int secondsToday = totalSeconds % secondsPerDay;
        int hours = secondsToday / 3600;
        int minutes = (secondsToday % 3600) / 60;

        //  UI가 파괴되었으면 접근하지 않도록 체크
        if (time != null)
        {
            time.text = string.Format("{0:00}:{1:00}", hours, minutes);
        }

        if (day != null)
        {
            day.text = date.ToString() + " 일차";
        }
    }

    public void OnBtnclick()
    {
        isclick = !isclick;
    }
}