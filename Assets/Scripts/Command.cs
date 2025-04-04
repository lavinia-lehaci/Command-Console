using UnityEngine;
using UnityEngine.Events;

public class CommandBase
{
    private string _commandName;
    private string _commandDescription;

    public string commandName { get { return _commandName; }}
    public string commandDescription { get { return _commandDescription; }}

    public CommandBase(string name, string description)
    {
        _commandName = name;
        _commandDescription = description;
    }
}

public class Command<T> : CommandBase
{
    private UnityEvent<T> _function;

    public Command(string name, string description, UnityEvent<T> function) : base (name, description)
    {
        _function = function;
    }

    public void Invoke(T value)
    {
        _function.Invoke(value);
    }
}