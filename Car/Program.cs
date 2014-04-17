using NETMF.OpenSource.XBee;
using NETMF.OpenSource.XBee.Api;
using System.Threading;
using System.Text;
using System;
using Microsoft.SPOT;
using Gadgeteer.Modules.GHIElectronics;

namespace Car
{
    public partial class Program
    {
        void ProgramStarted()
        {
            xBee.DebugPrintEnabled = true;

            xBee.Configure();
            //xBee.Enable();
            xBee.Api.DataReceived += Api_DataReceived;

            var timer = new Gadgeteer.Timer(1000);

            timer.Tick += timer_Tick;

            timer.Start();

            xBee.Api.Send("Hello from the car").ToAll().Invoke();
        }

        void timer_Tick(Gadgeteer.Timer timer)
        {
            
        }

        void Api_DataReceived(XBeeApi receiver, byte[] data, XBeeAddress sender)
        {
            var message = new string(UTF8Encoding.UTF8.GetChars(data));

            var y = double.Parse(message);
            var speed = (int)(y*70.0);

            Debug.Print(message);
            motorControllerL298.MoveMotor(MotorControllerL298.Motor.Motor1, speed);

            
        }
    }
}
