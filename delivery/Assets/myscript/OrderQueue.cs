using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrderQueueSystem : MonoBehaviour
{
    public static OrderQueueSystem Instance;

    [Header("UI & Managers")]
    public GameObject pendingIcon;
    public ManManager manManager;

    [Header("Order Settings")]
    public float timeLimit = 10.0f;

    [Header("Auto Generation Settings")]
    [SerializeField] private int maxTotalOrders = 3; // 총 생성할 주문 수
    [SerializeField] private float minInterval = 5.0f; // 최소 간격
    [SerializeField] private float maxInterval = 10.0f; // 최대 간격

    private Queue<string> orderQueue = new Queue<string>();
    private bool isWorking = false;
    private int generatedCount = 0; // 현재까지 생성된 주문 수
    private Coroutine currentOrderCoroutine;

    void Awake() => Instance = this;

    void Start()
    {
        if (pendingIcon != null) pendingIcon.SetActive(false);

        // [추가] 게임 시작 시 주문 생성 루틴 시작
        StartCoroutine(GenerateOrdersRoutine());
    }

    // [추가] 랜덤 주문 생성 루틴
    IEnumerator GenerateOrdersRoutine()
    {
        // 1. 첫 생성 대기 (0~3초)
        yield return new WaitForSeconds(Random.Range(0f, 3.0f));

        while (generatedCount < maxTotalOrders)
        {
            generatedCount++;
            string orderName = "Order #" + generatedCount;

            // 기존에 잘 만들어두신 AddNewOrder를 호출하여 큐 로직 실행
            AddNewOrder(orderName);

            // 3개가 다 생성되었다면 루틴 종료
            if (generatedCount >= maxTotalOrders) break;

            // 2. 다음 주문까지 랜덤 대기 (5~10초)
            float nextWait = Random.Range(minInterval, maxInterval);
            yield return new WaitForSeconds(nextWait);
        }
    }

    // --- 기존 로직 유지 ---

    public void AddNewOrder(string orderName)
    {
        if (isWorking)
        {
            orderQueue.Enqueue(orderName);
            UpdateIcon();
            Debug.Log($"<color=yellow>대기열 추가:</color> {orderName}");
        }
        else
        {
            currentOrderCoroutine = StartCoroutine(ProcessOrderRoutine(orderName));
        }
    }

    IEnumerator ProcessOrderRoutine(string orderName)
    {
        isWorking = true;
        ButtonKeeper.ForceReset();

        if (manManager != null) manManager.SpawnMan(orderName);

        Debug.Log($"<color=cyan>주문 시작:</color> {orderName}");

        yield return new WaitForSeconds(timeLimit);

        Debug.Log($"<color=red>시간 초과!</color>");
        MoveToNextOrder();
    }

    public void CompleteOrderSuccess()
    {
        if (isWorking)
        {
            if (currentOrderCoroutine != null) StopCoroutine(currentOrderCoroutine);
            MoveToNextOrder();
        }
    }

    private void MoveToNextOrder()
    {
        isWorking = false;
        if (manManager != null) manManager.DestroyMan();

        if (orderQueue.Count > 0)
        {
            string nextOrder = orderQueue.Dequeue();
            UpdateIcon();
            currentOrderCoroutine = StartCoroutine(ProcessOrderRoutine(nextOrder));
        }
        else
        {
            if (pendingIcon != null) pendingIcon.SetActive(false);
        }
    }

    private void UpdateIcon()
    {
        if (pendingIcon != null) pendingIcon.SetActive(orderQueue.Count > 0);
    }
}