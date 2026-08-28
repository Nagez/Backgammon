using System.Collections.Generic;
using Backgammon.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Backgammon.Unity
{
    /// <summary>
    /// Turns raw clicks into move attempts: click a checker (or the bar) to select it as an
    /// origin, then click a point (or the off tray) to play it as the destination. Reads state
    /// through GameController and BoardView but never mutates the engine directly — every move
    /// goes through GameController.TryPlayMove, the single mutation entry point.
    /// </summary>
    public class InputController : MonoBehaviour
    {
        [SerializeField] private GameController gameController;
        [SerializeField] private BoardView boardView;
        [SerializeField] private Camera boardCamera;
        [SerializeField] private InputActionReference pointAction;
        [SerializeField] private InputActionReference clickAction;

        private bool _hasSelection;
        private int? _selectedFrom;
        private bool _wasPressed;

        private void OnEnable()
        {
            pointAction.action.Enable();
            clickAction.action.Enable();
        }

        private void OnDisable()
        {
            clickAction.action.Disable();
            pointAction.action.Disable();
        }

        // Polled here rather than via an action-performed callback: IsPointerOverGameObject()
        // reads stale (previous-frame) UI state when called from an Input System callback,
        // since callbacks run before this frame's UI raycast. Update() runs after it.
        private void Update()
        {
            // UI/Click is a PassThrough action (fires on every value change, i.e. press AND
            // release), not a discrete "performed once" button action — so we track the press
            // edge ourselves rather than trusting WasPerformedThisFrame() here.
            bool isPressed = clickAction.action.IsPressed();
            bool justPressed = isPressed && !_wasPressed;
            _wasPressed = isPressed;

            if (!justPressed)
            {
                return;
            }

            // Clicks over UI (e.g. the Roll button) belong to the UI, not the board.
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            Vector2 screenPosition = pointAction.action.ReadValue<Vector2>();
            Vector3 worldPosition = boardCamera.ScreenToWorldPoint(
                new Vector3(screenPosition.x, screenPosition.y, -boardCamera.transform.position.z));

            if (_hasSelection)
            {
                TrySelectTarget(worldPosition);
            }
            else
            {
                TrySelectOrigin(worldPosition);
            }

            RefreshHighlights();
        }

        private void RefreshHighlights()
        {
            if (!_hasSelection)
            {
                boardView.ClearHighlights();
                return;
            }

            Player current = gameController.CurrentPlayer;
            var targets = new List<int>();
            foreach (Move move in gameController.GetLegalMoves())
            {
                if (move.From == _selectedFrom && !targets.Contains(move.To))
                {
                    targets.Add(move.To);
                }
            }

            boardView.SetHighlights(current, _selectedFrom, targets);
        }

        private void TrySelectOrigin(Vector2 worldPosition)
        {
            Player current = gameController.CurrentPlayer;

            Player? barOwner = boardView.FindBarOwner(worldPosition);
            if (barOwner == current && gameController.HasCheckerOnBar(current))
            {
                _hasSelection = true;
                _selectedFrom = null;
                return;
            }

            int? pointIndex = boardView.FindPointIndex(worldPosition);
            if (pointIndex.HasValue && gameController.OwnsCheckerAt(pointIndex.Value, current))
            {
                _hasSelection = true;
                _selectedFrom = pointIndex;
            }
        }

        private void TrySelectTarget(Vector2 worldPosition)
        {
            Player current = gameController.CurrentPlayer;
            int? targetIndex = boardView.FindPointIndex(worldPosition);

            if (!targetIndex.HasValue && boardView.FindOffOwner(worldPosition) == current)
            {
                targetIndex = PlayerRules.BearOffTarget(current);
            }

            _hasSelection = false;

            if (!targetIndex.HasValue)
            {
                return;
            }

            if (!gameController.TryPlayMove(_selectedFrom, targetIndex.Value))
            {
                // Not a legal destination — treat the click as picking a new origin instead
                // of leaving the player stuck with a selection they can't act on.
                TrySelectOrigin(worldPosition);
            }
        }
    }
}
