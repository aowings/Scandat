MSearchBroadcast

A lightweight C# console tool for broadcasting SSDP (Simple Service Discovery Protocol) M-SEARCH requests and listening for responses from UPnP devices on the local network.

Overview:

This program sends M-SEARCH discovery messages to common SSDP ports (1900, 1901, 1902) and listens for responses from network devices that support the Universal Plug and Play (UPnP) protocol.

It extracts and displays useful details like:

Device name (from USN field)

IP address (if included in XML/response)

MAC address (if available)

Other standard SSDP headers such as LOCATION, SERVER, and ST.

This is useful for:

Network diagnostics

IoT or smart home device discovery

UPnP device testing

Embedded network development

 Features:

Sends SSDP M-SEARCH discovery requests
Scans multiple common SSDP ports (1900–1902)
Listens for and parses UDP responses
Displays extracted device information
Automatically stops after 10 minutes or by user input

Example Output
Starting M-SEARCH broadcast...
M-SEARCH message sent to port 1900.
M-SEARCH message sent to port 1901.
M-SEARCH message sent to port 1902.
Listening for UDP responses...
Press any key to stop the program...

Device Information:
Device Name: uuid:12345678-90ab-cdef-1234-567890abcdef
MAC Address: 00:1A:2B:3C:4D:5E
IP Address: 192.168.1.101

Received response:
HTTP/1.1 200 OK
CACHE-CONTROL: max-age=1800
DATE: Sat, 09 Nov 2025 21:45:22 GMT
EXT:
LOCATION: http://192.168.1.101:80/rootDesc.xml
SERVER: Linux/3.14 UPnP/1.0 MyDevice/1.0
ST: urn:schemas-upnp-org:device:basic:1
USN: uuid:12345678-90ab-cdef-1234-567890abcdef::urn:schemas-upnp-org:device:basic:1

How It Works

Broadcasts M-SEARCH messages to 239.255.255.250 on the ports defined in portsToScan.

Listens for UDP responses on port 1900 and joins the multicast group.

Parses responses using regular expressions to extract key information.

Displays device and response details in the console.

Automatically stops after 10 minutes, or you can press any key to stop manually.

Requirements

.NET 6.0 or higher

Works on Windows, Linux, and macOS

Network access to the local multicast group (239.255.255.250)

 Build & Run
1. Clone the repository
git clone https://github.com/aowings/Scandat
cd Scandat

2. Build the project
dotnet build

3. Run the application
dotnet run

 Configuration:

You can modify these values in Program.cs:

Setting	Description	Default
portsToScan	Ports to send the M-SEARCH broadcast	{1900, 1901, 1902}
MSEARCH_MESSAGE	SSDP discovery message	Basic UPnP device discovery
Timer duration	Time before auto-stop	10 minutes (600,000 ms)
 Notes

If no devices respond, ensure that your network allows multicast traffic and that UPnP is enabled on your router or devices.

Some devices respond only to specific ST (Search Target) values — you can modify the ST: line in MSEARCH_MESSAGE to discover specific device types.

 References

UPnP Device Architecture 1.1 Specification

Microsoft SSDP Documentation

 Author:
 Alex Owings

License:

This project is licensed under the MIT License

