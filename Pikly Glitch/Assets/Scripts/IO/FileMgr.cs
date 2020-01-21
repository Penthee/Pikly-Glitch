using UnityEngine;
using System.IO;
using System.Xml;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Pikl.Profile {
    public class FileMgr : Singleton<FileMgr> {
        protected FileMgr() { }

        public const string profileVersion = "1.1.1";

        /// <summary>
        /// The global configuration file.
        /// </summary>
        public GlobalConfig config;
        #region Paths
        /// <summary>
        /// The Game Data folder within User/AppData/LocalLow/Hot Box Games/GameName/.
        /// </summary>
        public string gameDataPath {
            get;
            private set;
        }
        /// <summary>
        /// The global config file path within Game Data/Config.
        /// </summary>
        public string configPath {
            get;
            private set;
        }
        /// <summary>
        /// The folder for the profiles within Game Data.
        /// </summary>
        public string profilePath {
            get;
            private set;
        }
        /// <summary>
        /// The folder for the profile input files within Game Data.
        /// </summary>
        public string profileInputPath {
            get;
            private set;
        }
        ///// <summary>
        ///// The folder for the configuration file for the current profile.
        ///// </summary>
        //public string profileConfigPath;
        #endregion

        #region Backup Paths
        /// <summary>
        /// The Folder in which all backups are stored.
        /// </summary>
        public string backupPath {
            get;
            private set;
        }
        /// <summary>
        /// The backup global config file path within Game Data/Config.
        /// </summary>
        public string backupConfigPath {
            get;
            private set;
        }
        /// <summary>
        /// The backup folder for the profiles within Game Data.
        /// </summary>
        public string backupProfilePath {
            get;
            private set;
        }
        /// <summary>
        /// The backup folder for the profile input files within Game Data.
        /// </summary>
        public string backupProfileInputPath {
            get;
            private set;
        }
        ///// <summary>
        ///// The backup folder for the configuration file for the current profile.
        ///// </summary>
        //public string backupProfileConfigPath {
        //    get;
        //    private set;
        //}
        #endregion
        
        public override void Awake() {
            gameDataPath = Path.Combine(Application.persistentDataPath, (Debug.isDebugBuild ? "Development " : "") + "Game Data");

            profilePath = Path.Combine(gameDataPath, "Profiles");
            profileInputPath = Path.Combine(gameDataPath, "Input Config");
            configPath = Path.Combine(gameDataPath, "Config");
            //profileConfigPath = Path.Combine(configPath, "");

            backupPath = Path.Combine(gameDataPath, "Backups");
            backupProfilePath = Path.Combine(backupPath, "Profiles");
            backupProfileInputPath = Path.Combine(backupPath, "Input Config");
            backupConfigPath = Path.Combine(backupPath, "Config");
            //backupProfileConfigPath = Path.Combine(backupConfigPath, "");

#if UNITY_STANDALONE
            EnsureDirectories();

            EnsureReadme();
#endif
            OpenConfig();

            base.Awake();
        }

        void EnsureDirectories() {
            if (!Directory.Exists(gameDataPath))
                Directory.CreateDirectory(gameDataPath);

            if (!Directory.Exists(profilePath))
                Directory.CreateDirectory(profilePath);

            if (!Directory.Exists(configPath))
                Directory.CreateDirectory(configPath);

            if (!Directory.Exists(profileInputPath))
                Directory.CreateDirectory(profileInputPath);

            EnsureBackupDirectories();
        }

        void EnsureBackupDirectories() {

            if (!Directory.Exists(backupPath))
                Directory.CreateDirectory(backupPath);

            if (!Directory.Exists(backupProfilePath))
                Directory.CreateDirectory(backupProfilePath);

            if (!Directory.Exists(backupConfigPath))
                Directory.CreateDirectory(backupConfigPath);

            if (!Directory.Exists(backupProfileInputPath))
                Directory.CreateDirectory(backupProfileInputPath);
        }

        void EnsureReadme() {
            using (StreamWriter sw = new StreamWriter(File.Open(Path.Combine(gameDataPath, "README.txt"), FileMode.Create))) {
                sw.WriteLine("Everything in here is your save game data!");
                sw.WriteLine("Do not modify anything as it will break, and we can't help you if you do that.");
                sw.WriteLine("");
                sw.WriteLine("Backups are created when you close the game.");
                sw.WriteLine("If you're found to have been modifying the data, we will attempt to restore backups.");
                sw.WriteLine("If you break the backups, we really really can't help you out, it's on you.");
                sw.WriteLine("");
                sw.WriteLine("YOU HAVE BEEN WARNED!");
                sw.WriteLine("SRSLY.");
            }
        }

        void OpenConfig() {
#if UNITY_STANDALONE
            if (!File.Exists(Path.Combine(configPath, "Global.xml")))
                CreateNewGlobalConfig();
#endif
            config = new GlobalConfig(Path.Combine(configPath, "Global.xml"));
        }

        void OnApplicationQuit() {
#if UNITY_STANDALONE
            CreateBackups();
#endif
        }

        public FileStream CreateFile(string filename) {
            return File.Create(Path.Combine(gameDataPath, filename));
        }

        public void CreateNewProfile(string profileName) {
            string filename = Path.Combine(profilePath, profileName + ".xml");
            using (FileStream fs = new FileStream(filename, FileMode.Create)) {
                using (StreamWriter sw = new StreamWriter(fs)) {
                    using (XmlTextWriter xw = new XmlTextWriter(sw)) {
                        //xw.Settings.Indent = true;
                        xw.Formatting = Formatting.Indented;
                        xw.Indentation = 4;
                        xw.WriteStartDocument();

                        xw.WriteStartElement(profileName);
                        xw.WriteAttributeString("id", System.Guid.NewGuid().ToString());
                        xw.WriteAttributeString("version", profileVersion);
                        xw.WriteStartElement("Info");
                        xw.WriteAttributeString("deaths", "0");
                        xw.WriteAttributeString("highest-level", "1");
                        xw.WriteAttributeString("times-logged-in", "0");
                        xw.WriteEndElement();

                        //xw.WriteStartElement("Highscores");
                        //xw.WriteAttributeString("highscore", "0");
                        //xw.WriteAttributeString("hardcore-highscore", "0");
                        //xw.WriteAttributeString("endless-highscore", "0");
                        //xw.WriteAttributeString("endless-hardcore-highscore", "0");
                        //xw.WriteAttributeString("bossrush-highscore", "0");
                        //xw.WriteAttributeString("bossrush-hardcore-highscore", "0");
                        //xw.WriteEndElement();

                        xw.WriteStartElement("Settings");
                        xw.WriteStartElement("Controller");
                        xw.WriteAttributeString("config", "Default");
                        xw.WriteAttributeString("aim-assist", "true");
                        xw.WriteAttributeString("stick-smoothing", "true");

                        xw.WriteStartElement("Deadzones");
                        xw.WriteAttributeString("l-stick", "0.2");
                        xw.WriteAttributeString("r-stick", "0.2");
                        xw.WriteAttributeString("l-trigger", "0.2");
                        xw.WriteAttributeString("r-trigger", "0.2");
                        xw.WriteEndElement();
                        xw.WriteEndElement();
                        
                        xw.WriteStartElement("Audio");
                        xw.WriteAttributeString("master-vol", "1");
                        xw.WriteAttributeString("music-vol", "1");
                        xw.WriteAttributeString("sound-vol", "1");
                        //xw.WriteAttributeString("speech-vol", "1");
                        xw.WriteEndElement();

                        //xw.WriteStartElement("Quality");
                        //xw.WriteAttributeString("level", "5");
                        //xw.WriteEndElement();

                        xw.WriteStartElement("Resolution");
#if UNITY_EDITOR
                        xw.WriteAttributeString("width", "1024");
                        xw.WriteAttributeString("height", "768");
#else
                        xw.WriteAttributeString("width", Screen.width.ToString());
                        xw.WriteAttributeString("height", Screen.height.ToString());
#endif
                        xw.WriteAttributeString("fullscreen", "false");
                        xw.WriteEndElement();
                        xw.WriteEndElement();


                        xw.WriteEndElement();
                        xw.WriteEndDocument();
                        xw.Close();
                    }
                }
            }

            XmlDocument doc = new XmlDocument();
            doc.Load(filename);
            XmlElement element = doc.CreateElement("crc");
            element.SetAttribute("val", "");
            doc.DocumentElement.AppendChild(element);
            doc.Save(filename);

            Encrypt(filename);
        }

        public void CreateNewInputFile(string profileName) {
            string filename = Path.Combine(profileInputPath, profileName + ".xml");
            //InputMgr.Save(filename);

            XmlDocument doc = new XmlDocument();
            doc.Load(filename);
            XmlElement element = doc.CreateElement("crc");
            element.SetAttribute("val", "");
            doc.DocumentElement.AppendChild(element);
            doc.Save(filename);

            Encrypt(filename);
        }

//        public void CreateNewPlayerConfig(string profileName) {
//            string filename = Path.Combine(configPath, profileName + ".xml");

//            using (FileStream fs = new FileStream(filename, FileMode.Create)) {
//                using (StreamWriter sw = new StreamWriter(fs)) {
//                    using (XmlTextWriter xw = new XmlTextWriter(sw)) {
//                        xw.Settings.Indent = true;
//                        xw.Formatting = Formatting.Indented;
//                        xw.Indentation = 4;
//                        xw.WriteStartDocument();
//                        xw.WriteStartElement(profileName + "Config");

//                        xw.WriteStartElement("Audio");
//                        xw.WriteAttributeString("master-vol", "1");
//                        xw.WriteAttributeString("music-vol", "1");
//                        xw.WriteAttributeString("sfx-vol", "1");
//                        xw.WriteAttributeString("speech-vol", "1");
//                        xw.WriteEndElement();

//                        xw.WriteStartElement("Quality");
//                        xw.WriteAttributeString("level", "5");
//                        xw.WriteEndElement();

//                        xw.WriteStartElement("Resolution");
//#if UNITY_EDITOR
//                        xw.WriteAttributeString("width", "1024");
//                        xw.WriteAttributeString("height", "768");
//#else
//                        xw.WriteAttributeString("width", Screen.width.ToString());
//                        xw.WriteAttributeString("height", Screen.height.ToString());
//#endif
//                        xw.WriteAttributeString("fullscreen", "true");
//                        xw.WriteEndElement();

//                        xw.WriteStartElement("Game");
//                        xw.WriteAttributeString("subtitles", "true");
//                        xw.WriteEndElement();

//                        xw.WriteEndElement();
//                        xw.WriteEndDocument();
//                        xw.Close();
//                    }
//                }
//            }

//            XmlDocument doc = new XmlDocument();
//            doc.Load(filename);
//            XmlElement element = doc.CreateElement("crc");
//            element.SetAttribute("val", "");
//            doc.DocumentElement.AppendChild(element);
//            doc.Save(filename);

//            Encrypt(filename);
//        }

        public void CreateNewGlobalConfig() {
            string filename = Path.Combine(configPath, "Global.xml");

            using (FileStream fs = new FileStream(filename, FileMode.Create)) {
                using (StreamWriter sw = new StreamWriter(fs)) {
                    using (XmlTextWriter xw = new XmlTextWriter(sw)) {
                        
                        //xw.Settings.Indent = true;
                        xw.Formatting = Formatting.Indented;
                        xw.Indentation = 4;
                        xw.WriteStartDocument();
                        xw.WriteStartElement("GlobalConfig");

                        xw.WriteStartElement("Game");
                        xw.WriteAttributeString("last-player", "");
                        xw.WriteEndElement();

                        xw.WriteEndElement();
                        xw.WriteEndDocument();
                        xw.Close();
                    }
                }
            }

            XmlDocument doc = new XmlDocument();
            doc.Load(filename);
            XmlElement element = doc.CreateElement("crc");
            element.SetAttribute("val", "");
            doc.DocumentElement.AppendChild(element);
            doc.Save(filename);

            Encrypt(filename);
        }

        /// <summary>
        /// Copies the 3 profile files for the current user, <para/>
        /// The global config and lookups into the backups folder. <para/>
        /// Only the one set of the backups are stored at any one time.
        /// </summary>
        public void CreateBackups() {
            if (ProfileMgr.I == null || ProfileMgr.I.profile == null)
                return;

            for (int i = 0; i < 3; i++) {
                string tempBackupPath = "", tempPath = "";

                switch (i) {
                    case 0: tempBackupPath = Path.Combine(backupProfilePath, ProfileMgr.I.profile.Name + ".xml"); break;
                    //case 1: tempBackupPath = Path.Combine(backupProfileConfigPath, ProfileMgr.I.profile.Name + ".xml"); break;
                    case 1: tempBackupPath = Path.Combine(backupProfileInputPath, ProfileMgr.I.profile.Name + ".xml"); break;
                    case 2: tempBackupPath = Path.Combine(backupConfigPath, "Global.xml"); break;
                }

                if (File.Exists(tempBackupPath)) {
                    File.Delete(tempBackupPath);
                }

                switch (i) {
                    case 0: tempPath = Path.Combine(profilePath, ProfileMgr.I.profile.Name + ".xml"); break;
                    //case 1: tempPath = Path.Combine(profileConfigPath, ProfileMgr.I.profile.Name + ".xml"); break;
                    case 1: tempPath = Path.Combine(profileInputPath, ProfileMgr.I.profile.Name + ".xml"); break;
                    case 2: tempPath = Path.Combine(configPath, "Global.xml"); break;
                }

                File.Copy(tempPath, tempBackupPath);

            }
        }

        public void RestoreBackups() {
            for (int i = 0; i < 3; i++) {
                string tempBackupPath = "", tempPath = "";

                switch (i) {
                    case 0: tempPath = Path.Combine(profilePath, ProfileMgr.I.profile.Name + ".xml"); break;
                    //case 1: tempPath = Path.Combine(profileConfigPath, ProfileMgr.I.profile.Name + ".xml"); break;
                    case 1: tempPath = Path.Combine(profileInputPath, ProfileMgr.I.profile.Name + ".xml"); break;
                    case 2: tempPath = Path.Combine(configPath, "Global.xml"); break;
                }

                if (File.Exists(tempPath)) {
                    File.Delete(tempPath);
                }

                switch (i) {
                    case 0: tempBackupPath = Path.Combine(backupProfilePath, ProfileMgr.I.profile.Name + ".xml"); break;
                    //case 1: tempBackupPath = Path.Combine(backupProfileConfigPath, ProfileMgr.I.profile.Name + ".xml"); break;
                    case 1: tempBackupPath = Path.Combine(backupProfileInputPath, ProfileMgr.I.profile.Name + ".xml"); break;
                    case 2: tempBackupPath = Path.Combine(backupConfigPath, "Global.xml"); break;
                }

                File.Copy(tempBackupPath, tempPath);
            }
        }

        /// <summary>
        /// Deletes the 2 profile files - <para/>
        /// Profile Input,<para/>
        /// Profile.
        /// </summary>
        /// <param name="profileName"></param>
        public void DeleteProfileFiles(string profileName) {
            string filename = Path.Combine(configPath, profileName + ".xml");
            File.Delete(filename);
            filename = Path.Combine(profilePath, profileName + ".xml");
            File.Delete(filename);
            filename = Path.Combine(profileInputPath, profileName + ".xml");
            File.Delete(filename);
        }
#if UNITY_STANDALONE
        /// <summary>
        /// Gets an array of the created profiles within the profile folder.
        /// </summary>
        public string[] GetProfiles() {
            List<string> profiles = new List<string>();

            foreach (string file in Directory.GetFiles(profilePath))
                profiles.Add(Path.GetFileNameWithoutExtension(file));

            return profiles.ToArray();
        }
#endif
        public string Encrypt(string filename) {
            string encrypted = "";

            if (!Debug.isDebugBuild) {
                using (StreamReader fs = new StreamReader(File.Open(filename, FileMode.Open)))
                    encrypted = fs.ReadToEnd();

                encrypted = encrypted.Replace("  ", "");
                encrypted = Regex.Replace(encrypted, @"\t|\n|\r", "");
                encrypted = Regex.Replace(encrypted, "crc val=\".*\"", "crc val=\"\"");
                //Debug.HBDebug.Log(encrypted + System.Environment.NewLine + Utils.Hash.Md5Sum(encrypted));

                XmlDocument doc = new XmlDocument();
                doc.Load(filename);
                doc.SelectSingleNode("/*/crc").Attributes["val"].InnerText = Utils.Hash.Md5Sum(encrypted);
                doc.Save(filename);

                using (StreamReader fs = new StreamReader(File.Open(filename, FileMode.Open)))
                    encrypted = Utils.AesEncryptor.Encrypt(fs.ReadToEnd());

                using (StreamWriter fs = new StreamWriter(File.Open(filename, FileMode.Create)))
                    fs.Write(encrypted);
            }

            return encrypted;
        }

        public string Decrypt(string filename) {
            string decrypted = "", temp = "";

            if (!Debug.isDebugBuild) {
                try {
                    using (StreamReader fs = new StreamReader(File.Open(filename, FileMode.Open)))
                        temp = Utils.AesEncryptor.Decrypt(fs.ReadToEnd());
                } catch {
                    UnityEngine.Debug.LogWarning("Decrypted profiles when the encryption is turned on (Public build)." + System.Environment.NewLine +
                                                 "Delete your profiles folder in 'AppData/LocalLow', or encrypt all files using the AESCrypt tool.");
                }

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(temp);
                string hash = doc.SelectSingleNode("/*/crc").Attributes["val"].InnerText;
                doc.SelectSingleNode("/*/crc").Attributes["val"].InnerText = "";

                temp = Regex.Replace(doc.InnerXml, @"\t|\n|\r", "");

                //Debug.HBDebug.Log(temp + System.Environment.NewLine + Utils.Hash.Md5Sum(temp));
                //Debug.HBDebug.Log("hash = " + hash);

                if (hash != Utils.Hash.Md5Sum(temp)) {
                    Debug.Log($"FILE HAS BEEN TAMPERED WITH!{System.Environment.NewLine}{temp}");

                    Debug.Log("Restoring Backups...");
                    RestoreBackups();
                    //TODO - Have a confirmation dialog
                    return "";
                }
                
                using (StreamReader fs = new StreamReader(File.Open(filename, FileMode.Open)))
                    decrypted = Utils.AesEncryptor.Decrypt(fs.ReadToEnd());

                using (StreamWriter fs = new StreamWriter(File.Open(filename, FileMode.Create)))
                    fs.Write(decrypted);

                return decrypted;

            } else {
                return File.ReadAllText(filename);
            }


        }
    }
}