using EtherCAT.NET;
using Microsoft.DotNet.PlatformAbstractions;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using EtherCAT.NET.Infrastructure;
using EtherCAT.NET.Extension;
using System.Runtime.CompilerServices;

namespace SampleMaster
{
    class Program
    {
        static async Task Main(string[] args)
        {
            /*
             * TODO: List all interface names and a selection
             */
            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
            int count = 0;
            foreach (NetworkInterface nic in interfaces)
            {
                Console.WriteLine($"{count} Interface Name: {nic.Name} , " +
                    $"Speed: {nic.Speed} , " +
                    $"ID: {nic.Id} , Description: {nic.Description}");
                count++;
            }
            /* Set interface name. Edit this to suit your needs. */
            var interfaceName = "eth0";
            /*
             * Selection area
             * 
             */
            bool cicle = true;
            while (cicle) {
                Console.WriteLine("Please select via the number in the front of the desired network adapter or quit with \"q\":");
                string input = Console.ReadLine();
                if (input[0] == 'q')
                {
                    return;
                }
                if(int.TryParse(input, out int sel))
                {
                    Console.WriteLine($"Selected: {sel}, {interfaces[sel].Name}, {interfaces[sel].Description}, {interfaces[sel].Id}, {interfaces[sel].Speed}");
                    Console.WriteLine($"Is this the right interface? [y/n/q]");
                    string confirm = Console.ReadLine();
                    if(confirm == "q")
                    {
                        return ;
                    }else if(confirm == "n")
                    {
                        continue;
                    }else if(confirm == "y")
                    {
                        interfaceName = interfaces[sel].Name;
                        Console.WriteLine($"Writing Selected: {interfaceName}");
                        cicle = false;
                    }
                }
            }
            /* Set ESI location. Make sure it contains ESI files! The default path is /home/{user}/.local/share/ESI */
            var localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            Console.WriteLine($"PATH: {localAppDataPath}");
            var esiDirectoryPath = Path.Combine(localAppDataPath, "ESI");
            Console.WriteLine($"ESI-Path: {esiDirectoryPath}");
            Directory.CreateDirectory(esiDirectoryPath);
            Console.WriteLine($"File Count: {Directory.EnumerateFiles(esiDirectoryPath).ToList().Count}");

            /* Copy native file. NOT required in end user scenarios, where EtherCAT.NET package is installed via NuGet! */
            var codeBase = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            Directory.EnumerateFiles(Path.Combine(codeBase, "runtimes"), "*soem_wrapper.*", SearchOption.AllDirectories).ToList().ForEach(filePath =>
            {
                if (filePath.Contains(RuntimeEnvironment.RuntimeArchitecture))
                    File.Copy(filePath, Path.Combine(codeBase, Path.GetFileName(filePath)), true);
            });

            /* create logger */
            var loggerFactory = LoggerFactory.Create(loggingBuilder =>
            {
                loggingBuilder.SetMinimumLevel(LogLevel.Debug);
                loggingBuilder.AddConsole();
            });

            var logger = loggerFactory.CreateLogger("EtherCAT Master");

            /* create EtherCAT master settings (with 10 Hz cycle frequency) */
            var settings = new EcSettings(cycleFrequency: 1U, esiDirectoryPath, interfaceName);

            /* scan available slaves */
            var rootSlave = EcUtilities.ScanDevices(settings.InterfaceName);

            rootSlave.Descendants().ToList().ForEach(slave =>
            {
                // If you have special extensions for this slave, add it here:                    
                // slave.Extensions.Add(new MyFancyExtension());

                /*################ Sample code START ##################

                // Example code to add SDO write request during initialization
                // to Beckhoff "EL3021"
                if (slave.ProductCode == 0xBCD3052)
                {
                    var dataset = new List<object>();
                    dataset.Add((byte)0x01);

                    var requests = new List<SdoWriteRequest>()
                    {
                        // Index 0x8000 sub index 6: Filter on
                        new SdoWriteRequest(0x8000, 0x6, dataset)   
                    };

                    slave.Extensions.Add(new InitialSettingsExtension(requests));
                }

                ################## Sample code END #################*/

                EcUtilities.CreateDynamicData(settings.EsiDirectoryPath, slave);
            });

            /* print list of slaves */
            var message = new StringBuilder();
            var slaves = rootSlave.Descendants().ToList();

            message.AppendLine($"Found {slaves.Count()} slaves:");

            foreach (var slave in slaves)
            {
                message.AppendLine($"{slave.DynamicData.Name} (PDOs: {slave.DynamicData.Pdos.Count} - CSA: {slave.Csa})");
            }

            logger.LogInformation(message.ToString().TrimEnd());

            /* create variable references for later use */
            var variables = slaves.SelectMany(child => child.GetVariables()).ToList();

            /* create EC Master (short sample) */
            /*
            using (var master = new EcMaster(settings, logger))
            {
                try
                {
                    master.Configure(rootSlave);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex.Message);
                    throw;
                }

                /* start master *//*
                var random = new Random();
                var cts = new CancellationTokenSource();

                var task = Task.Run(() =>
                {
                    var sleepTime = 1000 / (int)settings.CycleFrequency;

                    while (!cts.IsCancellationRequested)
                    {
                        master.UpdateIO(DateTime.UtcNow);

                        unsafe
                        {
                            if (variables.Any())
                            {
                                var myVariableSpan = new Span<int>(variables.First().DataPtr.ToPointer(), 1);
                                myVariableSpan[0] = random.Next(0, 100);                              
                            }
                        }

                        Thread.Sleep(sleepTime);
                    }
                }, cts.Token);

                /* wait for stop signal *//*
                Console.ReadKey(true);

                cts.Cancel();
                await task;
            }
            */
            //return; /* remove this to run real world sample*/

            /* create EC Master (real world sample) */
            using (var master = new EcMaster(settings, logger))
            {
                try
                {
                    master.Configure(rootSlave);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex.Message);
                    throw;
                }

                /*################ Sample code START ##################
                */ 
                // Beckhoff EL2008 (8 channel digital output)
                var eL2008 = new DigitalOut(slaves[1]);

                eL2008.SetChannel(1, false);
                eL2008.SetChannel(2, true);
                eL2008.SetChannel(3, false);
                eL2008.SetChannel(4, true);
                /*
                // Beckhoff EL1014 (4 channel digital input)
                var eL1014 = new DigitalIn(slaves[2]);

                // Beckhoff EL3021 (1 channel analog input - 16bit)
                var pdoAnalogIn = slaves[3].DynamicData.Pdos;
                var varAnalogIn = pdoAnalogIn[0].Variables.Where(x => x.Name == "Value").First();
                var varUnderrange = pdoAnalogIn[0].Variables.Where(x => x.Name == "Status__Underrange").First();
                var varOverrange = pdoAnalogIn[0].Variables.Where(x => x.Name == "Status__Overrange").First();

                // Beckhoff EL3021 SDO read (index: 0x8000 sub index: 0x6)
                var datasetFilter = new byte[2];
                EcUtilities.SdoRead(master.Context, 4, 0x8000, 6, ref datasetFilter);
                var filterOn = BitConverter.ToBoolean(datasetFilter, 0);
                logger.LogInformation($"EL3021 filter on: {filterOn}");

                ################## Sample code END #################*/

                /* start master */
                var random = new Random();
                var cts = new CancellationTokenSource();
                int counter = 0;
                var task = Task.Run(() =>
                {
                    var sleepTime = 1000 / (int)settings.CycleFrequency;

                    while (!cts.IsCancellationRequested)
                    {
                        master.UpdateIO(DateTime.UtcNow);

                        /*################ Sample code START ##################
                        */
                        // Beckhoff EL2004 toggle digital output for ch1 and ch3
                        
                        eL2008.SetChannel(1, ((counter & (1 << 7)) != 0));
                        eL2008.SetChannel(2, ((counter & (1 << 6)) != 0));
                        eL2008.SetChannel(3, ((counter & (1 << 5)) != 0));
                        eL2008.SetChannel(4, ((counter & (1 << 4)) != 0));
                        eL2008.SetChannel(5, ((counter & (1 << 3)) != 0));
                        eL2008.SetChannel(6, ((counter & (1 << 2)) != 0));
                        eL2008.SetChannel(7, ((counter & (1 << 1)) != 0));
                        eL2008.SetChannel(8, ((counter & (1 << 0)) != 0));
                        counter++;
                        /*
                        for (int i = 1; i <= 8; i++)
                        {
                            eL2008.SetChannel((int)i, (counter & 1) != 0);
                        }
                        counter++;
                        */
                        //eL2008.ToggleChannel(2);
                        //eL2008.ToggleChannel(4);
                        /*
                        // Beckhoff EL1014 read digital input state 
                        logger.LogInformation($"EL1014 channel 1 input: {eL1014.GetChannel(1)}");
                        logger.LogInformation($"EL1014 channel 2 input: {eL1014.GetChannel(2)}");
                        logger.LogInformation($"EL1014 channel 3 input: {eL1014.GetChannel(3)}");
                        logger.LogInformation($"EL1014 channel 4 input: {eL1014.GetChannel(4)}");

                        // Beckhoff EL2004 read digital output state 
                        logger.LogInformation($"EL1014 channel 1 output: {eL2004.GetChannel(1)}");
                        logger.LogInformation($"EL1014 channel 2 output: {eL2004.GetChannel(2)}");
                        logger.LogInformation($"EL1014 channel 3 output: {eL2004.GetChannel(3)}");
                        logger.LogInformation($"EL1014 channel 4 output: {eL2004.GetChannel(4)}");
                        
                        logger.LogInformation($"EL2008: 1 {eL2008.GetChannel(1)}, 2 {eL2008.GetChannel(2)}, 3 {eL2008.GetChannel(3)}, 4 {eL2008.GetChannel(4)}");
                        logger.LogInformation($"EL2008: 5 {eL2008.GetChannel(5)}, 6 {eL2008.GetChannel(6)}, 7 {eL2008.GetChannel(7)}, 8 {eL2008.GetChannel(8)}");
                        Console.WriteLine($"Logger {counter}");
                        
                        // Beckhoff EL3021 SDO read (index: 0x6000 sub index: 0x2)
                        // overrange of 12 bit analog input.
                        var slaveIndex = (ushort)(Convert.ToUInt16(slaves.ToList().IndexOf(slaves[3])) + 1);
                        var dataset1 = new byte[2];
                        EcUtilities.SdoRead(master.Context, slaveIndex, 0x6000, 2, ref dataset1);
                        bool overrange = BitConverter.ToBoolean(dataset1, 0);
                        logger.LogInformation($"EL3021 overrange: {overrange}");

                        // Beckhoff EL3021 SDO read (index: 0x6000 sub index: 0x1)
                        // underrange of 12 bit analog input.
                        var dataset2 = new byte[2];
                        EcUtilities.SdoRead(master.Context, slaveIndex, 0x6000, 1, ref dataset2);
                        bool underrange = BitConverter.ToBoolean(dataset2, 0);
                        logger.LogInformation($"EL3021 underrange: {underrange}");
                        
                        ################## Sample code END #################*/

                        unsafe
                        {
                            if (variables.Any())
                            {
                                /*################ Sample code START ##################

                                // Read analog current from EL3021 (16 bit - PDO) 
                                void* data = varAnalogIn.DataPtr.ToPointer();
                                int bitmask = (1 << varAnalogIn.BitLength) - 1;
                                int shift = (*(int*)data >> varAnalogIn.BitOffset) & bitmask;
                                short analogIn = (short)shift; 
                                logger.LogInformation($"EL3021 analog current in: {analogIn}");

                                // Read analog current underrange status (1 bit - PDO) 
                                void* dataUnder = varUnderrange.DataPtr.ToPointer();
                                bitmask = (1 << varUnderrange.BitLength) - 1;
                                int under = (*(int*)dataUnder >> varUnderrange.BitOffset) & bitmask;
                                logger.LogInformation($"EL3021 underrange: {under}");

                                // Read analog current overrange status (1 bit - PDO) 
                                void* dataOver = varOverrange.DataPtr.ToPointer();
                                bitmask = (1 << varOverrange.BitLength) - 1;
                                int over = (*(int*)dataOver >> varOverrange.BitOffset) & bitmask;
                                logger.LogInformation($"EL3021 overrange: {over}");

                                ################## Sample code END #################*/
                            }
                        }

                        Thread.Sleep(sleepTime);
                    }
                }, cts.Token);

                /* wait for stop signal */
                Console.ReadKey(true);

                cts.Cancel();
                await task;
            }
        }
    }
}
