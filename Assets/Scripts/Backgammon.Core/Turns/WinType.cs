namespace Backgammon.Core
{
    /// <summary>How decisively a finished game was won.</summary>
    public enum WinType
    {
        /// <summary>The loser had already borne off at least one checker.</summary>
        Single,

        /// <summary>The loser bore off nothing, but has no checker on the bar or in the winner's home board.</summary>
        Gammon,

        /// <summary>The loser bore off nothing and still has a checker on the bar or in the winner's home board.</summary>
        Backgammon
    }
}
