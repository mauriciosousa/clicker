using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GyroClicker : MonoBehaviour {

    public Texture activeTexture;
    public Texture notActiveTexture;

    public bool click = false;

    public float texSize;
    private Rect rect;


    public string address;
    public int port;

    private UDPUnicast _udp = null;
    public int sendRate = 100;

    public bool ShowConfig;
    public Texture Network_ON;
    public Texture Network_OFF;

    private GUIStyle _titleStyle;
    private GUIStyle _normalText;
    private GUIStyle _yText;
    private GUIStyle _buttonText;

    private GUIStyle _red;
    private GUIStyle _green;
    private GUIStyle _blue;
    private GUIStyle _blue2;


    private string newAddress = "";
    private string newPort = "";

    public DominantHandType dominantHand = DominantHandType.Right;

    private bool _reset = false;
    public float resetTexSize;
    public Texture resetActiveTex;
    public Texture resetNotActiveTex;
    private Rect resetRect;

    void Start ()
    {
        Application.runInBackground = true;
        Input.gyro.enabled = true;

        texSize = 0.8f * Screen.width;
        rect = new Rect(Screen.width / 2 - texSize / 2, Screen.height / 2 - texSize / 2, texSize, texSize);

        resetTexSize = texSize / 4.0f;
        resetRect = new Rect(Screen.width / 2 - resetTexSize / 2, Screen.height / 8 - resetTexSize / 2, resetTexSize, resetTexSize);


        _udp = new UDPUnicast(address, port, sendRate);

        newAddress = address;
        newPort = "" + port;

        _titleStyle = new GUIStyle();
        _titleStyle.fontStyle = FontStyle.Bold;
        _titleStyle.normal.textColor = Color.white;
        _titleStyle.fontSize = 60;

        _normalText = new GUIStyle();
        _normalText.fontSize = 60;
        _normalText.normal.textColor = Color.white;

        _buttonText = new GUIStyle();
        _buttonText.fontSize = 70;
        _buttonText.normal.textColor = Color.yellow;
        _buttonText.alignment = TextAnchor.UpperRight;

        _yText = new GUIStyle();
        _yText.fontSize = 60;
        _yText.normal.textColor = Color.yellow;

        _red = new GUIStyle();
        _red.fontSize = 60;
        _red.normal.textColor = Color.red;

        _green = new GUIStyle();
        _green.fontSize = 60;
        _green.normal.textColor = Color.green;

        _blue = new GUIStyle();
        _blue.fontSize = 60;
        _blue.normal.textColor = Color.blue;

        _blue2 = new GUIStyle();
        _blue2.fontSize = 60;
        _blue2.normal.textColor = Color.cyan;

    }

    void Update ()
    {
        if (!Input.gyro.enabled) return;

        click = !ShowConfig && _checkClick();
        _reset = Input.GetMouseButton(0) && resetRect.Contains(new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y));


        Quaternion attitude = Input.gyro.attitude;

        string toSend =
            "hand=" + (dominantHand == DominantHandType.Right ? "Right" : "Left") + "/"
            + "reset=" + _reset + "/"
            + "click=" + click + "/"
            + "a.x=" + attitude.x + "/"
            + "a.y=" + attitude.y + "/"
            + "a.z=" + attitude.z + "/"
            + "a.w=" + attitude.w;

        //_reset = false;

        toSend.Replace(",", ".");

        print(toSend);

        if (!ShowConfig)
        {
            _udp.send(toSend);
        }
    }

    void OnGUI()
    {
        if (GUI.Button(new Rect(0, 0, 250, 250), ShowConfig ? Network_ON : Network_OFF, GUIStyle.none))
        {
            ShowConfig = !ShowConfig;
        }

        if (ShowConfig)
        {
            int newLine = 110;
            int width = 1500;
            int height = newLine;
            int bheight = height + ((int)0.5f) * height;


            int top = 5;
            int left = 100;

            GUI.Label(new Rect(left, top, width, height), "Sending to: " + address + ":" + port, _normalText); top += newLine;
            GUI.Label(new Rect(left, top, width, height), "Gyro: " + Input.gyro.enabled, Input.gyro.enabled ? _normalText : _red); top += newLine;
            if (Input.gyro.enabled) GUI.Label(new Rect(left, top, width, height), "Attitude: " + Input.gyro.attitude, _normalText); top += newLine;
            if (Input.gyro.enabled) GUI.Label(new Rect(left, top, width, height), "Rotation: " + gameObject.transform.rotation.ToString(), _normalText); top += newLine;

            GUI.Label(new Rect(left, top, width, height), "x = " + gameObject.transform.rotation.eulerAngles.x, _red); top += newLine;
            GUI.Label(new Rect(left, top, width, height), "y = " + gameObject.transform.rotation.eulerAngles.y, _green); top += newLine;
            GUI.Label(new Rect(left, top, width, height), "z = " + gameObject.transform.rotation.eulerAngles.z, _blue); top += newLine;

            top += newLine;

            GUI.Label(new Rect(left, top, width, height), "Network Settings:", _titleStyle);
            top += newLine; top += newLine;
            GUI.Label(new Rect(left + 100, top, width, height), "UDP Address:", _normalText);
            newAddress = GUI.TextField(new Rect(Screen.width - 450, top, 400, bheight), newAddress, _blue2);
            top += newLine; top += newLine;
            GUI.Label(new Rect(left + 100, top, width, height), "UDP Port:", _normalText);
            newPort = GUI.TextField(new Rect(Screen.width - 450, top, 400, bheight), newPort, _blue2);

            top += newLine; top += newLine;
            if (GUI.Button(new Rect(Screen.width - 550, top, 500, bheight), "Reset", _buttonText))
            {

                int p;
                if (int.TryParse(newPort, out p))
                {
                    port = p;
                    address = newAddress;
                    _udp = new UDPUnicast(address, port, sendRate);
                }
            }

            top += newLine; top += newLine;

            GUI.Label(new Rect(left, top, width, height), "Dominant Hand:", _titleStyle);
            if (GUI.Button(new Rect(Screen.width - 550, top, 500, bheight), dominantHand.ToString(), _buttonText))
            {
                if (dominantHand == DominantHandType.Right)
                {
                    dominantHand = DominantHandType.Left;
                }
                else dominantHand = DominantHandType.Right;
            }


        }
        else
        {
            if (click)
            {
                GUI.DrawTexture(rect, activeTexture);
            }
            else
            {
                GUI.DrawTexture(rect, notActiveTexture);
            }

            if (_reset)
            {
                GUI.DrawTexture(resetRect, resetActiveTex);
            }
            else
            {
                GUI.DrawTexture(resetRect, resetNotActiveTex);
            }
        }
    }

    private bool _checkClick()
    {
        Vector3 position = Input.mousePosition;
        bool click = Input.GetMouseButton(0);

        if (rect.Contains(new Vector2(position.x, position.y)) && click)
        {
            return true;
        }
        return false;
    }
}
