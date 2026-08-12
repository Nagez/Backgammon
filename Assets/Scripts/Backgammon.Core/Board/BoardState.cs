namespace Backgammon.Core
{
    /// <summary>
    /// An immutable snapshot of a backgammon board: the 24 points, each player's bar count,
    /// and each player's borne-off count. "With*" methods return a new snapshot rather than
    /// mutating this one, so a caller can hold onto an earlier snapshot while trying moves ahead.
    /// </summary>
    public sealed class BoardState
    {
        /// <summary>Number of points on the board, indexed 0-23.</summary>
        public const int PointCount = 24;

        /// <summary>Number of checkers each player starts the game with.</summary>
        public const int CheckersPerPlayer = 15;

        private readonly PointState[] _points;
        private readonly int _barWhite;
        private readonly int _barBlack;
        private readonly int _borneOffWhite;
        private readonly int _borneOffBlack;

        private BoardState(PointState[] points, int barWhite, int barBlack, int borneOffWhite, int borneOffBlack)
        {
            _points = points;
            _barWhite = barWhite;
            _barBlack = barBlack;
            _borneOffWhite = borneOffWhite;
            _borneOffBlack = borneOffBlack;
        }

        /// <summary>Builds the standard backgammon starting position (each side's 15 checkers on their opening points).</summary>
        public static BoardState CreateStartingPosition()
        {
            var points = new PointState[PointCount];
            for (int i = 0; i < PointCount; i++)
            {
                points[i] = PointState.Empty;
            }

            // Standard backgammon starting layout. Points are indexed 0-23;
            // White moves from index 0 toward 23, Black moves from 23 toward 0.
            points[0] = new PointState(Player.White, 2);
            points[11] = new PointState(Player.White, 5);
            points[16] = new PointState(Player.White, 3);
            points[18] = new PointState(Player.White, 5);

            points[23] = new PointState(Player.Black, 2);
            points[12] = new PointState(Player.Black, 5);
            points[7] = new PointState(Player.Black, 3);
            points[5] = new PointState(Player.Black, 5);

            return new BoardState(points, barWhite: 0, barBlack: 0, borneOffWhite: 0, borneOffBlack: 0);
        }

        /// <summary>Returns the occupancy of the point at the given 0-23 index.</summary>
        public PointState GetPoint(int index) => _points[index];

        /// <summary>Returns how many of the given player's checkers are on the bar.</summary>
        public int GetBar(Player player) => player == Player.White ? _barWhite : _barBlack;

        /// <summary>Returns how many of the given player's checkers have been borne off.</summary>
        public int GetBorneOff(Player player) => player == Player.White ? _borneOffWhite : _borneOffBlack;

        /// <summary>Returns a new snapshot with one point's occupancy replaced; this instance is left untouched.</summary>
        public BoardState WithPoint(int index, PointState newState)
        {
            var copy = (PointState[])_points.Clone();
            copy[index] = newState;
            return new BoardState(copy, _barWhite, _barBlack, _borneOffWhite, _borneOffBlack);
        }

        /// <summary>Returns a new snapshot with the given player's bar count replaced; this instance is left untouched.</summary>
        public BoardState WithBar(Player player, int newCount)
        {
            return player == Player.White
                ? new BoardState(_points, newCount, _barBlack, _borneOffWhite, _borneOffBlack)
                : new BoardState(_points, _barWhite, newCount, _borneOffWhite, _borneOffBlack);
        }

        /// <summary>Returns a new snapshot with the given player's borne-off count replaced; this instance is left untouched.</summary>
        public BoardState WithBorneOff(Player player, int newCount)
        {
            return player == Player.White
                ? new BoardState(_points, _barWhite, _barBlack, newCount, _borneOffBlack)
                : new BoardState(_points, _barWhite, _barBlack, _borneOffWhite, newCount);
        }
    }
}
