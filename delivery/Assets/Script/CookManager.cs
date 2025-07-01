using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CookManager : MonoBehaviour
{
    List<string> strings = CheckMenu.selectedNames; // 선택된 항목 불러옴
    public GameObject sandwich;
    public int ChildCnt;
    // Start is called before the first frame update
    void Start()
    {
        print(string.Join(',',strings));
        ChildCnt = sandwich.transform.childCount;
        approachChild(ChildCnt);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void approachChild(int cnt)
    {
        foreach (string s in strings) {
            for (int i = 0; i < cnt; i++) { 
                if (sandwich.transform.GetChild(i).name.Contains(s)){
                    sandwich.transform.GetChild(i).gameObject.SetActive(true);
                    print(s);
                }
            }

            
        }
    }
}
