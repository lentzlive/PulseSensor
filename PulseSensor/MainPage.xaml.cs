using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using System.ComponentModel;
using DevExpress.UI.Xaml.Charts;
using DeviceLibrary;
using DeviceLibrary.Model;
using System.Globalization;



// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace PulseSensor
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        #region static
        public static readonly DependencyProperty DataProperty;
        static MainPage()
        {
            DataProperty = DependencyProperty.Register("Data", typeof(SensorDataGenerator), typeof(MainPage), new PropertyMetadata(null));
        }
        #endregion


        #region dep props
        public SensorDataGenerator Data
        {
            get { return (SensorDataGenerator)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }


        #endregion

        DispatcherTimer timer;
        public static double minData = 10000;
        public static double MaxData = 0;
        SensorHelper _sensor = null;

        public MainPage()
        {
            this.InitializeComponent();
            rangeY.StartValue = 18.0;// minData - minData * 0.0002;
            rangeY.EndValue = 25.0;// MaxData + MaxData * 0.0002;

            try
            {
                _sensor = new SensorHelper();
                Data = new SensorDataGenerator();
                timer = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 0, 0, 1) };
                timer.Tick += OnTimerTick;
                Loaded += OnLoaded;
                Unloaded += OnUnloaded;
                StatusText.Text = _sensor.Message;
            }
            catch (Exception ex)
            { StatusText.Text = ex.Message; }
        }

        void OnLoaded(object sender, RoutedEventArgs e)
        {
            DataContext = this;
            timer.Start();
        }

        void OnUnloaded(object sender, RoutedEventArgs e)
        {            
            try
            {
                DataContext = null;
                this.timer.Stop();
            }
            catch (Exception ex)
            { this.StatusText.Text = ex.Message; }

        }


        public static int i = 1;
        public static string strTime = "";
        public static double sumTime = 0.00;

        void OnTimerTick(object sender, object e)
        {
            try
            {
                Data.Refresh();


                double _timeMedio = 0.00;
                double _time = Data.GetTimeWatch();
                sumTime += _time;
                _timeMedio = sumTime / i;
                txtCount.Text = Convert.ToString(i) + " - " + Data.GetIterazioni().ToString();
                txtTimeWatch.Text = _time.ToString("F") + " us";// strTime;
                txtTimeWatchMedio.Text = _timeMedio.ToString("F");
                i++;



                double _pulse = Data.GetPulseValue();
                if (_pulse > MaxData)
                {
                    MaxData = _pulse;
                    rangeY.EndValue = Convert.ToDouble(_pulse) + 1;
                }
                if (_pulse < minData)
                {
                    minData = _pulse;
                    rangeY.StartValue = Convert.ToDouble(_pulse) - 1;
                }

               chart.ActualAxisX.VisualRange = new VisualAxisRange() { StartValueInternal = Convert.ToDouble(chart.AxisX.WholeRange.EndValue) - Convert.ToDouble(slider.Value), EndValueInternal = Convert.ToDouble(chart.AxisX.WholeRange.EndValue) };



            }
            catch (Exception ex)
            { this.StatusText.Text = ex.Message; }
        }

   


    }


    public class SensorDataGenerator : ChartDataAdapter
    {

        const int PointsCount = 1000;
        const double Divider = 500;
        public int NewPointsCount = 5;

        double count = 0;
        double baseTime = 0;
        double baseTimeSelector = 1000;

        double timewatch = 0.00;
        double pValue = 0.00;
        Random random = new Random();
        List<Point> points = new List<Point>();

        protected override int RowsCount { get { return points.Count; } }

        public SensorDataGenerator()
        {
            for (int i = 0; i < PointsCount; i++)
            {
        
                points.Add(new Point(i, 0));
                IncreaseCount();
            }
        }
        void IncreaseCount()
        {
            count++;
        }

        double ReturnCount()
        {
            return count;
        }
        public void SetTimeBaseSelectior(double val)
        {
            baseTimeSelector = val;
        }


        public void ResetPar()
        {
            count = 0;
            baseTime = 0;
            timewatch = 0;
            pValue = 0;
        }



        public double GetTimeWatch()
        {
            return timewatch;
        }
        public double GetPulseValue()
        {
            return pValue;
        }
        int iter = 0;
        public double GetIterazioni()
        {
            return iter;
        }


        public void SetPointCount(int val)
        {
            NewPointsCount = val;
        }



        SensorValues GetSensorvalues()
        {
            Sensor s = new Sensor();
            SensorValues sValues = s.GetSensorValue("myPulseDevice");
            pValue = Convert.ToDouble(sValues.Pulse);
            this.timewatch = sValues.Timewatch;
            return sValues;

        }
        double GetValue(double count)
        {
            Sensor s = new Sensor();
            SensorValues sValues = s.GetSensorValue("myPulseDevice");
            this.timewatch = sValues.Timewatch;
            pValue = Convert.ToDouble(sValues.Pulse);
            return pValue;
            //return (Math.Sin((count / Divider) * 2.0 * Math.PI) + random.NextDouble() - 0.5) * 33;
        }
        protected override DateTime GetDateTimeValue(int index, ChartDataMemberType dataMember)
        {
            return DateTime.MinValue;
        }
        protected override object GetKey(int index)
        {
            return null;
        }
        protected override double GetNumericalValue(int index, ChartDataMemberType dataMember)
        {
            if (dataMember == ChartDataMemberType.Argument)
                return points[index].X;
            return points[index].Y;
        }
        protected override string GetQualitativeValue(int index, ChartDataMemberType dataMember)
        {
            return null;
        }
        protected override ActualScaleType GetScaleType(ChartDataMemberType dataMember)
        {
            return ActualScaleType.Numerical;
        }
        public void Refresh()
        {
            for (int i = 0; i < NewPointsCount; i++)
            {
                SensorValues sValues = GetSensorvalues();
                baseTime += sValues.Timewatch / baseTimeSelector;

                points.RemoveAt(0);
                //  points.Add(new Point(count, GetValue(count)));
                points.Add(new Point(baseTime, Convert.ToDouble(sValues.Pulse)));
                IncreaseCount();
                iter++;
            }
            OnDataChanged(ChartDataUpdateType.Reset, -1);
        }


    }
}
