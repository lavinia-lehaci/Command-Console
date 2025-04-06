using UnityEngine;
using UnityEditor;

/// <summary>
/// Custom editor for the CommandController class.
/// </summary>
[CustomEditor(typeof(CommandController))]
[CanEditMultipleObjects]
public class CommandEditor : Editor
{
    public SerializedProperty CommandList;
    private bool[] _shouldCommandsCollapse; 
    
    void OnEnable()
    {
        CommandList = serializedObject.FindProperty("Commands");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        if(CommandList != null && CommandList.isArray)
        {
            if(_shouldCommandsCollapse == null || _shouldCommandsCollapse.Length != CommandList.arraySize)
                _shouldCommandsCollapse = new bool[CommandList.arraySize];
            
            for(int i = 0; i < CommandList.arraySize; i++)
            {
                SerializedProperty command = CommandList.GetArrayElementAtIndex(i);
                SerializedProperty commandName = command.FindPropertyRelative("Name");
                SerializedProperty commandDescription = command.FindPropertyRelative("Description");
                SerializedProperty commandFunction = command.FindPropertyRelative("Function");

                GUILayout.BeginHorizontal();
                _shouldCommandsCollapse[i] = EditorGUILayout.Foldout(_shouldCommandsCollapse[i], "Command " + i + " [" + commandName.stringValue + "]");
                if(GUILayout.Button("Delete", GUILayout.Width(110)))
                {
                    CommandList.DeleteArrayElementAtIndex(i);
                }  
                GUILayout.EndHorizontal();

                if(_shouldCommandsCollapse[i])
                {
                    EditorGUILayout.PropertyField(commandName, new GUIContent("Name"));
                    EditorGUILayout.PropertyField(commandDescription, new GUIContent("Description"));
                    EditorGUILayout.PropertyField(commandFunction, new GUIContent("Function"));
                }
            }
        }

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Add new command", GUILayout.Width(150)))
        {
            CommandList.InsertArrayElementAtIndex(CommandList.arraySize);
        }
        if (GUILayout.Button("Delete all commands", GUILayout.Width(150)))
        {
            CommandList.ClearArray();
        }
        GUILayout.FlexibleSpace(); 
        GUILayout.EndHorizontal();
        
        serializedObject.ApplyModifiedProperties();
    }
}
