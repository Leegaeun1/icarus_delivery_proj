using UnityEngine;

public class LicenseJudge : MonoBehaviour
{
    public static LicenseJudge Instance;

    void Awake() { Instance = this; }

    // 음식을 배달원에게 전달했을 때 호출 (승인)
    public void AcceptDelivery()
    {
        var sheet = GameObject.Find("sheet").GetComponent<ReadSpreadSheets>();
        if (sheet == null || LicenseInfoManager.Instance == null) return;

        // 위조 여부 확인
        bool isForgery = LicenseInfoManager.Instance.isForgery;

        if (isForgery)
        {
            // 위조범인데 통과시킨 경우 -> 에러 증가
            sheet.delivery_incorrect++;
            Debug.Log("<color=red>배달 실패:</color> 위조범 통과!");
        }
        else
        {
            // 정상인에게 잘 준 경우 -> 에러 감소 (만회)
            if (sheet.delivery_incorrect > 0) sheet.delivery_incorrect--;
            Debug.Log("<color=green>배달 성공:</color> 정상 배달원 확인.");
            CheckMenu.selectedNames.Clear();
        }

        sheet.menu_num++; // 전체 주문 수 증가
    }

    // 거절 버튼 클릭 시 호출
    public void RejectDelivery()
    {
        var sheet = GameObject.Find("sheet").GetComponent<ReadSpreadSheets>();
        if (sheet == null || LicenseInfoManager.Instance == null) return;

        bool isForgery = LicenseInfoManager.Instance.isForgery;

        if (isForgery)
        {
            // 위조범을 잘 거절한 경우 -> 에러 감소
            if (sheet.delivery_incorrect > 0) sheet.delivery_incorrect--;
            Debug.Log("<color=green>검거 성공:</color> 위조범 거절.");
        }
        else
        {
            // 진짜를 잘못 거절한 경우 -> 에러 증가
            sheet.delivery_incorrect++;
            Debug.Log("<color=red>검거 실패:</color> 선량한 배달원 거절!");
        }

        sheet.menu_num++;
    }
}