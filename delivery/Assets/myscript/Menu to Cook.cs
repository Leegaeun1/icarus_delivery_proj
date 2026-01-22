using UnityEngine;
using UnityEngine.SceneManagement; 

public class MenutoCook : MonoBehaviour
{
    // 버튼의 OnClick 이벤트에 연결할 함수입니다.
    public void LoadScene(string cook)
    {
        SceneManager.LoadScene(cook);
    }
}