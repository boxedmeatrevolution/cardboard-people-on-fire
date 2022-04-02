using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCInteractArea : Interactable
{
    public override void Awake() {
        base.Awake();
    }

    public override void OnFocus()
    {
        Debug.Log("Focus");
    }

    public override void OnInteract()
    {
        TMPro.TextMeshPro textComponent = GetComponentInChildren<TMPro.TextMeshPro>();
        if (textComponent) 
        {
            textComponent.SetText("It's a plot.");
        }
        Debug.Log("Interact");
    }

    public override void OnLoseFocus()
    {
        Debug.Log("LoseFocus");
    }
}
