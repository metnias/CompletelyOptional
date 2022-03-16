﻿using BepInEx.Configuration;
using CompletelyOptional;
using RWCustom;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace OptionalUI
{
    /// <summary>
    /// Special type of <see cref="OpComboBox"/> that uses Rain World Resources instead of custom list.
    /// </summary>
    public class OpResourceSelector : OpComboBox
    {
        /// <summary>
        /// Special type of <see cref="OpComboBox"/> that uses Rain World Enum instead of custom list.
        /// </summary>
        /// <param name="pos">LeftBottom Position of folded <see cref="OpComboBox"/></param>
        /// <param name="width">The box width of folded <see cref="OpComboBox"/>. The height is fixed to 24f.</param>
        /// <param name="enumType">Type of Enum that you want to get items</param>
        /// <param name="cosmeticName"></param>
        /// <exception cref="ElementFormatException">Thrown when enumType is not <see cref="Enum"/>.</exception>
        public OpResourceSelector(ConfigEntry<string> config, Vector2 pos, float width, Type enumType, string cosmeticName = "") : base(config, pos, width, list: null)
        {
            if (!enumType.IsEnum) { throw new ElementFormatException(this, "OpResourceSelector's enumType is not Enum!", key); }
            this.listType = SpecialEnum.Enum;
            string[] nameList = Enum.GetNames(enumType);
            List<ListItem> list = new List<ListItem>();
            for (int i = 0; i < nameList.Length; i++)
            {
                var enumVal = (Enum)Enum.Parse(enumType, nameList[i]);
                ListItem item = new ListItem(nameList[i], (int)(object)enumVal);
                item.displayName = EnumHelper.GetEnumDesc(enumVal) ?? item.name;
                list.Add(item);
            }
            list.Sort(ListItem.Comparer);
            this.itemList = list.ToArray();
            this.ResetIndex();
            this.Initialize(!cosmetic ? defaultValue : cosmeticName);
        }

        /// <summary>
        /// Special type of <see cref="OpComboBox"/> that uses Rain World Resources instead of custom list. See also <seealso cref="SpecialEnum"/>.
        /// </summary>
        /// <param name="pos">LeftBottom Position of folded <see cref="OpComboBox"/></param>
        /// <param name="width">The box width of folded <see cref="OpComboBox"/>. The height is fixed to 24f.</param>
        /// <param name="listType">Type of List that you want to get items</param>
        /// <param name="cosmeticName"></param>
        /// <exception cref="ElementFormatException">Thrown when you used <see cref="SpecialEnum.Enum"/></exception>
        public OpResourceSelector(ConfigEntry<string> config, Vector2 pos, float width, SpecialEnum listType, string cosmeticName = "") : base(config, pos, width, list: null)
        {
            List<ListItem> list = new List<ListItem>();
            switch (listType)
            {
                case SpecialEnum.Enum: throw new ElementFormatException(this, "Do NOT use SpecialEnum.Enum. That's for another ctor.", this.key);
                case SpecialEnum.Shaders:
                    foreach (string k in ComOptPlugin.rw.Shaders.Keys.ToArray()) { list.Add(new ListItem(k)); }
                    break;

                case SpecialEnum.Decals:
                    DirectoryInfo di = new DirectoryInfo(string.Concat(Custom.RootFolderDirectory(), "Assets", Path.DirectorySeparatorChar, "Futile",
                        Path.DirectorySeparatorChar, "Resources", Path.DirectorySeparatorChar, "Decals", Path.DirectorySeparatorChar));
                    FileInfo[] fi0 = new DirectoryInfo(string.Concat(Custom.RootFolderDirectory(), "Assets", Path.DirectorySeparatorChar, "Futile",
                        Path.DirectorySeparatorChar, "Resources", Path.DirectorySeparatorChar, "Decals", Path.DirectorySeparatorChar)).GetFiles();
                    foreach (FileInfo f in fi0)
                    { if (f.Name.ToLower().EndsWith(".png")) { list.Add(new ListItem(f.Name.Remove(f.Name.Length - 4))); } }
                    break;

                case SpecialEnum.Illustrations:
                    FileInfo[] fi1 = new DirectoryInfo(string.Concat(Custom.RootFolderDirectory(), "Assets", Path.DirectorySeparatorChar, "Futile",
                        Path.DirectorySeparatorChar, "Resources", Path.DirectorySeparatorChar, "Illustrations", Path.DirectorySeparatorChar)).GetFiles();
                    foreach (FileInfo f in fi1)
                    { if (f.Name.ToLower().EndsWith(".png")) { list.Add(new ListItem(f.Name.Remove(f.Name.Length - 4))); } }
                    break;

                case SpecialEnum.Palettes:
                    FileInfo[] fi2 = new DirectoryInfo(string.Concat(Custom.RootFolderDirectory(), "Assets", Path.DirectorySeparatorChar, "Futile",
                        Path.DirectorySeparatorChar, "Resources", Path.DirectorySeparatorChar, "Palettes", Path.DirectorySeparatorChar)).GetFiles();
                    foreach (FileInfo f in fi2)
                    {
                        if (f.Name.ToLower().EndsWith(".png") && f.Name.ToLower().StartsWith("palette"))
                        {
                            string name = f.Name.Remove(f.Name.Length - 4);
                            int t = int.TryParse(name.Remove(0, 7), out t) ? t : int.MaxValue;
                            list.Add(new ListItem(name, t));
                        }
                    }
                    break;

                case SpecialEnum.Songs:
                    FileInfo[] fi3 = new DirectoryInfo(string.Concat(Custom.RootFolderDirectory(), "Assets", Path.DirectorySeparatorChar, "Futile",
                        Path.DirectorySeparatorChar, "Resources", Path.DirectorySeparatorChar, "Music", Path.DirectorySeparatorChar, "Songs", Path.DirectorySeparatorChar)).GetFiles();
                    foreach (FileInfo f in fi3)
                    { if (f.Name.ToLower().EndsWith(".mp3")) { list.Add(new ListItem(f.Name.Remove(f.Name.Length - 4))); } }
                    break;

                case SpecialEnum.Regions:
                    string[] array = File.ReadAllLines(Custom.RootFolderDirectory() + "/World/Regions/regions.txt");
                    for (int i = 0; i < array.Length; i++)
                    { if (array[i].Length > 0) { list.Add(new ListItem(array[i], i)); } }
                    break;
            }
            list.Sort(ListItem.Comparer);
            this.itemList = list.ToArray();
            this.ResetIndex();
            //ComOptPlugin.LogInfo(listType);
            //for (int i = 0; i < itemList.Length; i++) { ComOptPlugin.LogInfo(string.Concat(i, ": ", itemList[i].name)); }
            this.Initialize(!cosmetic ? defaultValue : cosmeticName);
        }

        public readonly SpecialEnum listType;

        /// <summary>
        /// The List of Rain World Resource that doesn't have enumType. See also <seealso cref="OpResourceSelector(Vector2, float, string, SpecialEnum, string)"/>
        /// </summary>
        public enum SpecialEnum : byte
        {
            /// <summary>
            /// Do NOT use this. This is for <see cref="OpResourceSelector(Vector2, float, string, Type, string)"/>.
            /// </summary>
            Enum = 0,

            /// <summary>
            /// World/Regions/regions.txt
            /// </summary>
            Regions,

            /// <summary>
            /// Resources/Decals
            /// </summary>
            Decals,

            /// <summary>
            /// Resources/Illustrations
            /// </summary>
            Illustrations,

            /// <summary>
            /// Resources/Music/Songs.
            /// </summary>
            Songs,

            /// <summary>
            /// Resources/Palettes
            /// </summary>
            Palettes,

            /// <summary>
            /// <see cref="RainWorld.Shaders"/>
            /// </summary>
            Shaders
        }
    }
}