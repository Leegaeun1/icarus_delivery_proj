using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.IO;


public class UIManager : MonoBehaviour
{
    public GameObject menuPanel;
    List<string> names;

    // 제한시간동안 고르고, 확인 누르면 넘어가도록 만들어봅시다

    // 확인 누르기 전에 선택한 카드에 해당하는 이름을 저장합시다
    private void Start()
    {
        names = CheckMenu.selectedNames;
    }
    public void OnMenuSelect(string label)
    {
        Debug.Log($"선택된 메뉴: {label}");
        //menuPanel.SetActive(false);
        // 기타 처리

    }

    public void info()
    {
        //names = CheckMenu.selectedNames;
        if (names.Count == 0)
        {
            Debug.Log("선택된 항목이 없습니다.");
        }
        else
        {
            foreach (string name in names)
            {
                Debug.Log("선택된 이름: " + name);
            }
            print(string.Join(",", names));
        }
    }

    public void nextScene()
    {
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
    public void LoadSandwiches()
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
