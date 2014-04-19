using System.Collections;
using System.Reflection;
using System.Text;
using Gadgeteer;
using Gadgeteer.Modules;
using Gadgeteer.Modules.GHIElectronics;
using GHIElectronics.Gadgeteer;
using Microsoft.SPOT;
using Modules;
using NETMF.OpenSource.XBee;
using NETMF.OpenSource.XBee.Api;
using NETMF.OpenSource.XBee.Api.Common;
using XBee = Gadgeteer.Modules.OpenSource.XBee;


namespace Remote
{
    public class DFRobotRFID : Module
    {
        public DFRobotRFID(int socketNumber)
        {
            Socket socket = Socket.GetSocket(socketNumber, true, this, null);
            socket.EnsureTypeIsSupported('U', this);
        }
    }

    public class Something
    {
    }

    public class SomethingElse
    {
        public Something Something { get; set; }

        public SomethingElse(Something e)
        {
            Something = e;
        }

    }


    public class Program : Gadgeteer.Program
    {
        ArrayList _nodes = new ArrayList();
        CharacterDisplay _characterDisplay;

        FEZSpider _mainBoard;
        XBee _xbee;
        Joystick _joystick;
        Display_T35 _display;


        public Program(FEZSpider mainboard)
        {
            var methods = typeof(SomethingElse).GetMethods(BindingFlags.CreateInstance | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            var constructor = methods[0];

            var instance = constructor.Invoke(this, new[] { new Something() });


            ConstructorInfo c;

            

            _mainBoard = mainboard;
            
            _xbee = new Gadgeteer.Modules.OpenSource.XBee(8);
            _joystick = new Gadgeteer.Modules.GHIElectronics.Joystick(9);
            _display = new Gadgeteer.Modules.GHIElectronics.Display_T35(14, 13, 12, 10);
            _characterDisplay = new CharacterDisplay(3, 0x20);
            _characterDisplay.SetCursor(0, 0);
            _characterDisplay.PrintString("Starting up");
            
            _xbee.DebugPrintEnabled = true;
            
            _xbee.Configure();
            _xbee.Api.StatusChanged += Api_StatusChanged;
            _xbee.Api.DataReceived += Api_DataReceived;
            Debug.Print("Connected : " + _xbee.Api.IsConnected());
            _xbee.Api.DiscoverNodes(NodeDiscovered);

            var joystickTimer = new Timer(100);
            joystickTimer.Tick += joystickTimer_Tick;
            joystickTimer.Start();


            _characterDisplay.Clear();
            _characterDisplay.SetCursor(0, 0);
            _characterDisplay.PrintString("Started");
        }

        void joystickTimer_Tick(Timer timer)
        {
            var position = _joystick.GetPosition();

            foreach (NodeInfo node in _nodes)
            {

                var positionString = "move: {{ x: "+position.X+", y: "+position.Y+" }}";
                positionString = position.Y.ToString();
                _xbee.Api.Send(positionString).To(node).NoResponse();
            }
        }

        void Api_StatusChanged(XBeeApi x, ModemStatus status)
        {
            if (status == ModemStatus.Associated)
                _xbee.Api.DiscoverNodes(NodeDiscovered);
        }


        void NodeDiscovered(DiscoverResult result)
        {
            _nodes.Add(result.NodeInfo);

            //xBee.Api.Send("Welcome to the network").To(result.NodeInfo).NoResponse();
            //xBee.Api.Send("Hello world").To(0x0013A20040ABCC07).NoResponse();
        }

        void Api_DataReceived(XBeeApi receiver, byte[] data, XBeeAddress sender)
        {
            var message = new string(UTF8Encoding.UTF8.GetChars(data));

            var i = 0;
            i++;
        }


        public static void Main()
        {
            var mainboard = new FEZSpider();
            Gadgeteer.Program.Mainboard = mainboard;

            var program = new Program(mainboard);
            program.Run();
        }
    }
}
