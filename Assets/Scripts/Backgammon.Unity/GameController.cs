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
