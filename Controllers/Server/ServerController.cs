using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using IDZ_SERVER.Controllers.Data;
using IDZ_SERVER.DataBase.Entities;

namespace IDZ_SERVER.Controllers.Server
{
    public class ServerController
    {
        private IPAddress localAddress = IPAddress.Parse(ConfigurationManager.AppSettings.Get("LocalAddress"));

        private int localPortRead = int.Parse(ConfigurationManager.AppSettings.Get("LocalPortRead")); // для вывода данных
        private int localPortEdit = int.Parse(ConfigurationManager.AppSettings.Get("LocalPortEdit")); // для добавления данных
        private int localPortDelete = int.Parse(ConfigurationManager.AppSettings.Get("LocalPortDelete"));
        private int localPortUpdate = int.Parse(ConfigurationManager.AppSettings.Get("LocalPortUpdate"));
        private int localPortArmorDefence = int.Parse(ConfigurationManager.AppSettings.Get("LocalPortArmorDefence"));
        private int remotePort = int.Parse(ConfigurationManager.AppSettings.Get("RemotePort"));

        private DataController dataController = new DataController();
        private string GetArmorDefenceList()
        {
            List<DataBase.ViewModels.ArmorDefence> list = dataController.GetArmorDefenceList();
            string armorDefence = JsonSerializer.Serialize(list);
            return armorDefence;
        }
        private string GetElementsOfArmors()
        {
            List<ELEMENT_OF_ARMOR> list = dataController.GetElementsOfArmors();
            string armors = JsonSerializer.Serialize(list);
            return armors;
        }
        public async Task AwaitRequestForGetArmorDefence()
        {
            using (UdpClient receiver = new UdpClient(localPortArmorDefence))
            {
                while (true)
                {
                    // получаем данные
                    var result = await receiver.ReceiveAsync();
                    string armorsDefence = GetArmorDefenceList();
                    await SendMessageAsync(armorsDefence);
                }
            }
        }
        public async Task AwaitRequestForReadData()
        {
            using (UdpClient receiver = new UdpClient(localPortRead))
            {
                while (true)
                {
                    // получаем данные
                    var result = await receiver.ReceiveAsync();
                    string armors = GetElementsOfArmors();
                    await SendMessageAsync(armors);
                }
            }
        }
        public async Task AwaitRequestForDeleteData()
        {
            using (UdpClient receiver = new UdpClient(localPortDelete))
            {
                while (true)
                {
                    // получаем данные
                    var result = await receiver.ReceiveAsync();
                    var message = Encoding.UTF8.GetString(result.Buffer);
                    try
                    {
                        ELEMENT_OF_ARMOR elementOfArmor = JsonSerializer.Deserialize<ELEMENT_OF_ARMOR>(message);
                        dataController.DeleteElementOfArmor(elementOfArmor);
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }
        public async Task AwaitRequestForUpdateData()
        {
            using (UdpClient receiver = new UdpClient(localPortUpdate))
            {
                while (true)
                {
                    // получаем данные
                    var result = await receiver.ReceiveAsync();
                    try
                    {
                        Parser.ParserController parserController = new Parser.ParserController();
                        parserController.AddDateInDataBase();
                        await SendMessageAsync(GetElementsOfArmors());
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }

                }
            }
        }
        public async Task AwaitRequestForEditData()
        {
            using (UdpClient receiver = new UdpClient(localPortEdit))
            {
                while (true)
                {
                    // получаем данные
                    var result = await receiver.ReceiveAsync();
                    var message = Encoding.UTF8.GetString(result.Buffer);
                    try
                    {
                        ELEMENT_OF_ARMOR elementOfArmor = JsonSerializer.Deserialize<ELEMENT_OF_ARMOR>(message);
                        dataController.UpdateElementOfArmor(elementOfArmor);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }

                }
            }
        }
        private async Task SendMessageAsync(string message)
        {
            using (UdpClient sender = new UdpClient())
            {
                byte[] data = Encoding.UTF8.GetBytes(message);
                int a = await sender.SendAsync(data, data.Length, new IPEndPoint(localAddress, remotePort));
            }
        }
    }
}
