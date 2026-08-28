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

        private BoardView _boardView;
        private GameEngine _engine;
        private System.Random _random;

        private void Awake()
        {
            _boardView = GetComponent<BoardView>();
        }

        private void Start()
        {
            _random = new System.Random();
            _engine = new GameEngine(Player.White);
            _engine.Changed += HandleEngineChanged;
            turnHud.RollClicked += HandleRollClicked;

            RenderAll();
        }

        private void OnDestroy()
        {
            if (_engine != null)
            {
                _engine.Changed -= HandleEngineChanged;
            }
            turnHud.RollClicked -= HandleRollClicked;
        }

        /// <summary>Whose turn it currently is.</summary>
        public Player CurrentPlayer => _engine.CurrentPlayer;

        /// <summary>Scratch/debug only — not meant to stick around past input troubleshooting.</summary>
        public GamePhase DebugPhase => _engine.Phase;

        /// <summary>True if the given player has a checker waiting on the bar.</summary>
        public bool HasCheckerOnBar(Player player) => _engine.Board.GetBar(player) > 0;

        /// <summary>True if the given player owns at least one checker on the given point.</summary>
        public bool OwnsCheckerAt(int pointIndex, Player player) => _engine.Board.GetPoint(pointIndex).Owner == player;

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
        }

        private void HandleEngineChanged()
        {
            RenderAll();
        }

        private void RenderAll()
        {
            _boardView.Render(_engine.Board);
            turnHud.Render(_engine);
        }
    }
}
