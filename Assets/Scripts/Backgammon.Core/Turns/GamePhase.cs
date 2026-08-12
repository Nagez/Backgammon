namespace Backgammon.Core
{
    /// <summary>Where a game currently is in its turn cycle.</summary>
    public enum GamePhase
    {
        /// <summary>The current player hasn't rolled yet this turn.</summary>
        WaitingForRoll,

        /// <summary>Dice have been rolled and at least one die is still unplayed.</summary>
        MovesRemaining,

        /// <summary>A player has borne off all their checkers; the game is over.</summary>
        GameOver
    }
}
