using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq; // OrderBy 사용
using DG.Tweening;

public class CookManager : MonoBehaviour
{
    private List<string> selectedIngredientsNames;
    public GameObject sandwichContainer; // 샌드위치 재료들의 부모 오브젝트

    [Header("샌드위치 드롭 애니메이션 설정")]
    public float dropStartYOffset = 500f; // 화면 상단 밖으로 재료를 보낼 Y 오프셋 (픽셀 기준)
    public float dropDuration = 0.25f;    // 각 재료가 떨어지는 데 걸리는 시간
    public float delayBetweenDrops = 0.1f; // 각 재료가 떨어지기 시작하는 시간 간격

    [Header("빵 이름 설정")]
    public string bottomBreadName = "bread2"; // 가장 아래 놓일 빵 이름
    public string topBreadName = "bread";   // 가장 위에 놓일 빵 이름

    [Header("봉투 애니메이션 설정")]
    public float moveDistance = 20f;   // 이동 거리
    public float moveSpeed = 2f;      // 이동 속도
    public float waitTime = 2f;       // 대기 시간
    public GameObject sandbag;
    public GameObject NewSandwich;
    private Vector3 startPos;
    private Vector3 endPos;

    void Start()
    {
        startPos = sandbag.transform.position;
        endPos = startPos - new Vector3(0, moveDistance, 0);

        selectedIngredientsNames = CheckMenu.selectedNames;
        print("선택된 재료들: " + string.Join(',', selectedIngredientsNames));

        if (sandwichContainer == null)
        {
            Debug.LogError("CookManager: 'Sandwich Container' GameObject가 할당되지 않았습니다!");
            return;
        }

        // 시작 시 재료 전부 비활성화
        for (int i = 0; i < sandwichContainer.transform.childCount; i++)
        {
            sandwichContainer.transform.GetChild(i).gameObject.SetActive(false);
        }

        StartCoroutine(MainRoutine());
    }

    IEnumerator MainRoutine()
    {
        // 1. 샌드위치 재료 떨어뜨리기
        yield return StartCoroutine(DropSandwichIngredients());

        // 2. 2초 대기
        yield return new WaitForSeconds(waitTime);

        // 3. 봉투 내려오기
        yield return StartCoroutine(MoveObject(sandbag, startPos, endPos));

        // 4. 2초 대기
        yield return new WaitForSeconds(waitTime);

        // 5. 봉투와 샌드위치 같이 올라가기
        yield return StartCoroutine(MoveBothUp());
    }

    IEnumerator DropSandwichIngredients()
    {
        // 모든 재료를 이름으로 찾기
        Dictionary<string, GameObject> allIngredientsDict = new Dictionary<string, GameObject>();
        for (int i = 0; i < sandwichContainer.transform.childCount; i++)
        {
            GameObject child = sandwichContainer.transform.GetChild(i).gameObject;
            allIngredientsDict[child.name] = child;
        }

        List<GameObject> dropOrderIngredients = new List<GameObject>();

        GameObject bottomBread = null;
        GameObject topBread = null;

        if (allIngredientsDict.TryGetValue(bottomBreadName, out bottomBread))
            dropOrderIngredients.Add(bottomBread);

        List<GameObject> middleIngredients = new List<GameObject>();
        foreach (string selectedName in selectedIngredientsNames)
        {
            if (selectedName != bottomBreadName && selectedName != topBreadName)
            {
                GameObject ingredient = allIngredientsDict.FirstOrDefault(pair => pair.Key.Contains(selectedName)).Value;
                if (ingredient != null)
                    middleIngredients.Add(ingredient);
            }
        }

        middleIngredients = middleIngredients.OrderBy(go => go.transform.position.y).ToList();
        dropOrderIngredients.AddRange(middleIngredients);

        if (allIngredientsDict.TryGetValue(topBreadName, out topBread))
            dropOrderIngredients.Add(topBread);

        foreach (GameObject ingredient in dropOrderIngredients)
        {
            Vector3 originalPos = ingredient.transform.position;
            Vector3 startPos = originalPos;
            startPos.y += dropStartYOffset;

            ingredient.transform.position = startPos;
            ingredient.SetActive(true);

            ingredient.transform.DOMove(originalPos, dropDuration).SetEase(Ease.OutBounce);

            yield return new WaitForSeconds(delayBetweenDrops);
        }
    }

    IEnumerator MoveObject(GameObject target, Vector3 from, Vector3 to)
    {
        float elapsed = 0f;
        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime * moveSpeed;
            target.transform.position = Vector3.Lerp(from, to, elapsed);
            yield return null;
        }
    }

    IEnumerator MoveBothUp()
    {
        Vector3 bagCurrent = sandbag.transform.position;
        Vector3 sandwichCurrent = NewSandwich.transform.position;

        Vector3 bagTarget = startPos;
        Vector3 sandwichTarget = sandwichCurrent + (bagTarget - bagCurrent);

        float elapsed = 0f;
        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime * moveSpeed;
            sandbag.transform.position = Vector3.Lerp(bagCurrent, bagTarget, elapsed);
            NewSandwich.transform.position = Vector3.Lerp(sandwichCurrent, sandwichTarget, elapsed);
            yield return null;
        }
    }
}