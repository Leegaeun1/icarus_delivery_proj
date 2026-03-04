using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterWatcher : MonoBehaviour
{
    [Header("모니터링 및 설정")]
    public Transform manContainer;
    public Transform targetContainer1; // 면허증 사진 칸
    public Transform targetContainer2; // 배달원 확인 사진 칸

    [Header("위조용 캐릭터 리스트")]
    public List<GameObject> allCharacterPrefabs;

    public Vector2 customPosition = Vector2.zero;
    public float customScale = 1.0f;

    private GameObject lastDetectedMan;
    private GameObject clonedMan1, clonedMan2;

    void Update()
    {
        if (manContainer.childCount > 0)
        {
            GameObject currentMan = manContainer.GetChild(0).gameObject;
            if (currentMan != lastDetectedMan)
            {
                
                CloneToPanel(currentMan, targetContainer2, ref clonedMan2, "Real_Photo");

                
                GameObject photoSource = currentMan;

                if (LicenseInfoManager.Instance != null && LicenseInfoManager.Instance.isPhotoForgery)
                {
                    
                    List<GameObject> candidates = allCharacterPrefabs.FindAll(p => p.name != currentMan.name.Replace("(Clone)", "").Trim());
                    if (candidates.Count > 0)
                    {
                        photoSource = candidates[Random.Range(0, candidates.Count)];
                        Debug.Log("<color=red>[사진 위조]</color> 다른 캐릭터의 외형을 사용합니다.");
                    }
                }

                CloneToPanel(photoSource, targetContainer1, ref clonedMan1, "License_Photo");
                lastDetectedMan = currentMan;
            }
        }
        else
        {
            if (clonedMan1 != null) Destroy(clonedMan1);
            if (clonedMan2 != null) Destroy(clonedMan2);
            lastDetectedMan = null;
        }
    }

    void CloneToPanel(GameObject original, Transform container, ref GameObject cloneRef, string cloneName)
    {
        if (container == null) return;
        if (cloneRef != null) Destroy(cloneRef);

        
        cloneRef = Instantiate(original, container);
        cloneRef.name = cloneName;

        
        MonoBehaviour[] allScripts = cloneRef.GetComponentsInChildren<MonoBehaviour>();
        foreach (var script in allScripts)
        {
            if (script is Image || script is CanvasRenderer || script is TMPro.TextMeshProUGUI || script is RectTransform)
            {
                continue;
            }
            Destroy(script);
        }

        
        RectTransform rt = cloneRef.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.anchoredPosition = customPosition;
            rt.localScale = new Vector3(customScale, customScale, 1f);
        }

       
        Image[] images = cloneRef.GetComponentsInChildren<Image>();
        foreach (var img in images)
        {
            Color c = img.color;
            c.a = 1f;
            img.color = c;
        }

        CanvasGroup cg = cloneRef.GetComponent<CanvasGroup>();
        if (cg != null) cg.alpha = 1f;

        cloneRef.transform.SetAsLastSibling();
    }
}