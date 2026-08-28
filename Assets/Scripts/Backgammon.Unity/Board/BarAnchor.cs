using Backgammon.Core;
using UnityEngine;

namespace Backgammon.Unity
{
    /// <summary>
    /// Marks a manually-positioned child Transform as where one player's checkers stack while
    /// on the bar. Dragged in the Scene view to match the board art, same as PointAnchor.
    /// </summary>
    public class BarAnchor : MonoBehaviour
    {
        [SerializeField] private Player owner;

        public Player Owner => owner;

        public void SetOwner(Player value) => owner = value;

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(transform.position, 0.08f);
#if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up * 0.15f, $"Bar {owner}");
#endif
        }
    }
}
