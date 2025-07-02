using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // UI ���� ���ӽ����̽� �߰�

public class click : MonoBehaviour
{
    public Button checkButton;  // ��ư ����
    public GameObject imageToShow;  // ������ �̹��� (GameObject�� ����)

    // Start is called before the first frame update
    void Start()
    {
        // ��ư�� Ŭ�� �̺�Ʈ �߰�
        checkButton.onClick.AddListener(OnCheckButtonClick);

        // ó������ �̹����� ������ �ʵ��� ����
        imageToShow.SetActive(false);
    }

    // ��ư Ŭ�� �� ����Ǵ� �Լ�
   public void OnCheckButtonClick()
    {
        // �̹����� ����Ͽ� ���̰ų� ����ϴ�.
        imageToShow.SetActive(!imageToShow.activeSelf);
    }

    // Update is called once per frame
    void Update()
    {
        // �� �����Ӹ��� �� ���� ���ٸ� ����ξ �˴ϴ�.
    }
}
