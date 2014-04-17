using System.Collections;
using System.Text;
using Gadgeteer;
using Gadgeteer.Modules;
using Microsoft.SPOT;
using Modules;
using NETMF.OpenSource.XBee;
using NETMF.OpenSource.XBee.Api;
using NETMF.OpenSource.XBee.Api.Common;

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


    public partial class Program
    {
        ArrayList _nodes = new ArrayList();

        CharacterDisplay _characterDisplay;

        void ProgramStarted()
        {
            _characterDisplay = new CharacterDisplay(3, 0x20);
            _characterDisplay.SetCursor(0, 0);
            _characterDisplay.PrintString("Starting up");
            
            xbee.DebugPrintEnabled = true;
            
            xbee.Configure();
            xbee.Api.StatusChanged += Api_StatusChanged;
            xbee.Api.DataReceived += Api_DataReceived;
            Debug.Print("Connected : " + xbee.Api.IsConnected());
            xbee.Api.DiscoverNodes(NodeDiscovered);

            var joystickTimer = new Timer(100);
            joystickTimer.Tick += joystickTimer_Tick;
            joystickTimer.Start();


            _characterDisplay.Clear();
            _characterDisplay.SetCursor(0, 0);
            _characterDisplay.PrintString("Started");
        }

        void joystickTimer_Tick(Timer timer)
        {
            var position = joystick.GetPosition();

            foreach (NodeInfo node in _nodes)
            {

                var positionString = "move: {{ x: "+position.X+", y: "+position.Y+" }}";
                positionString = position.Y.ToString();
                xbee.Api.Send(positionString).To(node).NoResponse();
            }
        }

        void Api_StatusChanged(XBeeApi x, ModemStatus status)
        {
            if (status == ModemStatus.Associated)
                xbee.Api.DiscoverNodes(NodeDiscovered);
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
    }
}
