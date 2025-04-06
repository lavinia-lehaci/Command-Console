using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// A base class that represents a command with a name and description.
/// Derived classes can extend this class to invoke specific events.
/// </summary>
public class CommandBase
{
    private string _commandName;
    private string _commandDescription;

    public string CommandName { get { return _commandName; }}
    public string CommandDescription { get { return _commandDescription; }}

    public CommandBase(string name, string description)
    {
        _commandName = name;
        _commandDescription = description;
    }
}

/// <summary>
/// A command that triggers a UnityEvent without any parameters.
/// </summary>
public class Command : CommandBase
{
    private UnityEvent _function;

    public Command(string name, string description, UnityEvent function) : base (name, description)
    {
        _function = function;
    }

    public void Invoke()
    {
        _function.Invoke();
    }
}

/// <summary>
/// A command that triggers a UnityEvent with a parameter of type T.
/// </summary>
/// <typeparam name="T">The type of the parameter the command will handle when invoked.</typeparam>
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