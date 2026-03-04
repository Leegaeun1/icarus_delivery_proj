using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
// SceneManager는 MenutoCook에서 사용하므로 여기선 지웠습니다.

public class OrrderManager : MonoBehaviour
{
    [Header("UI References")]
    public Button receiptIconBtn;
    public GameObject receiptPopup;
    public Button receiptCloseButton;
    public TextMeshProUGUI orderTextUI;

    [Header("Order Settings")]
    public List<string> possibleOrders = new List<string>(){};
    public List<string> activeOrders = new List<string>();
    private int currentViewIndex = 0;
    private void OnValidate()
    {
        // 인스펙터 리스트가 비어있을 때만 코드의 값을 강제로 넣고 싶다면
        if (possibleOrders == null || possibleOrders.Count == 0)
        {
            possibleOrders = new List<string>()
            {
                "피클이랑 칠리 빼고 크라켄 샌드위치랑 눈알 스무디 주세요.",
                "양배추 빼고 히드라 샌드위치 하나 주세요.",
                "칠리 빼고 마녀 샌드위치랑 은하수 스무디랑 불가사리 쿠키 주세요.",
                "불사조 샌드위치랑 불꽃 쿠키 주세요.",
                "머스타드 빼고 해파리 샌드위치 주세요."
            };
        }
    }
    void Start()
    {
        // 1. 초기 UI 상태 설정
        if (receiptPopup != null) receiptPopup.SetActive(false);
        if (receiptIconBtn != null) receiptIconBtn.gameObject.SetActive(false);

        // 2. 버튼 리스너 연결
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

        // 3. 주문 생성 루틴 시작
        StartCoroutine(SpawnOrderRoutine());
    }

    IEnumerator SpawnOrderRoutine()
    {
        // [수정] 사전 데이터가 로드될 때까지 대기 (최대 5초)
        float timeout = 5f;
        while (ReadSpreadSheets.Instance.menuVocab.Count == 0 && timeout > 0)
        {
            yield return new WaitForSeconds(0.5f);
            timeout -= 0.5f;
        }

        yield return new WaitForSeconds(Random.Range(1.0f, 3.0f)); // 약간의 랜덤 대기
        GenerateRandomOrders();
    }

    void GenerateRandomOrders()
    {
        int orderCount = Random.Range(1, 5);
        activeOrders.Clear();
        for (int i = 0; i < orderCount; i++)
        {
            if (possibleOrders.Count > 0)
            {
                activeOrders.Add(possibleOrders[Random.Range(0, possibleOrders.Count)]);
            }
        }

        if (receiptIconBtn != null) receiptIconBtn.gameObject.SetActive(true);
    }

    // ================= [UI 컨트롤] =================

    public void OpenPopup()
    {
        receiptPopup.SetActive(true);
        if (receiptIconBtn != null) receiptIconBtn.gameObject.SetActive(false);

        currentViewIndex = 0;
        UpdateOrderUI();
    }

