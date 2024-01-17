using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IDZ_SERVER.DataBase.Entities;

namespace IDZ_SERVER.Controllers.Data
{
    public class DataController
    {
        public List<DataBase.ViewModels.ArmorDefence> GetArmorDefenceList()
        {
            List <DataBase.ViewModels.ArmorDefence > list = new List<DataBase.ViewModels.ArmorDefence> ();
            using (DataBaseContext db = new DataBaseContext())
            {
                foreach (DataBase.Entities.ARMOR_DEFENCE_VIEW armorDefence in db.ARMOR_DEFENCE_VIEW.ToList())
                {
                    list.Add(new DataBase.ViewModels.ArmorDefence
                    {
                        ArmorName = armorDefence.NAME,
                        Defence = armorDefence.DEFENCE,
                        ArmorType = db.ARMOR_TYPE.Where(p => p.ID == armorDefence.ARMOR_TYPE_ID).First().NAME
                    });
                }
            }
            return list;
        }
        public List<ELEMENT_OF_ARMOR> GetElementsOfArmors()
        {
            List<ELEMENT_OF_ARMOR> list;
            using (DataBaseContext db = new DataBaseContext())
            {
                list = db.ELEMENT_OF_ARMOR.ToList();
            }
            return list;
        }
        public void DeleteElementOfArmor(ELEMENT_OF_ARMOR element)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                ELEMENT_OF_ARMOR e = db.ELEMENT_OF_ARMOR.Where(p => p.ID == element.ID).First();
                db.Entry(e).State = System.Data.Entity.EntityState.Deleted;
                db.SaveChanges();
            }
        }
        public void UpdateElementOfArmor(ELEMENT_OF_ARMOR element)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                ELEMENT_OF_ARMOR e = db.ELEMENT_OF_ARMOR.Where(p => p.ID == element.ID).First();
                e.NAME = element.NAME;
                e.PRICE = element.PRICE;
                e.WEIGHT = element.WEIGHT;
                e.DEFENCE = element.DEFENCE;
                db.Entry(e).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
            }
        }
    }
}
