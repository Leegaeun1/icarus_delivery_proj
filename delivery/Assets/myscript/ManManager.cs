using UnityEngine;
using TMPro;
using System.Collections;

public class ManManager : MonoBehaviour
{
    [Header("손님 프리팹 리스트")]
    public GameObject[] manPrefabs;
    public Transform manContainer;

    [Header("효과 설정")]
    public float fadeInDuration = 1.0f;

    private GameObject currentMan;

    public void SpawnMan(string orderName)
    {
        if (currentMan != null) return;

        int randomIndex = Random.Range(0, manPrefabs.Length);
        currentMan = Instantiate(manPrefabs[randomIndex], manContainer);

        var speechText = currentMan.GetComponentInChildren<TextMeshProUGUI>();
        if (speechText != null) speechText.text = orderName;

        CanvasGroup canvasGroup = currentMan.GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = currentMan.AddComponent<CanvasGroup>();

        StartCoroutine(FadeInRoutine(canvasGroup));
    }

    private IEnumerator FadeInRoutine(CanvasGroup cg)
    {
        float elapsedTime = 0f;
        cg.alpha = 0f;
        while (elapsedTime < fadeInDuration)
        {
            if (cg == null) yield break; // 중간에 파괴될 경우 방지
            cg.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeInDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        if (cg != null) cg.alpha = 1f;
    }

    public void DestroyMan()
    {
        if (currentMan != null)
        {
            Destroy(currentMan);
            currentMan = null;
        }
    }
}