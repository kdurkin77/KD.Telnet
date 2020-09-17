using System;
using System.Net;
using System.Threading.Tasks;
using KD.Telnet.TcpTelnetClient;

namespace Test
{
    public class Program
    {
        public static async Task Main()
        {
            //values needed to telnet into a system
            var ip = IPAddress.Parse("");
            var port = 23;
            var timeout = TimeSpan.FromSeconds(5.0);
            var dataToSend = "test";

            //make the telnet client
            using (ITcpTelnetClient telnetClient = new TcpTelnetClient())
            {
                //connect
                await telnetClient.ConnectAsync(ip, port);
                //receive the initial response to connecting
                var data = await telnetClient.ReceiveData(timeout).ConfigureAwait(false);
                Console.WriteLine($"Received Data: {data}");

                //send some data
                await telnetClient.SendData(dataToSend).ConfigureAwait(false);
                //or send data and receive the echo to ignore it
                //await telnetClient.SendDataReceiveEcho(dataToSend, timeout);

                //receive the response
                var responseData = await telnetClient.ReceiveData(timeout).ConfigureAwait(false);
                Console.WriteLine($"Response: {responseData}");
            }
        }
    }
}
