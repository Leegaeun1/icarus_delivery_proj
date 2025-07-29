using TMPro;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public TextMeshProUGUI time;
    public GameManager gameManager;

    private static float gameTime = 0f; // static으로 유지
    private const float timeScale = 120f;

    void Start()
    {
        if (time == null)
        {
            // Tag 또는 이름으로 찾아서 동적으로 연결
            GameObject found = GameObject.Find("DayUI");
            if (found != null)
            {
                time = found.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
            }
        }
    }
    void Update()
    {
        gameTime += Time.deltaTime * timeScale;

        int totalSeconds = Mathf.FloorToInt(gameTime / 600f) * 600;
        int hours = totalSeconds / 3600;
        int minutes = (totalSeconds % 3600) / 60;

        time.text = string.Format("{0:00}:{1:00}", hours, minutes);
    }
}
