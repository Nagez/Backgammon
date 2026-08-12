namespace Backgammon.Core
{
    /// <summary>
    /// Per-player facts about how movement works on the 0-23 index track: which way a player
    /// moves, and the sentinel index their bar acts as when re-entering (one step before their
    /// first real point, so entry math is just "bar sentinel + direction * die").
    /// </summary>
    public static class PlayerRules
    {
        /// <summary>+1 for White (moves 0 toward 23), -1 for Black (moves 23 toward 0).</summary>
        public static int Direction(Player player) => player == Player.White ? 1 : -1;

        /// <summary>The off-board index a bar re-entry move is considered to originate from.</summary>
        public static int BarSentinel(Player player) => player == Player.White ? -1 : BoardState.PointCount;

        /// <summary>The off-board index a bear-off move is considered to land on.</summary>
        public static int BearOffTarget(Player player) => player == Player.White ? BoardState.PointCount : -1;

        /// <summary>The lowest point index inside the player's home board (where they bear off from).</summary>
        public static int HomeBoardStart(Player player) => player == Player.White ? BoardState.PointCount - 6 : 0;

        /// <summary>The highest point index inside the player's home board.</summary>
        public static int HomeBoardEnd(Player player) => player == Player.White ? BoardState.PointCount - 1 : 5;

        /// <summary>How many pips a checker at the given point still needs to travel to bear off (1-6 within the home board).</summary>
        public static int BearOffDistance(Player player, int index) =>
            player == Player.White ? BoardState.PointCount - index : index + 1;
    }
}
