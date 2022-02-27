namespace OptionalUI
{
    /// <summary>
    /// struct that's used for handling items in <see cref="OpComboBox"/>
    /// </summary>
    public struct ListItem
    {
        /// <summary>
        /// struct that's used for handling items in <see cref="OpComboBox"/> and <see cref="OpListBox"/>
        /// </summary>
        /// <param name="name">The code name of this item, which will be used for its <see cref="UIconfig.value"/></param>
        /// <param name="value">Sorting priority of this item. The higher the lower it goes.</param>
        public ListItem(string name, int value = int.MaxValue)
        {
            this.name = name;
            this.value = value;
            this.index = -1;
            this.desc = "";
            this.displayName = name;
        }

        /// <summary>
        /// struct that's used for handling items in <see cref="OpComboBox"/> and <see cref="OpListBox"/>
        /// </summary>
        /// <param name="name">The code name of this item, which will be refered in its <see cref="UIconfig.value"/></param>
        /// <param name="value">Sorting priority of this item. The higher the lower it goes.</param>
        /// <param name="displayName">The display name of this item, which will be displayed in its <see cref="UIconfig"/></param>
        public ListItem(string name, string displayName, int value = int.MaxValue)
        {
            this.name = name;
            this.value = value;
            this.index = -1;
            this.desc = "";
            this.displayName = displayName;
        }

        /// <summary>
        /// Unique name of item.
        /// </summary>
        public readonly string name;

        /// <summary>
        /// value used for comparison and sorting.
        /// </summary>
        public readonly int value;

        /// <summary>
        /// index number in <see cref="OpComboBox"/>. This will be set automatically, and used for search function.
        /// </summary>
        internal int index;

        /// <summary>
        /// The description to be shown when the mouse is hovered. You can change this.
        /// </summary>
        public string desc;

        /// <summary>
        /// Display name shown to the actual user. Useful for supporting translation.
        /// </summary>
        public string displayName;

        public string EffectiveDisplayName => string.IsNullOrEmpty(displayName) ? name : displayName;

        public override bool Equals(object obj) => obj is ListItem i && this.name == i.name && this.value == i.value;

        public override int GetHashCode() => this.name.GetHashCode();

        public static bool operator ==(ListItem left, ListItem right) => left.Equals(right);

        public static bool operator !=(ListItem left, ListItem right) => !(left == right);

        /// <summary>
        /// Used for <c>List.Sort(IComparer{T})</c>
        /// </summary>
        public static int Comparer(ListItem x, ListItem y)
        {
            if (x.value == y.value) { return GetRealName(x.name).CompareTo(GetRealName(y.name)); }
            return x.value.CompareTo(y.value);
        }

        /// <summary>
        /// Remove initial articles before sorting
        /// </summary>
        public static string GetRealName(string text)
        {
            text = text.ToLower();
            if (text.StartsWith("a ")) { return text.Remove(2); }
            else if (text.StartsWith("an ")) { return text.Remove(3); }
            else if (text.StartsWith("the ")) { return text.Remove(4); }
            return text;
        }

        public static bool SearchMatch(string query, string text)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(query)) { return false; }
            query = query.Trim().ToLower();
            text = text.ToLower();
            if (query.Contains(" ")) // AND search
            {
                string[] qarray = query.Split(' ');
                foreach (string q in qarray)
                {
                    if (string.IsNullOrEmpty(q)) { continue; }
                    if (!text.Contains(q)) { return false; }
                }
                return true;
            }
            else
            {
                if (string.IsNullOrEmpty(query)) { return true; }
                if (text.Contains(query)) { return true; } // Simple Contain
                if (!text.Contains(query.Substring(0, 1))) { return false; }
                if (query.Length < 2) // One letter search
                {
                    // if (text.StartsWith(query.Substring(0, 1))) { return true; }
                    // if (text.Contains(" " + query.Substring(0, 1))) { return true; }
                    return false;
                }
                string test = text.Substring(text.IndexOf(query[0]));
                for (int i = 1; i < query.Length; i++)
                {
                    if (test.Contains(query[i].ToString()))
                    { test = test.Substring(test.IndexOf(query[i])); }
                    else { return false; }
                }
                return true;
            }
        }
    }
}