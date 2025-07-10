using UnityEngine;
using UnityEngine.UI;

public class pos : MonoBehaviour
{
    public Button posButton;
    public GameObject shimPanel;
    public GameObject popupPanel;

    public Button alarmButton;
    public GameObject alarmShimPanel;
    public GameObject alarmPopupPanel;

    public Button dsButton;
    public GameObject dsShimPanel;
    public GameObject dsPopupPanel;

    public Button daButton;
    public GameObject daShimPanel;
    public GameObject daPopupPanel;

    void Awake()
    {
        shimPanel.SetActive(false);
        popupPanel.SetActive(false);
        alarmShimPanel.SetActive(false);
        alarmPopupPanel.SetActive(false);
        dsShimPanel.SetActive(false);
        dsPopupPanel.SetActive(false);
        daShimPanel.SetActive(false);
        daPopupPanel.SetActive(false);

        alarmButton.gameObject.SetActive(false);
        dsButton.gameObject.SetActive(false);
        daButton.gameObject.SetActive(false);
    }

    void Start()
    {
        posButton.onClick.AddListener(OpenPopup);
        shimPanel.GetComponent<Button>().onClick.AddListener(ClosePopup);

        alarmButton.onClick.AddListener(OpenAlarmPopup);
        alarmShimPanel.GetComponent<Button>().onClick.AddListener(CloseAlarmPopup);

        dsButton.onClick.AddListener(OpenDsPopup);
        dsShimPanel.GetComponent<Button>().onClick.AddListener(CloseDsPopup);

        daButton.onClick.AddListener(OpenDaPopup);
        daShimPanel.GetComponent<Button>().onClick.AddListener(CloseDaPopup);
    }

    public void OpenPopup()
    {
        shimPanel.SetActive(true);
        popupPanel.SetActive(true);

        alarmButton.gameObject.SetActive(true);
        dsButton.gameObject.SetActive(true);
        daButton.gameObject.SetActive(true);
    }

    public void ClosePopup()
    {
        shimPanel.SetActive(false);
        popupPanel.SetActive(false);

        alarmButton.gameObject.SetActive(false);
        dsButton.gameObject.SetActive(false);
        daButton.gameObject.SetActive(false);
    }

    public void OpenAlarmPopup()
    {
        alarmShimPanel.SetActive(true);
        alarmPopupPanel.SetActive(true);
    }

    public void CloseAlarmPopup()
    {
        alarmShimPanel.SetActive(false);
        alarmPopupPanel.SetActive(false);
    }

    public void OpenDsPopup()
    {
        dsShimPanel.SetActive(true);
        dsPopupPanel.SetActive(true);
    }

    public void CloseDsPopup()
    {
        dsShimPanel.SetActive(false);
        dsPopupPanel.SetActive(false);
    }

    public void OpenDaPopup()
    {
        daShimPanel.SetActive(true);
        daPopupPanel.SetActive(true);
    }

    public void CloseDaPopup()
    {
        daShimPanel.SetActive(false);
        daPopupPanel.SetActive(false);
    }
}

