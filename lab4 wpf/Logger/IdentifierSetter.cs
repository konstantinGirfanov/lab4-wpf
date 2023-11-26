namespace lab4_wpf;

public static class IdentifierSetter
{
    private static int _currentIdentifier = 0;

    public static int GetId() => _currentIdentifier++;
}