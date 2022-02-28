namespace OptionalUI
{
    public interface FocusableUIelement
    {
        bool IsMouseOverMe { get; }

        bool CurrentlyFocusableMouse { get; }

        bool CurrentlyFocusableNonMouse { get; }
    }
}