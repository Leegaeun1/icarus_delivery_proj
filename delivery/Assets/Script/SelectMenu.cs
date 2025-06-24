using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SelectMenu : MonoBehaviour
{
    public UIManager manager;

    public void OnClick()
    {
        string label = GetComponentInChildren<TextMeshProUGUI>().text;
        manager.OnMenuSelect(label);
    }
}
