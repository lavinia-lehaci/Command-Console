using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections.Generic;

[ExecuteInEditMode]
public class CommandController : MonoBehaviour
{
    bool displayConsole;
    string commandInput;

    public List<CommandDetails> commands; 

    [Serializable]
    public struct CommandDetails
    {
        public string name;
        public string description;
        public UnityEvent<string> function;
    }

    private List<Command<string>> _commandList;

    public void Awake()
    {
        _commandList = new List<Command<string>>();
        foreach (CommandDetails command in commands)
        {
            _commandList.Add(new Command<string>(command.name, command.description, command.function));
        }
    }

    public void Update()
    {
        if(Input.GetKeyDown(KeyCode.BackQuote))
            displayConsole = !displayConsole;
    }

    public void PrintSomething(string args)
    {
        string print = "Printing with args: ";
        foreach(string str in args.Split(' '))
        {
            print += str + " ";
        }
        Debug.Log(print);
    }

    public void PrintNoArgs()
    {
        Debug.Log("Printing with no args");
    }

    private void OnGUI()
    {
        if(!displayConsole)
            return;

        GUI.Box(new Rect(0f, 0f, Screen.width, Screen.height * 0.05f), "");
        GUI.backgroundColor = new Color(0, 0, 0, 0);
        commandInput = GUI.TextField(new Rect(0f, 0f, Screen.width, Screen.height * 0.05f), commandInput);

        if(Event.current.keyCode == KeyCode.Return)
        {
            HandleCommandInput();
            commandInput = "";
        }
    }

    private void HandleCommandInput()
    {
        for(int i = 0; i < _commandList.Count; i++)
        {
            Command<string> command = _commandList[i];
            if(_commandList[i] != null && commandInput.Contains(command.commandName))
                (_commandList[i] as Command<string>).Invoke(commandInput);
        }
    }
}
