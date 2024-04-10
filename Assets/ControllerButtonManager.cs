using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;

public class ControllerButtonManager : MonoBehaviour
{
    public UnityEvent primaryButtonDown;
    public UnityEvent secondaryButtonDown;
    public UnityEvent menuButtonDown;

    private bool lastPrimaryButtonState = false;
    private bool lastSecondaryButtonState = false;
    private bool lastMenuButtonState = false;

    private readonly List<InputDevice> devices = new();

    void OnEnable()
    {
        // Enable時に接続されているデバイスを取得
        InputDevices.GetDevices(devices);

        // そのあとに接続・切断時のイベントを登録
        InputDevices.deviceConnected += InputDevices_deviceConnected;
        InputDevices.deviceDisconnected += InputDevices_deviceDisconnected;
    }

    private void OnDisable()
    {
        InputDevices.deviceConnected -= InputDevices_deviceConnected;
        InputDevices.deviceDisconnected -= InputDevices_deviceDisconnected;
        devices.Clear();
    }

    private void InputDevices_deviceConnected(InputDevice device)
    {
        devices.Add(device);
    }

    private void InputDevices_deviceDisconnected(InputDevice device)
    {
        if (devices.Contains(device))
            devices.Remove(device);
    }

    bool ButtonPressed(InputFeatureUsage<bool> button)
    {
        bool tempState = false;
        foreach (var device in devices)
        {
            tempState = device.TryGetFeatureValue(button, out bool buttonState) // did get a value
                        && buttonState // the value we got
                        || tempState; // cumulative result from other controllers
        }
        return tempState;
    }

    void Update()
    {
        if (ButtonPressed(CommonUsages.primaryButton))
        {
            if (lastPrimaryButtonState == false)
            {
                primaryButtonDown.Invoke();
                lastPrimaryButtonState = true;
            }
        }
        else
        {
            lastPrimaryButtonState = false;
        }

        if (ButtonPressed(CommonUsages.secondaryButton))
        {
            if (lastSecondaryButtonState == false)
            {
                secondaryButtonDown.Invoke();
                lastSecondaryButtonState = true;
            }
        }
        else
        {
            lastSecondaryButtonState = false;
        }

        if (ButtonPressed(CommonUsages.menuButton))
        {
            if (lastMenuButtonState == false)
            {
                menuButtonDown.Invoke();
                lastMenuButtonState = true;
            }
        }
        else
        {
            lastMenuButtonState = false;
        }
    }
}
