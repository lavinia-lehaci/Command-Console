using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections.Generic;

[ExecuteInEditMode]
public class CommandController : MonoBehaviour
{
    public List<CommandDetails> commands; 

    [Serializable]
    public struct CommandDetails
    {
        public string name;
        public string description;
        public UnityEvent<string> function;
    }

    private List<object> _commandList;
    private List<string> _help;
    private List<string> _consoleHistory;

    private bool _displayConsole;
    private Vector2 _scroll;
    private string _commandInput;

    public void Start()
    {
        _commandList = new List<object>();
        _help = new List<string>();
        _consoleHistory = new List<string>();

        _consoleHistory.Add("Type 'help' to see all commands");
        UnityEvent help = new UnityEvent();
        help.AddListener(() => {   
            foreach(string str in _help)
            {
                _consoleHistory.Add(str);
            }} 
        );
        _commandList.Add(new Command("help", "display all commands", help));
        _help.Add("help - display all commands");

        UnityEvent clear = new UnityEvent();
        clear.AddListener(() => { _consoleHistory.Clear(); });
        _commandList.Add(new Command("clear", "clear console", clear));
        _help.Add("clear - clear console");

        foreach (CommandDetails command in commands)
        {
            _commandList.Add(new Command<string>(command.name, command.description, command.function));
            _help.Add($"{command.name} - {command.description}");
        }
    }

    public void Update()
    {
        if(Input.GetKeyDown(KeyCode.BackQuote))
            _displayConsole = !_displayConsole;
    }

    public void PrintWithArgs(string args)
    {
        string print = "";
        for(int i = 1; i < args.Split(' ').Length; i++)
        {
            print += args.Split(' ')[i] + " ";
        }
        Debug.Log(print);
    }

    public void PrintNoArgs()
    {
        Debug.Log("Simple print, no args.");
    }

    public void PrintWarning()
    {
        Debug.LogWarning("A warning assigned to this transform!", transform);
    }

    public void AddLog(string str)
    {
        _consoleHistory.Add(str);
    }

    private void OnGUI()
    {
        if(!_displayConsole)
            return;

        float yBox = Screen.height * 0.6f;

        GUI.backgroundColor = Color.black;
        GUI.Box(new Rect(0f, yBox, Screen.width, Screen.height * 0.35f), "");
        
        _scroll = GUI.BeginScrollView(
            new Rect(0f, yBox, Screen.width, Screen.height * 0.35f), 
            _scroll, 
            new Rect(0f, 0f, Screen.width * 0.9f, 20 * _consoleHistory.Count));


        GUIStyle wrappedStyle = new GUIStyle(GUI.skin.label);
        wrappedStyle.wordWrap = true; 
        
        if(_consoleHistory.Count > 0)
        {
            for(int i = 0; i < _consoleHistory.Count; i++)
            {
                float labelHeight = wrappedStyle.CalcHeight(new GUIContent(_consoleHistory[i]), Screen.width);
                GUI.Label(new Rect(Screen.width * 0.01f, 20 * i, Screen.width, labelHeight), _consoleHistory[i]);
            }
        }

        GUI.EndScrollView();

        yBox += Screen.height * 0.35f;
        GUI.Box(new Rect(0f, yBox, Screen.width, Screen.height * 0.05f), "");
        GUI.backgroundColor = new Color(0, 0, 0, 0);
        _commandInput = GUI.TextField(new Rect(0f, yBox, Screen.width, Screen.height * 0.05f), _commandInput);

        if(Event.current.keyCode == KeyCode.Return)
        {
            if(_commandInput != "")
                _consoleHistory.Add(">> " + _commandInput);

            HandleCommandInput();
            _commandInput = "";
        }
    }

    private void HandleCommandInput()
    {
        for(int i = 0; i < _commandList.Count; i++)
        {
            CommandBase command = _commandList[i] as CommandBase;
            if(_commandInput.Split(' ' )[0] == command.commandName)
            {
                if(command as Command != null)
                {
                    (command as Command).Invoke();
                }
                else if (command as Command<string> != null)
                {
                    (command as Command<string>).Invoke(_commandInput);
                }
            }
        }
    }
}
