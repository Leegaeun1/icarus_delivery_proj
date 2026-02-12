using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class OrrerManager : MonoBehaviour
{
    [Header("UI References")]
    // 아이콘이 버튼 역할을 해야 하므로 Button 컴포넌트를 직접 참조하는 것이 좋습니다.
    public Button receiptIconBtn;
    public GameObject receiptPopup;
    public Button receiptCloseButton;
    public TextMeshProUGUI orderTextUI;

    [Header("Order Settings")]
    public string[] possibleOrders = new string[]
    {
        "피클이랑 칠리 빼고 크라켄 샌드위치랑 눈알 스무디 주세요.",
        "양배추 빼고 히드라 버거 하나 주세요.",
        "갤럭시 음료랑 불가사리 쿠키 주세요. 아, 머스타드는 빼고요.",
        "마녀 스프랑 불꽃 쿠키 주세요."
    };

    public List<string> activeOrders = new List<string>();
    private int currentViewIndex = 0;

    void Start()
    {
        // 1. 초기 UI 상태 설정
        if (receiptPopup != null) receiptPopup.SetActive(false);
        if (receiptIconBtn != null) receiptIconBtn.gameObject.SetActive(false);

        // 2. [UI 연결] 인스펙터 대신 코드에서 리스너를 직접 연결 (더 안전함)
        if (receiptIconBtn != null)
        {
            receiptIconBtn.onClick.RemoveAllListeners();
            receiptIconBtn.onClick.AddListener(OpenPopup);
        }

        if (receiptCloseButton != null)
        {
            receiptCloseButton.onClick.RemoveAllListeners();
            receiptCloseButton.onClick.AddListener(ClosePopup);
        }

        // 3. 주문 생성 시작
        StartCoroutine(SpawnOrderRoutine());
    }

    IEnumerator SpawnOrderRoutine()
    {
        yield return new WaitForSeconds(Random.Range(1.0f, 7.0f));
        GenerateRandomOrders();
    }

    void GenerateRandomOrders()
    {
        int orderCount = Random.Range(1, 5);
        activeOrders.Clear();
        for (int i = 0; i < orderCount; i++)
        {
            activeOrders.Add(possibleOrders[Random.Range(0, possibleOrders.Length)]);
        }

        if (receiptIconBtn != null) receiptIconBtn.gameObject.SetActive(true);
    }

    // ================= [UI 컨트롤] =================

    public void OpenPopup()
    {
        // 모든 조건이 맞으면 4번이 뜨고 화면에 나타남
        receiptPopup.SetActive(true);
        // (선택사항) 아이콘 숨기기
        if (receiptIconBtn != null) receiptIconBtn.gameObject.SetActive(false);

        currentViewIndex = 0;
        UpdateOrderUI();
    }

    public void ClosePopup()
    {
        receiptPopup.SetActive(false);
        if (activeOrders.Count > 0)
        {
            receiptIconBtn.gameObject.SetActive(true);
        }
    }

    public void NextOrder()
    {
        if (activeOrders.Count == 0) return;
        currentViewIndex = (currentViewIndex + 1) % activeOrders.Count;
        UpdateOrderUI();
    }

    void UpdateOrderUI()
    {
        if (activeOrders.Count > currentViewIndex && orderTextUI != null)
        {
            orderTextUI.text = activeOrders[currentViewIndex];
        }
    }

    // ================= [요리 시작 및 파싱 - 그대로 유지됨] =================

    public void OnClickStartCooking()
    {
        // [범인 1] 주문이 하나도 없으면 여기서 그냥 끝남 (로그도 안 찍힘)
        if (activeOrders.Count == 0)
        {
            Debug.Log("주문이 없어서 시작하지 못함"); // 확인용 로그 추가
            return;
        }
        string selectedOrder = activeOrders[currentViewIndex];

        // 싱글톤 데이터 세팅 (데이터 전달을 위한 초기화)
        if (ReadSpreadSheets.Instance != null)
        {
            ReadSpreadSheets.Instance.ClearRequests();
            ParseOrder(selectedOrder); // 아래 파싱 로직 호출
                                       // 1. 포함된 재료(Include) 확인
            string includeLog = string.Join(", ", ReadSpreadSheets.Instance.CurrentRequest_Include);
            Debug.Log($"<color=green>[주문 데이터 확인]</color> 포함해야 할 재료: {includeLog}");

            // 2. 제외된 재료(Exclude) 확인 (필요하다면)
            string excludeLog = string.Join(", ", ReadSpreadSheets.Instance.CurrentRequest_Exclude);
            Debug.Log($"<color=red>[주문 데이터 확인]</color> 빼야 할 재료: {excludeLog}");
            //SceneManager.LoadScene("CookScene");
            Debug.Log("씬 이동 막고 테스트 중: 여기까지 코드가 오나요?");
        }
        else
        {
            Debug.LogError("ReadSpreadSheets 인스턴스를 찾을 수 없습니다!");
        }
    }

    void ParseOrder(string sentence)
    {
        var vocab = ReadSpreadSheets.Instance.menuVocab;
        if (vocab == null || vocab.Count == 0) return;

        // "빼고", "없이" 단어를 기준으로 앞뒤 분리
        string[] parts = sentence.Split(new string[] { "빼고", "없이", "제외" }, System.StringSplitOptions.RemoveEmptyEntries);

        string excludePart = parts.Length > 1 ? parts[0] : "";
        string includePart = parts.Length > 1 ? parts[1] : parts[0];

        foreach (var kvp in vocab)
        {
            string korName = kvp.Key;
            string engID = kvp.Value;

            // 1. 제외 리스트에 추가
            if (excludePart.Contains(korName))
            {
                ReadSpreadSheets.Instance.CurrentRequest_Exclude.Add(engID);
            }
            // 2. 포함 리스트에 추가
            else if (includePart.Contains(korName))
            {
                ReadSpreadSheets.Instance.CurrentRequest_Include.Add(engID);
                ReadSpreadSheets.Instance.currentRequestName.Add(engID);
            }
        }
    }
}