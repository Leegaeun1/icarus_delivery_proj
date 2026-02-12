using UnityEngine;
using UnityEngine.SceneManagement; 

public class MenutoCook : MonoBehaviour
{
    public OrrerManager orrdermanager;

    private void Start()
    {
        orrdermanager = GameObject.Find("OrderObject").GetComponent<OrrerManager>();
    }
    // 버튼의 OnClick 이벤트에 연결할 함수입니다.
    public void LoadScene(string cook)
    {
        orrdermanager.OnClickStartCooking();
        SceneManager.LoadScene(cook);
    }
}