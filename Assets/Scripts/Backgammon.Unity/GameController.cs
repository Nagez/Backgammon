using System.Collections.Generic;
using Backgammon.Core;
using UnityEngine;

namespace Backgammon.Unity
{
    /// <summary>
    /// The only class that mutates game state: owns the GameEngine and pushes its state to
    /// the presentation layer whenever it changes. Nothing else should touch GameEngine directly.
    /// </summary>
    [RequireComponent(typeof(BoardView))]
    public class GameController : MonoBehaviour
    {
        [SerializeField] private TurnHudView turnHud;
        [SerializeField] private DiceView diceView;

        private BoardView _boardView;
        private GameEngine _engine;
        private System.Random _random;
        private string _pendingNotice;

        private void Awake()
        {
            _boardView = GetComponent<BoardView>();
        }

        private void Start()
        {
            _random = new System.Random();
            _engine = new GameEngine(Player.White);
            _engine.Changed += HandleEngineChanged;
            _engine.NoLegalMoves += HandleNoLegalMoves;
            turnHud.RollClicked += HandleRollClicked;

            RenderAll();
        }

        private void OnDestroy()
        {
            if (_engine != null)
            {
                _engine.Changed -= HandleEngineChanged;
                _engine.NoLegalMoves -= HandleNoLegalMoves;
            }
            turnHud.RollClicked -= HandleRollClicked;
        }

        /// <summary>Whose turn it currently is.</summary>
        public Player CurrentPlayer => _engine.CurrentPlayer;

        /// <summary>True if the given player has a checker waiting on the bar.</summary>
        public bool HasCheckerOnBar(Player player) => _engine.Board.GetBar(player) > 0;

        /// <summary>True if the given player owns at least one checker on the given point.</summary>
        public bool OwnsCheckerAt(int pointIndex, Player player) => _engine.Board.GetPoint(pointIndex).Owner == player;

        /// <summary>The moves legal to play right now; empty outside the current player's move phase.</summary>
        public IReadOnlyList<Move> GetLegalMoves() => _engine.GetLegalMoves();

        /// <summary>
        /// Attempts to play a move from the given origin (null = bar) to the given destination.
        /// Returns false without changing anything if no legal move matches.
        /// </summary>
        public bool TryPlayMove(int? from, int to)
        {
            foreach (Move candidate in _engine.GetLegalMoves())
            {
                if (candidate.From == from && candidate.To == to)
                {
                    _engine.ApplyMove(candidate);
                    return true;
                }
            }

            return false;
        }

        private void HandleRollClicked()
        {
            _engine.RollDice(_random);

            if (_engine.CurrentDice.HasValue)
            {
                diceView.PlayRoll(_engine.CurrentDice.Value.Die1, _engine.CurrentDice.Value.Die2);
            }
        }

        private void HandleNoLegalMoves(Player player, Dice dice, IReadOnlyList<int> stuckDice)
        {
            _pendingNotice = $"{player} rolled {dice.Die1}-{dice.Die2} but had no legal move — turn passed.";
        }

        private void HandleEngineChanged()
        {
            RenderAll();
        }

        private void RenderAll()
        {
            _boardView.Render(_engine.Board);
            turnHud.Render(_engine, _pendingNotice);
            _pendingNotice = null;
        }
    }
}
