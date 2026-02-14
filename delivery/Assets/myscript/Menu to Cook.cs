using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenutoCook : MonoBehaviour
{
    public void LoadScene(string cook)
    {
        SceneManager.LoadScene(cook);
    }
}