using UnityEngine;
using UnityEngine.UI;

public class FoodButton : MonoBehaviour
{
    private Button button;
    private bool isPressed = false;

    void Start()
    {
        button = GetComponent<Button>();

        if (button != null)
        {
            button.onClick.AddListener(OnButtonClick);
        }
    }

    void OnButtonClick()
    {
        if (!isPressed)
        {
            isPressed = true;
            button.interactable = false;
            Debug.Log("버튼 눌림 - 눌린 상태 유지");
        }
    }
}