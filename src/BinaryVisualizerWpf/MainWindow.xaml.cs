using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace BinaryVisualizerWpf
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog
            {
                Title = "開くファイルを選択してください",
                RestoreDirectory = true
            };

            var result = fileDialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                PathTextBox.Text = fileDialog.FileName;
                await OpenFileAsBinary(fileDialog.FileName);
            }

        }

        private static readonly int RowLength = 16;
        private static readonly int TextWidth = 35;
        private static readonly int TextHeight = 35;

        private GridLabel _previousHexSelection;
        private GridLabel _previousCharSelection;

        private List<GridLabel> _hexLabels;
        private List<GridLabel> _charLabels;

        private async Task OpenFileAsBinary(string filePath)
        {
            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                byte[] buffer = new byte[stream.Length];
                await stream.ReadAsync(buffer, 0, (int)stream.Length);
                int cnt = 0;

                var hexLabels = new List<GridLabel>((int)stream.Length);
                var charLabels = new List<GridLabel>((int) stream.Length);

                foreach (var b in buffer)
                {
                    var hex = Convert.ToString(b, 16);
                    if (hex.Length == 1) hex = $"0{hex}";
                    
                    var hexLabel = BuildGridLabel(cnt, $"{hex}", GridType.Hex);
                    hexLabels.Add(hexLabel);

                    var charLabel = BuildGridLabel(cnt, $"{(char) b}", GridType.Char);
                    charLabels.Add(charLabel);

                    cnt++;
                }

                foreach (var item in hexLabels)
                {
                    Grid1.Children.Add(item);
                }
                foreach (var item in charLabels)
                {
                    Grid2.Children.Add(item);
                }

                _hexLabels = hexLabels;
                _charLabels = charLabels;
            }
        }

        private GridLabel BuildGridLabel(int index, string text, GridType gridType)
        {
            var label = new GridLabel(index % RowLength, index / RowLength, gridType)
            {
                Content = text,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(index % RowLength * TextWidth, index / RowLength * TextHeight, 0, 0),
                Width = TextWidth + 5,
                Height = TextHeight + 5,
                FontFamily = new FontFamily("monospace"),
                FontSize = 24
            };
            label.MouseDown += (s, e) =>
            {
                SelectLabel((GridLabel)s);
            };
            return label;
        }


        private void SelectLabel(GridLabel selection)
        {
            ResetLabel(_previousHexSelection);
            ResetLabel(_previousCharSelection);

            SetBackgroundColor(selection, BackgroundTheme.Black);
            switch (selection.GridType)
            {
                case GridType.Hex:
                    _previousHexSelection = selection;
                    var charSelection = GetPreviousSelectionAccordingTo(selection, _charLabels);
                    if (charSelection != null)
                    {
                        _previousCharSelection = charSelection;
                        SetBackgroundColor(charSelection, BackgroundTheme.Black);
                    }
                    break;
                case GridType.Char:
                    _previousCharSelection = selection;
                    var hexSelection = GetPreviousSelectionAccordingTo(selection, _hexLabels);
                    if (hexSelection != null)
                    {
                        _previousHexSelection = hexSelection;
                        SetBackgroundColor(hexSelection, BackgroundTheme.Black);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        private static void ResetLabel(GridLabel selection)
        {
            if (selection != null) SetBackgroundColor(selection, BackgroundTheme.White);
        }

        private static GridLabel GetPreviousSelectionAccordingTo(GridLabel target, List<GridLabel> searchTarget)
        {
            var selection =
                searchTarget.FirstOrDefault(label => label.X == target.X && label.Y == target.Y);
            return selection;
        }

        private enum BackgroundTheme
        {
            Black,
            White
        }

        private static void SetBackgroundColor(GridLabel target, BackgroundTheme theme)
        {
            switch (theme)
            {
                case BackgroundTheme.Black:
                    target.Background = Brushes.Black;
                    target.Foreground = Brushes.White;
                    break;
                case BackgroundTheme.White:
                    target.Background = Brushes.White;
                    target.Foreground = Brushes.Black;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(theme), theme, null);
            }
        }

        private void ScrollViewer1_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ScrollViewer2.ScrollToVerticalOffset(ScrollViewer1.VerticalOffset);
        }

        private void ScrollViewer2_OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ScrollViewer1.ScrollToVerticalOffset(ScrollViewer2.VerticalOffset);
        }

        private int searchIndex;
        private string currentSearch;
        private List<GridLabel> currentSearchResult;

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            if(string.IsNullOrWhiteSpace(SearchTextBox.Text)) return;
            var search = SearchTextBox.Text;
            if (search != currentSearch)
            {
                searchIndex = 0;
                currentSearchResult = null;
                currentSearch = search;
            }
            
            if (currentSearchResult == null)
            {
                currentSearchResult = _hexLabels.FindAll(label => ((string)label.Content).Contains(search));
                if (currentSearchResult.Count == 0)
                {
                    MessageBox.Show("見つかりませんでした。");
                    currentSearchResult = null;
                    return;
                }
            }

            if (searchIndex != 0 && searchIndex % currentSearchResult.Count == 0)
            {
                MessageBox.Show("先頭に戻りました");
            }

            var selection = currentSearchResult[searchIndex++ % currentSearchResult.Count];
            SelectLabel(selection);
            
            ScrollViewer1.ScrollToVerticalOffset(selection.Margin.Top); // 2 は勝手に動く
        }
    }

    internal enum GridType
    {
        Hex,
        Char
    }

    public class GridLabel : Label
    {
        internal int X { get; }
        internal int Y { get; }
        internal GridType GridType { get; }

        internal GridLabel(int x, int y, GridType gridType)
        {
            X = x;
            Y = y;
            GridType = gridType;
        }
    }
}
