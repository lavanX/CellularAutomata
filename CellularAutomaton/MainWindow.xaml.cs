using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CellularAutomaton.Core;
using Timer = System.Timers.Timer;

namespace CellularAutomata
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private readonly Color _white = Color.FromArgb(255, 255, 255, 255);
        private readonly Color _black = Color.FromArgb(255, 0, 0, 0);
        private bool _matrixCreated;
        private WriteableBitmap _imageSource;
        private int _scale;
        private bool _isAutoUpdateEnabled;
        private int _autoUpdateDelay = 1000;

        public MainWindow()
        {
            InitializeComponent();
            RenderOptions.SetBitmapScalingMode(CrystalViewImage, BitmapScalingMode.NearestNeighbor);
            RenderOptions.SetEdgeMode(CrystalViewImage, EdgeMode.Aliased);
           _timer = new Timer();
        }

        private int Size => (int) MatrixSizeSlider.Value;

        public int Scale
        {
            get => _scale;
            set
            {
                _scale = value;
                OnPropertyChanged();
            }
        }

        public ICrystal Crystal { get; private set; } 

        public bool MatrixCreated
        {
            get => _matrixCreated;
            set
            {
                _matrixCreated = value;
                OnPropertyChanged();
                IsAutoUpdateEnabled = false;
            }
        }

        public bool IsAutoUpdateEnabled
        {
            get => !(!_isAutoUpdateEnabled && MatrixCreated);
            set
            {
                _isAutoUpdateEnabled = value;
                OnPropertyChanged();
            }
        }

        public int AutoUpdateDelay
        {
            get => _autoUpdateDelay;
            set
            {

                _autoUpdateDelay = value;
                _timer.Interval = value;
                OnPropertyChanged();
            }
        }

        private void CreateMatrixButton_OnClick(object sender, RoutedEventArgs e)
        {
            Crystal = CrystalFactory.CreateCrystal((int) MatrixSizeSlider.Value, (int) RuleSlider.Value);

            CrystalViewImage.Source = new WriteableBitmap(Size, Size, 96, 96, PixelFormats.Bgr32, null);
            _imageSource = (WriteableBitmap)CrystalViewImage.Source;
            Scale = Size;
            MatrixCreated = true;
            DrawMatrix();
        }

        private void DrawMatrix()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _imageSource?.ForEach(
                    (column, row, color) => Crystal.GetState(column, row) switch {
                        PixelState.On => _white,
                        PixelState.Off => _black,
                        _ => throw new ArgumentOutOfRangeException()
                    }
                );
            });
        }

        private void MakeStep()
        {
            Crystal.UpdateMatrixState();
            DrawMatrix();
        }
        private void ClearButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (IsAutoUpdateEnabled)
            {
                ToggleAutoUpdate();
            }
            MatrixCreated = false;
            Crystal = null;
            _imageSource = null;
            CrystalViewImage.Source = null;

        }

        private void OnCrystalClicked(object sender, MouseButtonEventArgs e)
        {
            var column = (int)( e.GetPosition(CrystalViewImage).X / Scale* Size);
            var row = (int)( e.GetPosition(CrystalViewImage).Y / Scale* Size);
            Crystal.SetState(column,row, PixelState.On);
            _imageSource.SetPixel(column, row, _white);
        }
        
        private void CrystalViewImage_OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if(Scale == Size && e.Delta < 0)
                return;

            var gain = e.Delta > 0 ? 2 : Math.Pow(2, -1);
            Scale = (int)(Scale * gain);
        }

        #region NotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        private void StepButton_OnClick(object sender, RoutedEventArgs e)
        {
            MakeStep();
        }



        #region AutoUpdate
        private void AutoUpdateButton_OnClick(object sender, RoutedEventArgs e)
        {
           ToggleAutoUpdate();
        }

        private void ToggleAutoUpdate()
        {
            IsAutoUpdateEnabled = !_isAutoUpdateEnabled;
            if (IsAutoUpdateEnabled)
            {
                _timer.Interval = AutoUpdateDelay;
                _timer.Elapsed += (o, args) => AutoUpdate();
                _timer.Start();
            }
            else
            {
                _timer.Stop();
            }
        }
        public void AutoUpdate()
        {
            if (!MatrixCreated) 
                return;
            if (!IsAutoUpdateEnabled) 
                return;

            _timer.Stop();
            MakeStep();
            _timer.Start();
        }
        private readonly Timer _timer;

        #endregion

    }
}
