using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Media;
using System.Windows.Media.Animation;
using System.IO;

namespace WGAPuzzleGame
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private PuzzleGame _puzzle;
        private Dictionary<PuzzleGame.CellState, IEnumerator<Rectangle>> _tiles;
        private Dictionary<PuzzleGame.CellState, Brush> _colors;
        private Rectangle _buffer;

        private SoundPlayer _player;
        private DoubleAnimation _anim;

        private void PayRespect()
        {
            _player.Load();
            MCI.Visibility = Visibility.Visible;
            MCI.BeginAnimation(Image.OpacityProperty, _anim);
            _player.Play();
        }
        private void InitRespect()
        {
            MemoryStream memory = new MemoryStream();
            WGA.Properties.Resources.MissionComplete
                .Save(memory, System.Drawing.Imaging.ImageFormat.Png);
            memory.Position = 0;
            BitmapImage bitmapimage = new BitmapImage();
            bitmapimage.BeginInit();
            bitmapimage.StreamSource = memory;
            bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapimage.EndInit();
            memory.Dispose();
            MCI.Source = bitmapimage;
            HideMCI();

            _player = new SoundPlayer(WGA.Properties.Resources.MissionCompleteSnd);

            _anim = new DoubleAnimation(0, 1, new Duration(new TimeSpan(0, 0, 3)));
        }
        private void HideMCI()
        {
            MCI.Opacity = 0;
            MCI.Visibility = Visibility.Collapsed;
        }

        public MainWindow()
        {
            InitializeComponent();
            InitPuzzleBoard();
            InitRespect();
        }

        public void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                    MoveTile(0, -1, PuzzleGame.MoveDirection.Up);
                    break;
                case Key.Down:
                    MoveTile(0, 1, PuzzleGame.MoveDirection.Down);
                    break;
                case Key.Right:
                    MoveTile(1, 0, PuzzleGame.MoveDirection.Right);
                    break;
                case Key.Left:
                    MoveTile(-1, 0, PuzzleGame.MoveDirection.Left);
                    break;
                case Key.Space:
                    ChooseTile();
                    break;
                case Key.R:
                    Reset();
                    HideMCI();
                    break;
                default:
                    break;
            }
        }

        private void MoveTile(int x, int y, PuzzleGame.MoveDirection dir)
        {
            int cY = Grid.GetRow(Cursor);
            int cX = Grid.GetColumn(Cursor);
            if (_buffer!=null)
            {
                if (!_puzzle.TryMoveTile(cY, cX, dir)) return;
                Grid.SetRow(Cursor, cY+y);
                Grid.SetColumn(Cursor, cX+x);
                Grid.SetRow(_buffer, cY+y);
                Grid.SetColumn(_buffer, cX+x);
                if (_puzzle.IsPuzzleCompleted)
                    PayRespect();
            }
            else
            {
                int rX = cX + x >= 0 ? (cX + x) % 5 : 4;
                int rY = cY + y >= 0 ? (cY + y) % 5 : 4;
                Grid.SetRow(Cursor, rY);
                Grid.SetColumn(Cursor, rX);
            }
        }

        private void ChooseTile()
        {
            if (_buffer == null)
            {
                int cY = Grid.GetRow(Cursor);
                int cX = Grid.GetColumn(Cursor);
                try
                {
                    _buffer = Board.Children.Cast<UIElement>()
                        .First( e => 
                        {
                            bool res = Grid.GetRow(e) == cY && Grid.GetColumn(e) == cX;
                            if (e is Rectangle)
                                res &= (e as Rectangle).Fill != Brushes.Gray;
                            else
                                res = false;
                            return res;
                        }) as Rectangle;
                }
                catch (InvalidOperationException exc) { }
                if (_buffer != null)
                    Cursor.BorderBrush = Brushes.HotPink;

            }
            else
            {
                _buffer = null;
                Cursor.BorderBrush = Brushes.Black;
            }
        }

        private void InitPuzzleBoard()
        {
            _colors = new Dictionary<PuzzleGame.CellState, Brush>(3);
            _colors.Add(PuzzleGame.LeftColumnTileType, Brushes.Red);
            _colors.Add(PuzzleGame.MiddleColumnTileType, Brushes.Green);
            _colors.Add(PuzzleGame.RightColumnTileType, Brushes.Blue);
            _tiles = new Dictionary<PuzzleGame.CellState, IEnumerator<Rectangle>>(3);

            foreach (var k in _colors.Keys)
            {
                var list = new List<Rectangle>(5);
                for (int i = 0; i < 5; i++)
                {
                    var r = new Rectangle();
                    r.Fill = _colors[k];
                    list.Add(r);
                    Board.Children.Add(r);
                }
                _tiles.Add(k, list.GetEnumerator());
            }
            _puzzle = new PuzzleGame();
            Reset();
        }

        public void Reset()
        {
            _puzzle.Reset();
            _buffer = null;
            Cursor.BorderBrush = Brushes.Black;
            var board = _puzzle.Board;
            for (int i = 0; i <= 4; i += 2)
                for (int j = 0; j < 5; j++)
                {
                    var iter = _tiles[board[j, i]];
                    iter.MoveNext();
                    var tile = iter.Current;
                    Grid.SetRow(tile, j);
                    Grid.SetColumn(tile, i);
                }
            foreach (var iter in _tiles.Values)
                iter.Reset();
        }
    }
}
