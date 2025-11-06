using UnityEngine;
using UnityEngine.UI;

public class ButtonKeeper : MonoBehaviour
{
    private Button button;
    private Order linkedOrder;

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
            }
        }
    }

    void OnButtonClick()
    {
        if (linkedOrder != null && !linkedOrder.isCompleted)
        {
            linkedOrder.isCompleted = true;
            button.interactable = false;
            Debug.Log("버튼 눌림 - 눌린 상태 유지");
        }
    }
}