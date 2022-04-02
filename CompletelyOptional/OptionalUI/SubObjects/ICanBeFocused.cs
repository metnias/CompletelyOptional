using CompletelyOptional;
using UnityEngine;

namespace OptionalUI
{
    /// <summary>
    /// Add this to any <see cref="UIelement"/> that can be interacted with end-users.
    /// <para><see cref="UIconfig"/> and <see cref="UItrigger"/> are <see cref="ICanBeFocused"/> by default, but you may override its properties in your custom UIelement.</para>
    /// </summary>
    public interface ICanBeFocused
    {
        /// <summary>
        /// Whether its change is prevented
        /// </summary>
        bool GreyedOut { get; }

        /// <summary>
        /// How the Focus Pointer should surround this. Left, Bottom(NOT Top), Width, Height.
        /// <code>Rect ICanBeFocused.FocusRect => new Rect(pos.x, pos.y, size.x, size.y);</code>
        /// </summary>
        Rect FocusRect { get; }

        /// <summary>
        /// Whether this can be focused with Mouse pointer
        /// </summary>
        bool CurrentlyFocusableMouse { get; }

        /// <summary>
        /// Whether this can be focused with Gamepad/Keyboard
        /// </summary>
        bool CurrentlyFocusableNonMouse { get; }

        /// <summary>
        /// Called by ConfigContainer when this is <see cref="ICanBeFocusedExt.Focused"/> and the user pressed the Jump button.
        /// <para>This is called after all of <see cref="UIelement.Update"/>s happened.</para>
        /// </summary>
        void NonMouseHold();
    }

    public static class ICanBeFocusedExt
    {
        /// <summary>
        /// Whether this element is focused or not.
        /// </summary>
        public static bool Focused(this ICanBeFocused focusable) => ConfigContainer.focusedElement == focusable;
    }
}