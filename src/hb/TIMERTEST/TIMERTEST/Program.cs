using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Threading.Tasks;

namespace TIMERTEST
{
    class Program
    {
        private static System.Timers.Timer aTimer;
        public static DateTime compare = DateTime.Now;
    
        private static void OnTimedEvent(object source, ElapsedEventArgs e)
        {

            TimeSpan ts = e.SignalTime - compare;

            if (ts.Seconds >= 30)
            {
                Console.WriteLine("30초 넘음");
            }
            else
            {
                compare = e.SignalTime;
                Console.WriteLine("The Elapsed event was raised at {0}", e.SignalTime.GetDateTimeFormats('u'));
            }

            
        }
        static void Main(string[] args)
        {

            aTimer = new System.Timers.Timer(31000);

            aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            //aTimer.Interval = 2000;
            aTimer.Enabled = true;

            Console.WriteLine("Press the Enter key to exit the program.");
            Console.ReadLine();
        }

       
    }
}
