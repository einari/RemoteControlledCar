using System.Text;
using Gadgeteer.Modules.GHIElectronics;
using GHIElectronics.Gadgeteer;
using Microsoft.SPOT;
using Modules;
using NETMF.OpenSource.XBee;
using NETMF.OpenSource.XBee.Api;
using XBee = Gadgeteer.Modules.OpenSource.XBee;

namespace Car
{
    public class Program : Gadgeteer.Program
    {
        FEZCerberus _mainboard;
        XBee _xbee;
        MotorControllerL298 _motorController;
        CharacterDisplay _characterDisplay;


        public Program(FEZCerberus mainboard)
        {
            _mainboard = mainboard;
            _characterDisplay = new CharacterDisplay(1, 0x20);
            _characterDisplay.TurnOnBacklight();
            _characterDisplay.SetCursor(0, 0);
            _characterDisplay.PrintString("Starting up");
            


            _xbee = new Gadgeteer.Modules.OpenSource.XBee(6);
            _motorController = new Gadgeteer.Modules.GHIElectronics.MotorControllerL298(4);

            _xbee.DebugPrintEnabled = true;

            _xbee.Configure();
            //xBee.Enable();
            _xbee.Api.DataReceived += Api_DataReceived;
            _xbee.Api.Send("Hello from the car").ToAll().Invoke();

            _characterDisplay.PrintString("Started");
        }


        void Api_DataReceived(XBeeApi receiver, byte[] data, XBeeAddress sender)
        {
            var message = new string(UTF8Encoding.UTF8.GetChars(data));

            var y = double.Parse(message);
            var speed = (int)(y*70.0);

            Debug.Print(message);
            _motorController.MoveMotor(MotorControllerL298.Motor.Motor1, speed);
        }


        public static void Main()
        {
            var mainboard = new FEZCerberus();
            Gadgeteer.Program.Mainboard = mainboard;

            var program = new Program(mainboard);
            program.Run();
        }
    }
}
