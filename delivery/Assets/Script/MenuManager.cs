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
    //public GameObject correctIngredient;
    public Transform cardStackParent; // 새로 추가할 카드 더미의 부모 오브젝트
    public TextMeshProUGUI correctName;

    [Header("카드 배치 설정")]
    public int cardsToDisplay = 3; // 현재 스테이지에서 표시할 카드 수
    public float cardSpacing = 20f; // 카드와 카드 사이의 간격 (픽셀)
    public float dealInterval = 0.5f; // 카드가 펼쳐지는 시간 간격

    private List<GameObject> cardStack; // 더미에 쌓아둘 카드 리스트

    private int[] num; // 올바른 메뉴가 아닌 카드들의 인덱스
    private int correctCardIndex = -1; //  카드의 실제 special_menus 내 인덱스

    string correctmenu;

    void Start()
    {
        check_menu.gameObject.SetActive(true);
        select_menu.gameObject.SetActive(false);

        // 이 부분에서 special_menus 배열이 null이 아닌지 다시 한 번 확인
        if (special_menus != null && special_menus.Length > 0)
        {
            correctCardIndex = Random.Range(0, special_menus.Length);
            correctmenu = special_menus[correctCardIndex].name;
            correctName.text = special_menus[correctCardIndex].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text;
        }
        else
        {
            Debug.LogError("special_menus 배열이 비어있습니다. 에디터에서 프리팹들을 할당해주세요.");
        }

        List<int> tempNumList = new List<int>();
        for (int i = 0; i < special_menus.Length; i++)
        {
            if (i != correctCardIndex)// 올바른 메뉴(craken)가 아닌 것만 리스트에 추가
            {
                tempNumList.Add(i);
            }
        }
        num = tempNumList.ToArray();

        // 올바른 재료 카드가 special_menus에 존재하는지 확인
        if (correctCardIndex == -1)
        {
            Debug.LogError($"'{correctmenu}' 카드를 special_menus 배열에서 찾을 수 없습니다. 이름 또는 배열 인덱스를 확인해주세요.");
        }
        //correctIngredient.GetComponent<TextMeshProUGUI>().text = correctmenu;
    }

    public void CreateCardStack()
    {
        Debug.Log("CreateCardStack 함수가 호출되었습니다."); // 이 줄을 추가합니다.
        // 배치할 카드가 없거나, special_menus가 비어있으면 종료
        if (num.Length == 0 || special_menus.Length == 0 || correctCardIndex == -1)
        {
            Debug.LogWarning($"배치할 카드 프리팹이 부족하거나, '{correctmenu}' 카드를 찾을 수 없습니다.");
            return;
        }

        // 첫 번째 카드 프리팹에서 너비를 가져와 모든 카드의 너비로 가정합니다.
        float cardWidth = 0f;
        if (special_menus[0].TryGetComponent<RectTransform>(out RectTransform firstCardRect))
        {
            cardWidth = firstCardRect.rect.width;
        }
        else
        {
            Debug.LogError("special_menus 배열의 첫 번째 프리팹에 RectTransform이 없습니다. UI 프리팹인지 확인하세요.");
            return;
        }

        // 실제 배치할 카드 수 조정:
        int actualCardsToDisplay = Mathf.Min(cardsToDisplay, num.Length + 1);
        if (actualCardsToDisplay == 0)
        {
            Debug.LogWarning("표시할 카드가 없습니다 (cardsToDisplay 설정 또는 사용 가능한 카드 부족).");
            return;
        }

        // 배치할 카드들의 실제 인덱스를 담을 리스트
        List<int> cardIndicesToPlace = new List<int>();

        // 1. 올바른 카드를 배치할 리스트에 추가 (무조건 포함)
        cardIndicesToPlace.Add(correctCardIndex);

        // 2. 나머지 자리에 들어갈 랜덤 카드들을 num 배열에서 선택
        int remainingSlots = actualCardsToDisplay - 1;
        List<int> tempAvailableNums = new List<int>(num);
        for (int k = 0; k < remainingSlots; k++)
        {
            if (tempAvailableNums.Count == 0)
            {
                Debug.LogWarning($"랜덤으로 배치할 카드가 더 이상 없습니다. {correctmenu}과 함께 표시될 랜덤 카드 수가 부족합니다.");
                break;
            }
            int randomIdx = Random.Range(0, tempAvailableNums.Count);
            cardIndicesToPlace.Add(tempAvailableNums[randomIdx]);
            tempAvailableNums.RemoveAt(randomIdx);
        }

        // 최종 배치될 카드들의 순서를 섞음 (올바른 재료가 랜덤 위치에 놓이도록)
        cardIndicesToPlace = cardIndicesToPlace.OrderBy(x => Random.value).ToList();

        // 이제 최종적으로 배치할 cardIndicesToPlace 리스트를 사용하여 카드 스택을 만듭니다.
        cardStack = new List<GameObject>();
        foreach (var cardIndex in cardIndicesToPlace)
        {
            cardStack.Add(special_menus[cardIndex]);
        }

        // 카드 펼치기 코루틴 시작
        StartCoroutine(DealCards(cardWidth, actualCardsToDisplay));
    }

    // 카드를 하나씩 펼치는 코루틴
    IEnumerator DealCards(float cardWidth, int actualCardsToDisplay)
    {
        // 전체 카드 그룹이 차지할 너비 계산
        float totalGroupWidth = (actualCardsToDisplay * cardWidth) + ((actualCardsToDisplay - 1) * cardSpacing);
        float startPosX = -totalGroupWidth / 2f + cardWidth / 2f;

        for (int i = 0; i < cardStack.Count; i++)
        {
            // 1. 카드 인스턴스화
            GameObject instantiatedMenu = Instantiate(cardStack[i], cardStackParent);
            instantiatedMenu.transform.SetSiblingIndex(i);
            // 2. CardGo 컴포넌트가 존재한다면 Flip 메서드를 호출합니다.
            CardGo cardGoComponent = instantiatedMenu.GetComponent<CardGo>();
            if (cardGoComponent != null)
            {
                cardGoComponent.Invoke("Flip", 1f);
                print("Flip실행");
            }
            else
            {
                Debug.LogWarning($"경고: {instantiatedMenu.name} 오브젝트에 CardGo 컴포넌트가 없습니다.");
            }

            // 3. RectTransform 설정
            RectTransform instantiatedRectTransform = instantiatedMenu.GetComponent<RectTransform>();
            if (instantiatedRectTransform == null)
            {
                Debug.LogError($"인스턴스화된 오브젝트 {instantiatedMenu.name}에 RectTransform 컴포넌트가 없습니다.");
                Destroy(instantiatedMenu);
                continue;
            }

            instantiatedRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            instantiatedRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            instantiatedRectTransform.pivot = new Vector2(0.5f, 0.5f);
            instantiatedRectTransform.localScale = Vector3.one;
            instantiatedRectTransform.localRotation = Quaternion.identity;

            // 4. 카드의 최종 위치 계산
            float currentPosX = startPosX + i * (cardWidth + cardSpacing);
            Vector2 finalPosition = new Vector2(currentPosX, 0f);

            // 5. 애니메이션 코루틴 시작
            StartCoroutine(AnimateCard(instantiatedRectTransform, finalPosition, dealInterval));

            // 다음 카드가 펼쳐지기까지 대기
            yield return new WaitForSeconds(dealInterval);
        }
    }

    // 카드 이동 애니메이션 코루틴
    IEnumerator AnimateCard(RectTransform card, Vector2 targetPosition, float duration)
    {
        Vector2 startPosition = card.anchoredPosition;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            // Lerp 함수를 사용하여 시작 위치에서 목표 위치까지 부드럽게 이동
            float t = elapsedTime / duration;
            card.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, t);
            yield return null; // 다음 프레임까지 대기
        }

        // 애니메이션 완료 후 최종 위치 보장
        card.anchoredPosition = targetPosition;
    }
}