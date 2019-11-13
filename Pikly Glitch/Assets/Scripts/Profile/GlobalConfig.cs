using UnityEngine;
using System.Collections.Generic;
using System.Xml;
using Pikl.Profile;

namespace Pikl {
    /// <summary>
    /// A front-end for the configuration file for the game.
    /// A lot of this will have to be game-specific i think.
    /// </summary>
	public class GlobalConfig {

#if UNITY_STANDALONE
        XmlDocument config;
        string filename;
#endif
#if (UNITY_WEBPLAYER || UNITY_WEBGL)
        Dictionary<string, string> config = new Dictionary<string, string>();
#endif

        public string LastPlayer {
            get {
#if UNITY_STANDALONE
                FileMgr.I.Decrypt(filename);
                string val = config.SelectSingleNode("/GlobalConfig/Game").Attributes["last-player"].InnerText;
                FileMgr.I.Encrypt(filename);
                return val;
#endif
#if (UNITY_WEBPLAYER || UNITY_WEBGL)
                return config["LastPlayer"];
#endif
            }
            set {
#if UNITY_STANDALONE
                FileMgr.I.Decrypt(filename);
                config.SelectSingleNode("/GlobalConfig/Game").Attributes["last-player"].InnerText = value;
                config.Save(filename);
                FileMgr.I.Encrypt(filename);
#endif
#if (UNITY_WEBPLAYER || UNITY_WEBGL)
                config["LastPlayer"] = value;
#endif
            }
        }

        string lastPlayer;

        public GlobalConfig(string filename) {
#if UNITY_STANDALONE
            this.filename = filename;
            config = new XmlDocument();
            FileMgr.I.Decrypt(filename);
            //try {
                config.Load(filename);
            //} catch {
                //Debug.HBDebug.LogWarning("Encrypted profiles when the encryption is turned off (Editor or Dev build)." + System.Environment.NewLine +
            //                             "Delete your profiles folder in 'AppData/LocalLow', or decrypt all files using the AESCrypt tool.");
           // }
            FileMgr.I.Encrypt(filename);
#endif
#if (UNITY_WEBPLAYER || UNITY_WEBGL)
            config.Add("LastPlayer", System.IO.Path.GetFileNameWithoutExtension(filename));
#endif
        }
	}
}