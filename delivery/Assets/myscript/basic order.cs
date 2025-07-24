using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BasicOrder : MonoBehaviour
{
    [Header("UI References")]
    public ScrollRect scrollRect; // ScrollRect 컴포넌트 (스크롤 필요시)
    public Transform orderContainer; // order 버튼들이 들어갈 컨테이너
    public GameObject orderButtonPrefab; // order 버튼 프리팹

    [Header("Random Order Settings")]
    public int maxOrders = 5; // 최대 order 개수
    public float minSpawnTime = 2f; // 최소 생성 간격 (초)
    public float maxSpawnTime = 8f; // 최대 생성 간격 (초)
    public float orderButtonWidth = 200f; // 각 order 버튼의 너비
    public float orderButtonHeight = 100f; // 각 order 버튼의 높이
    public float spacing = 10f; // 버튼 간 간격

    [Header("Order Types")]
    public string[] orderTypes = { "피자", "햄버거", "치킨", "파스타", "샐러드", "스테이크", "초밥", "라면" };

    private List<GameObject> orderButtons = new List<GameObject>();
    private int orderCount = 0;
    private bool isGenerating = false;

    void Start()
    {
        InitializeSystem();
        StartRandomOrderGeneration();
    }

    // 스크롤을 끝으로 이동
    IEnumerator ScrollToEnd()
    {
        yield return new WaitForEndOfFrame();
        if (scrollRect != null)
            scrollRect.horizontalNormalizedPosition = 1f;
    }

    // 특정 order 제거 (인덱스로)
    public void RemoveOrderByIndex(int index)
    {
        if (index >= 0 && index < orderButtons.Count)
        {
            GameObject orderToRemove = orderButtons[index];
            orderButtons.RemoveAt(index);
            Destroy(orderToRemove);

            Debug.Log($"Order 인덱스 {index} 제거됨. 남은 주문 수: {orderButtons.Count}/{maxOrders}");
        }
    }

    // 첫 번째 order 제거
    public void RemoveFirstOrder()
    {
        if (orderButtons.Count > 0)
        {
            RemoveOrderByIndex(0);
        }
    }

    // 마지막 order 제거
    public void RemoveLastOrder()
    {
        if (orderButtons.Count > 0)
        {
            RemoveOrderByIndex(orderButtons.Count - 1);
        }
    }

    // 시스템 초기화
    void InitializeSystem()
    {
        // ScrollRect 설정 (있는 경우)
        if (scrollRect != null)
        {
            scrollRect.horizontal = true;
            scrollRect.vertical = false;

            // Content 크기 자동 조정
            if (orderContainer.GetComponent<ContentSizeFitter>() == null)
            {
                ContentSizeFitter sizeFitter = orderContainer.gameObject.AddComponent<ContentSizeFitter>();
                sizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                sizeFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
            }
        }

        // HorizontalLayoutGroup 설정
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

    // 랜덤 order 생성 시작
    public void StartRandomOrderGeneration()
    {
        if (!isGenerating)
        {
            isGenerating = true;
            StartCoroutine(RandomOrderCoroutine());
        }
    }

    // 랜덤 order 생성 중지
    public void StopRandomOrderGeneration()
    {
        isGenerating = false;
        StopAllCoroutines();
    }

    // 랜덤 order 생성 코루틴
    IEnumerator RandomOrderCoroutine()
    {
        while (isGenerating)
        {
            // 현재 order 개수가 최대치보다 적을 때만 생성
            if (orderButtons.Count < maxOrders)
            {
                // 랜덤 시간 대기
                float waitTime = Random.Range(minSpawnTime, maxSpawnTime);
                yield return new WaitForSeconds(waitTime);

                // order 생성
                if (isGenerating && orderButtons.Count < maxOrders)
                {
                    CreateRandomOrder();
                }
            }
            else
            {
                // 최대 개수에 도달하면 잠시 대기
                yield return new WaitForSeconds(1f);
            }
        }
    }

    // 랜덤 order 생성
    void CreateRandomOrder()
    {
        if (orderButtonPrefab == null || orderContainer == null)
        {
            Debug.LogError("OrderButtonPrefab 또는 OrderContainer가 설정되지 않았습니다!");
            return;
        }

        // 새 order 버튼 생성
        GameObject newOrderButton = Instantiate(orderButtonPrefab, orderContainer);

        // 버튼 크기 설정
        RectTransform rectTransform = newOrderButton.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.sizeDelta = new Vector2(orderButtonWidth, orderButtonHeight);
        }

        // 랜덤 order 타입 선택
        string randomOrderType = orderTypes[Random.Range(0, orderTypes.Length)];

        // 버튼 텍스트 설정
        Text buttonText = newOrderButton.GetComponentInChildren<Text>();
        if (buttonText != null)
        {
            orderCount++;
            buttonText.text = $"{randomOrderType}\\nOrder #{orderCount}";
        }

        // 버튼 비활성화 (클릭 불가능하게 설정)
        Button button = newOrderButton.GetComponent<Button>();
        if (button != null)
        {
            button.interactable = false;
        }

        // 리스트에 추가
        orderButtons.Add(newOrderButton);

        // 스크롤 위치 조정 (3개 이상일 때)
        if (scrollRect != null && orderButtons.Count > 3)
        {
            Canvas.ForceUpdateCanvases();
            StartCoroutine(ScrollToEnd());
        }

        Debug.Log($"{randomOrderType} Order #{orderCount} 생성됨. 현재 주문 수: {orderButtons.Count}/{maxOrders}");
    }

    // 모든 order 제거
    public void ClearAllOrders()
    {
        foreach (GameObject orderButton in orderButtons)
        {
            if (orderButton != null)
                Destroy(orderButton);
        }

        orderButtons.Clear();
        Debug.Log("모든 주문이 제거되었습니다.");
    }

    // 수동으로 랜덤 order 생성 (테스트용)
    [ContextMenu("Create Random Order")]
    public void ManualCreateOrder()
    {
        if (orderButtons.Count < maxOrders)
        {
            CreateRandomOrder();
        }
        else
        {
            Debug.Log("최대 주문 개수에 도달했습니다!");
        }
    }

    // 현재 order 개수 반환
    public int GetOrderCount()
    {
        return orderButtons.Count;
    }

    // 최대 order 개수 설정
    public void SetMaxOrders(int newMaxOrders)
    {
        maxOrders = Mathf.Max(1, newMaxOrders);
    }

    // 생성 간격 설정
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