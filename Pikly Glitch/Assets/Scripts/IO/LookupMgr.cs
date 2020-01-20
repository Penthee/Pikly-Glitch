using UnityEngine;
using System.IO;
using System.Xml;
using System.Linq;
using System.Collections.Generic;

namespace Pikl {
    public class LookupMgr : Singleton<LookupMgr> {
        #region Fields & Initialisation
        protected LookupMgr() { }

        public class ControllerConfigs {
            public Dictionary<string, List<Mapping>> configs = new Dictionary<string, List<Mapping>>();

            public List<Mapping> this[string name] {
                get {
                    return configs[name];
                }
            }

            public string this[int index] {
                get {
                    return configs.Keys.ElementAt(index);
                }
            }
            
            public ControllerConfigs(string xml) {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xml);
                foreach(XmlNode config in doc.SelectNodes("ControllerConfigs/Config")) {
                    List<Mapping> mappings = new List<Mapping>();
                    string name = config.Attributes["name"].InnerText;
                    configs.Add(name, mappings);

                    foreach(XmlNode mapping in config) {
                        string mappingName = mapping.Attributes["name"].InnerText;
                        Mapping.Type type = (Mapping.Type)System.Enum.Parse(typeof(Mapping.Type), mapping.Attributes["type"].InnerText);
                        string axis = "", altAxis = "";

                        switch(type) {
                            case Mapping.Type.Button:
                            case Mapping.Type.Trigger:
                                axis = mapping.Attributes["axis"].InnerText;
                                break;
                            case Mapping.Type.Stick:
                                axis = mapping.Attributes["h-axis"].InnerText;
                                altAxis = mapping.Attributes["v-axis"].InnerText;
                                break;
                        }

                        mappings.Add(new Mapping(mappingName, type, axis, altAxis));
                    }
                }
            }
            
            public Mapping FindMapping(string controlName) {
                foreach(var config in configs) {
                    if (config.Key != Profile.ProfileMgr.I.profile.controllerConfig.Value)
                        continue;
                    foreach(var mapping in config.Value) {
                        if (mapping.name == controlName)
                            return mapping;
                    }
                }

                return null;
            }

            public class Mapping {
                public enum Type { Stick, Button, Trigger };
                public readonly string name, axis, altAxis;
                public readonly Type type;

                public Mapping(string name, Type type, string axis, string altAxis = "") {
                    this.name = name;
                    this.type = type;
                    this.axis = axis;
                    this.altAxis = altAxis;
                }
            }
        }

        //public class ItemDB {

        //    Dictionary<string, Item> items = new Dictionary<string, Item>();
        //    public bool isLoading;

        //    public ItemDB(string xml) {
        //        isLoading = true;

        //        XmlDocument doc = new XmlDocument();
        //        doc.LoadXml(xml);
        //        int i = 0;
        //        foreach(XmlNode n in doc.SelectNodes("ItemDB/Item")) {
        //            Item.ItemType type = (Item.ItemType)System.Enum.Parse(typeof(Item.ItemType), n.Attributes["type"].InnerText);


        //            Item item = new Item(n.Attributes["name"].InnerText);

        //            switch (type) {
        //                case Item.ItemType.Weapon:
        //                    XmlNode n2 = n.SelectSingleNode("Weapon");
        //                    item = new Weapon(n.Attributes["name"].InnerText);
        //                    (item as Weapon).clipSize = System.Convert.ToInt32(n2.Attributes["clip-size"].InnerText);
        //                    (item as Weapon).reloadSpeed = System.Convert.ToSingle(n2.Attributes["reload-speed"].InnerText);
        //                    //(item as Weapon).shootObj = System.Convert.ToInt32(n2.Attributes["shoot-obj"].InnerText);
        //                    break;
        //            }

        //            item.type = type;
        //            item.description = n.Attributes["description"].InnerText;
        //            item.sprite = Resources.Load<Sprite>(n.Attributes["sprite"].InnerText);

        //            items.Add(item.name, item);
        //        }

        //        isLoading = false;
        //    }

        //    public Item Find(string name) {
        //        if (isLoading || !items.ContainsKey(name))
        //            return null;

        //        return items[name];
        //    }
        //}

        public ControllerConfigs controllerConfigs;
        //public ItemDB itemDB;

        public override void Awake() {
            base.Awake();
            LoadLookups();
        }

        void LoadLookups() {
            controllerConfigs = new ControllerConfigs(Resources.Load<TextAsset>("Lookups/ControllerConfigs").text);
            //itemDB = new ItemDB(Resources.Load<TextAsset>("Lookups/ItemDB").text);
        }
        #endregion
    }
}