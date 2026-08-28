using System;
using System.Linq;
using Backgammon.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Backgammon.Unity
{
    /// <summary>
    /// Renders whose turn it is and the current dice/game-over state, and surfaces the Roll
    /// button as an event. Never touches GameEngine directly — GameController owns that.
    /// </summary>
    public class TurnHudView : MonoBehaviour
    {
        [SerializeField] private Button rollButton;
        [SerializeField] private TextMeshProUGUI statusText;

        /// <summary>Raised when the player presses the Roll button.</summary>
        public event Action RollClicked;

        private void Awake()
        {
            rollButton.onClick.AddListener(() => RollClicked?.Invoke());
        }

        /// <param name="notice">
        /// An extra one-off line to show alongside the normal status — e.g. that the previous
        /// turn auto-passed with no legal move. Shown once, for this render only.
        /// </param>
        public void Render(GameEngine engine, string notice = null)
        {
            switch (engine.Phase)
            {
                case GamePhase.WaitingForRoll:
                    statusText.text = $"{engine.CurrentPlayer}'s turn — click Roll";
                    rollButton.interactable = true;
                    break;

                case GamePhase.MovesRemaining:
                    string dice = string.Join(", ", engine.RemainingDice);
                    statusText.text = $"{engine.CurrentPlayer}: rolled {engine.CurrentDice.Value.Die1}-{engine.CurrentDice.Value.Die2} — remaining: {dice}";
                    rollButton.interactable = false;
                    break;

                case GamePhase.GameOver:
                    statusText.text = $"{engine.Winner} wins ({engine.WinResult})!";
                    rollButton.interactable = false;
                    break;
            }

            if (!string.IsNullOrEmpty(notice))
            {
                statusText.text = $"{notice}\n{statusText.text}";
            }
        }
    }
}
