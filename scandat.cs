using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Timers;
using System.Threading;
using System.Threading.Tasks;

namespace MSearchBroadcast
{
    class Program
    {
        // Define the ports to scan
        private static readonly int[] portsToScan = { 1900, 1901, 1902 };

        private const string MSEARCH_MESSAGE = "M-SEARCH * HTTP/1.1\r\n" +
                                        "HOST: 239.255.255.250:1900\r\n" +
                                        "MAN: \"ssdp:discover\"\r\n" +
                                        "MX: 2\r\n" +
                                        "ST: urn:schemas-upnp-org:device:basic:1\r\n" +
                                        "\r\n";

        static async Task Main(string[] args)
        {
            Console.WriteLine("Starting M-SEARCH broadcast...");
            // Send M-SEARCH to all ports
            foreach (int port in portsToScan)
            {
                BroadcastMsearch(port); // Pass the port to BroadcastMsearch
            }

            Console.WriteLine("Listening for UDP responses...");

            // Set a timer to stop listening after a certain time
            var cancellationTokenSource = new CancellationTokenSource();
            var timer = new System.Timers.Timer(600000); // 10 minutes (600000 milliseconds)
            timer.Elapsed += (sender, e) => {
                Console.WriteLine("Stopping listening after 10 minutes.");
                cancellationTokenSource.Cancel();
                timer.Dispose();
            };
            timer.Start();

            // Create a task to listen for user input
            var inputTask = Task.Run(() => {
                Console.WriteLine("Press any key to stop the program...");
                Console.Read
                ();
                cancellationTokenSource.Cancel();
            });

            // Listen for responses (await the task)
            await Task.WhenAny(ListenForResponses(cancellationTokenSource.Token), inputTask); // Await the task

            Console.WriteLine("Program finished.");
        }

        private static void BroadcastMsearch(int port)
        {
            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                socket.Bind(new IPEndPoint(IPAddress.Any, 0));
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);

                var broadcastAddress = IPAddress.Parse("239.255.255.250");
                var bytes = Encoding.ASCII.GetBytes(MSEARCH_MESSAGE);
                socket.SendTo(bytes, new IPEndPoint(broadcastAddress, port));

                Console.WriteLine($"M-SEARCH message sent to port {port}.");
            }
        }

        private static async Task ListenForResponses(CancellationToken cancellationToken)
        {
            using (var udpClient = new UdpClient(1900)) // Listening on the default SSDP port
            {
                // Join the multicast group
                var multicastAddress = IPAddress.Parse("239.255.255.250");
                udpClient.JoinMulticastGroup(multicastAddress);

                // Loop to receive responses
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        // Set a timeout for receiving
                        udpClient.Client.ReceiveTimeout = 5000; // 5 seconds

                        // Receive data asynchronously
                        var result = await udpClient.ReceiveAsync();

                        // Decode the received bytes
                        string response = Encoding.ASCII.GetString(result.Buffer);

                        // Extract device information
                        var locationMatch = Regex.Match(response, @"LOCATION:(.*?)\r\n", RegexOptions.IgnoreCase);
                        var serverMatch = Regex.Match(response, @"SERVER:(.*?)\r\n", RegexOptions.IgnoreCase);
                        var stMatch = Regex.Match(response, @"ST:(.*?)\r\n", RegexOptions.IgnoreCase);

                        // Extract device name, MAC address, and IP address
                        var deviceNameRegex = new Regex(@"(?<=USN: uuid:).*?(?=\::)", RegexOptions.IgnoreCase);
                        var macAddressRegex = new Regex(@"(?<=<MACAddress>).*?(?=<)", RegexOptions.IgnoreCase);
                        var ipAddressRegex = new Regex(@"(?<=<IPAddress>).*?(?=<)", RegexOptions.IgnoreCase);

                        string deviceName = deviceNameRegex.Match(response).Value.Trim();
                        string macAddress = macAddressRegex.Match(response).Value.Trim();
                        string ipAddress = ipAddressRegex.Match(response).Value.Trim();

                        Console.WriteLine("Device Information:");
                        Console.WriteLine($"Device Name: {deviceName}");
                        Console.WriteLine($"MAC Address: {macAddress}");
                        Console.WriteLine($"IP Address: {ipAddress}");
                        Console.WriteLine();

                        Console.WriteLine("Received response:");
                        Console.WriteLine(response);
                        Console.WriteLine(); // Add an empty line for better readability
                    }
                    catch (SocketException ex)
                    {
                        if (ex.SocketErrorCode != SocketError.TimedOut)
                        {
                            Console.WriteLine("SocketException: " + ex.Message);
                            break;
                        }
                        // Timeout is expected, just continue to the next iteration
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Exception: " + ex.Message);
                        break;
                    }
                }
            }
        }
    }
}