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
    [SerializeField] private int maxTotalOrders = 3;
    [SerializeField] private float minInterval = 5.0f;
    [SerializeField] private float maxInterval = 10.0f;

    private Queue<string> orderQueue = new Queue<string>();
    private bool isWorking = false;
    private int generatedCount = 0;
    private Coroutine currentOrderCoroutine;

    void Awake() => Instance = this;

    void Start()
    {
        if (pendingIcon != null) pendingIcon.SetActive(false);

        StartCoroutine(GenerateOrdersRoutine());
    }

    IEnumerator GenerateOrdersRoutine()
    {
        yield return new WaitForSeconds(Random.Range(0f, 3.0f));

        while (generatedCount < maxTotalOrders)
        {
            generatedCount++;
            string orderName = "Order #" + generatedCount;

            var sheet = GameObject.Find("sheet")?.GetComponent<ReadSpreadSheets>();
            if (sheet != null)
            {
                sheet.currentRequestName.Clear();
                sheet.currentRequestName.Add(orderName);
                Debug.Log($"<color=yellow>데이터 생성 완료:</color> {orderName}");
            }

            // AddNewOrder(orderName);

            if (generatedCount >= maxTotalOrders) break;


            float nextWait = Random.Range(minInterval, maxInterval);
            yield return new WaitForSeconds(nextWait);
        }
    }

    // --- 비활성화 및 원형 보존 ---

    /* 
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
        // ButtonKeeper.ForceReset();

        // if (manManager != null) manManager.SpawnMan(orderName);

        Debug.Log($"<color=cyan>주문 데이터 준비됨:</color> {orderName}");

        // yield return new WaitForSeconds(timeLimit);

        // Debug.Log($"<color=red>시간 초과!</color>");
        // MoveToNextOrder();
        yield return null;
    }
    */
    public void CompleteOrderSuccess()
    {
        /*if (isWorking)
        {
            if (currentOrderCoroutine != null) StopCoroutine(currentOrderCoroutine);
            MoveToNextOrder();
        }*/
    }
    /*
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
    */
}