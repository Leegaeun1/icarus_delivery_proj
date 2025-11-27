using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class SimpleDeliverySystem : MonoBehaviour
{
    [Header("팝업 UI")]
    public GameObject posPopup;
    public GameObject deliveryPopup;    // 전체 팝업창
    public Button delivery1Button;      // Delivery 1 버튼
    public Button okButton;            // OK 버튼

    [Header("완료 메시지 UI")]
    public GameObject completeMessageUI;  // "delivery complete!" 메시지 UI
    public TextMeshProUGUI completeText;            // 완료 메시지 텍스트

    [Header("설정")]
    public float messageShowTime = 2f;   // 메시지 표시 시간 (2초)

    private bool isDelivery1Selected = false;

    void Start()
    {
        // 버튼 이벤트 연결
        delivery1Button.onClick.AddListener(SelectDelivery1);
        okButton.onClick.AddListener(OnOkClick);

        // 초기 설정
        okButton.interactable = false;  // OK 버튼 비활성화
        completeMessageUI.SetActive(false);  // 완료 메시지 숨김
    }

    // Delivery 1 버튼 클릭
    void SelectDelivery1()
    {
        isDelivery1Selected = true;
        okButton.interactable = true;  // OK 버튼 활성화

        // 선택 표시 (버튼 색상 변경)
        ColorBlock colors = delivery1Button.colors;
        colors.normalColor = Color.yellow;
        delivery1Button.colors = colors;

        Debug.Log("Delivery 1 선택됨");
    }

    // OK 버튼 클릭
    void OnOkClick()
    {
        if (isDelivery1Selected)
        {
            StartCoroutine(ShowDeliveryComplete());
        }
        else
        {
            Debug.Log("먼저 배달을 선택해주세요!");
        }
    }

    // 배달 완료 처리
    IEnumerator ShowDeliveryComplete()
    {
        // 1단계: 팝업창 닫기
        deliveryPopup.SetActive(false);
        posPopup.SetActive(false);

        // 2단계: "delivery complete!" 메시지 표시
        completeText.text = "delivery complete!";
        completeMessageUI.SetActive(true);

        // 3단계: 2초 대기
        yield return new WaitForSeconds(messageShowTime);

        // 4단계: 메시지 숨기기
        completeMessageUI.SetActive(false);

        // 5단계: 시스템 리셋 (다음 사용을 위해)
        ResetSystem();
    }

    // 시스템 초기화
    void ResetSystem()
    {
        isDelivery1Selected = false;
        okButton.interactable = false;

        // 버튼 색상 원래대로
        ColorBlock colors = delivery1Button.colors;
        colors.normalColor = Color.white;
        delivery1Button.colors = colors;

        Debug.Log("시스템 초기화 완료");
    }

    // 팝업 다시 열기 (외부에서 호출용)
    public void OpenPopup()
    {
        deliveryPopup.SetActive(true);
        posPopup.SetActive(false);
        ResetSystem();
    }
}