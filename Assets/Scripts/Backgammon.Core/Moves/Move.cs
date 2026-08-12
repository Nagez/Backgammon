namespace Backgammon.Core
{
    /// <summary>
    /// A single checker move using one die value: either from a point to another point,
    /// or (when <see cref="From"/> is null) entering from the bar onto <see cref="To"/>.
    /// </summary>
    public readonly struct Move
    {
        /// <summary>The player making the move.</summary>
        public Player Player { get; }

        /// <summary>The origin point index, or null if this move enters a checker from the bar.</summary>
        public int? From { get; }

        /// <summary>The destination point index.</summary>
        public int To { get; }

        /// <summary>The die value being spent on this move.</summary>
        public int Die { get; }

        public Move(Player player, int? from, int to, int die)
        {
            Player = player;
            From = from;
            To = to;
            Die = die;
        }
    }
}
