using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CommandController))]
[CanEditMultipleObjects]
public class CommandEditor : Editor
{
    SerializedProperty commands;
    private bool[] _itemsCollapsed;
    
    void OnEnable()
    {
        commands = serializedObject.FindProperty("commands");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        if(commands != null && commands.isArray)
        {
            if(_itemsCollapsed == null || _itemsCollapsed.Length != commands.arraySize)
                _itemsCollapsed = new bool[commands.arraySize];

            for(int i = 0; i < commands.arraySize; i++)
            {
                _itemsCollapsed[i] = EditorGUILayout.Foldout(_itemsCollapsed[i], "Command " + i);

                SerializedProperty command = commands.GetArrayElementAtIndex(i);
                SerializedProperty commandName = command.FindPropertyRelative("name");
                SerializedProperty commandDescription = command.FindPropertyRelative("description");
                SerializedProperty commandEvent = command.FindPropertyRelative("function");

                if(_itemsCollapsed[i])
                {
                    EditorGUILayout.PropertyField(commandName, new GUIContent("Name"));
                    EditorGUILayout.PropertyField(commandDescription, new GUIContent("Description"));
                    EditorGUILayout.PropertyField(commandEvent, new GUIContent("Event"));

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if(GUILayout.Button("Delete command", GUILayout.Width(110)))
                    {
                        commands.DeleteArrayElementAtIndex(i);
                    }  
                    GUILayout.EndHorizontal();
                }
            }
        }

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Add new command", GUILayout.Width(150)))
        {
            commands.InsertArrayElementAtIndex(commands.arraySize);
        }
        GUILayout.FlexibleSpace(); 
        GUILayout.EndHorizontal();
        
        serializedObject.ApplyModifiedProperties();
    }
}
