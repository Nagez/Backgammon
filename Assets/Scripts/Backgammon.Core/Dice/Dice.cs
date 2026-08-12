using System;

namespace Backgammon.Core
{
    /// <summary>The two values from a single dice roll.</summary>
    public readonly struct Dice
    {
        /// <summary>The first rolled value, 1-6.</summary>
        public int Die1 { get; }

        /// <summary>The second rolled value, 1-6.</summary>
        public int Die2 { get; }

        /// <summary>True if both dice show the same value (a double).</summary>
        public bool IsDouble => Die1 == Die2;

        public Dice(int die1, int die2)
        {
            Die1 = die1;
            Die2 = die2;
        }

        /// <summary>Rolls a new pair of dice using the given random source.</summary>
        public static Dice Roll(Random random)
        {
            return new Dice(random.Next(1, 7), random.Next(1, 7));
        }
    }
}
