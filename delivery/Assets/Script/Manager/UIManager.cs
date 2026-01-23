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
        names = CheckMenu.selectedNames;
        menuPanel = GameObject.Find("Menu_Canvas").transform.GetChild(2).gameObject;
        addPanel = GameObject.Find("add_Panel");
    }
    public void nextScene()
    {
        menuPanel.SetActive(false); // 넘어가면 메뉴 선택창 안보이도록 하기 
        addPanel.SetActive(false); // 넘어가면 추가 재료 글자 안보이도록 하기

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
