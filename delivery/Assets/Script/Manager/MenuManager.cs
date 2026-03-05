using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public GameObject check_menu;
    public GameObject select_menu;
    public GameObject[] special_menus; // 카드 프리팹들
    public RectTransform canvasRectTransform; // Canvas의 RectTransform
    public Transform cardStackParent; // 카드가 배치될 부모 오브젝트
    public TextMeshProUGUI correctName;
    [SerializeField] 
    private int cookie_cnt;
    private StageManage drink_cnt;


    [Header("카드 배치 설정")]
    public int cardsToDisplay; // 현재 스테이지에서 표시할 카드 수
    public float cardSpacing = 20f; // 카드와 카드 사이의 간격 (픽셀)
    public float dealInterval = 0.5f; // 카드가 펼쳐지는 시간 간격

    private List<GameObject> cardStack; // 더미에 쌓아둘 카드 리스트
    private int[] num; // 올바른 메뉴가 아닌 카드들의 인덱스
    private int correctCardIndex = -1; // 카드의 실제 special_menus 내 인덱스
    public string correctmenu;

    void Start()
    {
        // --- 필수 참조 체크 ---
        if (check_menu == null || select_menu == null)
        {
            Debug.LogError("[MenuManager] check_menu 또는 select_menu 참조가 누락되었습니다.");
            enabled = false;
            return;
        }

        if (cardStackParent == null)
        {
            Debug.LogError("[MenuManager] cardStackParent 참조가 없습니다.");
            enabled = false;
            return;
        }

        if (special_menus == null || special_menus.Length == 0)
        {
            Debug.LogError("[MenuManager] special_menus 배열이 비어있습니다. 프리팹을 할당해주세요.");
            enabled = false;
            return;
        }

        if (correctName == null)
        {
            Debug.LogError("[MenuManager] correctName UI 참조가 없습니다.");
            enabled = false;
            return;
        }
        // print(instance.main_sand_cnt); // 오ㅓㅐ 안돼ㅐㅐ
        //cardsToDisplay = 3;

        // --- 초기 UI 상태 설정 ---
        check_menu.SetActive(true);
        select_menu.SetActive(false);

        List<string> sand_list = new List<string>() { "jellyfish","hydra","kraken","planet","witch","phoenix"};

        // --- 정답 카드 선택 ---
        foreach(var item in ReadSpreadSheets.Instance.CurrentRequest_Include)
        {
            foreach (string s in sand_list) {
                if (item.Contains(s))
                {
                    correctmenu = s.ToString();
                }

            }
            
        }


        for (int i = 0; i < special_menus.Length; i++)
        {
            if (special_menus[i].name == correctmenu)
            {
                correctCardIndex = i;
                break;
            }
        }

        if (correctCardIndex == -1)
        {
            Debug.LogError($"[MenuManager] {correctmenu} 이름의 프리팹을 찾지 못했습니다.");
            return;
        }
        //while (true)
        //{
        //    correctCardIndex = Random.Range(0, special_menus.Length);
        //    correctmenu = special_menus[correctCardIndex].name;
        //    //ReadSpreadSheets.Instance.currentRequestName.Add(correctmenu+"_sand");
        //    if (correctmenu != null) {
        //        if (correctmenu == "tuna" || correctmenu == "anchovy") // 히든재료아니면 계속 다시 돌아야함
        //            continue;
        //        else
        //            break;
        //    }

        //}

        // --- 정답 카드 이름 UI 표시 ---
        TextMeshProUGUI menuNameText = special_menus[correctCardIndex].transform
            .GetComponentInChildren<TextMeshProUGUI>();
        if (menuNameText != null)
        {
            correctName.text = menuNameText.text;
        }
        else
        {
            Debug.LogWarning($"[MenuManager] {special_menus[correctCardIndex].name} 카드에서 이름 텍스트를 찾을 수 없습니다.");
            correctName.text = correctmenu; // 프리팹 이름이라도 표시
        }

        // --- 오답 카드 인덱스 수집 ---
        List<int> tempNumList = new List<int>();
        for (int i = 0; i < special_menus.Length; i++)
        {
            if (i != correctCardIndex)
                tempNumList.Add(i);
        }
        num = tempNumList.ToArray();
    }

    public IEnumerator CreateCardStack()
    {
        Debug.Log("[MenuManager] CreateCardStack 호출됨.");

        if (num == null || special_menus == null)
        {
            Debug.LogError("[MenuManager] num 배열 또는 special_menus 배열이 초기화되지 않았습니다.");
            yield break;
        }
        if (num.Length == 0 || special_menus.Length == 0 || correctCardIndex < 0)
        {
            Debug.LogWarning("[MenuManager] 배치할 카드가 부족하거나 정답 카드 인덱스가 유효하지 않습니다.");
            yield break;
        }

        // 카드 크기 체크
        float cardWidth = 0f;
        if (special_menus[0].TryGetComponent<RectTransform>(out RectTransform firstCardRect))
        {
            cardWidth = firstCardRect.rect.width;
        }
        else
        {
            Debug.LogError("[MenuManager] 첫 번째 카드 프리팹에 RectTransform이 없습니다. UI 프리팹인지 확인하세요.");
            yield break;
        }

        // 배치할 카드 개수 결정
        int actualCardsToDisplay = Mathf.Min(cardsToDisplay, num.Length + 1);
        if (actualCardsToDisplay <= 0)
        {
            Debug.LogWarning("[MenuManager] 표시할 카드 수가 0입니다.");
            yield break;
        }

        // 카드 인덱스 목록 생성 (정답 + 랜덤 오답)
        List<int> cardIndicesToPlace = new List<int> { correctCardIndex };
        List<int> tempAvailableNums = new List<int>(num);
        for (int k = 0; k < actualCardsToDisplay - 1; k++)
        {
            if (tempAvailableNums.Count == 0) break;
            int randomIdx = Random.Range(0, tempAvailableNums.Count);
            cardIndicesToPlace.Add(tempAvailableNums[randomIdx]);
            tempAvailableNums.RemoveAt(randomIdx);
        }

        // 순서 섞기
        cardIndicesToPlace = cardIndicesToPlace.OrderBy(x => Random.value).ToList();

        // 카드 프리팹 리스트 생성
        cardStack = new List<GameObject>();
        foreach (var cardIndex in cardIndicesToPlace)
        {
            if (cardIndex >= 0 && cardIndex < special_menus.Length)
                cardStack.Add(special_menus[cardIndex]);
            else
                Debug.LogWarning($"[MenuManager] 잘못된 카드 인덱스 {cardIndex}입니다.");
        }

        // 카드 펼치기
        yield return StartCoroutine(DealCards(cardWidth, actualCardsToDisplay));
    }

    IEnumerator DealCards(float cardWidth, int actualCardsToDisplay)
    {
        float totalGroupWidth = (actualCardsToDisplay * cardWidth) + ((actualCardsToDisplay - 1) * cardSpacing);
        float startPosX = -totalGroupWidth / 2f + cardWidth / 2f;

        for (int i = 0; i < cardStack.Count; i++)
        {
            GameObject cardPrefab = cardStack[i];
            if (cardPrefab == null)
            {
                Debug.LogWarning("[MenuManager] 카드 프리팹이 null입니다.");
                continue;
            }

            GameObject instantiatedMenu = Instantiate(cardPrefab, cardStackParent);
            instantiatedMenu.transform.SetSiblingIndex(i);

            // Flip 호출
            CardGo cardGoComponent = instantiatedMenu.GetComponent<CardGo>();
            if (cardGoComponent != null)
            {
                cardGoComponent.Invoke("Flip", 1f);
            }
            else
            {
                Debug.LogWarning($"[MenuManager] {instantiatedMenu.name}에 CardGo 컴포넌트가 없습니다.");
            }

            // RectTransform 체크
            RectTransform instantiatedRectTransform = instantiatedMenu.GetComponent<RectTransform>();
            if (instantiatedRectTransform == null)
            {
                Debug.LogError($"[MenuManager] {instantiatedMenu.name}에 RectTransform이 없습니다. 오브젝트를 삭제합니다.");
                Destroy(instantiatedMenu);
                continue;
            }

            // 위치 및 회전 초기화
            instantiatedRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            instantiatedRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            instantiatedRectTransform.pivot = new Vector2(0.5f, 0.5f);
            instantiatedRectTransform.localScale = Vector3.one;
            instantiatedRectTransform.localRotation = Quaternion.identity;

            // 최종 위치
            float currentPosX = startPosX + i * (cardWidth + cardSpacing);
            Vector2 finalPosition = new Vector2(currentPosX, 0f);

            // 애니메이션 시작
            StartCoroutine(AnimateCard(instantiatedRectTransform, finalPosition, dealInterval));

            yield return new WaitForSeconds(dealInterval);
        }
        yield return new WaitForSeconds(1f);
    }

    IEnumerator AnimateCard(RectTransform card, Vector2 targetPosition, float duration)
    {
        if (card == null) yield break;

        Vector2 startPosition = card.anchoredPosition;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            card.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, elapsedTime / duration);
            yield return null;
        }

        card.anchoredPosition = targetPosition;
    }

    public void OnStageDataLoaded(int mainCount, int cookieCount, int drinkCount)
    {
        // StageManage 데이터가 로드된 후 호출됨
        cardsToDisplay = mainCount; // main_sand_cnt 적용
        print(cardsToDisplay);
        cookie_cnt = cookieCount;
        //drink_cnt = drinkCount;

        Debug.Log($"Stage data loaded. cardsToDisplay={cardsToDisplay}, cookie={cookie_cnt}, drink={drink_cnt}");

    }

}
