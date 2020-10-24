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
using CellularAutomaton.Core.Annotations;
using Color = System.Drawing.Color;
using Timer = System.Timers.Timer;

namespace CellularAutomata
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
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
            //_timer = new Timer(state => MakeStep());
           // _timer.
           _timer = new Timer();
        }

        private int Size
        {
            get
            {
                int result = 0;
                Application.Current.Dispatcher.Invoke(() => result = (int) MatrixSizeSlider.Value);
                return result;
            }
        }

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
            Task.Run(action: AutoUpdate);
        }

        private void DrawMatrix()
        {
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    var state = Crystal.GetState(i, j);
                    Color color = state switch
                    {
                        PixelState.On => Color.White,
                        PixelState.Off => Color.Black,
                        _ => throw new ArgumentOutOfRangeException()
                    };
                    //Application.Current.Dispatcher.Invoke(() => { _imageSource.SetPixelValue(i, j, color); });
                    _imageSource.SetPixelValue(i, j, color);
                }
            }
        }

        private void MakeStep()
        {
            Crystal.UpdateMatrixState();
            DrawMatrix();
        }
        private void ClearButton_OnClick(object sender, RoutedEventArgs e)
        {
            MatrixCreated = false;
            Crystal = null;
            _imageSource = null;
            CrystalViewImage.Source = null;

        }

        private void OnCrystalClicked(object sender, MouseButtonEventArgs e)
        {
            var x = (int)( e.GetPosition(CrystalViewImage).X / Scale* Size);
            var y = (int)( e.GetPosition(CrystalViewImage).Y / Scale* Size);
            Crystal.SetState(x,y, PixelState.On);
            _imageSource.SetPixelValue(x, y, Color.White);
        }
        
        private void CrystalViewImage_OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if(Scale == Size && e.Delta < 0)
                return;

            double gain = e.Delta > 0 ? 2 : Math.Pow(2, -1);
            Scale = (int)((double)Scale * gain);

            /*System.Windows.Media.Matrix m = CrystalViewImage.RenderTransform.Value;
            double scaleOdd;
            if (e.Delta > 0)
            {
                scaleOdd = 1.5;
            }
            else
            {
                var scaleTmp = Scale * 1.0 / 1.5;
                scaleOdd = scaleTmp < Size ? Scale / Size : 1.0 / 1.5;
            }

            m.ScaleAt(
                scaleOdd,
                scaleOdd,
                e.GetPosition(CrystalViewImage).X,
                e.GetPosition(CrystalViewImage).Y);

            Scale *= scaleOdd;

            CrystalViewImage.RenderTransform = new MatrixTransform(m);*/
        }

        #region NotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
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
            if (MatrixCreated)
            {
                if (IsAutoUpdateEnabled)
                {
                    _timer.Stop();
                    MakeStep();
                    _timer.Start();
                    
                }
                //Thread.Sleep(AutoUpdateDelay);
            }
        }
        //private System.Timers.Timer _timer ;//= new Timer(state => { MakeStep(); });
        private readonly Timer _timer;

        #endregion

    }
}
