using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;

[System.Serializable]
public class PrimaryButtonEvent : UnityEvent<bool> { }

public class SwitchPassthrough : MonoBehaviour
{
    public Camera mainCamera;
    public GameObject ground;

    private bool isPassthrough = false;

    void Start()
    {
        ChangePassthrough(false);
    }

    public void ChangePassthrough(bool isPassthrough)
    {
        mainCamera.clearFlags = isPassthrough ? CameraClearFlags.SolidColor : CameraClearFlags.Skybox;
        ground.SetActive(!isPassthrough);
        this.isPassthrough = isPassthrough;
    }

    public void TriggerPassthrough()
    {
        ChangePassthrough(!isPassthrough);
    }
}