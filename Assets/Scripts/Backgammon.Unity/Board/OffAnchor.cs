using Backgammon.Core;
using UnityEngine;

namespace Backgammon.Unity
{
    /// <summary>
    /// Marks a manually-positioned child Transform as one player's borne-off tray. Dragged in
    /// the Scene view to match the board art, same as PointAnchor.
    /// </summary>
    public class OffAnchor : MonoBehaviour
    {
        [SerializeField] private Player owner;

        public Player Owner => owner;

        public void SetOwner(Player value) => owner = value;

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(transform.position, 0.08f);
#if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up * 0.15f, $"Off {owner}");
#endif
        }
    }
}
