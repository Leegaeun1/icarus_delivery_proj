using System.Collections;
using UnityEngine;

public class FoodLaunch : MonoBehaviour
{
    [Header("음식 설정")]
    public GameObject foodPrefab;
    public Transform foodContainer;
    public float minFoodInterval = 5f;
    public float maxFoodInterval = 15f;

    private GameObject currentFood;

    void Start()
    {
        StartCoroutine(GenerateRandomFood());
    }

    IEnumerator GenerateRandomFood()
    {
        float waitTime = Random.Range(minFoodInterval, maxFoodInterval);
        yield return new WaitForSeconds(waitTime);
        CreateNewFood();
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

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            CreateNewFood();
        }
    }
}