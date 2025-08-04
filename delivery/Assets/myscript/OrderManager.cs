using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OrderManager : MonoBehaviour
{
    [Header("주문 설정")]
    public float minOrderInterval = 5f;
    public float maxOrderInterval = 15f;
    public string[] orderNames = { "ramen", "pizza", "burger", "sushi" };

    [Header("UI 설정")]
    public Transform orderContainer;
    public GameObject orderPrefab;
    public Button nextPageButton;
    public Button prevPageButton;
    public int maxOrdersPerPage = 4;

    private List<Order> allOrders = new List<Order>();
    private List<GameObject> orderUIObjects = new List<GameObject>();
    private int currentPage = 0;
    private int totalPages = 1;
    private bool autoPageNavigation = true;

    void Start()
    {
        StartCoroutine(GenerateRandomOrders());

        if (nextPageButton != null)
            nextPageButton.onClick.AddListener(NextPage);
        if (prevPageButton != null)
            prevPageButton.onClick.AddListener(PrevPage);

        UpdatePageButtons();
    }

    IEnumerator GenerateRandomOrders()
    {
        while (true)
        {
            float waitTime = Random.Range(minOrderInterval, maxOrderInterval);
            yield return new WaitForSeconds(waitTime);
            CreateNewOrder();
        }
    }

    void CreateNewOrder()
    {
        string randomOrderName = orderNames[Random.Range(0, orderNames.Length)];
        Order newOrder = new Order(randomOrderName, Time.time);
        allOrders.Add(newOrder);

        totalPages = Mathf.CeilToInt((float)allOrders.Count / maxOrdersPerPage);

        if (autoPageNavigation && allOrders.Count > maxOrdersPerPage)
        {
            currentPage = totalPages - 1;
        }

        UpdateOrderUI();
        UpdatePageButtons();

        Debug.Log("새 주문 생성: " + randomOrderName);
    }

    void UpdateOrderUI()
    {
        foreach (GameObject orderUI in orderUIObjects)
        {
            if (orderUI != null)
                DestroyImmediate(orderUI);
        }
        orderUIObjects.Clear();

        int startIndex = currentPage * maxOrdersPerPage;
        int endIndex = Mathf.Min(startIndex + maxOrdersPerPage, allOrders.Count);

        for (int i = startIndex; i < endIndex; i++)
        {
            CreateOrderUI(allOrders[i]);
        }
    }

    void CreateOrderUI(Order order)
    {
        if (orderPrefab == null || orderContainer == null) return;

        GameObject orderUI = Instantiate(orderPrefab, orderContainer);
        orderUIObjects.Add(orderUI);

        Text orderText = orderUI.GetComponentInChildren<Text>();
        if (orderText != null)
        {
            orderText.text = order.orderName;
        }

        Button orderButton = orderUI.GetComponent<Button>();
        if (orderButton != null)
        {
            orderButton.onClick.AddListener(() => CompleteOrder(order));
        }
    }

    public void CompleteOrder(Order order)
    {
        allOrders.Remove(order);

        totalPages = Mathf.Max(1, Mathf.CeilToInt((float)allOrders.Count / maxOrdersPerPage));

        if (currentPage >= totalPages)
        {
            currentPage = Mathf.Max(0, totalPages - 1);
            autoPageNavigation = true;
        }

        UpdateOrderUI();
        UpdatePageButtons();

        Debug.Log("주문 완료: " + order.orderName);
    }

    void UpdatePageButtons()
    {
        if (nextPageButton != null)
        {
            nextPageButton.gameObject.SetActive(currentPage < totalPages - 1);
        }

        if (prevPageButton != null)
        {
            prevPageButton.gameObject.SetActive(currentPage > 0);
        }
    }

    public void NextPage()
    {
        if (currentPage < totalPages - 1)
        {
            currentPage++;
            autoPageNavigation = false;
            UpdateOrderUI();
            UpdatePageButtons();
        }
    }

    public void PrevPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            autoPageNavigation = false;
            UpdateOrderUI();
            UpdatePageButtons();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            CreateNewOrder();
        }
    }
}