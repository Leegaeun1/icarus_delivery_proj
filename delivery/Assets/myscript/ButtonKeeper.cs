using UnityEngine;
using UnityEngine.UI;
using TMPro; // 추가
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

    // Text 대신 TextMeshProUGUI 사용
    [SerializeField] private TextMeshProUGUI missionCompleteText;

    private static ButtonKeeper pressedOrderButton = null;
    private static ButtonKeeper pressedFoodButton = null;
    private static ButtonKeeper pressedDeliveryManButton = null;

    // TextMeshProUGUI로 변경
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
            Debug.Log("Mission Complete Text 설정 완료");
        }
    }

    public void Initialize(Order order)
    {
        linkedOrder = order;
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnButtonClick);
            if (linkedOrder.isCompleted)
            {
                button.interactable = false;
                isPressed = true;
            }
        }
    }

    void OnButtonClick()
    {
        if (!isPressed)
        {
            isPressed = true;
            button.interactable = false;

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
            button.interactable = false;
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
        {
            pressedOrderButton.gameObject.SetActive(false);
        }

        if (pressedFoodButton != null && pressedFoodButton.gameObject != null)
        {
            pressedFoodButton.gameObject.SetActive(false);
        }

        if (pressedDeliveryManButton != null && pressedDeliveryManButton.gameObject != null)
        {
            pressedDeliveryManButton.gameObject.SetActive(false);
        }

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

        Debug.Log("Mission Complete 표시 - 3초 대기");

        yield return new WaitForSeconds(3f);

        sharedMissionText.gameObject.SetActive(false);

        Debug.Log("Mission Complete 숨김");
    }
}