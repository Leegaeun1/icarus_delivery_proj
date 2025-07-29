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

    // ���ѽð����� ����, Ȯ�� ������ �Ѿ���� �����ô�

    private void Start()
    {
        names = CheckMenu.selectedNames;
    }
    public void nextScene()
    {
        menuPanel.SetActive(false); // �Ѿ�� �޴� ����â �Ⱥ��̵��� �ϱ� 
        addPanel.SetActive(false); // �Ѿ�� �߰� ��� ���� �Ⱥ��̵��� �ϱ�
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
    public void LoadSandwiches() // Json ���� �о�ͼ� �˸��� ������ġ�� �ҷ��;���.
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
