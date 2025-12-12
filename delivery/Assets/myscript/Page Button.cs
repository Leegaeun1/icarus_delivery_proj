using UnityEngine;
using UnityEngine.UI;

public class PageButton : MonoBehaviour
{
    public Button nextPageButton;
    public Button prevPageButton;

    private int currentPage = 0;

    void Start()
    {
        nextPageButton.onClick.AddListener(NextPage);
        prevPageButton.onClick.AddListener(PrevPage);
    }

    public void NextPage()
    {
        currentPage++;
    }

    public void PrevPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
        }
    }
}