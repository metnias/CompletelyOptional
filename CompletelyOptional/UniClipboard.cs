using UnityEngine;
using System.Reflection;
using System;

namespace CompletelyOptional
{
    /// <summary>
    /// https://github.com/sanukin39/UniClipboard/blob/master/Assets/UniClipboard/UniClipboard.cs
    /// </summary>
    public static class UniClipboard
    {
        private static IBoard _board;
        private static IBoard board
        {
            get
            {
                if (_board == null) { _board = new StandardBoard(); }
                return _board;
            }
        }

        public static void SetText(string str)
        {
            board.SetText(str);
        }

        public static string GetText()
        {
            return board.GetText();
        }
    }

    internal interface IBoard
    {
        void SetText(string str);

        string GetText();
    }

    internal class StandardBoard : IBoard
    {
        private static PropertyInfo m_systemCopyBufferProperty = null;

        private static PropertyInfo GetSystemCopyBufferProperty()
        {
            if (m_systemCopyBufferProperty == null)
            {
                Type T = typeof(GUIUtility);
                m_systemCopyBufferProperty = T.GetProperty("systemCopyBuffer", BindingFlags.Static | BindingFlags.Public);
                if (m_systemCopyBufferProperty == null)
                {
                    m_systemCopyBufferProperty =
                        T.GetProperty("systemCopyBuffer", BindingFlags.Static | BindingFlags.NonPublic);
                }

                if (m_systemCopyBufferProperty == null)
                {
                    throw new Exception(
                        "Can't access internal member 'GUIUtility.systemCopyBuffer' it may have been removed / renamed");
                }
            }
            return m_systemCopyBufferProperty;
        }

        public void SetText(string str)
        {
            PropertyInfo P = GetSystemCopyBufferProperty();
            P.SetValue(null, str, null);
        }

        public string GetText()
        {
            PropertyInfo P = GetSystemCopyBufferProperty();
            return (string)P.GetValue(null, null);
        }
    }
}
