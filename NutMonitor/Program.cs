using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace NutMonitor
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = String.Empty;
            var port = 3493;
            var upsName = String.Empty;
            var obAction = String.Empty;
            var dryRun = false;

            for (var c = 0; c < args.Length; c++)
            {
                if (args[c] != null && args[c].StartsWith("--"))
                {
                    switch (args[c])
                    {
                        case "--server":
                            if (args.Length > c + 1 && !args[c + 1].StartsWith("--"))
                            {
                                server = args[c + 1];
                            }
                            else
                            {
                                Console.WriteLine("Invalid arguments.");
                                Environment.Exit(-1);
                            }

                            break;

                        case "--port":
                            if (args.Length > c + 1 && !args[c + 1].StartsWith("--"))
                            {
                                if (!Int32.TryParse(args[c + 1], out port))
                                {
                                    Console.WriteLine("The value of argument '--port' is invalid.");
                                    Environment.Exit(-1);
                                }
                            }
                            else
                            {
                                Console.WriteLine("Invalid arguments.");
                                Environment.Exit(-1);
                            }

                            break;

                        case "--ups-name":
                            if (args.Length > c + 1 && !args[c + 1].StartsWith("--"))
                            {
                                upsName = args[c + 1];
                            }
                            else
                            {
                                Console.WriteLine("Invalid arguments.");
                                Environment.Exit(-1);
                            }

                            break;

                        case "--ob-action":
                            if (args.Length > c + 1 && !args[c + 1].StartsWith("--"))
                            {
                                obAction = args[c + 1];
                            }
                            else
                            {
                                Console.WriteLine("Invalid arguments.");
                                Environment.Exit(-1);
                            }

                            break;

                        case "--dry-run":
                            dryRun = true;

                            break;

                        default:
                            Console.WriteLine("The argument '" + args[c] + "' is not recognized.");
                            Environment.Exit(-1);

                            break;
                    }
                }
            }

            if (!String.IsNullOrEmpty(server))
            {
                if (!String.IsNullOrEmpty(upsName))
                {
                    var upsStatus = dryRun ? "OB" : GetStatus(server, port, upsName);

                    switch (upsStatus)
                    {
                        case "OL":
                        case "OL CHRG":
                            Console.WriteLine("The UPS is on AC power.");

                            break;

                        case "OB":
                        case "OB DISCHRG":
                        case "DRYRUN":
                            Console.WriteLine("The UPS is on battery.");

                            if (!String.IsNullOrEmpty(obAction))
                            {
                                try
                                {
                                    var actionFileName = String.Empty;
                                    var actionArguments = String.Empty;

                                    if (obAction.Split(' ').Count() > 1)
                                    {
                                        actionFileName = obAction.Substring(0, obAction.IndexOf(' '));
                                        actionArguments = obAction.Substring(obAction.IndexOf(' ') + 1);
                                    }
                                    else
                                    {
                                        actionFileName = obAction;
                                    }

                                    Console.WriteLine("Executing: " + actionFileName + " " + actionArguments);
                                    Process.Start(new ProcessStartInfo(actionFileName, actionArguments));
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine("An exception occurred while running the OB action: " + ex.Message);
                                }
                            }

                            break;

                        case "COMMAND ERROR":
                            Console.WriteLine("The response from the NUT server was unrecognized.");

                            break;

                        case "NETWORK ERROR":
                            Console.WriteLine("Failed to connect to the NUT server.");

                            break;
                    }
                }
                else
                {
                    Console.WriteLine("The UPS name was not specified.");
                }
            }
            else
            {
                Console.WriteLine("The NUT server was not specified.");
            }
        }

        private static String GetStatus(String server, Int32 port, String upsName)
        {
            var upsStatus = String.Empty;

            try
            {
                using (var client = new TcpClient(server, port))
                {
                    using (var stream = client.GetStream())
                    {
                        var command = "get var " + upsName + " ups.status" + "\n";
                        stream.Write(Encoding.ASCII.GetBytes(command), 0, command.Length);

                        var readBuffer = new Byte[64];
                        var response = String.Empty;

                        do
                        {
                            stream.Read(readBuffer, 0, readBuffer.Length);
                            response += Encoding.ASCII.GetString(readBuffer, 0, readBuffer.Length).Split('\n')[0].Trim();
                        } while (stream.DataAvailable);

                        command = "logout" + Environment.NewLine;
                        stream.WriteAsync(Encoding.ASCII.GetBytes(command), 0, command.Length);

                        if (response.StartsWith("VAR " + upsName + " ups.status "))
                        {
                            upsStatus = response.Replace("VAR " + upsName + " ups.status ", String.Empty).Replace("\"", String.Empty);
                        }
                        else
                        {
                            upsStatus = "COMMAND ERROR";
                        }
                    }
                }
            }
            catch (SocketException)
            {
                upsStatus = "NETWORK ERROR";
            }

            return upsStatus;
        }
    }
}