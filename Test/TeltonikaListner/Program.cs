﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using SmartFleet.Core.Contracts.Commands;
using SmartFleet.Core.Infrastructure.MassTransit;
using SmartFleet.Core.Protocols;

namespace TeltonikaListner
{
    class Program
    {
        private static IBusControl _bus;
       
        private static Task<ISendEndpoint> _endpoint;

        static void Main(string[] args)
        {
            TcpListener listener = new TcpListener(IPAddress.Any, 34400);
            _bus = MassTransitConfig.ConfigureSenderBus();
            //using of masstransit Bus / rabbitMq: for queuing data received from GPS devices (current account supports only 20  concurrent connections)
            _endpoint = _bus.GetSendEndpoint(new Uri(
                "rabbitmq://zcckffbw:QKVVIKHQgsx_QQ8qbxeb1Dl-E9jsKlSJ@eagle.rmq.cloudamqp.com/zcckffbw/Teltonika.endpoint"));
            listener.Start();

            while (true) // Add your exit flag here
            {
                var client = listener.AcceptTcpClient();
                ThreadPool.QueueUserWorkItem(ThreadProc, client);
            }

        }

        private static async void ThreadProc(object state)
        {
            string imei = string.Empty;
            var client = ((TcpClient)state);
            NetworkStream nwStream = ((TcpClient)state).GetStream();
            byte[] buffer = new byte[client.ReceiveBufferSize];

            try
            {
                 var gpsResult = new List<CreateTeltonikaGps>();
                while (true)
                {
                    int bytesRead = nwStream.Read(buffer, 0, client.ReceiveBufferSize) - 2;

                    string dataReceived = Encoding.ASCII.GetString(buffer, 2, bytesRead);
                    if (imei == string.Empty)
                    {
                        imei = dataReceived;
                        Console.WriteLine("IMEI received : " + dataReceived);

                        Byte[] b = {0x01};
                        nwStream.Write(b, 0, 1);
                        var command = new CreateBoxCommand();
                        command.Imei = imei;
                        await _endpoint.Result.Send(command);

                    }
                    else
                    {
                        int dataNumber = Convert.ToInt32(buffer.Skip(9).Take(1).ToList()[0]);
                        ;

                        while (dataNumber > 0)
                        {
                            var parser = new DevicesParser();
                            gpsResult.AddRange(parser.Decode(new List<byte>(buffer), imei));
                            dataNumber--;

                        }
                        nwStream.Write(new byte[] {0x00, 0x00, 0x00, 0x01}, 0, 4);

                    }
                    if (gpsResult.Count > 0)
                        foreach (var gpSdata in gpsResult)
                            await _endpoint.Result.Send(gpSdata);
                }






            }
            catch (Exception e)
            {
                // Console.WriteLine(e);
                client.Close();
                //throw;
            }

            //throw new NotImplementedException();
        }
    }
}