using UnityEngine;
using TMPro;

public class OrderText : MonoBehaviour
{
    [Header("주문 설정")]
    public string[] orderNames; // Order Manager의 Element 배열

    [Header("UI 설정")]
    public Transform itemContainer; // 흰색 박스의 Transform
    public GameObject textPrefab; // TextMeshProUGUI가 있는 프리팹

    void Start()
    {
        DisplayOrderItems();
    }

    public void DisplayOrderItems()
    {
        // 기존 텍스트 삭제
        foreach (Transform child in itemContainer)
        {
            Destroy(child.gameObject);
        }

        // 각 주문 항목 표시
        foreach (string itemName in orderNames)
        {
            GameObject newText = Instantiate(textPrefab, itemContainer);
            TextMeshProUGUI tmp = newText.GetComponent<TextMeshProUGUI>();
            tmp.text = itemName;
        }
    }
}