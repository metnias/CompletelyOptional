namespace OptionalUI
{
    /// <summary>
    /// This is for the controller support that will never come.
    /// </summary>
    public interface SelectableUIelement
    {
        bool IsMouseOverMe { get; }

        bool CurrentlySelectableMouse { get; }

        bool CurrentlySelectableNonMouse { get; }
    }
}