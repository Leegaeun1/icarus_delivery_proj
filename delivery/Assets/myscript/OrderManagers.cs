using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 1. 데이터 클래스 (스크립트 상단에 위치)
[System.Serializable]
public class Order
{
    public string orderName;
    public float orderTime;
    public bool isCompleted;

    public Order(string orderName, float time)
    {
        this.orderName = orderName;
        this.orderTime = time;
        this.isCompleted = false;
    }
}

// 2. 매니저 클래스
public class OrderManagers : MonoBehaviour
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

    public void CreateNewOrder()
    {
        if (allOrders.Count >= maxOrderCount) return;

        string randomOrderName = orderNames[Random.Range(0, orderNames.Length)];
        Order newOrder = new Order(randomOrderName, Time.time);
        allOrders.Add(newOrder);

        totalPages = Mathf.CeilToInt((float)allOrders.Count / maxOrdersPerPage);

        // 마지막 페이지를 보고 있거나 UI가 부족할 때 갱신
        if (currentPage == totalPages - 1 || orderUIObjects.Count < maxOrdersPerPage)
        {
            UpdateOrderUI();
        }

        UpdatePageButtons();
    }

    void UpdateOrderUI()
    {
        // 기존 UI 제거
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

        // [해결 포인트] ButtonKeeper 컴포넌트를 찾아서 데이터 전달
        ButtonKeeper keeper = orderUI.GetComponent<ButtonKeeper>();
        if (keeper != null)
        {
            // ButtonKeeper 스크립트에 public void Initialize(Order order)가 있어야 함
            keeper.Initialize(order);
        }
        else
        {
            Debug.LogWarning($"{orderPrefab.name}에 ButtonKeeper 스크립트가 없습니다!");
        }
    }

    // ... (이하 페이지 버튼 로직 동일)
    void UpdatePageButtons()
    {
        if (nextPageButton != null) nextPageButton.gameObject.SetActive(currentPage < totalPages - 1);
        if (prevPageButton != null) prevPageButton.gameObject.SetActive(currentPage > 0);
    }

    public void NextPage() { if (currentPage < totalPages - 1) { currentPage++; UpdateOrderUI(); UpdatePageButtons(); } }
    public void PrevPage() { if (currentPage > 0) { currentPage--; UpdateOrderUI(); UpdatePageButtons(); } }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) CreateNewOrder();
    }
}