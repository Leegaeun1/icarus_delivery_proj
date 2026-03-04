using UnityEngine;
using UnityEngine.SceneManagement;

public class LicenseJudge : MonoBehaviour
{
    public static LicenseJudge Instance;
    public TimeManager timeManager;

    void Awake() { 
        
        Instance = this;
        timeManager = GameObject.Find("TimeManager").GetComponent<TimeManager>();

    }

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

        if (ManManager.Instance != null)
        {
            ManManager.Instance.DestroyMan();
        }

        sheet.menu_num++; // 전체 주문 수 증가
        // 시간 끝이면 
        if (timeManager.isDayEnded)
        {
            SceneManager.LoadScene("DayFinish");
        }
    }

    // 거절 버튼 클릭 시 호출
    public void RejectDelivery()
    {
        var sheet = GameObject.Find("sheet").GetComponent<ReadSpreadSheets>();
        bool isForgery = LicenseInfoManager.Instance.isForgery;

        // 1. 위조 판정 및 에러 횟수 조절
        if (isForgery)
        {
            if (sheet.delivery_incorrect > 0) sheet.delivery_incorrect--;
            Debug.Log("<color=green>성공: 위조범을 내보냈습니다.</color>");
        }
        else
        {
            sheet.delivery_incorrect++;
            Debug.Log("<color=red>실패: 정상 배달원을 내보냈습니다.</color>");
        }

        // 2. 현재 배달원 제거 및 새 배달원 소환
        if (ManManager.Instance != null)
        {
            // 기존 배달원 삭제
            ManManager.Instance.DestroyMan();

            // 즉시 새로운 배달원 소환 (무작위 캐릭터)
            string orderText = (sheet.currentRequestName.Count > 0) ? sheet.currentRequestName[0] : "";
            ManManager.Instance.SpawnMan(orderText);
        }
    }
}