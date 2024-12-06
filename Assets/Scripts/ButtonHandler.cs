using System;
using UnityEngine;
using UnityEngine.UI;

public class ButtonHandler : MonoBehaviour
{
    // public GameObject confrimPanel;

    // private void Start()
    // {
    //     // confrimPanel = GameObject.FindWithTag("confirmPanel");
    //     confrimPanel = MainController.confrimPanel;
    // }

    public void OnButtonClicked()
    {
        GameObject buttonGameObject = gameObject;

        Text textComponent = buttonGameObject.GetComponent<Text>();

        // if (textComponent != null)
        // {
        //     textComponent.text = "Button Clicked!";
        // }
        
        // GameObject confrimPanel = GameObject.FindWithTag("confirmPanel");
        
        
        var selectWord = textComponent.text.Split(":");
        MainController.confrimWord = selectWord[0];
        MainController.openConfrimPanelBool = true;

    }
}