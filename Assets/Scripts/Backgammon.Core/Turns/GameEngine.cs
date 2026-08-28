using System;
using System.Collections.Generic;
using System.Linq;

namespace Backgammon.Core
{
    /// <summary>
    /// Owns a game's authoritative state and drives its turn cycle: roll, play legal moves until
    /// the dice run out or none remain, hand the turn to the other player, detect the win. This is
    /// the only class a caller should mutate game state through — everything else in this assembly
    /// is read via its board/state, never written to directly.
    /// </summary>
    public sealed class GameEngine
    {
        private List<int> _remainingDice = new List<int>();

        /// <summary>The current board snapshot.</summary>
        public BoardState Board { get; private set; }

        /// <summary>Whose turn it currently is.</summary>
        public Player CurrentPlayer { get; private set; }

        /// <summary>Where the game is in its turn cycle.</summary>
        public GamePhase Phase { get; private set; }

        /// <summary>The dice rolled for the current turn, if any have been rolled yet.</summary>
        public Dice? CurrentDice { get; private set; }

        /// <summary>The winner, once <see cref="Phase"/> is <see cref="GamePhase.GameOver"/>.</summary>
        public Player? Winner { get; private set; }

        /// <summary>How the game was won, once <see cref="Phase"/> is <see cref="GamePhase.GameOver"/>.</summary>
        public WinType? WinResult { get; private set; }

        /// <summary>Raised after every roll, move, or turn change, so a presentation layer can refresh from <see cref="Board"/>.</summary>
        public event Action Changed;

        /// <summary>
        /// Raised when a turn auto-ends because no legal move exists for it — either right after
        /// rolling, or mid-turn with a die left that can't be played. Fires before the turn
        /// actually changes, so a presentation layer can tell the player why nothing happened.
        /// </summary>
        public event Action<Player, Dice, IReadOnlyList<int>> NoLegalMoves;

        public GameEngine(Player startingPlayer)
        {
            Board = BoardState.CreateStartingPosition();
            CurrentPlayer = startingPlayer;
            Phase = GamePhase.WaitingForRoll;
        }

        /// <summary>The die values the current player still has left to play this turn.</summary>
        public IReadOnlyList<int> RemainingDice => _remainingDice;

        /// <summary>The moves legal to play right now; empty outside <see cref="GamePhase.MovesRemaining"/>.</summary>
        public IReadOnlyList<Move> GetLegalMoves()
        {
            if (Phase != GamePhase.MovesRemaining)
            {
                return new List<Move>();
            }

            return MoveGenerator.GetLegalMoves(Board, CurrentPlayer, _remainingDice);
        }

        /// <summary>Rolls the dice for the current player's turn. If no legal move exists, the turn passes immediately.</summary>
        public void RollDice(Random random)
        {
            if (Phase != GamePhase.WaitingForRoll)
            {
                throw new InvalidOperationException("Dice can only be rolled at the start of a turn.");
            }

            CurrentDice = Dice.Roll(random);
            _remainingDice = MoveGenerator.ExpandDice(CurrentDice.Value).ToList();
            Phase = GamePhase.MovesRemaining;

            EndTurnIfStuck();

            Changed?.Invoke();
        }

        /// <summary>Plays a move that must be one of <see cref="GetLegalMoves"/>; ends the turn or the game as appropriate.</summary>
        public void ApplyMove(Move move)
        {
            if (Phase != GamePhase.MovesRemaining)
            {
                throw new InvalidOperationException("No move can be played outside a player's turn.");
            }

            if (!GetLegalMoves().Any(legal => IsSameMove(legal, move)))
            {
                throw new InvalidOperationException("That move is not legal.");
            }

            Board = MoveValidator.Apply(Board, move);
            _remainingDice.Remove(move.Die);

            if (Board.GetBorneOff(CurrentPlayer) == BoardState.CheckersPerPlayer)
            {
                EndGame();
                Changed?.Invoke();
                return;
            }

            EndTurnIfStuck();

            Changed?.Invoke();
        }

        // Ends the turn outright once its dice are used up, or auto-passes (with notice) if dice
        // remain but none of them have a legal move — e.g. rolling into a fully blocked position,
        // or playing one die of a pair and finding the other unplayable.
        private void EndTurnIfStuck()
        {
            if (_remainingDice.Count == 0)
            {
                EndTurn();
                return;
            }

            if (GetLegalMoves().Count == 0)
            {
                NoLegalMoves?.Invoke(CurrentPlayer, CurrentDice.Value, new List<int>(_remainingDice));
                EndTurn();
            }
        }

        private void EndTurn()
        {
            CurrentPlayer = CurrentPlayer == Player.White ? Player.Black : Player.White;
            CurrentDice = null;
            _remainingDice = new List<int>();
            Phase = GamePhase.WaitingForRoll;
        }

        private void EndGame()
        {
            Player loser = CurrentPlayer == Player.White ? Player.Black : Player.White;

            Winner = CurrentPlayer;
            WinResult = DetermineWinType(loser);
            Phase = GamePhase.GameOver;
        }

        private WinType DetermineWinType(Player loser)
        {
            if (Board.GetBorneOff(loser) > 0)
            {
                return WinType.Single;
            }

            return LoserHasCheckerInWinnersHomeOrBar(loser) ? WinType.Backgammon : WinType.Gammon;
        }

        private bool LoserHasCheckerInWinnersHomeOrBar(Player loser)
        {
            if (Board.GetBar(loser) > 0)
            {
                return true;
            }

            int homeStart = PlayerRules.HomeBoardStart(CurrentPlayer);
            int homeEnd = PlayerRules.HomeBoardEnd(CurrentPlayer);
            for (int i = homeStart; i <= homeEnd; i++)
            {
                if (Board.GetPoint(i).Owner == loser)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsSameMove(Move a, Move b)
        {
            return a.Player == b.Player && a.From == b.From && a.To == b.To && a.Die == b.Die;
        }
    }
}
