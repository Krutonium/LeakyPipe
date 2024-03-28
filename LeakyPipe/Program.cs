using System.Net;
using System.Net.NetworkInformation;
using System.Text;

namespace LeakyPipe;

class Program
{
    static void Main(string[] args)
    {
        // Get all tip IPs in hops from me to 8.8.8.8
        Console.WriteLine("Getting all IPs in route...");
        var traceRoute = TraceRoute.GetTraceRoute("8.8.8.8");
        Dictionary<IPAddress, int> failures = new();
        Dictionary<IPAddress, int> successes = new();
        int rounds = 0;
        while (true)
        {
            rounds++;
            //Ping each IP to see if it's alive
            Parallel.ForEach(traceRoute,  ip =>
            {
                var ping = new Ping();
                var reply = ping.Send(ip, 250);
                if (reply.Status != IPStatus.Success)
                {
                    if(!failures.ContainsKey(ip))
                    {
                        failures.Add(ip, 1);
                    }
                    else
                    {
                        failures[ip]++;
                    }
                }
                else
                {
                    if (!successes.ContainsKey(ip))
                    {
                        successes.Add(ip, 1);
                    }
                    else
                    {
                        successes[ip]++;
                    }
                }
            });
            //Print out the results
            Console.Clear();
            Console.WriteLine($"Round {rounds}");
            Console.WriteLine("Successes:");
            foreach (var success in successes)
            {
                Console.WriteLine($"{success.Key}: {success.Value}");
            }
            Console.WriteLine("Failures:");
            foreach (var failure in failures)
            {
                Console.WriteLine($"{failure.Key}: {failure.Value}");
            }
            
            Console.WriteLine();
            Console.WriteLine("What you want to look for is the failure that is the big deviator; some nodes just don't respond to pings. If you see a node that is failing a lot more than the others, that's a good indication that it's the problem.");
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
            int timeout = 1000;
            byte[] buffer = Encoding.ASCII.GetBytes(Data);
            PingReply reply = default(PingReply);

            reply = pinger.Send(hostNameOrAddress, timeout, buffer, pingerOptions);

            List<IPAddress> result = new List<IPAddress>();
            if (reply.Status == IPStatus.Success)
            { 
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