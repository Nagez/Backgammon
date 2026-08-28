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
        [Tooltip("Vertical distance from the board's center line to each player's bar stack (in the gap between the two groups of 6).")]
        [Range(0f, 6f)]
        [SerializeField] private float generatorBarInset = 1.2f;
        [Tooltip("Horizontal distance from board center to each player's borne-off tray, beyond the rightmost point.")]
        [Range(0f, 6f)]
        [SerializeField] private float generatorOffX = 5.5f;
        [Tooltip("Vertical distance from the board's center line to each player's borne-off tray.")]
        [Range(0f, 6f)]
        [SerializeField] private float generatorOffY = 2.5f;

        private Transform[] _pointAnchors;
        private Transform[] _barAnchors;
        private Transform[] _offAnchors;
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
                    SpawnChecker(_pointAnchors[index], stack, point.Owner.Value);
                }
            }

            foreach (Player player in new[] { Player.White, Player.Black })
            {
                Transform barAnchor = _barAnchors[(int)player];
                for (int stack = 0; stack < board.GetBar(player); stack++)
                {
                    SpawnChecker(barAnchor, stack, player);
                }

                Transform offAnchor = _offAnchors[(int)player];
                for (int stack = 0; stack < board.GetBorneOff(player); stack++)
                {
                    SpawnChecker(offAnchor, stack, player);
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

            _barAnchors = new Transform[2];
            foreach (BarAnchor anchor in GetComponentsInChildren<BarAnchor>())
            {
                _barAnchors[(int)anchor.Owner] = anchor.transform;
            }

            _offAnchors = new Transform[2];
            foreach (OffAnchor anchor in GetComponentsInChildren<OffAnchor>())
            {
                _offAnchors[(int)anchor.Owner] = anchor.transform;
            }

            foreach (Player player in new[] { Player.White, Player.Black })
            {
                if (_barAnchors[(int)player] == null)
                {
                    Debug.LogError($"BoardView is missing a BarAnchor for {player}.");
                }
                if (_offAnchors[(int)player] == null)
                {
                    Debug.LogError($"BoardView is missing an OffAnchor for {player}.");
                }
            }
        }

        private void SpawnChecker(Transform anchor, int stackIndex, Player owner)
        {
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
            foreach (string name in new[] { "Points", "Bar", "Off" })
            {
                Transform existing = transform.Find(name);
                if (existing != null)
                {
                    DestroyImmediate(existing.gameObject);
                }
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

            var barParent = new GameObject("Bar");
            barParent.transform.SetParent(transform, false);
            foreach (Player player in new[] { Player.White, Player.Black })
            {
                var anchorObject = new GameObject($"Bar{player}");
                anchorObject.transform.SetParent(barParent.transform, false);
                anchorObject.transform.localPosition = ComputeBarAnchorLocalPosition(player);

                BarAnchor anchor = anchorObject.AddComponent<BarAnchor>();
                anchor.SetOwner(player);
            }

            var offParent = new GameObject("Off");
            offParent.transform.SetParent(transform, false);
            foreach (Player player in new[] { Player.White, Player.Black })
            {
                var anchorObject = new GameObject($"Off{player}");
                anchorObject.transform.SetParent(offParent.transform, false);
                anchorObject.transform.localPosition = ComputeOffAnchorLocalPosition(player);

                OffAnchor anchor = anchorObject.AddComponent<OffAnchor>();
                anchor.SetOwner(player);
            }

            Debug.Log("Generated point, bar, and off anchors. Drag the spacing/gap/inset sliders above to reposition them live.");
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

            Transform bar = transform.Find("Bar");
            if (bar != null)
            {
                foreach (BarAnchor anchor in bar.GetComponentsInChildren<BarAnchor>())
                {
                    anchor.transform.localPosition = ComputeBarAnchorLocalPosition(anchor.Owner);
                }
            }

            Transform off = transform.Find("Off");
            if (off != null)
            {
                foreach (OffAnchor anchor in off.GetComponentsInChildren<OffAnchor>())
                {
                    anchor.transform.localPosition = ComputeOffAnchorLocalPosition(anchor.Owner);
                }
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

        // Sits in the bar gap at board center; White stacks above the center line, Black below,
        // matching the same top-half/bottom-half convention as the point rows.
        private Vector3 ComputeBarAnchorLocalPosition(Player owner)
        {
            float y = owner == Player.White ? generatorBarInset : -generatorBarInset;
            return new Vector3(0f, y, 0f);
        }

        // Sits beyond the rightmost point column, one tray per player above/below center.
        private Vector3 ComputeOffAnchorLocalPosition(Player owner)
        {
            float y = owner == Player.White ? generatorOffY : -generatorOffY;
            return new Vector3(generatorOffX, y, 0f);
        }
    }
}
