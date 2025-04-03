using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections.Generic;

public class CommandConsole : MonoBehaviour
{
    bool displayConsole;
    string input;

    public List<Command> commands;

    [Serializable]
    public struct Command
    {
        public string name;
        public string description;
        public UnityEvent function;
    }

    public void Update()
    {
        if(Input.GetKeyDown(KeyCode.BackQuote))
            displayConsole = !displayConsole;
    }

    public void doSomething()
    {
        Debug.Log("Doing something");
    }

    private void OnGUI()
    {
        if(!displayConsole)
            return;

        GUI.Box(new Rect(0f, 0f, Screen.width, Screen.height * 0.05f), "");
        GUI.backgroundColor = new Color(0, 0, 0, 0);
        input = GUI.TextField(new Rect(0f, 0f, Screen.width, Screen.height * 0.05f), input);
    }
}
