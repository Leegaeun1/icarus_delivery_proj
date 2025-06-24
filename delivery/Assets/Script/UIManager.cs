using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public GameObject menuPanel;
    // 제한시간동안 고르고, 확인 누르면 넘어가도록 만들어봅시다
    
    // 확인 누르기 전에 선택한 카드에 해당하는 이름을 저장합시다

    public void OnMenuSelect(string label)
    {
        Debug.Log($"선택된 메뉴: {label}");
        //menuPanel.SetActive(false);
        // 기타 처리
        SceneManager.LoadScene("Kitchen");
    }
}
