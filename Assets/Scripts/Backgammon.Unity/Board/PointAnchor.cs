using UnityEngine;

namespace Backgammon.Unity
{
    /// <summary>
    /// Marks a manually-positioned child Transform as the anchor for one of the board's 24
    /// points — dragged in the Scene view to match where that point's triangle actually sits
    /// in the board art, since that position can't be derived analytically from the image.
    /// </summary>
    public class PointAnchor : MonoBehaviour
    {
        [SerializeField] private int index;

        public int Index => index;

        public void SetIndex(int value) => index = value;

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position, 0.08f);
#if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up * 0.15f, index.ToString());
#endif
        }
    }
}
