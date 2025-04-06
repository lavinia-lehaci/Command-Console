using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections.Generic;
using System.IO;

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

    private bool _displayConsole;
    private Vector2 _scroll;
    private string _commandInput;
    private const int _maxCommandConsoleSize = 100;

    private List<object> _commandList = new List<object>();
    private Queue<string> _consoleHistory = new Queue<string>();
    private Queue<string> _logs = new Queue<string>();

    public void Start()
    {
        _consoleHistory.Enqueue("Type 'help' to see all commands.");
        _logs.Enqueue("Type 'help' to see all commands.");

        RegisterDefaultCommands();
        AddCommandsFromInspector();
    }

    public void Update()
    {
        if(Input.GetKeyDown(KeyCode.BackQuote))
            _displayConsole = !_displayConsole;

        if(_consoleHistory.Count > _maxCommandConsoleSize)
            _consoleHistory.Dequeue();

        if(_consoleHistory.Count == 0)
            _consoleHistory.Enqueue("Type 'help' to see all commands.");
    }

    private void RegisterDefaultCommands()
    {
        UnityEvent showHelpEvent = new UnityEvent();
        showHelpEvent.AddListener(() => {   
            for(int i = 0; i < _commandList.Count; i++)
            {
                CommandBase command = _commandList[i] as CommandBase;
                string str = $"{command.commandName} - {command.commandDescription}";
                _consoleHistory.Enqueue(str);
                _logs.Enqueue(str);
            }
            } 
        );
        Command showHelpCommand = new Command("help", "display all commands", showHelpEvent);
        _commandList.Add(showHelpCommand);

        UnityEvent clearEvent = new UnityEvent();
        clearEvent.AddListener(() => { _consoleHistory.Clear(); });
        Command clearCommand = new Command("clear", "clear console (does not delete console history)", clearEvent);
        _commandList.Add(clearCommand);

        UnityEvent<string> saveLogsEvent = new UnityEvent<string>();
        saveLogsEvent.AddListener(WriteLogsToFile);
        Command<string> saveLogsCommand = new Command<string>("save_logs", "save the console history to a file", saveLogsEvent);
        _commandList.Add(saveLogsCommand);
    }

    private void AddCommandsFromInspector()
    {
        foreach (CommandDetails command in commands)
        {
            _commandList.Add(new Command<string>(command.name, command.description, command.function));
        }
    }

    public void WriteLogsToFile(string args)
    {
        string folderPath = "Assets/Logs";    
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        string fileName = "";
        string[] argsArray = args.Split(' ');
        if(argsArray.Length == 1)
            fileName = "output";
        else
        {
            for(int i = 1; i < argsArray.Length; i++)
            {
                fileName += argsArray[i];
            }
        }
        string filePath = Path.Combine(folderPath, fileName + ".txt");

        try
        {
            using (StreamWriter writer = new StreamWriter(filePath, true)) 
            {
                foreach(string log in _logs)
                {
                    writer.WriteLine(log);
                }
            }

            Debug.Log($"Logs written to {filePath}");
        }
        catch(System.Exception ex)
        {
            Debug.LogWarning($"Failed to save logs to file. \nError: {ex.Message} \nStackTrace: {ex.StackTrace}");
        }
    }

    public void AddLog(string str)
    {
        _consoleHistory.Enqueue(str);
        _logs.Enqueue(str);
    }

    private void OnGUI()
    {
        if(!_displayConsole)
            return;

        float yBox = Screen.height * 0.6f;

        GUI.backgroundColor = Color.black;
        GUI.Box(new Rect(0f, yBox, Screen.width, Screen.height * 0.35f), "");

        GUIStyle wrapStyle = new GUIStyle(GUI.skin.label);
        wrapStyle.wordWrap = true; 
        float contentHeight = 0f;

        if (_consoleHistory.Count > 0)
        {
            foreach(string elem in _consoleHistory)
            {
                float labelHeight = wrapStyle.CalcHeight(new GUIContent(elem), Screen.width * 0.95f);
                contentHeight += labelHeight;
            }
        }

        _scroll = GUI.BeginScrollView(
            new Rect(0f, yBox, Screen.width, Screen.height * 0.35f), 
            _scroll, 
            new Rect(0f, 0f, Screen.width * 0.9f, contentHeight));

        float yOffset = 0f;
        if(_consoleHistory.Count > 0)
        {
            foreach(string elem in _consoleHistory)
            {
                float labelHeight = wrapStyle.CalcHeight(new GUIContent(elem), Screen.width * 0.98f);

                if(elem == _consoleHistory.Peek())
                {
                    GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
                    labelStyle.fontStyle = FontStyle.Italic; 
                    GUI.Label(new Rect(Screen.width * 0.01f, yOffset, Screen.width * 0.98f, labelHeight), elem, labelStyle);
                }
                else
                { 
                    GUI.Label(new Rect(Screen.width * 0.01f, yOffset, Screen.width * 0.98f, labelHeight), elem);
                }

                yOffset += labelHeight;
            }
        }

        GUI.EndScrollView();

        yBox += Screen.height * 0.35f;
        GUI.Box(new Rect(0f, yBox, Screen.width, Screen.height * 0.05f), "");
        GUI.backgroundColor = new Color(0, 0, 0, 0);
        _commandInput = GUI.TextField(new Rect(0f, yBox, Screen.width, Screen.height * 0.05f), _commandInput);

        if(Event.current.keyCode == KeyCode.Return)
        {
            if(String.IsNullOrEmpty(_commandInput))
                return;

            _consoleHistory.Enqueue("> " + _commandInput);
            _logs.Enqueue($"> {_commandInput}");
            
            HandleCommandInput();
            _commandInput = "";
            _scroll.y = float.MaxValue;
        }
    }

    private void HandleCommandInput()
    {
        for(int i = 0; i < _commandList.Count; i++)
        {
            CommandBase command = _commandList[i] as CommandBase;
            if(_commandInput.Split(' ')[0] == command.commandName)
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

    // Testing commands
    public void PrintWithArgs(string args)
    {
        string print = "";
        string[] argsArray = args.Split(' ');
        for(int i = 1; i < argsArray.Length; i++)
        {
            print += argsArray[i] + " ";
        }
        Debug.Log(print);
    }

    public void PrintNoArgs()
    {
        Debug.Log("Simple print, no args.");
    }

    public void PrintWarning()
    {
        Debug.LogWarning($"A warning assigned to this transform! {transform}");
    }
}
