using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class MenuManager : MonoBehaviour
{
    public GameObject check_menu;
    public GameObject select_menu;
    public GameObject[] special_menus;
    public Transform[] positions;
    public Transform canvasTransform;
    int[] num;

    string correctmenu = "craken";
    // Start is called before the first frame update
    void Start()
    {
        check_menu.gameObject.SetActive(true);
        select_menu.gameObject.SetActive(false);
        List<int> tempNumList = new List<int>();
        for (int i = 0; i < special_menus.Length; i++)
        {
            if (special_menus[i].name != correctmenu) // �ùٸ� �޴� �ƴѰ�
            {
                print(i);
                tempNumList.Add(i);
            }
        }
        num = tempNumList.ToArray();
    }

    public void random_card()
    {
        for (int i = 0; i < positions.Length; i++) { // �ߺ� �Ȱɷ���. ��ġ�� �� �ڴ�� ����
            int randomIdx = Random.Range(0, num.Length);
            int randomNum = num[randomIdx];
            print(positions[i].position);
            Instantiate(special_menus[randomNum], positions[i].position, positions[i].rotation); // �־ȵ���
            Debug.Log($"Instantiate: {special_menus[randomNum].name} at {positions[i].position}");
        }

    }
}
