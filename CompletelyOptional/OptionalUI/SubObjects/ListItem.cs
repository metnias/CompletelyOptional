namespace OptionalUI
{
    /// <summary>
    /// struct that's used for handling items in <see cref="OpComboBox"/>
    /// </summary>
    public struct ListItem
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public ListItem(string name, int value = int.MaxValue)
        {
            this.name = name;
            this.value = value;
            this.index = -1;
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
        public int index;

        public override bool Equals(object obj) => obj is ListItem i && this.name == i.name && this.value == i.value;

        public override int GetHashCode() => this.name.GetHashCode();

        public static bool operator ==(ListItem left, ListItem right) => left.Equals(right);

        public static bool operator !=(ListItem left, ListItem right) => !(left == right);

        /// <summary>
        /// Used for <c>List.Sort(IComparer{T})</c>
        /// </summary>
        public static int Comparer(ListItem x, ListItem y)
        {
            if (x.value == y.value) { return x.name.CompareTo(y.name); }
            return x.value.CompareTo(y.value);
        }
    }
}