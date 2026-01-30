using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq; // OrderBy 사용
using DG.Tweening;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CookManager : MonoBehaviour
{
    private List<string> selectedIngredientsNames;

    [Header("컨테이너(=NewSandwich 루트 추천)")]
    public GameObject sandwichContainer; // 샌드위치 재료들의 부모 오브젝트

    [Header("샌드위치 드롭 애니메이션 설정")]
    public float dropStartYOffset = 500f; // 화면 상단 밖으로 재료를 보낼 Y 오프셋 (픽셀 기준)
    public float dropDuration = 5f;    // 각 재료가 떨어지는 데 걸리는 시간
    public float delayBetweenDrops = 1f; // 각 재료가 떨어지기 시작하는 시간 간격

    [Header("빵 이름 설정")]
    public string bottomBreadName = "bread2"; // 가장 아래 놓일 빵 이름
    public string topBreadName = "bread";     // 가장 위에 놓일 빵 이름

    [Header("구분자 이름")]
    public string drinksSeparatorName = "====Drinks====";

    // 샌드위치(드링크 이전)만 담는 임시 부모
    private GameObject sandwichCore;
    private readonly List<SpriteRenderer> sandwichCoreRenderers = new();
    private readonly List<GameObject> sandwichCoreObjects = new();

    // 구분자 이후(드링크,쿠키 등) - 선택되면 켜지도록
    private readonly List<GameObject> staticObjects = new();
    private readonly List<SpriteRenderer> staticRenderers = new();

    //[Header("봉투 애니메이션 설정")]
    //public float moveDistance = 20f; // 이동 거리
    //public float moveSpeed = 2f;     // 이동 속도
    //public float waitTime = 5f;      // 대기 시간

    [Header("봉투/샌드위치 오브젝트")]
    public GameObject sandbag;
    public GameObject NewSandwich;


    [Header("연출 타이밍")]
    public float waitTime = 5f;

    //private Vector3 startPos;
    //private Vector3 endPos;

    private SpriteRenderer bagRenderer;
    private SpriteRenderer sandwichRenderer;
    //임시로 다음날로 넘어가는 버튼
    private Button nextDayBtn;
    private TimeManager timeManager;
    private ReadSpreadSheets sheet;

    void Start()
    {
        // --- 필수 참조 검사 ---
        if (sandbag == null || NewSandwich == null)
        {
            Debug.LogError("[CookManager] sandbag 또는 NewSandwich가 할당되지 않았습니다.");
            enabled = false;
            return;
        }
        if (sandwichContainer == null)
        {
            Debug.LogError("[CookManager] sandwichContainer가 할당되지 않았습니다.");
            enabled = false;
            return;
        }
        nextDayBtn = GameObject.Find("Canvas").transform.GetChild(0).GetComponent<Button>();
        nextDayBtn.gameObject.SetActive(false);

        bagRenderer = sandbag.GetComponent<SpriteRenderer>();
        sandwichRenderer = NewSandwich.GetComponent<SpriteRenderer>();
        timeManager = GameObject.Find("TimeManager").GetComponent<TimeManager>();
        sheet = GameObject.Find("sheet").GetComponent<ReadSpreadSheets>();
        // 선택된 재료 확인
        selectedIngredientsNames = CheckMenu.selectedNames;
        if (selectedIngredientsNames == null || selectedIngredientsNames.Count == 0)
        {
            Debug.LogWarning("[CookManager] 선택된 재료가 없습니다.");
            selectedIngredientsNames = new List<string>();
        }

        print("선택된 재료들: " + string.Join(", ", selectedIngredientsNames));

        //// 초기 위치 저장
        //startPos = sandbag.transform.position;
        //endPos = startPos - new Vector3(0, moveDistance, 0);

        // 시작 시 모든 재료 비활성화
        for (int i = 0; i < sandwichContainer.transform.childCount; i++)
        {
            sandwichContainer.transform.GetChild(i).gameObject.SetActive(false);
        }

        // 1) 구분자 기준으로 SandwichCore/Static 분리
        BuildSandwichCore();

        // 2) 선택된 드링크/쿠키는 즉시 켜기 (샌드위치는 드롭에서 켜짐)
        TurnOnSelectedStatic();

        // 3) 샌드위치 파트는 전부 OFF로 시작
        foreach (var go in sandwichCoreObjects) go.SetActive(false);

        StartCoroutine(MainRoutine());
    }
    private void TurnOnSelectedStatic()
    {
        var set = new HashSet<string>(selectedIngredientsNames);
        foreach (var go in staticObjects)
        {
            if (go != null && set.Contains(go.name))
                go.SetActive(true);
        }
    }

    private void BuildSandwichCore()
    {
        sandwichCore = new GameObject("SandwichCore");
        sandwichCore.transform.SetParent(NewSandwich.transform, worldPositionStays: true);
        sandwichCore.transform.localPosition = Vector3.zero;
        sandwichCore.transform.localRotation = Quaternion.identity;
        sandwichCore.transform.localScale    = Vector3.one;

        bool reachedSeparator = false;

        // 스냅샷
        var children = new List<Transform>();
        for (int i = 0; i < NewSandwich.transform.childCount; i++)
            children.Add(NewSandwich.transform.GetChild(i));

        foreach (var child in children)
        {
            if (child.name == drinksSeparatorName)
            {
                reachedSeparator = true;
                // 구분자 자체도 static 그룹에 포함 (켜질 일은 거의 없지만 레퍼런스 용)
                staticObjects.Add(child.gameObject);
                staticRenderers.AddRange(child.GetComponentsInChildren<SpriteRenderer>(true));
                continue;
            }

            if (!reachedSeparator)
            {
                // 샌드위치 파트 → SandwichCore로 이동
                child.SetParent(sandwichCore.transform, worldPositionStays: true);
                sandwichCoreObjects.Add(child.gameObject);
                sandwichCoreRenderers.AddRange(child.GetComponentsInChildren<SpriteRenderer>(true));
            }
            else
            {
                // 이후(드링크/쿠키 등)
                staticObjects.Add(child.gameObject);
                staticRenderers.AddRange(child.GetComponentsInChildren<SpriteRenderer>(true));
            }
        }
    }




    IEnumerator MainRoutine()
    {
        // 1. 재료 떨어뜨리기
        yield return StartCoroutine(DropSandwichIngredients());

        // 2. 대기
        yield return new WaitForSeconds(waitTime);

        // 3. 샌드위치 봉투에 넣기
        yield return StartCoroutine(SandwichIntoBag());

        // 4. 대기
        //yield return new WaitForSeconds(waitTime);

        // 6. 그 후에 다시 메인 화면으로 돌아가도록 !!!!!!!!!!!!!!!
        //yield return StartCoroutine(finishStage());
        nextDayBtn.gameObject.SetActive(true);

        // 혹시 모를 중복 방지를 위해 기존 연결된 기능을 싹 지우고
        nextDayBtn.onClick.RemoveAllListeners();

        // 함수를 새로 연결합니다.
        nextDayBtn.onClick.AddListener(finishStage);
    }
    public void finishStage()
    {
        // 1. 최종 채점 및 정산 실행
        sheet.CheckFinalResult();

        sheet.is_menu_incorrect = false;
        sheet.menu_num += 1; // 요청 증가 ( 임시 )!!
        sheet.currentRequestName.Clear();
        // 2. 씬 전환 처리
        if (timeManager.isDayEnded)
        {
            SceneManager.LoadScene("DayFinish");
        }
        else
        {
            // 뒷정리
            CheckMenu.selectedNames.Clear();
            sheet.tmp_selected.Clear();
            // 다음 요리를 위해 오답 플래그 초기화

            SceneManager.LoadScene("cook");
        }
    }
    IEnumerator DropSandwichIngredients()
    {
        if (sandwichCore == null) yield break;

        // SandwichCore 자식들만으로 딕셔너리
        var dict = new Dictionary<string, GameObject>();
        for (int i = 0; i < sandwichCore.transform.childCount; i++)
        {
            var g = sandwichCore.transform.GetChild(i).gameObject;
            if (!dict.ContainsKey(g.name)) dict[g.name] = g;
        }

        var dropOrder = new List<GameObject>();

        // 아래빵
        if (dict.TryGetValue(bottomBreadName, out var bottomBread))
            dropOrder.Add(bottomBread);
        else
            Debug.LogWarning($"[CookManager] '{bottomBreadName}' 못 찾음");

        // 중간 재료: 선택 목록 기반
        var middle = new List<GameObject>();
        foreach (var name in selectedIngredientsNames)
        {
            if (name == bottomBreadName || name == topBreadName) continue;

            // 정확 일치 우선 → 없으면 contains 보조
            GameObject found = null;
            dict.TryGetValue(name, out found);
            if (found == null)
                found = dict.FirstOrDefault(p => p.Key.Contains(name)).Value;

            if (found != null) middle.Add(found);
            else Debug.LogWarning($"[CookManager] 선택 재료 '{name}' 없음");
        }
        // 현재 y 기준 낮은 것부터
        middle = middle.OrderBy(go => go.transform.position.y).ToList();
        dropOrder.AddRange(middle);

        // 윗빵
        if (dict.TryGetValue(topBreadName, out var topBread))
            dropOrder.Add(topBread);
        else
            Debug.LogWarning($"[CookManager] '{topBreadName}' 못 찾음");

        // 드롭 실행 (이때 켠다)
        foreach (var ing in dropOrder)
        {
            if (ing == null) continue;

            Vector3 dst = ing.transform.position;
            Vector3 src = dst + new Vector3(0, dropStartYOffset, 0);

            ing.transform.position = src;
            ing.SetActive(true);

            ing.transform.DOMove(dst, dropDuration).SetEase(Ease.OutBounce);
            yield return new WaitForSeconds(delayBetweenDrops);
        }
    }


    IEnumerator SandwichIntoBag()
    {
        if (sandwichCore == null || bagRenderer == null) yield break;

        Vector3 originalPos = sandwichCore.transform.position;
        Vector3 upPos = originalPos + new Vector3(0, 5f, 0);
        Vector3 downPos = originalPos +new Vector3(0, 2f, 0);

        // 위로(그냥 연출) : 정렬 변경 안 함
        yield return sandwichCore.transform.DOMove(upPos, 0.5f)
            .SetEase(Ease.OutQuad).WaitForCompletion();

        bagRenderer.sortingOrder = 7;

        yield return sandwichCore.transform.DOMove(downPos, 0.5f)
            .SetEase(Ease.InQuad).WaitForCompletion();

        // 샌드위치만 숨김 (드링크/쿠키는 그대로 노출)
        foreach (var go in sandwichCoreObjects) go.SetActive(false);

    }

}
