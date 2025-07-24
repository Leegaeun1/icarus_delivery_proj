using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BasicOrder : MonoBehaviour
{
    [Header("UI References")]
    public ScrollRect scrollRect; // ScrollRect ������Ʈ (��ũ�� �ʿ��)
    public Transform orderContainer; // order ��ư���� �� �����̳�
    public GameObject orderButtonPrefab; // order ��ư ������

    [Header("Random Order Settings")]
    public int maxOrders = 5; // �ִ� order ����
    public float minSpawnTime = 2f; // �ּ� ���� ���� (��)
    public float maxSpawnTime = 8f; // �ִ� ���� ���� (��)
    public float orderButtonWidth = 200f; // �� order ��ư�� �ʺ�
    public float orderButtonHeight = 100f; // �� order ��ư�� ����
    public float spacing = 10f; // ��ư �� ����

    [Header("Order Types")]
    public string[] orderTypes = { "����", "�ܹ���", "ġŲ", "�Ľ�Ÿ", "������", "������ũ", "�ʹ�", "���" };

    private List<GameObject> orderButtons = new List<GameObject>();
    private int orderCount = 0;
    private bool isGenerating = false;

    void Start()
    {
        InitializeSystem();
        StartRandomOrderGeneration();
    }

    // ��ũ���� ������ �̵�
    IEnumerator ScrollToEnd()
    {
        yield return new WaitForEndOfFrame();
        if (scrollRect != null)
            scrollRect.horizontalNormalizedPosition = 1f;
    }

    // Ư�� order ���� (�ε�����)
    public void RemoveOrderByIndex(int index)
    {
        if (index >= 0 && index < orderButtons.Count)
        {
            GameObject orderToRemove = orderButtons[index];
            orderButtons.RemoveAt(index);
            Destroy(orderToRemove);

            Debug.Log($"Order �ε��� {index} ���ŵ�. ���� �ֹ� ��: {orderButtons.Count}/{maxOrders}");
        }
    }

    // ù ��° order ����
    public void RemoveFirstOrder()
    {
        if (orderButtons.Count > 0)
        {
            RemoveOrderByIndex(0);
        }
    }

    // ������ order ����
    public void RemoveLastOrder()
    {
        if (orderButtons.Count > 0)
        {
            RemoveOrderByIndex(orderButtons.Count - 1);
        }
    }

    // �ý��� �ʱ�ȭ
    void InitializeSystem()
    {
        // ScrollRect ���� (�ִ� ���)
        if (scrollRect != null)
        {
            scrollRect.horizontal = true;
            scrollRect.vertical = false;

            // Content ũ�� �ڵ� ����
            if (orderContainer.GetComponent<ContentSizeFitter>() == null)
            {
                ContentSizeFitter sizeFitter = orderContainer.gameObject.AddComponent<ContentSizeFitter>();
                sizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                sizeFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
            }
        }

        // HorizontalLayoutGroup ����
        if (orderContainer.GetComponent<HorizontalLayoutGroup>() == null)
        {
            HorizontalLayoutGroup layoutGroup = orderContainer.gameObject.AddComponent<HorizontalLayoutGroup>();
            layoutGroup.spacing = spacing;
            layoutGroup.childAlignment = TextAnchor.MiddleLeft;
            layoutGroup.childControlWidth = false;
            layoutGroup.childControlHeight = true;
            layoutGroup.childForceExpandWidth = false;
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.padding = new RectOffset(10, 10, 0, 0);
        }
    }

    // ���� order ���� ����
    public void StartRandomOrderGeneration()
    {
        if (!isGenerating)
        {
            isGenerating = true;
            StartCoroutine(RandomOrderCoroutine());
        }
    }

    // ���� order ���� ����
    public void StopRandomOrderGeneration()
    {
        isGenerating = false;
        StopAllCoroutines();
    }

    // ���� order ���� �ڷ�ƾ
    IEnumerator RandomOrderCoroutine()
    {
        while (isGenerating)
        {
            // ���� order ������ �ִ�ġ���� ���� ���� ����
            if (orderButtons.Count < maxOrders)
            {
                // ���� �ð� ���
                float waitTime = Random.Range(minSpawnTime, maxSpawnTime);
                yield return new WaitForSeconds(waitTime);

                // order ����
                if (isGenerating && orderButtons.Count < maxOrders)
                {
                    CreateRandomOrder();
                }
            }
            else
            {
                // �ִ� ������ �����ϸ� ��� ���
                yield return new WaitForSeconds(1f);
            }
        }
    }

    // ���� order ����
    void CreateRandomOrder()
    {
        if (orderButtonPrefab == null || orderContainer == null)
        {
            Debug.LogError("OrderButtonPrefab �Ǵ� OrderContainer�� �������� �ʾҽ��ϴ�!");
            return;
        }

        // �� order ��ư ����
        GameObject newOrderButton = Instantiate(orderButtonPrefab, orderContainer);

        // ��ư ũ�� ����
        RectTransform rectTransform = newOrderButton.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.sizeDelta = new Vector2(orderButtonWidth, orderButtonHeight);
        }

        // ���� order Ÿ�� ����
        string randomOrderType = orderTypes[Random.Range(0, orderTypes.Length)];

        // ��ư �ؽ�Ʈ ����
        Text buttonText = newOrderButton.GetComponentInChildren<Text>();
        if (buttonText != null)
        {
            orderCount++;
            buttonText.text = $"{randomOrderType}\\nOrder #{orderCount}";
        }

        // ��ư ��Ȱ��ȭ (Ŭ�� �Ұ����ϰ� ����)
        Button button = newOrderButton.GetComponent<Button>();
        if (button != null)
        {
            button.interactable = false;
        }

        // ����Ʈ�� �߰�
        orderButtons.Add(newOrderButton);

        // ��ũ�� ��ġ ���� (3�� �̻��� ��)
        if (scrollRect != null && orderButtons.Count > 3)
        {
            Canvas.ForceUpdateCanvases();
            StartCoroutine(ScrollToEnd());
        }

        Debug.Log($"{randomOrderType} Order #{orderCount} ������. ���� �ֹ� ��: {orderButtons.Count}/{maxOrders}");
    }

    // ��� order ����
    public void ClearAllOrders()
    {
        foreach (GameObject orderButton in orderButtons)
        {
            if (orderButton != null)
                Destroy(orderButton);
        }

        orderButtons.Clear();
        Debug.Log("��� �ֹ��� ���ŵǾ����ϴ�.");
    }

    // �������� ���� order ���� (�׽�Ʈ��)
    [ContextMenu("Create Random Order")]
    public void ManualCreateOrder()
    {
        if (orderButtons.Count < maxOrders)
        {
            CreateRandomOrder();
        }
        else
        {
            Debug.Log("�ִ� �ֹ� ������ �����߽��ϴ�!");
        }
    }

    // ���� order ���� ��ȯ
    public int GetOrderCount()
    {
        return orderButtons.Count;
    }

    // �ִ� order ���� ����
    public void SetMaxOrders(int newMaxOrders)
    {
        maxOrders = Mathf.Max(1, newMaxOrders);
    }

    // ���� ���� ����
    public void SetSpawnTimeRange(float minTime, float maxTime)
    {
        minSpawnTime = Mathf.Max(0.5f, minTime);
        maxSpawnTime = Mathf.Max(minSpawnTime + 0.5f, maxTime);
    }

    void OnDestroy()
    {
        StopRandomOrderGeneration();
    }
}