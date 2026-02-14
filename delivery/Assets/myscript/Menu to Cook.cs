using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenutoCook : MonoBehaviour
{
    public OrrderManager orrdermanager;

    private void Start()
    {
        // 매니저 찾기
        if (orrdermanager == null)
        {
            orrdermanager = GameObject.Find("OrderObject").GetComponent<OrrderManager>();
        }
    }

    // [버튼 연결 함수]
    // 인스펙터에서 cook 문자열에 "CookScene"을 입력해두셨을 겁니다.
    public void LoadScene(string cook)
    {
        // 1. 매니저에게 "데이터 준비해!"라고 시킴 (이동은 하지 말라고 해야 함)
        orrdermanager.OnClickStartCooking();

        // 2. 여기서 실제로 씬을 이동시킴
        SceneManager.LoadScene(cook);
    }
}