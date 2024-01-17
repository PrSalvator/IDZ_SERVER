using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IDZ_SERVER.DataBase.Entities;
using System.Data.Entity.Validation;

namespace IDZ_SERVER.Controllers.Parser
{
    public class ParserController
    {
        private string forgeableArmorUrl;
        private string unforgableArmorUrl;
        public ParserController()
        {
            forgeableArmorUrl = ConfigurationManager.AppSettings.Get("ForgeableArmorUrl");
            unforgableArmorUrl = ConfigurationManager.AppSettings.Get("UnforgeableArmorUrl");
        }
        public void GetArmor()
        {

        }
        public void AddDateInDataBase()
        {
            HtmlWeb htmlWeb = new HtmlWeb();
            HtmlDocument htmlDoc = new HtmlDocument();
            try
            {
                htmlDoc = htmlWeb.Load(forgeableArmorUrl);
                UpdateDataBase(htmlDoc);
                htmlDoc = htmlWeb.Load(unforgableArmorUrl);
                UpdateDataBase(htmlDoc);
            }
            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
                    {
                        Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage);
                    }
                }
                throw;
            }
        }
        private void UpdateDataBase(HtmlDocument document)
        {
            HtmlNodeCollection armorTypes = document.DocumentNode.SelectNodes("//div[@id='toc']/ul/li/a/span[@class='toctext']");
            using (DataBaseContext db = new DataBaseContext())
            {
                ARMOR_TYPE dbArmorType = null;
                ARMOR dbArmor = null;

                foreach (HtmlNode armorType in armorTypes)
                {
                    string typeName = armorType.InnerText;

                    if (!db.ARMOR_TYPE.Any(p => p.NAME == typeName))
                    {
                        db.ARMOR_TYPE.Add(new ARMOR_TYPE() { NAME = typeName });
                        db.SaveChanges();
                    }
                    dbArmorType = db.ARMOR_TYPE.Where(p => p.NAME == typeName).First();

                    HtmlNodeCollection armors = document.DocumentNode.SelectNodes($"//div[@id='toc']/ul/li/a/span[@class='toctext'][text()='{typeName}']/ancestor::li/ul/li/a/span[@class='toctext']");
                    if(armors != null)
                    {
                        foreach (HtmlNode armor in armors)
                        {
                            string armorName = armor.InnerText;

                            if (!db.ARMOR.Any(p => p.NAME == armorName))
                            {
                                db.ARMOR.Add(new ARMOR() { NAME = armorName, ARMOR_TYPE_ID = dbArmorType.ID });
                                db.SaveChanges();
                            }
                            dbArmor = db.ARMOR.Where(p => p.NAME == armorName).First();

                            List<HtmlNode> elements = document.DocumentNode.SelectNodes($"//span[@class='mw-headline']/*[text()='{armorName}']/following::table")?[0].SelectNodes("./tbody/tr").ToList();
                            if(elements == null)
                            {
                                elements = document.DocumentNode.SelectNodes($"//span[@class='mw-headline'][@id='{armorName.Replace(' ', '_')}']/following::table")?[0].SelectNodes("./tbody/tr").ToList();
                            }
                            if(elements != null)
                            {
                                elements.RemoveAt(0);
                                foreach (HtmlNode element in elements)
                                {
                                    string imgUrl = element.SelectNodes("./td")[0].SelectSingleNode("./a")?.Attributes["href"].DeEntitizeValue;
                                    string elementName = element.SelectNodes("./td")[1].FirstChild.InnerText;
                                    string devId = element.SelectNodes("./td")[2].InnerText.Trim().Substring(0, 8);
                                    int defence = int.Parse(element.SelectNodes("./td")[3].InnerText.Replace("\n", ""));
                                    int price = priceConverter(element.SelectNodes("./td")[4].InnerText.Replace("\n", ""));
                                    double weight = double.Parse(element.SelectNodes("./td")[5].InnerText.Replace("\n", "").Replace(".", ","));
                                    string part = armorPartConverter(elementName);
                                    if(!db.ELEMENT_OF_ARMOR.Any(p => p.DEV_ID == devId))
                                    {
                                        db.ELEMENT_OF_ARMOR.Add(new ELEMENT_OF_ARMOR()
                                        {
                                            DEV_ID = devId,
                                            NAME = elementName,
                                            IMG_URL = imgUrl,
                                            DEFENCE = defence,
                                            PRICE = price,
                                            WEIGHT = weight,
                                            PART_ID = db.PART_OF_ARMOR.Where(p => p.NAME == part).Select(p => p.ID).First(),
                                            ARMOR_ID = dbArmor.ID
                                    });
                                    }
                                }
                            }
                        }
                    }
                }
                db.SaveChanges();
            }

        }
        private int priceConverter(string price)
        {
            if (price.Contains("–"))
            {
                string[] splitPrice = price.Split('–');
                int a = int.Parse(splitPrice[0]);
                int b = int.Parse(splitPrice[1]);
                int result = (a + b)/2;
                return result;
            }
            return int.Parse(price);
        }
        private string armorPartConverter(string elementName)
        {
            elementName = elementName.ToLower(); //  else ne nuzhen
            if (elementName.Contains("броня") || elementName.Contains("шкура")) return "броня";
            else if (elementName.Contains("сапоги") || elementName.Contains("ботинки")) return "обувь";
            else if (elementName.Contains("перчатки") || elementName.Contains("наручи")) return "перчатки";
            else if (elementName.Contains("капюшон") || elementName.Contains("шлем") || elementName.Contains("головной убор")) return "головной убор";
            else if (elementName.Contains("щит")) return "щит";
            return "артефакт";
        }
    }
}
