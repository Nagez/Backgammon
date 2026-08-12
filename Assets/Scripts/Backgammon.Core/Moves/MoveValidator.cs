namespace Backgammon.Core
{
    /// <summary>
    /// Checks whether a single <see cref="Move"/> is legal against a board, and applies one.
    /// Only single-move legality lives here (blocked points, forced re-entry, hitting) — deciding
    /// which combinations of moves a whole dice roll forces is the job of the move generator.
    /// </summary>
    public static class MoveValidator
    {
        /// <summary>True if the given move is legal to play on the given board.</summary>
        public static bool IsLegal(BoardState board, Move move)
        {
            bool enteringFromBar = move.From == null;

            // A player with a checker on the bar must enter it before playing any other move.
            if (!enteringFromBar && board.GetBar(move.Player) > 0)
            {
                return false;
            }

            if (enteringFromBar && board.GetBar(move.Player) <= 0)
            {
                return false;
            }

            if (!enteringFromBar && move.To == PlayerRules.BearOffTarget(move.Player))
            {
                return IsLegalBearOff(board, move);
            }

            int fromIndex = move.From ?? PlayerRules.BarSentinel(move.Player);
            int expectedTo = fromIndex + PlayerRules.Direction(move.Player) * move.Die;
            if (expectedTo != move.To)
            {
                return false;
            }

            if (move.To < 0 || move.To >= BoardState.PointCount)
            {
                // Any other off-board destination isn't a real move.
                return false;
            }

            if (!enteringFromBar)
            {
                PointState origin = board.GetPoint(move.From.Value);
                if (origin.Owner != move.Player || origin.Count <= 0)
                {
                    return false;
                }
            }

            PointState destination = board.GetPoint(move.To);
            bool destinationBlocked = destination.Owner.HasValue
                && destination.Owner.Value != move.Player
                && destination.Count >= 2;

            return !destinationBlocked;
        }

        /// <summary>True if the player has every checker in their home board (and none on the bar), so they may bear off.</summary>
        public static bool CanBearOff(BoardState board, Player player)
        {
            if (board.GetBar(player) > 0)
            {
                return false;
            }

            int homeStart = PlayerRules.HomeBoardStart(player);
            int homeEnd = PlayerRules.HomeBoardEnd(player);

            for (int i = 0; i < BoardState.PointCount; i++)
            {
                if (i >= homeStart && i <= homeEnd)
                {
                    continue;
                }

                if (board.GetPoint(i).Owner == player)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsLegalBearOff(BoardState board, Move move)
        {
            if (move.From == null || !CanBearOff(board, move.Player))
            {
                return false;
            }

            PointState origin = board.GetPoint(move.From.Value);
            if (origin.Owner != move.Player || origin.Count <= 0)
            {
                return false;
            }

            int distance = PlayerRules.BearOffDistance(move.Player, move.From.Value);
            int maxDistance = MaxBearOffDistance(board, move.Player);

            // Exact match always works. An overshoot (die bigger than this checker needs) only
            // works if nothing further from home is left to play instead.
            return distance == move.Die || (distance < move.Die && move.Die >= maxDistance);
        }

        private static int MaxBearOffDistance(BoardState board, Player player)
        {
            int homeStart = PlayerRules.HomeBoardStart(player);
            int homeEnd = PlayerRules.HomeBoardEnd(player);

            int max = 0;
            for (int i = homeStart; i <= homeEnd; i++)
            {
                if (board.GetPoint(i).Owner == player)
                {
                    int distance = PlayerRules.BearOffDistance(player, i);
                    if (distance > max)
                    {
                        max = distance;
                    }
                }
            }

            return max;
        }

        /// <summary>
        /// Applies a legal move to the board, returning the resulting snapshot. A lone opposing
        /// checker on the destination is hit and sent to that player's bar. A bear-off move
        /// removes the checker from the board entirely instead of landing it on a point.
        /// </summary>
        public static BoardState Apply(BoardState board, Move move)
        {
            if (move.From.HasValue && move.To == PlayerRules.BearOffTarget(move.Player))
            {
                board = RemoveFromSource(board, move);
                return board.WithBorneOff(move.Player, board.GetBorneOff(move.Player) + 1);
            }

            PointState destination = board.GetPoint(move.To);
            bool isHit = destination.Owner.HasValue
                && destination.Owner.Value != move.Player
                && destination.Count == 1;

            board = RemoveFromSource(board, move);

            if (isHit)
            {
                Player hitPlayer = destination.Owner.Value;
                board = board.WithBar(hitPlayer, board.GetBar(hitPlayer) + 1);
                board = board.WithPoint(move.To, new PointState(move.Player, 1));
            }
            else
            {
                int newCount = (destination.Owner == move.Player ? destination.Count : 0) + 1;
                board = board.WithPoint(move.To, new PointState(move.Player, newCount));
            }

            return board;
        }

        private static BoardState RemoveFromSource(BoardState board, Move move)
        {
            if (move.From == null)
            {
                return board.WithBar(move.Player, board.GetBar(move.Player) - 1);
            }

            PointState origin = board.GetPoint(move.From.Value);
            PointState newOrigin = origin.Count > 1
                ? new PointState(move.Player, origin.Count - 1)
                : PointState.Empty;

            return board.WithPoint(move.From.Value, newOrigin);
        }
    }
}
