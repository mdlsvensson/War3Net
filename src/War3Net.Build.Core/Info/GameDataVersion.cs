namespace War3Net.Build.Info
{
    public enum GameDataVersion
    {
        /// <summary>
        /// Reign of Chaos.
        /// </summary>
        RoC = 0,

        /// <summary>
        /// The Frozen Throne.
        /// </summary>
        TFT = 1,

        Unset = 2,

        /// <summary>
        /// Forsaken Kingdom, the game data version World Editor 3.0 offers beside Reign of Chaos and The Frozen Throne. It has the value of <see cref="Unset"/>;
        /// <see cref="Unset"/> stays the name that is written and returned by <see cref="object.ToString"/>.
        /// </summary>
        ForsakenKingdom = 2,
    }
}