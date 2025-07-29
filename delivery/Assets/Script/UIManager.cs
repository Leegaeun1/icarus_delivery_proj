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
    public void LoadSandwiches() // Json 파일 읽어와서 알맞은 샌드위치를 불러와야함.
    {
        string path = Path.Combine(Application.streamingAssetsPath, "menu.json");
        int cnt = names.Count;

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            SandwichListWrapper wrapper = JsonUtility.FromJson<SandwichListWrapper>(json);

            foreach (var sandwich in wrapper.sandwiches)
            {
                int tmp_len = 0;
                //Debug.Log($" 샌드위치: {sandwich.name}");
                foreach (var ingredient in sandwich.ingredients)
                {
                    //Debug.Log($" 재료: {ingredient.key} = {ingredient.value}");
                    for(int i = 0; i < cnt; i++)
                    {
                        if (ingredient.key == names[i] && ingredient.value !="X") // X인거 제외해줘야함.
                        {
                            tmp_len++;
                        }
                    }
                    
                }
                if (tmp_len == cnt)
                {
                    Debug.Log($" 샌드위치: {sandwich.name}");
                }
            }
        }
        else
        {
            Debug.LogWarning("menu.json 파일이 없습니다.");
        }
    }

}
