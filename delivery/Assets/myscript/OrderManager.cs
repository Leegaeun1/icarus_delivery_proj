using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OrderManager : MonoBehaviour
{
    [Header("주문 설정")]
    public float minOrderInterval = 5f;
    public float maxOrderInterval = 15f;
    public string[] orderNames = { "ramen", "pizza", "burger", "sushi" };
    public int maxOrderCount = 5;

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
            yield return new WaitForSeconds(Random.Range(minOrderInterval, maxOrderInterval));
            CreateNewOrder();
        }
    }

    void CreateNewOrder()
    {
        if (allOrders.Count >= maxOrderCount) return;

        string randomOrderName = orderNames[Random.Range(0, orderNames.Length)];
        Order newOrder = new Order(randomOrderName, Time.time);
        allOrders.Add(newOrder);

        totalPages = Mathf.CeilToInt((float)allOrders.Count / maxOrdersPerPage);
        UpdatePageButtons();

        if (orderUIObjects.Count < maxOrdersPerPage)
        {
            CreateOrderUI(newOrder);
        }

        UpdatePageButtons();
        Debug.Log("새 주문 생성: " + randomOrderName);
    }

    void UpdateOrderUI()
    {
        foreach (GameObject ui in orderUIObjects)
            if (ui != null) Destroy(ui);
        orderUIObjects.Clear();

        int start = currentPage * maxOrdersPerPage;
        int end = Mathf.Min(start + maxOrdersPerPage, allOrders.Count);

        for (int i = start; i < end; i++)
            CreateOrderUI(allOrders[i]);
    }

    void CreateOrderUI(Order order)
    {
        if (orderPrefab == null || orderContainer == null) return;

        GameObject orderUI = Instantiate(orderPrefab, orderContainer);
        orderUIObjects.Add(orderUI);

        TextMeshProUGUI tmpText = orderUI.GetComponentInChildren<TextMeshProUGUI>();
        if (tmpText != null) tmpText.text = order.orderName;

        ButtonKeeper keeper = orderUI.GetComponent<ButtonKeeper>();
        if (keeper != null) keeper.Initialize(order);
    }

    void UpdatePageButtons()
    {
        if (nextPageButton != null)
            nextPageButton.gameObject.SetActive(currentPage < totalPages - 1);
        if (prevPageButton != null)
            prevPageButton.gameObject.SetActive(currentPage > 0);
    }

    public void NextPage()
    {
        if (currentPage < totalPages - 1)
        {
            currentPage++;
            UpdateOrderUI();
            UpdatePageButtons();
        }
    }

    public void PrevPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            UpdateOrderUI();
            UpdatePageButtons();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            CreateNewOrder();
    }
}