using UnityEngine;
using System.Collections;
using TMPro;

public class RandomLaunch : MonoBehaviour
{
    // 음식 설정
    [Header("음식 설정")]
    public GameObject foodPrefab;        // Inspector에서 음식 프리팹 연결
    public Transform foodContainer;      // 음식이 배치될 부모 오브젝트 연결
    public float minFoodInterval = 5f;   // 음식 등장 최소 간격
    public float maxFoodInterval = 15f;  // 음식 등장 최대 간격

    private GameObject currentFood; 

    // 배달원 설정 (man과 ment를 포함한 프리팹 사용)
    [Header("배달원 설정")]
    public GameObject manIconPrefab;     // 배달원 프리팹 연결 (man과 ment 포함)
    public Transform manIconContainer;   // 배달원이 배치될 부모 오브젝트 연결
    public float minManInterval = 3f;   // 배달원 등장 최소 간격
    public float maxManInterval = 7f;   // 배달원 등장 최대 간격

    private GameObject currentManIcon;   

    // 시작 및 코루틴 호출
    void Start()
    {
        StartCoroutine(GenerateRandomFood());
        StartCoroutine(GenerateRandomManIcon());
    }

    // 음식 생성 로직
    IEnumerator GenerateRandomFood()
    {
        while (true)
        {
            float waitTime = Random.Range(minFoodInterval, maxFoodInterval);
            yield return new WaitForSeconds(waitTime);
            CreateNewFood();
        }
    }

    void CreateNewFood()
    {
        if (currentFood != null)
        {
            Debug.Log("이미 음식이 존재합니다.");
            return;
        }

        if (foodPrefab != null && foodContainer != null)
        {
            currentFood = Instantiate(foodPrefab, foodContainer);
            Debug.Log("새 음식 생성");
        }
    }

    public void CompleteFood()
    {
        if (currentFood != null)
        {
            Destroy(currentFood);
            currentFood = null;
            Debug.Log("음식 완료");
        }
    }

    // 배달원 생성 로직
    IEnumerator GenerateRandomManIcon()
    {
        while (true)
        {
            float waitTime = Random.Range(minManInterval, maxManInterval);
            yield return new WaitForSeconds(waitTime); // 랜덤 시간 대기

            CreateNewManIcon();

            // 배달원 등장 후 10초 후에 자동 사라짐
            yield return new WaitForSeconds(10f);
            CompleteManIcon();
        }
    }

    void CreateNewManIcon()
    {
        if (currentManIcon != null)
        {
            Debug.Log("이미 배달원이 존재합니다.");
            return;
        }

        if (manIconPrefab != null && manIconContainer != null)
        {
            currentManIcon = Instantiate(manIconPrefab, manIconContainer);
            Debug.Log("새 배달원 생성");
        }
    }

    public void CompleteManIcon()
    {
        if (currentManIcon != null)
        {
            Destroy(currentManIcon);
            currentManIcon = null;
            Debug.Log("배달원 사라짐");
        }
    }
}