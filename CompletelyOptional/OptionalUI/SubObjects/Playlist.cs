namespace OptionalUI
{
    public static class Playlist
    {
        public static readonly string[] playlistMoody =
        {
            "NA_07 - Phasing", //16%
            "NA_11 - Reminiscence",
            "NA_18 - Glass Arcs",
            "NA_19 - Halcyon Memories",
            "NA_21 - New Terra",
        };

        public static readonly string[] playlistWork =
        {
            "NA_20 - Crystalline", //7%
            "NA_29 - Flutter",
            "NA_30 - Distance"
        };

        /// <summary>
        /// List of Random Song gets played in ConfigMenu,
        /// carefully chosen by me :P
        /// </summary>
        internal static string randomSong
        {
            get
            {
                if (UnityEngine.Random.value < 0.8f)
                { return playlistMoody[UnityEngine.Random.Range(0, playlistMoody.Length - 1)]; }
                else
                { return playlistWork[UnityEngine.Random.Range(0, playlistWork.Length - 1)]; }
            }
        }
    }
}