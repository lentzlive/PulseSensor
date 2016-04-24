using DeviceLibrary.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceLibrary
{
   public  class Sensor
    {  

        public SensorValues GetSensorValue(string deviceId)
        {
            SensorValues s = new SensorValues();
            s.DeviceId = deviceId;
            try
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();                
                s.Pulse = getPulsevaluse();
                sw.Stop();

                s.Timewatch= sw.ElapsedTicks / (Stopwatch.Frequency / (1000L * 1000L));             
                s.StatusDevice = "On";

                return s;
            }
            catch (Exception exc)
            {
                s.StatusDevice = "Error: " + exc.Message;

                s.Pulse = "nd";
              
                return s;
            }
            finally
            { }

            return null;
        }


        /**************************************************************************************/
        /******************                Metodi Statici                **********************/
        /**************************************************************************************/


        public string getPulsevaluse()
        {
            try
            {
                double avgPulse = 21; // m/s
                Random rand = new Random();

                //double currentTemperature = avgTemp + rand.NextDouble() * 4 - 2;
                double currentPulse = SensorHelper.GetSensorPulseValue();
                return currentPulse.ToString("F");

            }
            catch (Exception exc)
            {
                return "nd";
            }


        }
  

    }
}
