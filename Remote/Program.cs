using NETMF.OpenSource.XBee;
using NETMF.OpenSource.XBee.Api;
namespace Remote
{
    public partial class Program
    {
        void ProgramStarted()
        {
            xBee.DebugPrintEnabled = true;

            xBee.Configure();
            xBee.Api.DataReceived += Api_DataReceived;
        }

        void Api_DataReceived(XBeeApi receiver, byte[] data, XBeeAddress sender)
        {
            var i = 0;
            i++;
        }
    }
}
