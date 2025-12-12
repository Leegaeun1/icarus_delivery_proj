using UnityEngine;
using UnityEngine.UI;

public class pos : MonoBehaviour
{
    public Button posButton;
    public GameObject shimPanel;
    public GameObject popupPanel;

    void Awake()
    {
        shimPanel.SetActive(false);
        popupPanel.SetActive(false);
    }

    void Start()
    {
        posButton.onClick.AddListener(OpenPopup);
        shimPanel.GetComponent<Button>().onClick.AddListener(ClosePopup);
    }

    public void OpenPopup()
    {
        shimPanel.SetActive(true);
        popupPanel.SetActive(true);
    }

    public void ClosePopup()
    {
        shimPanel.SetActive(false);
        popupPanel.SetActive(false);
    }
}