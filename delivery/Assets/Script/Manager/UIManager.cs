using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.IO;
using UnityEngine.UI;
using System.Collections;


public class UIManager : MonoBehaviour
{
    public GameObject menuPanel;
    List<string> names;
    public GameObject addPanel;

    // 제한시간동안 고르고, 확인 누르면 넘어가도록 만들어봅시다

    private void Start()
    {
        // 안전하게 찾기 (이름으로 찾다가 실패하면 null이 됨)
        GameObject canvas = GameObject.Find("Menu_Canvas");
        if (canvas != null)
        {
            // 자식 개수가 충분한지 확인 후 가져오기
            if (canvas.transform.childCount > 2)
                menuPanel = canvas.transform.GetChild(2).gameObject;

            // 계층 구조가 복잡하므로 예외 처리
            if (canvas.transform.childCount > 1 && canvas.transform.GetChild(1).childCount > 2)
                addPanel = canvas.transform.GetChild(1).transform.GetChild(2).gameObject;
        }
    }
    public void nextScene()
    {
        // null 체크를 해야 에러가 안 나고 씬이 넘어감
        if (menuPanel != null) menuPanel.SetActive(false);
        if (addPanel != null) addPanel.SetActive(false);

        Debug.Log("Kitchen 씬으로 이동합니다.");
        SceneManager.LoadScene("Kitchen");
    }

    [System.Serializable]
    public class Ingredient
    {
        public string key;
        public string value;
    }

    [System.Serializable]
    public class Sandwich
    {
        public string name;
        public List<Ingredient> ingredients;
    }

    [System.Serializable]
    public class SandwichListWrapper
    {
        public List<Sandwich> sandwiches;
    }
    
}
