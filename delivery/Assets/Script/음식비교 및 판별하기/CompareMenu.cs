using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TMPro;

public class DeliveryFinalManager : MonoBehaviour
{
    public static DeliveryFinalManager Instance;
    public TextMeshProUGUI statusText; // 면허증의 Status TMP 연결

    void Awake()
    {
        Instance = this;
    }

    // 음식 봉투 버튼이 클릭되었을 때 실행될 핵심 함수 
    public void ConfirmDelivery()
    {
        if (ReadSpreadSheets.Instance == null) return;

        List<string> playerList = CheckMenu.selectedNames;
        List<string> includeReq = ReadSpreadSheets.Instance.CurrentRequest_Include;
        List<string> excludeReq = ReadSpreadSheets.Instance.CurrentRequest_Exclude;
        List<string> mainOrder = ReadSpreadSheets.Instance.currentRequestName;

        bool isSuccess = true;
        string failReason = "";

        // 1. 포함/제외/메인 메뉴 검증 로직 
        foreach (string item in includeReq)
        {
            if (!playerList.Contains(item)) { isSuccess = false; failReason = $"누락: {item}"; break; }
        }

        if (isSuccess)
        {
            if (LicenseInfoManager.Instance != null && LicenseInfoManager.Instance.isForgery)
            {
                // 위조범인데 '승인'을 눌러버린 경우 (실패 처리하거나 감점)
                Debug.Log("<color=red>위조범을 통과시켰습니다!</color>");
                ScoreManager.Instance.AddScore(100, true, false);
            }
            else
            {
                // 정상 배달원이고 음식도 잘 만든 경우
                ScoreManager.Instance.AddScore(100, true, true);
            }

            Debug.Log("<color=green>배달 성공!</color>");
            if (statusText != null) statusText.text = "STATUS: VALID";

            // 배달 성공 점수 100점 추가
            if (ScoreManager.Instance != null)
                ScoreManager.Instance.AddScore(100, true, true);
        }
        else
        {
            Debug.Log($"<color=red>배달 실패: {failReason}</color>");
            if (statusText != null) statusText.text = "STATUS: INVALID";

            // 배달 실패 시 50점 감점
            if (ScoreManager.Instance != null)
                ScoreManager.Instance.AddScore(100, true, false);
        }

        // 2. 결과 UI 반영
        if (isSuccess)
        {
            Debug.Log("<color=green>배달 성공!</color>");
            if (statusText != null) statusText.text = "STATUS: VALID";
        }
        else
        {
            Debug.Log($"<color=red>배달 실패: {failReason}</color>");
            if (statusText != null) statusText.text = "STATUS: INVALID";
        }
    }
}