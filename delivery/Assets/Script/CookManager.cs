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
    public float dropDuration = 0.5f;    // 각 재료가 떨어지는 데 걸리는 시간
    public float delayBetweenDrops = 0.2f; // 각 재료가 떨어지기 시작하는 시간 간격

    [Header("빵 이름 설정")]
    public string bottomBreadName = "bread2"; // 가장 아래 놓일 빵 이름
    public string topBreadName = "bread";   // 가장 위에 놓일 빵 이름

    void Start()
    {
        selectedIngredientsNames = CheckMenu.selectedNames;
        print("선택된 재료들: " + string.Join(',', selectedIngredientsNames));

        if (sandwichContainer == null)
        {
            Debug.LogError("CookManager: 'Sandwich Container' GameObject가 할당되지 않았습니다!");
            return;
        }

        // 모든 재료를 일단 비활성화합니다. (씬 시작 시 모든 재료가 비활성 상태라고 가정)
        for (int i = 0; i < sandwichContainer.transform.childCount; i++)
        {
            sandwichContainer.transform.GetChild(i).gameObject.SetActive(false);
        }

        StartCoroutine(AnimateSandwichDrops());
    }

    IEnumerator AnimateSandwichDrops()
    {
        // 1. 모든 재료 오브젝트를 이름으로 찾기 위한 딕셔너리 생성 (성능 최적화)
        Dictionary<string, GameObject> allIngredientsDict = new Dictionary<string, GameObject>();
        for (int i = 0; i < sandwichContainer.transform.childCount; i++)
        {
            GameObject child = sandwichContainer.transform.GetChild(i).gameObject;
            allIngredientsDict[child.name] = child;
        }

        // 2. 드롭 애니메이션에 사용할 최종 정렬된 재료 리스트
        List<GameObject> dropOrderIngredients = new List<GameObject>();

        GameObject bottomBread = null;
        GameObject topBread = null;

        // 3. 빵을 먼저 처리하여 드롭 순서 리스트에 추가
        if (allIngredientsDict.TryGetValue(bottomBreadName, out bottomBread))
        {
            dropOrderIngredients.Add(bottomBread);
        }
        else
        {
            Debug.LogWarning($"'{bottomBreadName}' (바닥 빵)을 샌드위치 컨테이너에서 찾을 수 없습니다.");
        }

        // 4. 선택된 재료들 중 빵을 제외하고 중간 재료들만 필터링하여 Y축으로 정렬
        List<GameObject> middleIngredients = new List<GameObject>();
        foreach (string selectedName in selectedIngredientsNames)
        {
            // 선택된 재료 이름이 빵 이름과 일치하지 않는 경우만 추가
            if (selectedName != bottomBreadName && selectedName != topBreadName)
            {
                // 실제 GameObject를 딕셔너리에서 찾아 추가 (이름.Contains() 방식은 정확한 이름으로 변경)
                GameObject ingredient = allIngredientsDict.FirstOrDefault(pair => pair.Key.Contains(selectedName)).Value;
                // 또는 정확한 이름 일치: GameObject ingredient = allIngredientsDict.GetValueOrDefault(selectedName);
                if (ingredient != null)
                {
                    middleIngredients.Add(ingredient);
                }
            }
        }

        // 중간 재료들을 Y축 위치에 따라 정렬 (가장 아래 놓일 재료부터)
        // 이 부분은 재료 프리팹들이 최종적으로 놓일 Y 위치에 따라 정렬됩니다.
        middleIngredients = middleIngredients.OrderBy(go => go.transform.position.y).ToList();
        dropOrderIngredients.AddRange(middleIngredients); // 중간 재료들을 드롭 순서 리스트에 추가

        // 5. 마지막 빵 처리
        if (allIngredientsDict.TryGetValue(topBreadName, out topBread))
        {
            dropOrderIngredients.Add(topBread);
        }
        else
        {
            Debug.LogWarning($"'{topBreadName}' (윗 빵)을 샌드위치 컨테이너에서 찾을 수 없습니다.");
        }

        //// 최종 드롭 순서 확인 (디버깅 용)
        //Debug.Log("최종 드롭 순서:");
        //foreach (var ing in dropOrderIngredients)
        //{
        //    Debug.Log($"- {ing.name} (Y: {ing.transform.position.y})");
        //}


        // 각 재료에 대한 애니메이션 실행
        foreach (GameObject ingredient in dropOrderIngredients)
        {
            // 애니메이션 시작 전 초기 위치 설정 (최종 위치 + dropStartYOffset)
            Vector3 originalPos = ingredient.transform.position; // 최종적으로 멈출 위치
            Vector3 startPos = originalPos;
            startPos.y += dropStartYOffset; // 시작 Y 위치 (화면 상단 밖)

            ingredient.transform.position = startPos; // 초기 위치 설정
            ingredient.SetActive(true); // 재료 활성화

            // 애니메이션 실행: 위에서 아래로 떨어지도록
            ingredient.transform.DOMove(originalPos, dropDuration).SetEase(Ease.OutBounce);

            // 다음 재료가 떨어지기까지 잠시 대기
            yield return new WaitForSeconds(delayBetweenDrops);
        }
    }
}