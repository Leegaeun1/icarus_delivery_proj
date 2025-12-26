using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ButtonKeeper : MonoBehaviour
{
    private Button button;
    private Order linkedOrder;
    private bool isPressed = false;

    public enum ButtonType
    {
        Order,
        Food,
        DeliveryMan
    }

    public ButtonType buttonType;

    [SerializeField] private TextMeshProUGUI missionCompleteText;

    private static ButtonKeeper pressedOrderButton = null;
    private static ButtonKeeper pressedFoodButton = null;
    private static ButtonKeeper pressedDeliveryManButton = null;

    private static TextMeshProUGUI sharedMissionText = null;

    void Start()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnButtonClick);
        }

        if (missionCompleteText != null && sharedMissionText == null)
        {
            sharedMissionText = missionCompleteText;
            sharedMissionText.gameObject.SetActive(false);
        }
    }

    public void Initialize(Order order)
    {
        linkedOrder = order;
        button = GetComponent<Button>();

        // 이미 완료된 주문이면 버튼 비활성화
        if (linkedOrder != null && linkedOrder.isCompleted)
        {
            if (button != null) button.interactable = false;
            isPressed = true;
        }

        // 리스너는 Start에서 등록하므로 중복 등록 방지
    }

    void OnButtonClick()
    {
        if (!isPressed)
        {
            isPressed = true;
            if (button != null) button.interactable = false;

            switch (buttonType)
            {
                case ButtonType.Order:
                    pressedOrderButton = this;
                    Debug.Log("Order 버튼 눌림");
                    break;
                case ButtonType.Food:
                    pressedFoodButton = this;
                    Debug.Log("Food 버튼 눌림");
                    break;
                case ButtonType.DeliveryMan:
                    pressedDeliveryManButton = this;
                    Debug.Log("DeliveryMan 버튼 눌림");
                    break;
            }
        }

        if (linkedOrder != null && !linkedOrder.isCompleted)
        {
            linkedOrder.isCompleted = true;
            if (button != null) button.interactable = false;
        }

        CheckMissionComplete();
    }

    private void CheckMissionComplete()
    {
        if (pressedOrderButton != null &&
            pressedFoodButton != null &&
            pressedDeliveryManButton != null)
        {
            Debug.Log("미션 완료! 3가지 버튼 모두 선택됨");
            HideSelectedButtons();
            ShowMissionComplete();
        }
    }

    private void HideSelectedButtons()
    {
        if (pressedOrderButton != null && pressedOrderButton.gameObject != null)
            pressedOrderButton.gameObject.SetActive(false);

        if (pressedFoodButton != null && pressedFoodButton.gameObject != null)
            pressedFoodButton.gameObject.SetActive(false);

        if (pressedDeliveryManButton != null && pressedDeliveryManButton.gameObject != null)
            pressedDeliveryManButton.gameObject.SetActive(false);

        pressedOrderButton = null;
        pressedFoodButton = null;
        pressedDeliveryManButton = null;
    }

    private void ShowMissionComplete()
    {
        if (sharedMissionText != null)
        {
            StartCoroutine(ShowMissionCompleteCoroutine());
        }
        else
        {
            Debug.LogWarning("Mission Complete Text가 할당되지 않았습니다!");
        }
    }

    private IEnumerator ShowMissionCompleteCoroutine()
    {
        sharedMissionText.gameObject.SetActive(true);
        sharedMissionText.text = "Mission Complete!";

        yield return new WaitForSeconds(3f);

        sharedMissionText.gameObject.SetActive(false);
    }
}