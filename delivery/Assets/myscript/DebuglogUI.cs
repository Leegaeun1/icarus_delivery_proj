using UnityEngine;
using System.Collections.Generic;

public class DebugLogUI : MonoBehaviour
{
    private Queue<string> logs = new Queue<string>();
    private int maxLogs = 15; // 화면에 표시할 최대 로그 수

    // GUI 스타일 설정
    private GUIStyle logStyle;
    private bool styleInitialized = false;

    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        // 타임스탬프 추가 (선택사항)
        string timestamp = System.DateTime.Now.ToString("HH:mm:ss");
        string formattedLog = $"[{timestamp}] {logString}";

        logs.Enqueue(formattedLog);

        // 최대 로그 수 초과 시 오래된 로그 제거
        if (logs.Count > maxLogs)
        {
            logs.Dequeue();
        }
    }

    void InitializeStyle()
    {
        if (!styleInitialized)
        {
            logStyle = new GUIStyle();
            logStyle.fontSize = 14;
            logStyle.normal.textColor = Color.white;
            logStyle.wordWrap = true;
            logStyle.padding = new RectOffset(10, 10, 5, 5);

            styleInitialized = true;
        }
    }

    void OnGUI()
    {
        InitializeStyle();

        // 배경 박스 (선택사항)
        GUI.Box(new Rect(10, 10, 500, maxLogs * 25 + 20), "");

        // 로그 표시
        int yOffset = 15;
        foreach (string log in logs)
        {
            GUI.Label(new Rect(15, yOffset, 490, 25), log, logStyle);
            yOffset += 25;
        }
    }
}

