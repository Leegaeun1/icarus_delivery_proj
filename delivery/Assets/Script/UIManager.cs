using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.IO;


public class UIManager : MonoBehaviour
{
    public GameObject menuPanel;
    List<string> names;

    // ���ѽð����� ����, Ȯ�� ������ �Ѿ���� �����ô�

    // Ȯ�� ������ ���� ������ ī�忡 �ش��ϴ� �̸��� �����սô�
    private void Start()
    {
        names = CheckMenu.selectedNames;
    }
    public void OnMenuSelect(string label)
    {
        Debug.Log($"���õ� �޴�: {label}");
        //menuPanel.SetActive(false);
        // ��Ÿ ó��

    }

    public void info()
    {
        //names = CheckMenu.selectedNames;
        if (names.Count == 0)
        {
            Debug.Log("���õ� �׸��� �����ϴ�.");
        }
        else
        {
            foreach (string name in names)
            {
                Debug.Log("���õ� �̸�: " + name);
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
                //Debug.Log($" ������ġ: {sandwich.name}");
                foreach (var ingredient in sandwich.ingredients)
                {
                    //Debug.Log($" ���: {ingredient.key} = {ingredient.value}");
                    for(int i = 0; i < cnt; i++)
                    {
                        if (ingredient.key == names[i] && ingredient.value !="X") // X�ΰ� �����������.
                        {
                            tmp_len++;
                        }
                    }
                    
                }
                if (tmp_len == cnt)
                {
                    Debug.Log($" ������ġ: {sandwich.name}");
                }
            }
        }
        else
        {
            Debug.LogWarning("menu.json ������ �����ϴ�.");
        }
    }
}
