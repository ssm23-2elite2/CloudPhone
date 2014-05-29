using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Server
{
    class CheckBatteryValue
    {
        CloudPhoneWindow cloudPhonewindow;

        public CheckBatteryValue()
        {

        }


        public void batteryCheckThread()
        {
            
            





            Thread.Sleep(6000); // 60초동안 기다린다.(배터리는 1분마다 검사)
        }
    }
}
