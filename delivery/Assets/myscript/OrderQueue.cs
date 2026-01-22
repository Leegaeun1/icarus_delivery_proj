using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrderQueueSystem : MonoBehaviour
{
    [Header("UI Settings")]
    public GameObject pendingIcon; // 주문 밀림(대기열) 아이콘

    [Header("Order Settings")]
    public float timeLimit = 5.0f; // 주문당 제한 시간

    private Queue<string> orderQueue = new Queue<string>();
    private bool isWorking = false;
    private Coroutine currentOrderCoroutine; // 실행 중인 타이머 루틴 제어

    void Start()
    {
        // 시작 시 아이콘 비활성화
        if (pendingIcon != null) pendingIcon.SetActive(false);
    }

    // 1. 새로운 주문 추가
    public void AddNewOrder(string orderName)
    {
        if (isWorking)
        {
            // 이미 처리 중이면 대기열에 추가
            orderQueue.Enqueue(orderName);
            UpdateIcon();
            Debug.Log($"<color=yellow>대기열 추가:</color> {orderName} (대기: {orderQueue.Count})");
        }
        else
        {
            // 처리 중이 아니면 즉시 주문 프로세스 시작
            currentOrderCoroutine = StartCoroutine(ProcessOrderRoutine(orderName));
        }
    }

    // 2. 핵심 주문 처리 루틴 (타이머)
    IEnumerator ProcessOrderRoutine(string orderName)
    {
        isWorking = true;

        // [핵심] 주문 시작 시 이전 버튼 상태들을 강제로 초기화
        ButtonKeeper.ForceReset();

        Debug.Log($"<color=cyan>주문 시작:</color> {orderName} (제한시간: {timeLimit}초)");

        // 설정된 시간만큼 대기 (플레이어가 CompleteOrderSuccess를 호출하지 않으면 계속 진행)
        yield return new WaitForSeconds(timeLimit);

        // --- 여기 아래는 시간 내에 완료하지 못했을 때(실패) 실행됨 ---
        Debug.Log($"<color=red>시간 초과!</color> '{orderName}' 주문이 실패 처리되었습니다.");

        // 시간 초과 시에도 버튼 상태를 리셋해줘야 다음 주문을 깨끗하게 시작할 수 있음
        ButtonKeeper.ForceReset();

        MoveToNextOrder();
    }

    // 3. 주문 성공 시 호출 (ButtonKeeper에서 미션 완료 시 호출함)
    public void CompleteOrderSuccess()
    {
        if (isWorking)
        {
            Debug.Log("<color=green>주문 성공 완료!</color>");

            // 현재 실행 중인 5초 타이머(코루틴)를 강제로 멈춰서 '시간 초과' 로직 방지
            if (currentOrderCoroutine != null)
                StopCoroutine(currentOrderCoroutine);

            // 성공 시에는 ButtonKeeper 내부에서 이미 Reset을 호출하거나 
            // 다음 주문 시작 시 ForceReset이 실행되므로 바로 다음 주문으로 이동
            MoveToNextOrder();
        }
    }

    // 4. 다음 주문으로 넘어가는 공통 로직
    private void MoveToNextOrder()
    {
        isWorking = false;

        if (orderQueue.Count > 0)
        {
            string nextOrder = orderQueue.Dequeue();
            UpdateIcon();
            // 다음 주문 루틴 실행
            currentOrderCoroutine = StartCoroutine(ProcessOrderRoutine(nextOrder));
        }
        else
        {
            // 대기열이 비었으면 아이콘 숨김
            if (pendingIcon != null) pendingIcon.SetActive(false);
            Debug.Log("<color=white>모든 주문 처리가 끝났습니다.</color>");
        }
    }

    // 대기열 아이콘 상태 업데이트
    private void UpdateIcon()
    {
        if (pendingIcon != null)
        {
            pendingIcon.SetActive(orderQueue.Count > 0);
        }
    }
}