using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Custom editor for the CommandController class.
/// </summary>
[CustomEditor(typeof(CommandController))]
[CanEditMultipleObjects]
public class CommandEditor : Editor
{
    public SerializedProperty CommandList;
    private bool[] _shouldCommandsCollapse; 

    /// <summary>
    /// Stores the state of each command.
    /// `False` if it is collapsed, `true` otherwise.
    /// </summary>
    private List<bool> _commandsState = new List<bool>();
    
    /// <summary>
    /// When the `Delete` button of each command is clicked, said command will be added to the list. 
    /// The action will be executed after the rendering of all commands. 
    /// </summary>
    private List<int> _commandsToDelete = new List<int>();

    void OnEnable()
    {
        CommandList = serializedObject.FindProperty("Commands");

        for (int i = 0; i < CommandList.arraySize; i++)
        {
            _commandsState.Add(true);
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        if(CommandList != null && CommandList.isArray)
        {
            if(_shouldCommandsCollapse == null || _shouldCommandsCollapse.Length != CommandList.arraySize)
            {
                _shouldCommandsCollapse = new bool[CommandList.arraySize];

                if (_commandsState.Count < CommandList.arraySize)
                {
                    for (int i = _commandsState.Count; i < CommandList.arraySize; i++)
                    {
                        _commandsState.Add(true);
                    }
                }
                
                for(int i = 0; i < CommandList.arraySize; i++)
                {
                    _shouldCommandsCollapse[i] = _commandsState[i];
                }
            }
            
            for(int i = 0; i < CommandList.arraySize; i++)
            {
                SerializedProperty command = CommandList.GetArrayElementAtIndex(i);
                SerializedProperty commandName = command.FindPropertyRelative("Name");
                SerializedProperty commandDescription = command.FindPropertyRelative("Description");
                SerializedProperty commandFunction = command.FindPropertyRelative("Function");

                GUILayout.BeginHorizontal();
                _shouldCommandsCollapse[i] = EditorGUILayout.Foldout(_shouldCommandsCollapse[i], "Command " + i + " [" + commandName.stringValue + "]");
                _commandsState[i]= _shouldCommandsCollapse[i];

                if(GUILayout.Button("Delete", GUILayout.Width(110)))
                {
                     _commandsToDelete.Add(i);
                }  
                GUILayout.EndHorizontal();

                if(_shouldCommandsCollapse[i])
                {
                    EditorGUILayout.PropertyField(commandName, new GUIContent("Name"));
                    EditorGUILayout.PropertyField(commandDescription, new GUIContent("Description"));
                    EditorGUILayout.PropertyField(commandFunction, new GUIContent("Function"));
                }
            }

            if (_commandsToDelete.Count > 0)
            {
                foreach (int index in _commandsToDelete)
                {
                    CommandList.DeleteArrayElementAtIndex(index);
                    _commandsState.RemoveAt(index);
                }

                _commandsToDelete.Clear();
            }
        }

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Add new command", GUILayout.Width(150)))
        {
            CommandList.InsertArrayElementAtIndex(CommandList.arraySize);
            _commandsState.Add(true);
        }
        if (GUILayout.Button("Delete all commands", GUILayout.Width(150)))
        {
            CommandList.ClearArray();
            _commandsState.Clear();
        }
        GUILayout.FlexibleSpace(); 
        GUILayout.EndHorizontal();
        
        serializedObject.ApplyModifiedProperties();
    }
}
