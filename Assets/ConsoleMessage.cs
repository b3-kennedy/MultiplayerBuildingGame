using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ConsoleMessage : MonoBehaviour
{

    public TextMeshProUGUI messageText;

    public void SetMessage(string text, Color colour = default)
    {
        if(colour == default)
        {
            messageText.color = Color.white;
        }

        messageText.text = text;
        messageText.color = colour;
    }

}
