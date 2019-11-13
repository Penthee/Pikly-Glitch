using System;
using System.Xml;
using UnityEngine;

namespace Pikl.Profile {
    public class XmlAttribute<T> {
        public delegate void OnChange(T val);

        XmlDocument doc;
        public readonly string xpath, attributeName, filename;
        readonly T defaultValue;
        T value;
        
        OnChange onChange;

        public T Value
        {
            get
            {
                //Debug.HBDebug.Log("Filename : " + filename + ", Xpath : " + xpath);
                string textVal = doc.SelectSingleNode(xpath).Attributes[attributeName].InnerText;
                try {
                    value = (T)Convert.ChangeType(textVal, typeof(T));
                } catch (Exception e) {
                    Debug.Log(string.Format("{0}\n'{1}' setting was probably corrupt ({2}), using default value '{3}'...", 
                                      e.Message, attributeName, textVal, defaultValue));
                    Value = defaultValue;
                }

                return value;
            }
            set
            {
                this.value = value;
                
                doc.LoadXml(FileMgr.I.Decrypt(filename));
                doc.SelectSingleNode(xpath).Attributes[attributeName].InnerText = value.ToString();
                doc.Save(filename);
                FileMgr.I.Encrypt(filename);
                
                if (onChange != null)
                    onChange(value);
            }
        }



        public XmlAttribute(string xpath, string attributeName, string filename, T defaultValue, OnChange onChange) {
            this.xpath = xpath;
            this.attributeName = attributeName;
            this.defaultValue = defaultValue;
            this.filename = filename;
            this.onChange = onChange;

            doc = new XmlDocument();
            doc.LoadXml(FileMgr.I.Decrypt(filename));
            FileMgr.I.Encrypt(filename);
        }

    }
}
