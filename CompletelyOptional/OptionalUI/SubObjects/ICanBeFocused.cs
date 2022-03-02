namespace OptionalUI
{
    public interface ICanBeFocused
    {
        /// <summary>
        /// Whether this is focused
        /// </summary>
        bool Focused { get; set; }

        /// <summary>
        /// Whether its change is prevented
        /// </summary>
        bool GreyedOut { get; }

        bool CurrentlyFocusableMouse { get; }

        bool CurrentlyFocusableNonMouse { get; }
    }
}