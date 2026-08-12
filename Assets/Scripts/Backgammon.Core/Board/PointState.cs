namespace Backgammon.Core
{
    /// <summary>The occupancy of a single one of the board's 24 points: who owns it (if anyone) and how many checkers are stacked there.</summary>
    public readonly struct PointState
    {
        /// <summary>An unowned point with zero checkers.</summary>
        public static readonly PointState Empty = new PointState(null, 0);

        /// <summary>The player whose checkers occupy this point, or null if it's empty.</summary>
        public Player? Owner { get; }

        /// <summary>How many checkers are stacked on this point.</summary>
        public int Count { get; }

        public PointState(Player? owner, int count)
        {
            Owner = owner;
            Count = count;
        }
    }
}
