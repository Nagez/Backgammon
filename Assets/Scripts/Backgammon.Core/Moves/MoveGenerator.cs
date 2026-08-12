using System.Collections.Generic;
using System.Linq;

namespace Backgammon.Core
{
    /// <summary>
    /// Determines which moves a player is legally allowed to make with the dice remaining in
    /// their turn, enforcing that a player must use as many dice as possible and, when exactly
    /// one of two different dice can be played, must play the larger one.
    /// </summary>
    public static class MoveGenerator
    {
        /// <summary>Expands a roll into its available die values: two values normally, four equal values for a double.</summary>
        public static IReadOnlyList<int> ExpandDice(Dice dice)
        {
            return dice.IsDouble
                ? new List<int> { dice.Die1, dice.Die1, dice.Die1, dice.Die1 }
                : new List<int> { dice.Die1, dice.Die2 };
        }

        /// <summary>The greatest number of the remaining dice the player can legally use from this position, playing optimally.</summary>
        public static int MaxDiceUsable(BoardState board, Player player, IReadOnlyList<int> remainingDice)
        {
            int best = 0;

            foreach (int dieValue in remainingDice.Distinct())
            {
                foreach (Move move in CandidateMoves(board, player, dieValue))
                {
                    BoardState next = MoveValidator.Apply(board, move);
                    List<int> nextDice = RemoveOne(remainingDice, dieValue);
                    int result = 1 + MaxDiceUsable(next, player, nextDice);
                    if (result > best)
                    {
                        best = result;
                    }
                }
            }

            return best;
        }

        /// <summary>
        /// The moves legal to play right now: single moves that are individually legal AND don't
        /// strand a die the player could otherwise have used, with the larger-die tie-break applied
        /// when only one of two different dice can be played at all.
        /// </summary>
        public static IReadOnlyList<Move> GetLegalMoves(BoardState board, Player player, IReadOnlyList<int> remainingDice)
        {
            int maxUsable = MaxDiceUsable(board, player, remainingDice);
            if (maxUsable == 0)
            {
                return new List<Move>();
            }

            List<int> distinctValues = remainingDice.Distinct().ToList();

            var legalMoves = new List<Move>();
            foreach (int dieValue in distinctValues)
            {
                foreach (Move move in CandidateMoves(board, player, dieValue))
                {
                    BoardState next = MoveValidator.Apply(board, move);
                    List<int> nextDice = RemoveOne(remainingDice, dieValue);
                    if (MaxDiceUsable(next, player, nextDice) == maxUsable - 1)
                    {
                        legalMoves.Add(move);
                    }
                }
            }

            bool couldNeedTieBreak = maxUsable == 1
                && remainingDice.Count == 2
                && distinctValues.Count == 2;

            if (couldNeedTieBreak)
            {
                int largerValue = distinctValues.Max();
                bool largerDieIsPlayable = legalMoves.Any(move => move.Die == largerValue);

                // Only exclude the smaller die's moves if the larger die is playable too —
                // "must play the larger" only applies when there's actually a choice.
                if (largerDieIsPlayable)
                {
                    legalMoves = legalMoves.Where(move => move.Die == largerValue).ToList();
                }
            }

            return legalMoves;
        }

        /// <summary>Every individually-legal single move the player can make using one specific die value.</summary>
        private static IEnumerable<Move> CandidateMoves(BoardState board, Player player, int dieValue)
        {
            int direction = PlayerRules.Direction(player);

            if (board.GetBar(player) > 0)
            {
                int to = PlayerRules.BarSentinel(player) + direction * dieValue;
                var entryMove = new Move(player, null, to, dieValue);
                if (MoveValidator.IsLegal(board, entryMove))
                {
                    yield return entryMove;
                }
                yield break;
            }

            for (int from = 0; from < BoardState.PointCount; from++)
            {
                int to = from + direction * dieValue;
                var move = new Move(player, from, to, dieValue);
                if (MoveValidator.IsLegal(board, move))
                {
                    yield return move;
                }
            }

            int homeStart = PlayerRules.HomeBoardStart(player);
            int homeEnd = PlayerRules.HomeBoardEnd(player);
            for (int from = homeStart; from <= homeEnd; from++)
            {
                var bearOffMove = new Move(player, from, PlayerRules.BearOffTarget(player), dieValue);
                if (MoveValidator.IsLegal(board, bearOffMove))
                {
                    yield return bearOffMove;
                }
            }
        }

        private static List<int> RemoveOne(IReadOnlyList<int> values, int valueToRemove)
        {
            var result = new List<int>(values);
            result.Remove(valueToRemove);
            return result;
        }
    }
}