    public void ClosePopup()
    {
        receiptPopup.SetActive(false);
        OnClickStartCooking();
        if (activeOrders.Count > 0 && receiptIconBtn != null)
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

    // ================= [요리 시작 및 데이터 처리 (스마트 파싱 로직)] =================

    public void OnClickStartCooking()
    {
        if (activeOrders.Count == 0) return;
        if (ReadSpreadSheets.Instance == null) return;

        string selectedOrder = activeOrders[currentViewIndex];

        // 1. 데이터 초기화
        ReadSpreadSheets.Instance.ClearRequests();

        // 2. 주문장 분석 (파싱)
        ParseOrder(selectedOrder);
    }

    // 임시 데이터를 담아둘 작은 클래스
    private class ParsedItem
    {
        public string name;
        public string engID;
        public int index; // 문장에서 단어가 발견된 위치
    }

    void ParseOrder(string sentence)
    {
        var vocab = ReadSpreadSheets.Instance.menuVocab;

        // [디버깅 추가] 사전이 진짜 들어왔는지 확인
        if (vocab == null || vocab.Count == 0)
        {
            Debug.LogError($"[오류] 단어 사전이 비어있습니다! (현재 개수: 0)");
            return;
        }
        else
        {
            Debug.Log($"[점검] 단어 사전 로드됨 (총 {vocab.Count}개 단어)");
        }

        Debug.Log($"[파싱 시작] 원본 문장: {sentence}");

        // 1. 긴 단어부터 매칭되도록 정렬 (예: '해파리'보다 '해파리 샌드위치' 우선)
        List<string> sortedKeys = new List<string>(vocab.Keys);
        sortedKeys.Sort((a, b) => b.Length.CompareTo(a.Length));

        List<ParsedItem> foundItems = new List<ParsedItem>();
        string tempSentence = sentence; // 인덱스 검색용 임시 문장

        // 2. 문장에서 메뉴/재료 이름 찾기
        foreach (string korName in sortedKeys)
        {
            if (string.IsNullOrEmpty(korName)) continue;

            int idx = tempSentence.IndexOf(korName);
            if (idx != -1)
            {
                // 찾은 단어를 리스트에 보관
                foundItems.Add(new ParsedItem { name = korName, engID = vocab[korName], index = idx });

                // 중복 검색 방지를 위해 찾은 단어를 같은 길이의 별표(*)로 치환 
                // (길이를 유지해야 다른 단어들의 위치 인덱스가 꼬이지 않음)
                tempSentence = tempSentence.Replace(korName, new string('*', korName.Length));
            }
        }

        // 3. "빼고", "없이", "제외" 키워드가 등장하는 위치 찾기
        List<int> excludeIndexes = new List<int>();
        string[] excludeKeywords = { "빼고", "없이", "제외" };
        foreach (var keyword in excludeKeywords)
        {
            int idx = sentence.IndexOf(keyword);
            while (idx != -1)
            {
                excludeIndexes.Add(idx);
                idx = sentence.IndexOf(keyword, idx + keyword.Length);
            }
        }

        // 4. 찾은 아이템들을 문장에 나타난 순서대로 정렬
        foundItems.Sort((a, b) => a.index.CompareTo(b.index));

        // 5. 포함 / 제외 분류 로직 (한국어 문맥 파악)
        foreach (var item in foundItems)
        {
            bool isExclude = false;
            int closestExcludeIdx = -1;
            int minDistance = int.MaxValue;

            foreach (int exIdx in excludeIndexes)
            {
                if (exIdx > item.index && (exIdx - item.index) < minDistance)
                {
                    minDistance = exIdx - item.index;
                    closestExcludeIdx = exIdx;
                }
            }

            if (closestExcludeIdx != -1 && minDistance < 15) isExclude = true;

            if (isExclude)
            {
                ReadSpreadSheets.Instance.CurrentRequest_Exclude.Add(item.engID);
                Debug.Log($"[제외] {item.name} ({item.engID})");
            }
            else
            {
                // 만약 샌드위치를 주문했다면, 기본 재료들을 Include에 자동 추가
                if (item.name.Contains("샌드위치"))
                {
                    // 샌드위치 본체 ID 추가
                    ReadSpreadSheets.Instance.CurrentRequest_Include.Add(item.engID);

                    // 기본 재료 리스트
                    string[] defaultIngredients = { "chili", "cabbage", "pickle", "mustard" };

                    foreach (string ing in defaultIngredients)
                    {
                        // 이번 주문에서 '빼달라고 한(Exclude)' 재료가 아닐 때만 Include에 추가
                        bool isRequiredToExclude = false;
                        foreach (var exItem in foundItems)
                        {
                            if (exItem.engID == ing && sentence.Contains(exItem.name + " 빼고"))
                            {
                                isRequiredToExclude = true;
                                break;
                            }
                        }

                        if (!isRequiredToExclude)
                        {
                            ReadSpreadSheets.Instance.CurrentRequest_Include.Add(ing);
                        }
                    }
                }
                else
                {
                    // 샌드위치가 아닌 단품(쿠키, 스무디 등) 추가
                    ReadSpreadSheets.Instance.CurrentRequest_Include.Add(item.engID);
                }
                Debug.Log($"[포함] {item.name} ({item.engID})");
            }
        }

        Debug.Log($"[최종 파싱 완료] 포함: {ReadSpreadSheets.Instance.CurrentRequest_Include.Count}개 / 제외: {ReadSpreadSheets.Instance.CurrentRequest_Exclude.Count}개");
    }
}