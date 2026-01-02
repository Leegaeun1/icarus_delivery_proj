using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ButtonKeeper : MonoBehaviour
{
    private Button button;
    private Order linkedOrder;
    private bool isPressed = false;

    public enum ButtonType { Order, Food, DeliveryMan }
    public ButtonType buttonType;

    [Header("점수 및 UI 설정")]
    [SerializeField] private TextMeshProUGUI scoreText;

    // 모든 버튼이 공유하는 데이터 (static)
    private static int totalScore = 0;
    private static TextMeshProUGUI sharedScoreDisplay;

    // 현재 선택된 버튼들을 저장
    private static ButtonKeeper pressedOrderButton = null;
    private static ButtonKeeper pressedFoodButton = null;
    private static ButtonKeeper pressedDeliveryManButton = null;

    void Start()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnButtonClick);
        }

        // 공유 UI 텍스트 설정
        if (scoreText != null)
        {
            sharedScoreDisplay = scoreText;
            UpdateScoreUI();
        }
    }

    public void Initialize(Order order)
    {
        linkedOrder = order;
        // 이미 완료된 주문일 경우의 초기 처리
        if (linkedOrder != null && linkedOrder.isCompleted)
        {
            isPressed = true;
            if (button != null) button.interactable = false;
        }
    }

    void OnButtonClick()
    {
        if (!isPressed)
        {
            isPressed = true;
            if (button != null) button.interactable = false; // 버튼 중복 클릭 방지

            // 1. 타입에 맞게 정적 변수에 자기 자신 저장
            switch (buttonType)
            {
                case ButtonType.Order: pressedOrderButton = this; break;
                case ButtonType.Food: pressedFoodButton = this; break;
                case ButtonType.DeliveryMan: pressedDeliveryManButton = this; break;
            }

            // 2. 주문 데이터 상태 업데이트
            if (linkedOrder != null) linkedOrder.isCompleted = true;

            // 3. 미션 완료 체크
            CheckMissionComplete();
        }
    }

    private void CheckMissionComplete()
    {
        // 세 종류의 버튼이 모두 선택되었는지 확인
        if (pressedOrderButton != null && pressedFoodButton != null && pressedDeliveryManButton != null)
        {
            // 점수 5점 추가
            totalScore += 5;
            UpdateScoreUI();

            Debug.Log("미션 완료! 5점 획득.");

            // 버튼 숨기기 및 초기화 실행
            HideAndResetButtons();
        }
    }

    private void HideAndResetButtons()
    {
        // 1. 선택되었던 버튼 오브젝트들을 화면에서 숨김
        if (pressedOrderButton != null) pressedOrderButton.gameObject.SetActive(false);
        if (pressedFoodButton != null) pressedFoodButton.gameObject.SetActive(false);
        if (pressedDeliveryManButton != null) pressedDeliveryManButton.gameObject.SetActive(false);

        // 2. 다음 미션을 위해 관리 변수들 리셋
        ResetSystem();
    }

    private void ResetSystem()
    {
        // 참조 변수 초기화 (이걸 해줘야 다음 버튼들을 다시 클릭했을 때 인지함)
        pressedOrderButton = null;
        pressedFoodButton = null;
        pressedDeliveryManButton = null;

        Debug.Log("시스템 리셋 완료. 다음 버튼들을 선택할 수 있습니다.");
    }

    private void UpdateScoreUI()
    {
        if (sharedScoreDisplay != null)
        {
            sharedScoreDisplay.text = "Score: " + totalScore;
        }
    }
}