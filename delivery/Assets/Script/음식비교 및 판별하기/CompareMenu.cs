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
            foreach (string item in excludeReq)
            {
                if (playerList.Contains(item)) { isSuccess = false; failReason = $"제외항목포함: {item}"; break; }
            }
        }

        if (isSuccess)
        {
            foreach (string sand in mainOrder)
            {
                if (!playerList.Contains(sand)) { isSuccess = false; failReason = $"메인메뉴({sand}) 누락"; break; }
            }
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