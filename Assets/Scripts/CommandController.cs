using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// Class that controls the logic of the command console.
/// </summary>
[ExecuteInEditMode]
public class CommandController : MonoBehaviour
{
    /// <summary>
    /// Commands set in the Unity Inspector.
    /// </summary>
    public List<CommandDetails> Commands; 
    [Serializable]
    public struct CommandDetails
    {
        public string Name;
        public string Description;
        public UnityEvent<string> Function;
    }

    private bool _isConsoleVisible;
    private Vector2 _scrollPosition;
    private string _input;
    private const int _maxCommandSize = 100;    // Maximum commands stored in the console history.

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
            _isConsoleVisible = !_isConsoleVisible;

        // If the limit is exceeded, the oldest command is deleted from the console history.
        if(_consoleHistory.Count > _maxCommandSize)
            _consoleHistory.Dequeue();

        // Print the default help message if the console history is empty.
        if(_consoleHistory.Count == 0)
            _consoleHistory.Enqueue("Type 'help' to see all commands.");
    }

    /// <summary>
    /// Registers a predefined set of commands.
    /// - 'help' displays a list of all commands and their descriptions.
    /// - 'clear' clears the console screen. It does not delete the console history.
    /// - 'save_logs' saves the console history to a file.
    /// </summary>
    private void RegisterDefaultCommands()
    {
        UnityEvent showHelpEvent = new UnityEvent();
        showHelpEvent.AddListener(() => {   
            for(int i = 0; i < _commandList.Count; i++)
            {
                CommandBase command = _commandList[i] as CommandBase;
                string str = $"{command.CommandName} - {command.CommandDescription}";
                _consoleHistory.Enqueue(str);
                _logs.Enqueue(str);
            }
        });
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

    /// <summary>
    /// Adds the commands set in the Unity Inspector to the command list.
    /// </summary>
    private void AddCommandsFromInspector()
    {
        foreach (CommandDetails command in Commands)
        {
            _commandList.Add(new Command<string>(command.Name, command.Description, command.Function));
        }
    }

    /// <summary>
    /// Writes the current console history to a file.
    /// <param name="args">String that contains multiple arguments. When parsed, it contains the file name.</param>
    /// </summary>
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

    /// <summary>
    /// Called by the SubscribeToUnityLogs class when a log message is received.
    /// Adds the log message to the console, as well as the list used to save the logs. 
    /// <param name="log">Log message</param>
    /// </summary>
    public void AddLog(string log)
    {
        _consoleHistory.Enqueue(log);
        _logs.Enqueue(log);
    }

    /// <summary>
    /// Renders and handles GUI events.
    /// </summary>
    private void OnGUI()
    {
        if(!_isConsoleVisible)
            return;

        // Create the console box.
        float yBox = Screen.height * 0.6f;
        GUI.backgroundColor = Color.black;
        GUI.Box(new Rect(0f, yBox, Screen.width, Screen.height * 0.35f), "");

        // Calculate the height of the scrolling area's viewport based on the height of the console text.
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

        // Create a scrolling area.
        _scrollPosition = GUI.BeginScrollView(
            new Rect(0f, yBox, Screen.width, Screen.height * 0.35f), 
            _scrollPosition, 
            new Rect(0f, 0f, Screen.width * 0.9f, contentHeight));

        // Display the console text. 
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

        // Create the input field.
        yBox += Screen.height * 0.35f;
        GUI.Box(new Rect(0f, yBox, Screen.width, Screen.height * 0.05f), "");
        GUI.backgroundColor = new Color(0, 0, 0, 0);
        _input = GUI.TextField(new Rect(0f, yBox, Screen.width, Screen.height * 0.05f), _input);

        // Handle input.
        if(Event.current.keyCode == KeyCode.Return)
        {
            if(String.IsNullOrEmpty(_input))
                return;

            _consoleHistory.Enqueue("> " + _input);
            _logs.Enqueue($"> {_input}");
            
            HandleCommandInput();
            _input = "";
            _scrollPosition.y = float.MaxValue; // Scroll down to the last element added to the console.
        }
    }

    /// <summary>
    /// Processes the input based on the type of command inserted.
    /// </summary>
    private void HandleCommandInput()
    {
        for(int i = 0; i < _commandList.Count; i++)
        {
            CommandBase command = _commandList[i] as CommandBase;
            if(_input.Split(' ')[0] == command.CommandName)
            {
                if(command as Command != null)
                {
                    (command as Command).Invoke();
                }
                else if (command as Command<string> != null)
                {
                    (command as Command<string>).Invoke(_input);
                }
            }
        }
    }

    #region Testing commands
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
    #endregion
}
