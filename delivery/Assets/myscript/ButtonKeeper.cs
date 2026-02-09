using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ButtonKeeper : MonoBehaviour
{
    private Button button;
    private Order linkedOrder; // OrderManagers에서 넘겨준 데이터를 저장
    private bool isPressed = false;

    public enum ButtonType { Order, Food, DeliveryMan }
    [Header("버튼 설정")]
    public ButtonType buttonType;

    [Header("UI 연결 (Score Text는 하나만 연결해도 공유됨)")]
    [SerializeField] private TextMeshProUGUI scoreText;

    // 모든 버튼이 공유하는 데이터 (static)
    private static int totalScore = 0;
    private static TextMeshProUGUI sharedScoreDisplay;

    // 현재 선택된 버튼들을 저장하는 정적 변수
    private static ButtonKeeper pressedOrderButton = null;
    private static ButtonKeeper pressedFoodButton = null;
    private static ButtonKeeper pressedDeliveryManButton = null;

    void Awake()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnButtonClick);
        }
    }

    void Start()
    {
        // 공유 UI 텍스트 설정 (씬에 하나만 있어도 static으로 공유)
        if (scoreText != null)
        {
            sharedScoreDisplay = scoreText;
            UpdateScoreUI();
        }
    }

    // [중요] OrderManagers에서 호출하는 초기화 함수
    public void Initialize(Order order)
    {
        linkedOrder = order;
        isPressed = false;

        if (button != null) button.interactable = true;

        // 버튼의 텍스트를 주문 이름으로 변경 (자식에 TMP가 있을 경우)
        TextMeshProUGUI tmpText = GetComponentInChildren<TextMeshProUGUI>();
        if (tmpText != null && linkedOrder != null)
        {
            tmpText.text = linkedOrder.orderName;
        }

        // 이미 완료된 주문인지 체크
        if (linkedOrder != null && linkedOrder.isCompleted)
        {
            SetButtonComplete();
        }
    }

    void OnButtonClick()
    {
        if (!isPressed)
        {
            isPressed = true;
            if (button != null) button.interactable = false;

            // 1. 타입에 맞게 정적 변수에 자기 자신 저장
            switch (buttonType)
            {
                case ButtonType.Order: pressedOrderButton = this; break;
                case ButtonType.Food: pressedFoodButton = this; break;
                case ButtonType.DeliveryMan: pressedDeliveryManButton = this; break;
            }

            // 2. 미션 완료 체크
            CheckMissionComplete();
        }
    }

    private void CheckMissionComplete()
    {
        // 세 종류의 버튼이 모두 선택되었는지 확인
        if (pressedOrderButton != null && pressedFoodButton != null && pressedDeliveryManButton != null)
        {
            // 점수 추가 및 UI 반영
            totalScore += 5;
            UpdateScoreUI();

            Debug.Log("<color=green>미션 완료! 5점 획득.</color>");

            // 주문 데이터 완료 처리
            if (pressedOrderButton.linkedOrder != null)
                pressedOrderButton.linkedOrder.isCompleted = true;

            // [추가] OrderQueueSystem에 성공 알림 (싱글톤 호출)
            if (OrderQueueSystem.Instance != null)
            {
                OrderQueueSystem.Instance.CompleteOrderSuccess();
            }

            // 버튼 숨기기 및 초기화
            HideAndResetButtons();
        }
    }

    private void HideAndResetButtons()
    {
        if (pressedOrderButton != null) pressedOrderButton.gameObject.SetActive(false);
        if (pressedFoodButton != null) pressedFoodButton.gameObject.SetActive(false);
        if (pressedDeliveryManButton != null) pressedDeliveryManButton.gameObject.SetActive(false);

        ForceReset();
    }

    // OrderQueueSystem에서 강제로 리셋할 때도 호출됨
    public static void ForceReset()
    {
        pressedOrderButton = null;
        pressedFoodButton = null;
        pressedDeliveryManButton = null;
    }

    private void UpdateScoreUI()
    {
        if (sharedScoreDisplay != null)
        {
            sharedScoreDisplay.text = "Score: " + totalScore;
        }
    }

    private void SetButtonComplete()
    {
        isPressed = true;
        if (button != null) button.interactable = false;
    }
}