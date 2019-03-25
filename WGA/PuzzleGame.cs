using System;
using System.Text;
using static WGAPuzzleGame.PuzzleGame.CellState;
using static WGAPuzzleGame.PuzzleGame.MoveDirection;

namespace WGAPuzzleGame
{
    public sealed class PuzzleGame
    {
        public enum CellState
        {
            Empty ,
            Forbidden ,
            TileT1 ,
            TileT2 ,
            TileT3 
        }
        public enum MoveDirection
        {
            Up,
            Right,
            Down,
            Left
        }

        private readonly CellState[,] _board;
        private readonly Random _random = new Random();
        private readonly int[] _randomizedPositions = { 0, 1, 2, 3, 4 };
        public const CellState LeftColumnTileType = TileT1;
        public const CellState MiddleColumnTileType = TileT2;
        public const CellState RightColumnTileType = TileT3;
        public CellState[,] Board
        {
            get
            {
                var copy = new CellState[5, 5];
                Array.Copy(_board, copy, _board.Length);
                return copy;
            }
        }
        public bool IsPuzzleCompleted
        {
            get
            {
                bool result = true;
                int i = 0;
                while (result && i<5)
                {
                    result &= _board[i, 0] == LeftColumnTileType;
                    result &= _board[i, 2] == MiddleColumnTileType;
                    result &= _board[i, 4] == RightColumnTileType;
                    i++;
                }
                return result;
            }
        }

        public PuzzleGame()
        {
            _board = new CellState[5, 5];
            for (int i = 0; i <= 4; i += 2)
                _board[i, 1] = _board[i, 3] = Forbidden;
        }

        public void Reset()
        {
            SetColumn(0, TileT1, TileT2, TileT3);
            SetColumn(2, TileT2, TileT1, TileT3);
            SetColumn(4, TileT3, TileT2, TileT1);
            for (int i = 1; i <= 3; i += 2)
                _board[i, 1] = _board[i, 3] = Empty;
        }

        private void SetColumn(int columnIndex, CellState proper, CellState nonProper1, CellState nonProper2)
        {
            _random.Shuffle<int>(_randomizedPositions);
            _board[_randomizedPositions[0], columnIndex] = proper;
            _board[_randomizedPositions[1], columnIndex] = nonProper1;
            _board[_randomizedPositions[2], columnIndex] = nonProper1;
            _board[_randomizedPositions[3], columnIndex] = nonProper2;
            _board[_randomizedPositions[4], columnIndex] = nonProper2;
        }

        public bool TryMoveTile(int fromRow, int fromColumn, MoveDirection dir)
        {
            if (fromRow < 0 || fromRow > 4 || fromColumn < 0 || fromColumn > 4)
                throw new ArgumentException("Bad row's/column's index");
            if (_board[fromRow, fromColumn] == Empty || _board[fromRow, fromColumn] == Forbidden)
                return false;
            int toRow = fromRow, toCol = fromColumn;
            switch (dir)
            {
                case Up:
                    toRow -= 1;
                    break;
                case Down:
                    toRow += 1;
                    break;
                case Right:
                    toCol += 1;
                    break;
                case Left:
                    toCol -= 1;
                    break;
            }
            if (toCol < 0 || toCol > 4 || toRow < 0 || toRow > 4)
                return false;
            if (_board[toRow, toCol] != Empty)
                return false;
            _board[toRow, toCol] = _board[fromRow, fromColumn];
            _board[fromRow, fromColumn] = Empty;
            return true;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                    sb.Append(string.Format("{0,10} ", _board[i, j]));
                sb.Append("\n");
            }
            return sb.ToString();
        }

    }
}
