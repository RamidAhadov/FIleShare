namespace FileShare.App.Extensions;

public static class ButtonValueExtension
{
    private static readonly Dictionary<Button, string> buttonValues = new();

    public static void SetValue(this Button button, string value)
    {
        buttonValues[button] = value;
    }

    public static string? GetValue(this Button button)
    {
        return buttonValues.TryGetValue(button, out var value) ? value : null;
    }
}