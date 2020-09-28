using System;
using System.IO;
using UnityEngine;

namespace OptionalUI
{
    /// <summary>
    /// Allow users to choose relative path.
    /// </summary>
    public class OpPathSelector : UIconfig
    {
        /// <summary>
        /// Allow users to choose relative path.
        /// </summary>
        /// <param name="pos">LeftBottom position. This is a circular button with radius of 30 pxl.</param>
        /// <param name="key">Unique <see cref="UIconfig.key"/></param>
        /// <param name="defaultPath"></param>
        /// <param name="extension">Requested extension ('dir' for directory, default is *)</param>
        /// <exception cref="ElementFormatException">Thrown when defaultPath is Invalid</exception>
        public OpPathSelector(Vector2 pos, string key, string defaultPath = "", string extension = "*") : base(pos, 30f, key, defaultPath)
        {
            this.extension = extension;
            if (!string.IsNullOrEmpty(defaultPath))
            {
                try { Path.GetFullPath(this.value); }
                catch (Exception e) { throw new ElementFormatException($"OpPathSelector (key: {key} has Invalid defaultPath: {defaultPath}", e); }
            }
            throw new NotImplementedException("OpPathSelector might come to you, in future! If you're seeing this error as an user, download the latest ConfigMachine.");
        }

        /// <summary>
        /// Requested extension ('dir' for directory)
        /// </summary>
        public readonly string extension;

        /// <summary>
        /// Whether this selector is for Directory or File
        /// </summary>
        public bool isDir => extension.ToLower() == "dir";

        /// <summary>
        /// FileInfo casting for the value
        /// </summary>
        /// <exception cref="InvalidGetPropertyException">Thrown when the extention is 'dir'</exception>
        public FileInfo valueFile
        {
            get
            {
                if (isDir) { throw new InvalidGetPropertyException(this, "valueFile"); }
                return new FileInfo(Path.GetFullPath(this.value));
            }
        }

        /// <summary>
        /// DirectoryInfo casting for the value
        /// </summary>
        /// <exception cref="InvalidGetPropertyException">Thrown when the extention is not 'dir'</exception>
        public DirectoryInfo valueDir
        {
            get
            {
                if (!isDir) { throw new InvalidGetPropertyException(this, "valueFile"); }
                return new DirectoryInfo(Path.GetFullPath(this.value));
            }
        }
    }
}