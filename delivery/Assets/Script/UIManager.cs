using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public GameObject menuPanel;
    // ���ѽð����� ����, Ȯ�� ������ �Ѿ���� �����ô�
    
    // Ȯ�� ������ ���� ������ ī�忡 �ش��ϴ� �̸��� �����սô�

    public void OnMenuSelect(string label)
    {
        Debug.Log($"���õ� �޴�: {label}");
        //menuPanel.SetActive(false);
        // ��Ÿ ó��
        SceneManager.LoadScene("Kitchen");
    }
}
