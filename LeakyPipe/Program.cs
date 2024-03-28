using System.Net;
using System.Net.NetworkInformation;
using System.Text;

namespace LeakyPipe;

class Program
{
    static void Main(string[] args)
    {
        // Get all tip IPs in hops from me to 8.8.8.8
        while (true)
        {
            var traceRoute = TraceRoute.GetTraceRoute("8.8.8.8");
            //Ping each IP to see if it's alive
            foreach (var ip in traceRoute)
            {
                var ping = new Ping();
                var reply = ping.Send(ip);
                if (reply.Status == IPStatus.Success)
                {
                    Console.WriteLine($"Ping to {ip} took {reply.RoundtripTime} ms");
                }
                else
                {
                    Console.WriteLine($"Ping to {ip} failed");
                }
            }
        }
    }
    public class TraceRoute
    {
        private const string Data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";

        public static IEnumerable<IPAddress> GetTraceRoute(string hostNameOrAddress)
        {
            return GetTraceRoute(hostNameOrAddress, 1);
        }
        private static IEnumerable<IPAddress> GetTraceRoute(string hostNameOrAddress, int ttl)
        {
            Ping pinger = new Ping();
            PingOptions pingerOptions = new PingOptions(ttl, true);
            int timeout = 10000;
            byte[] buffer = Encoding.ASCII.GetBytes(Data);
            PingReply reply = default(PingReply);

            reply = pinger.Send(hostNameOrAddress, timeout, buffer, pingerOptions);

            List<IPAddress> result = new List<IPAddress>();
            if (reply.Status == IPStatus.Success)
            {
                Console.WriteLine($"Added {reply.Address}");
                result.Add(reply.Address);
            }
            else if (reply.Status == IPStatus.TtlExpired || reply.Status == IPStatus.TimedOut)
            {
                //add the currently returned address if an address was found with this TTL
                if (reply.Status == IPStatus.TtlExpired) result.Add(reply.Address);
                //recurse to get the next address...
                IEnumerable<IPAddress> tempResult = default(IEnumerable<IPAddress>);
                tempResult = GetTraceRoute(hostNameOrAddress, ttl + 1);
                result.AddRange(tempResult);
            }
            else
            {
                //failure 
            }

            return result;
        }
    }
}