﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IDZ_SERVER.Controllers.Server;
using IDZ_SERVER.Controllers.Parser;

namespace IDZ_SERVER
{
    class Program
    {
        static void Main(string[] args)
        {
            ServerController serverController = new ServerController();
            Task.Run(serverController.AwaitRequestForReadData);
            Task.Run(serverController.AwaitRequestForDeleteData);
            Task.Run(serverController.AwaitRequestForEditData);
            Task.Run(serverController.AwaitRequestForUpdateData);
            Task.Run(serverController.AwaitRequestForGetArmorDefence);
            Console.WriteLine("UDP сервер запущен...");
            Console.ReadLine();
        }
    }
}
