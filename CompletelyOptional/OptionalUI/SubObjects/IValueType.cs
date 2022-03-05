﻿using UnityEngine;

namespace OptionalUI.ValueTypes
{
    public static class ValueExt
    {
        /// <summary>
        /// Set <see cref="UIconfig.value"/> in <see cref="float"/>.
        /// <para>Note: If this is called by <see cref="IValueInt"/>, the value will get Floored. Just use <see cref="SetValueInt"/> instead.</para>
        /// </summary>
        /// <param name="UIconfig"><see cref="UIconfig"/> that accepts <see cref="float"/> for its <see cref="UIconfig.value"/>.</param>
        /// <param name="value">New <see cref="UIconfig.value"/> in <see cref="float"/>.</param>
        public static void SetValueFloat(this IValueFloat UIconfig, float value)
        {
            if (UIconfig is IValueInt) { UIconfig.valueString = Mathf.FloorToInt(value).ToString(); }
            else { UIconfig.valueString = value.ToString(); }
        }

        /// <summary>
        /// Get <see cref="UIconfig"/> in <see cref="float"/>.
        /// </summary>
        /// <param name="UIconfig"><see cref="UIconfig"/> that accepts <see cref="float"/> for its <see cref="UIconfig.value"/>.</param>
        /// <returns>Current <see cref="UIconfig.value"/> in <see cref="float"/>.</returns>
        public static float GetValueFloat(this IValueFloat UIconfig) => float.TryParse(UIconfig.valueString, out float d) ? d : 0f;

        /// <summary>
        /// Set <see cref="UIconfig.value"/> in <see cref="int"/>.
        /// </summary>
        /// <param name="UIconfig"><see cref="UIconfig"/> that accepts <see cref="int"/> or <see cref="float"/> for its <see cref="UIconfig.value"/>.</param>
        /// <param name="value">New <see cref="UIconfig.value"/> in <see cref="int"/>.</param>
        public static void SetValueInt(this IValueFloat UIconfig, int value) => UIconfig.valueString = value.ToString();

        /// <summary>
        /// Get <see cref="UIconfig"/> in <see cref="int"/>.
        /// </summary>
        /// <param name="UIconfig"><see cref="UIconfig"/> that accepts <see cref="int"/> for its <see cref="UIconfig.value"/>.</param>
        /// <returns>Current <see cref="UIconfig.value"/> in <see cref="int"/>.</returns>
        public static int GetValueInt(this IValueInt UIconfig) => int.TryParse(UIconfig.valueString, out int i) ? i : 0;

        /// <summary>
        /// Set <see cref="UIconfig.value"/> in <see cref="bool"/>.
        /// </summary>
        /// <param name="UIconfig"><see cref="UIconfig"/> that accepts <see cref="bool"/> for its <see cref="UIconfig.value"/>.</param>
        /// <param name="value">New <see cref="UIconfig.value"/> in <see cref="bool"/>.</param>
        public static void SetValueBool(this IValueBool UIconfig, bool value) => UIconfig.valueString = value ? "true" : "false";

        /// <summary>
        /// Get <see cref="UIconfig"/> in <see cref="bool"/>.
        /// </summary>
        /// <param name="UIconfig"><see cref="UIconfig"/> that accepts <see cref="bool"/> for its <see cref="UIconfig.value"/>.</param>
        /// <returns>Current <see cref="UIconfig.value"/> in <see cref="bool"/>.</returns>
        public static bool GetValueBool(this IValueBool UIconfig) => UIconfig.valueString == "true";
    }

    #region Flags

    /// <summary>
    /// Use this to flag your custom <see cref="UIconfig"/> that uses <see cref="float"/> for <see cref="UIconfig.value"/>, and hook <see cref="IValueType.valueString"/> to <see cref="UIconfig.value"/>.
    /// <para>Allows using <seealso cref="ValueExt.GetValueFloat"/>, <seealso cref="ValueExt.SetValueFloat"/>, and <seealso cref="ValueExt.SetValueInt"/>.</para>
    /// </summary>
    public interface IValueFloat : IValueType
    { }

    /// <summary>
    /// Use this to flag your custom <see cref="UIconfig"/> that uses <see cref="int"/> for <see cref="UIconfig.value"/>, and hook <see cref="IValueType.valueString"/> to <see cref="UIconfig.value"/>.
    /// <para>Allows using <seealso cref="ValueExt.GetValueInt"/>, <seealso cref="ValueExt.SetValueInt"/>, and <seealso cref="ValueExt.GetValueFloat"/>.</para>
    /// </summary>
    public interface IValueInt : IValueFloat
    { }

    /// <summary>
    /// Use this to flag your custom <see cref="UIconfig"/> that uses <see cref="float"/> for <see cref="UIconfig.value"/>, and hook <see cref="IValueType.valueString"/> to <see cref="UIconfig.value"/>.
    /// <para>Allows using <seealso cref="ValueExt.GetValueFloat"/> and <seealso cref="ValueExt.SetValueFloat"/>.</para>
    /// </summary>
    public interface IValueBool : IValueType
    { }

    /// <summary>
    /// Abstract interface for IValueType interfaces
    /// </summary>
    public interface IValueType
    {
        /// <summary>
        /// Hook this to <see cref="UIconfig.value"/>.
        /// <code>string IValueType.valueString { get => this.value; set => this.value = value; }</code>
        /// </summary>
        string valueString { get; set; }
    }

    #endregion Flags
}