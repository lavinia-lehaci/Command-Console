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

        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        string logMessage = $"[{timestamp}] [{type}] {logString}";

        if (type == LogType.Error || type == LogType.Exception)
        {
            logMessage += $"\nStack Trace: {stackTrace}";
        }

        commandController.AddLog(logMessage);
    }
}
