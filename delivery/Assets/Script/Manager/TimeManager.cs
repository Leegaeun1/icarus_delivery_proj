using TMPro;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public TextMeshProUGUI time;
    public TextMeshProUGUI day;
    public GameManager gameManager;

    private int date = 1;
    private static float gameTime = 0f; // 누적 시간(초)
    private const float timeScale = 1000f;
    private const int secondsPerDay = 3 * 3600; // 하루 = 3시간

    void Awake()
    {
        // time UI 찾기
        if (time == null)
        {
            GameObject found = GameObject.Find("DayUI");
            if (found != null && found.transform.childCount > 1)
            {
                time = found.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
            }
            else
            {
                Debug.LogError("[TimeManager] 'DayUI' 오브젝트를 찾을 수 없거나 구조가 다릅니다.");
            }
        }

        // day UI 찾기 (필요 시 구조에 맞게 수정)
        if (day == null)
        {
            GameObject foundDay = GameObject.Find("DayUI");
            if (foundDay != null && foundDay.transform.childCount > 0)
            {
                day = foundDay.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            }
            else
            {
                Debug.LogError("[TimeManager] Day UI 참조를 찾을 수 없습니다.");
            }
        }

        // GameManager 참조 체크
        if (gameManager == null)
        {
            Debug.LogWarning("[TimeManager] GameManager 참조가 없습니다. 일부 기능이 제한될 수 있습니다.");
        }

        // UI 필수 참조 없으면 스크립트 비활성화
        if (time == null || day == null)
        {
            Debug.LogError("[TimeManager] UI 참조 누락으로 TimeManager를 비활성화합니다.");
            enabled = false;
        }
    }

    void Update()
    {
        gameTime += Time.deltaTime * timeScale;

        // 날짜 계산
        int totalSeconds = Mathf.FloorToInt(gameTime / 600f) * 600; // 분 단위로 맞춤
        date = (totalSeconds / secondsPerDay) + 1;

        // 하루 시간 계산 (3시간 단위)
        int secondsToday = totalSeconds % secondsPerDay;
        int hours = secondsToday / 3600;
        int minutes = (secondsToday % 3600) / 60;

        // 안전하게 UI 갱신
        if (time != null) 
            time.text = string.Format("{0:00}:{1:00}", hours, minutes);
        if (day != null) 
            day.text = date.ToString() + " 일차";
    }
}
