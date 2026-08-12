using Backgammon.Core;
using UnityEngine;

namespace Backgammon.Unity
{
    /// <summary>
    /// The only class that mutates game state: owns the GameEngine and pushes its BoardState to
    /// the presentation layer whenever it changes. Nothing else should touch GameEngine directly.
    /// </summary>
    [RequireComponent(typeof(BoardView))]
    public class GameController : MonoBehaviour
    {
        private BoardView _boardView;
        private GameEngine _engine;

        private void Awake()
        {
            _boardView = GetComponent<BoardView>();
        }

        private void Start()
        {
            _engine = new GameEngine(Player.White);
            _engine.Changed += HandleEngineChanged;
            _boardView.Render(_engine.Board);
        }

        private void OnDestroy()
        {
            if (_engine != null)
            {
                _engine.Changed -= HandleEngineChanged;
            }
        }

        private void HandleEngineChanged()
        {
            _boardView.Render(_engine.Board);
        }
    }
}
