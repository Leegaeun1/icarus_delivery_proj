using TMPro;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public TextMeshProUGUI time;
    public GameManager gameManager;
    public int nowtime;

    void Update()
    {
        nowtime = Mathf.FloorToInt(gameManager.ElapsedTime);

        int minutes = nowtime / 60;
        int seconds = nowtime % 60;

        time.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}
