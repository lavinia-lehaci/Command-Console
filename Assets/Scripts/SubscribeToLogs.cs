using UnityEngine;
using System.Collections.Generic;

public class SubscribeToLogs : MonoBehaviour
{
    private CommandController commandController;

    void Start()
    {
        commandController = GetComponent<CommandController>();

        if (commandController == null)
        {
            Debug.LogError("CommandController not found on this GameObject.");
            return;
        } 

        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        if (commandController == null)
            return;

        commandController.AddLog($"[{type}] {logString}");

        if (type == LogType.Error || type == LogType.Exception)
        {
            commandController.AddLog($"Stack Trace: {stackTrace}");
        }
    }
}
