using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class FoodManager : MonoBehaviour
{
    [Header("음식 프리팹 설정")]
    public GameObject[] foodPrefabs;

    [Header("생성 위치")]
    public Transform foodContainer;

    [Header("생성 시간 설정")]
    public float minWait = 0f;
    public float maxWait = 3f;



    void Start()
    {
        if (CheckMenu.selectedNames != null && CheckMenu.selectedNames.Count > 0)
        {
            StartCoroutine(SpawnFoodRoutine());
        }
        else
        {
            Debug.Log("<color=white>선택된 항목이 없어 음식을 생성하지 않습니다.</color>");
        }
    }

    IEnumerator SpawnFoodRoutine()
    {
        float randomWait = Random.Range(minWait, maxWait);
        yield return new WaitForSeconds(randomWait);

        if (foodPrefabs != null && foodPrefabs.Length > 0)
        {
            int randomIndex = Random.Range(0, foodPrefabs.Length);
            GameObject selectedFood = foodPrefabs[randomIndex];

            // 1. 음식 생성
            Instantiate(selectedFood, foodContainer);

            // 2. 배달원 동시 소환
            if (ManManager.Instance != null)
            {
                var sheet = GameObject.Find("sheet").GetComponent<ReadSpreadSheets>();
                string orderText = (sheet != null && sheet.currentRequestName.Count > 0)
                                   ? sheet.currentRequestName[0] : "주문 내용 없음";

                ManManager.Instance.SpawnMan(orderText);
            }

            Debug.Log("<color=orange>음식, 배달원 생성</color>");
        }
    }
}