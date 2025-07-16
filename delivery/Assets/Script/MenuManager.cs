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
    public GameObject correctIngredient;
    public Transform cardStackParent; // 새로 추가할 카드 더미의 부모 오브젝트

    [Header("카드 배치 설정")]
    public int cardsToDisplay = 3; // 현재 스테이지에서 표시할 카드 수
    public float cardSpacing = 20f; // 카드와 카드 사이의 간격 (픽셀)
    public float dealInterval = 0.5f; // 카드가 펼쳐지는 시간 간격

    private List<GameObject> cardStack; // 더미에 쌓아둘 카드 리스트

    private int[] num; // 올바른 메뉴(craken)가 아닌 카드들의 인덱스
    private int correctCardIndex = -1; //  카드의 실제 special_menus 내 인덱스

    string correctmenu ;

    void Awake()
    {
        // Awake에서 미리 초기화
        check_menu.gameObject.SetActive(true);
        select_menu.gameObject.SetActive(false);

        // 이 부분에서 special_menus 배열이 null이 아닌지 다시 한 번 확인하는 방어 코드 추가
        if (special_menus != null && special_menus.Length > 0)
        {
            correctmenu = special_menus[Random.Range(0, special_menus.Length)].name;
        }
        else
        {
            Debug.LogError("special_menus 배열이 비어있습니다. 에디터에서 프리팹들을 할당해주세요.");
        }
    }
    void Start()
    {
        //check_menu.gameObject.SetActive(true);
        //select_menu.gameObject.SetActive(false);

        //correctmenu = special_menus[Random.Range(0,special_menus.Length)].name; //  얜 추후에 메뉴에 알맞게 바뀌도록 해야함. 지금은 랜덤임

        List<int> tempNumList = new List<int>();
        for (int i = 0; i < special_menus.Length; i++)
        {
            if (special_menus[i].name == correctmenu)
            {
                // 카드의 인덱스를 저장
                correctCardIndex = i;
            }
            else // 올바른 메뉴(craken)가 아닌 것만 리스트에 추가
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
        correctIngredient.GetComponent<TextMeshProUGUI>().text = correctmenu;
    }

    public void random_card()
    {
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
        // cardsToDisplay는 올바른 재료 카드 포함한 총 배치 수
        // 올바른 재료를 제외한 나머지 카드는 num.Length(랜덤으로 선택할 수 있는 카드 수) 내에서 선택
        int actualCardsToDisplay = Mathf.Min(cardsToDisplay, num.Length + 1); // +1은 올바른 재료 카드를 위함
        if (actualCardsToDisplay == 0)
        {
            Debug.LogWarning("표시할 카드가 없습니다 (cardsToDisplay 설정 또는 사용 가능한 카드 부족).");
            return;
        }
        // 만약 cardsToDisplay가 1개인데 그게 올바른 재료가 아니라면 경고
        if (actualCardsToDisplay == 1 && (cardsToDisplay > 1 || num.Length == 0)) // num.Length == 0은 craken만 있다는 의미
        {
            Debug.LogWarning($"cardsToDisplay가 1이며, {correctmenu}만 배치됩니다.");
        }


        // 전체 카드 그룹이 차지할 너비 계산
        float totalGroupWidth = (actualCardsToDisplay * cardWidth) + ((actualCardsToDisplay - 1) * cardSpacing);
        float startPosX = -totalGroupWidth / 2f + cardWidth / 2f;

        // 배치할 카드들의 실제 인덱스를 담을 리스트
        List<int> cardIndicesToPlace = new List<int>();

        // 1. 카드를 배치할 리스트에 추가 (무조건 포함)
        cardIndicesToPlace.Add(correctCardIndex);

        // 2. 나머지 자리에 들어갈 랜덤 카드들을 num 배열에서 선택
        // 올바른 재료를 제외하고 배치해야 할 카드 개수
        int remainingSlots = actualCardsToDisplay - 1;

        // num 배열 (올바른 재료 제외 카드들)에서 랜덤으로 remainingSlots 개수만큼 선택
        List<int> tempAvailableNums = new List<int>(num); // 중복 방지를 위한 임시 리스트
        for (int k = 0; k < remainingSlots; k++)
        {
            if (tempAvailableNums.Count == 0)
            {
                Debug.LogWarning($"랜덤으로 배치할 카드가 더 이상 없습니다. {correctmenu}과 함께 표시될 랜덤 카드 수가 부족합니다.");
                break;
            }
            int randomIdx = Random.Range(0, tempAvailableNums.Count);
            cardIndicesToPlace.Add(tempAvailableNums[randomIdx]);
            tempAvailableNums.RemoveAt(randomIdx); // 중복 방지
        }

        // 최종 배치될 카드들의 순서를 섞음 (올바른 재료가 랜덤 위치에 놓이도록)
        cardIndicesToPlace = cardIndicesToPlace.OrderBy(x => Random.value).ToList();

        // 이제 최종적으로 배치할 cardIndicesToPlace 리스트를 사용하여 카드 배치
        for (int i = 0; i < cardIndicesToPlace.Count; i++)
        {
            int cardPrefabIndex = cardIndicesToPlace[i]; // special_menus 배열에서 가져올 실제 인덱스

            GameObject instantiatedMenu = Instantiate(special_menus[cardPrefabIndex]);
            instantiatedMenu.transform.SetParent(canvasRectTransform, false);

            CardGo cardGoComponent = instantiatedMenu.GetComponent<CardGo>();

            // 2. CardGo 컴포넌트가 존재한다면 해당 인스턴스의 Flip 메서드를 호출합니다.
            if (cardGoComponent != null)
            {
                cardGoComponent.Invoke("Flip", 1f);  // 이제 각 카드 인스턴스의 Flip 메서드가 호출됩니다.
                print("Filp실행");
            }
            else
            {
                Debug.LogWarning($"경고: {instantiatedMenu.name} 오브젝트에 CardGo 컴포넌트가 없습니다.");
            }

            RectTransform instantiatedRectTransform = instantiatedMenu.GetComponent<RectTransform>();
            if (instantiatedRectTransform == null)
            {
                Debug.LogError($"인스턴스화된 오브젝트 {instantiatedMenu.name}에 RectTransform 컴포넌트가 없습니다.");
                Destroy(instantiatedMenu);
                continue;
            }

            // 모든 카드에 중앙 Anchor 및 Pivot 적용
            instantiatedRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            instantiatedRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            instantiatedRectTransform.pivot = new Vector2(0.5f, 0.5f);

            float currentPosX = startPosX + i * (cardWidth + cardSpacing);
            instantiatedRectTransform.anchoredPosition = new Vector2(currentPosX, 0f);

            instantiatedRectTransform.localScale = Vector3.one;
            instantiatedRectTransform.localRotation = Quaternion.identity;

            Debug.Log($"인스턴스화됨: {special_menus[cardPrefabIndex].name} 위치: {instantiatedRectTransform.anchoredPosition}");
        }
    }
}