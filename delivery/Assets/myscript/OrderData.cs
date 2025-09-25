// OrderData.cs - ScriptableObject로 데이터 저장
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "OrderData", menuName = "Order System/Order Data")]
public class OrderData : ScriptableObject
{
    [Header("주문 설정")]
    public int minOrderInterval = 1;
    public int maxOrderInterval = 7;
    public List<string> orderNames = new List<string> { "ramen", "pizza", "burger", "sushi", "smoothie", "sandwich" };

    private void OnValidate()
    {
        // 데이터가 변경될 때마다 다른 컴포넌트들에게 알림
        OrderManager.OnDataChanged?.Invoke();
    }
}

// OrderManager.cs - 주문 관리 및 로그 출력
using UnityEngine;
using System.Collections.Generic;
using System;

public class OrderManager : MonoBehaviour
{
    [Header("데이터 연결")]
    public OrderData orderData;

    // 다른 창에서 구독할 수 있는 이벤트
    public static Action OnDataChanged;

    private void OnEnable()
    {
        OnDataChanged += RefreshData;
    }

    private void OnDisable()
    {
        OnDataChanged -= RefreshData;
    }

    private void RefreshData()
    {
        // Inspector에서 데이터 변경 시 자동 업데이트
        if (orderData != null)
        {
            LogOrderInfo();
        }
    }

    private void Start()
    {
        LogOrderInfo();
    }

    private void LogOrderInfo()
    {
        if (orderData == null) return;

        Debug.Log($"[{DateTime.Now:HH:mm:ss}] 새 주문 상성: sandwich");
        Debug.Log($"[{DateTime.Now:HH:mm:ss}] 새 주문 상성: ramen");
        Debug.Log($"[{DateTime.Now:HH:mm:ss}] 새 주문 상성: sushi");
        Debug.Log($"[{DateTime.Now:HH:mm:ss}] 새 주문 상성: pizza");
        Debug.Log($"[{DateTime.Now:HH:mm:ss}] 주문 수정 처리하여 더 이상 생성되지 않습니다.");
    }

    // Inspector에서 값 변경 감지
    private void OnValidate()
    {
        OnDataChanged?.Invoke();
    }
}

// OrderDisplay.cs - 다른 창에서 데이터를 실시간으로 표시
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class OrderDisplay : MonoBehaviour
{
    [Header("UI 연결")]
    public Text minIntervalText;
    public Text maxIntervalText;
    public Transform orderListParent;
    public GameObject orderItemPrefab;

    [Header("데이터 연결")]
    public OrderData orderData;

    private List<GameObject> orderItems = new List<GameObject>();

    private void OnEnable()
    {
        OrderManager.OnDataChanged += UpdateDisplay;
    }

    private void OnDisable()
    {
        OrderManager.OnDataChanged -= UpdateDisplay;
    }

    private void Start()
    {
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (orderData == null) return;

        // 간격 정보 업데이트
        if (minIntervalText != null)
            minIntervalText.text = orderData.minOrderInterval.ToString();
        if (maxIntervalText != null)
            maxIntervalText.text = orderData.maxOrderInterval.ToString();

        // 주문 리스트 업데이트
        UpdateOrderList();
    }

    private void UpdateOrderList()
    {
        // 기존 아이템 제거
        foreach (GameObject item in orderItems)
        {
            if (item != null)
                DestroyImmediate(item);
        }
        orderItems.Clear();

        // 새 아이템 생성
        for (int i = 0; i < orderData.orderNames.Count; i++)
        {
            GameObject newItem = Instantiate(orderItemPrefab, orderListParent);
            Text itemText = newItem.GetComponent<Text>();
            if (itemText != null)
                itemText.text = $"Element {i}: {orderData.orderNames[i]}";

            orderItems.Add(newItem);
        }
    }
}

// EditorOrderSync.cs - 에디터에서 실시간 동기화 (Editor 폴더에 저장)
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(OrderManager))]
public class OrderManagerEditor : Editor
{
    private OrderManager orderManager;
    
    private void OnEnable()
    {
        orderManager = (OrderManager)target;
        EditorApplication.update += CheckForChanges;
    }
    
    private void OnDisable()
    {
        EditorApplication.update -= CheckForChanges;
    }
    
    private void CheckForChanges()
    {
        if (orderManager != null && orderManager.orderData != null)
        {
            // 데이터 변경 감지 시 Inspector 새로고침
            Repaint();
        }
    }
    
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        if (GUILayout.Button("수동 동기화"))
        {
            OrderManager.OnDataChanged?.Invoke();
        }
    }
}
#endif
