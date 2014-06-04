using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Collections;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Diagnostics;

namespace CMDTEST
{
    public class CMD
    {
        ProcessStartInfo cmd = null;
        Process process = null ;
        
        

       public CMD(){
            cmd = new ProcessStartInfo();
            process = new Process();
        }

        public void start(){
            cmd.FileName = @"cmd";
            cmd.WindowStyle = ProcessWindowStyle.Hidden;             // cmd창이 숨겨지도록 하기
            cmd.CreateNoWindow = false;                               // cmd창을 띄우지 안도록 하기
            cmd.UseShellExecute = false;
            cmd.RedirectStandardOutput = true;        // cmd창에서 데이터를 가져오기
            cmd.RedirectStandardInput = true;          // cmd창으로 데이터 보내기
            cmd.RedirectStandardError = true;
            process.EnableRaisingEvents = false;
            process.StartInfo = cmd;
            process.Start();
        }

        public void controlCMD(String msg)
        {
            process.StandardInput.Write(msg + Environment.NewLine);
            
           
        }

        public void exit()
        {
            process.StandardInput.Flush();
            process.StandardInput.Close();
            // String msg = process.StandardOutput.ReadToEnd();        
           // Console.WriteLine(msg);
            //process.WaitForExit();
            //process.Close();
        }
           
    }

    class Program
    {
       
        static void Main(string[] args)
        {
            CMD c = new CMD();

            c.start();
            c.controlCMD(@"start emulator -avd client_dev -sdcard ""D:\Program Files\Java\adt\adt-bundle-windows-x86_64-20140321\sdk\img\sdcard.iso"" -gpu on");
            c.controlCMD(@"start emulator -avd client_dev -sdcard ""D:\Program Files\Java\adt\adt-bundle-windows-x86_64-20140321\sdk\img\sdcard.iso"" -gpu on");
            c.controlCMD(@"start emulator -avd client_dev -sdcard ""D:\Program Files\Java\adt\adt-bundle-windows-x86_64-20140321\sdk\img\sdcard.iso"" -gpu on");
           c.exit();
                     
            
        }
    }
}
