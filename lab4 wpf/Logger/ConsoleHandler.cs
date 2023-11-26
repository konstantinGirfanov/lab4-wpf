namespace lab4_wpf;
using System;
public class ConsoleHandler : IMessageHandler
{
    public void Log(string message)
    {
        Console.WriteLine(message);
    }
}