using System.Collections.Generic;
using Backgammon.Core;
using UnityEngine;

namespace Backgammon.Unity
{
    /// <summary>
    /// Renders a <see cref="BoardState"/> onto the imported board and checker art. Point positions
    /// come from manually-placed <see cref="PointAnchor"/> children rather than a computed layout,
    /// since they depend on where the triangles actually sit in a specific piece of art. Checker
    /// size and stacking are derived from the actual spacing between anchors, so they self-adjust
    /// to however the anchors end up placed instead of relying on a second set of guessed numbers.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class BoardView : MonoBehaviour
    {
        [Header("Checker art")]
        [SerializeField] private Sprite whiteCheckerSprite;
        [SerializeField] private Sprite blackCheckerSprite;

        [Header("Sizing (relative to the spacing between point anchors)")]
        [SerializeField] private float checkerDiameterToSpacingRatio = 0.85f;
        [SerializeField] private float checkerStackStepRatio = 0.8f;

        [Header("Point Anchor Generator (world units, from board center)")]
        [Tooltip("Distance between two adjacent points within the same group of 6.")]
        [Range(0.1f, 2f)]
        [SerializeField] private float generatorPointSpacing = 0.668f;
        [Tooltip("Extra gap between the left group of 6 and the right group of 6 (the bar), on top of point spacing.")]
        [Range(0f, 3f)]
        [SerializeField] private float generatorGroupGap = 1.68f;
        [Tooltip("Vertical distance from the board's center line to each row of points.")]
        [Range(0.5f, 6f)]
        [SerializeField] private float generatorRowInset = 4.1f;

        private Transform[] _pointAnchors;
        private Transform _checkersParent;
        private float _checkerDiameter;
        private readonly List<GameObject> _checkerObjects = new List<GameObject>();

        private void Awake()
        {
            CollectAnchors();

            float spacing = Vector3.Distance(_pointAnchors[0].position, _pointAnchors[1].position);
            _checkerDiameter = spacing * checkerDiameterToSpacingRatio;

            _checkersParent = transform.Find("Checkers");
            if (_checkersParent == null)
            {
                _checkersParent = new GameObject("Checkers").transform;
                _checkersParent.SetParent(transform, false);
            }
        }

        /// <summary>Redraws every checker to match the given board.</summary>
        public void Render(BoardState board)
        {
            foreach (GameObject checker in _checkerObjects)
            {
                Destroy(checker);
            }
            _checkerObjects.Clear();

            for (int index = 0; index < BoardState.PointCount; index++)
            {
                PointState point = board.GetPoint(index);
                if (point.Owner == null)
                {
                    continue;
                }

                for (int stack = 0; stack < point.Count; stack++)
                {
                    SpawnChecker(index, stack, point.Owner.Value);
                }
            }
        }

        private void CollectAnchors()
        {
            _pointAnchors = new Transform[BoardState.PointCount];
            foreach (PointAnchor anchor in GetComponentsInChildren<PointAnchor>())
            {
                _pointAnchors[anchor.Index] = anchor.transform;
            }

            for (int i = 0; i < _pointAnchors.Length; i++)
            {
                if (_pointAnchors[i] == null)
                {
                    Debug.LogError($"BoardView is missing a PointAnchor for index {i}.");
                }
            }
        }

        private void SpawnChecker(int index, int stackIndex, Player owner)
        {
            Transform anchor = _pointAnchors[index];
            float stackDirection = anchor.localPosition.y >= 0f ? -1f : 1f;
            float stepDistance = _checkerDiameter * checkerStackStepRatio;

            var checker = new GameObject($"Checker_{owner}");
            checker.transform.SetParent(_checkersParent, true);
            checker.transform.position = anchor.position + new Vector3(0f, stackDirection * stepDistance * stackIndex, 0f);

            var renderer = checker.AddComponent<SpriteRenderer>();
            renderer.sprite = owner == Player.White ? whiteCheckerSprite : blackCheckerSprite;
            renderer.sortingOrder = 1;

            float nativeSize = renderer.sprite.bounds.size.x;
            float scale = nativeSize > 0f ? _checkerDiameter / nativeSize : 1f;
            checker.transform.localScale = new Vector3(scale, scale, 1f);

            _checkerObjects.Add(checker);
        }

        [ContextMenu("Generate Point Anchors")]
        private void GeneratePointAnchors()
        {
            Transform existing = transform.Find("Points");
            if (existing != null)
            {
                DestroyImmediate(existing.gameObject);
            }

            CreateAnchors();
        }

        private void CreateAnchors()
        {
            var pointsParent = new GameObject("Points");
            pointsParent.transform.SetParent(transform, false);

            for (int index = 0; index < BoardState.PointCount; index++)
            {
                var anchorObject = new GameObject($"Point{index}");
                anchorObject.transform.SetParent(pointsParent.transform, false);
                anchorObject.transform.localPosition = ComputeAnchorLocalPosition(index);

                PointAnchor anchor = anchorObject.AddComponent<PointAnchor>();
                anchor.SetIndex(index);
            }

            Debug.Log("Generated 24 point anchors. Drag the spacing/gap/inset sliders above to reposition them live.");
        }

        // Called by Unity whenever a serialized field changes in the Inspector — including while
        // dragging a slider. If anchors already exist, reposition them live to match. If they
        // don't exist yet (e.g. BoardView was just added), create them automatically instead of
        // requiring a manual "Generate Point Anchors" click first.
        //
        // Object creation is deferred via delayCall rather than done inline: Unity disallows
        // creating/destroying objects synchronously from inside OnValidate (it can throw
        // "SendMessage cannot be called during OnValidate"), since OnValidate can run while the
        // object is still mid-(de)serialization.
        private void OnValidate()
        {
            Transform points = transform.Find("Points");
            if (points == null)
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.delayCall += () =>
                {
                    if (this == null)
                    {
                        return;
                    }
                    if (transform.Find("Points") == null)
                    {
                        CreateAnchors();
                    }
                };
#endif
                return;
            }

            foreach (PointAnchor anchor in points.GetComponentsInChildren<PointAnchor>())
            {
                anchor.transform.localPosition = ComputeAnchorLocalPosition(anchor.Index);
            }
        }

        // Each group of 6 spans (spacing * 5) from its first to last point, centered on itself;
        // the two groups then sit on either side of the bar gap. Shared by the initial generator
        // and the live OnValidate update so they can never drift apart.
        private Vector3 ComputeAnchorLocalPosition(int index)
        {
            float groupWidth = generatorPointSpacing * 5f;
            float groupCenterX = generatorGroupGap / 2f + groupWidth / 2f;

            bool topRow = index >= 12;
            int folded = index < 12 ? index : 23 - index;
            bool rightGroup = folded >= 6;
            int column = folded % 6;

            float localOffset = (column - 2.5f) * generatorPointSpacing;
            float x = (rightGroup ? groupCenterX : -groupCenterX) + localOffset;
            float y = topRow ? generatorRowInset : -generatorRowInset;

            return new Vector3(x, y, 0f);
        }
    }
}
