using UnityEngine;
using System.Collections.Generic;
using System.Xml;
//using Pikl.Audio;

namespace Pikl.Profile {
    /// <summary>
    /// A front-end for the configuration file for the game.
    /// A lot of this will have to be game-specific i think.
    /// </summary>
	public class PlayerConfig {

        //public float MasterVol {
        //    get {
        //        FileMgr.I.Decrypt(filename);
        //        float val = System.Convert.ToSingle(config.SelectSingleNode("/" + playerName + "Config/Audio")
        //                                                 .Attributes["master-vol"].InnerText);
        //        FileMgr.I.Encrypt(filename);
        //        return val;
        //    }
        //    set {
        //        AudioMgr.I.SetMasterLevel(value);
        //        FileMgr.I.Decrypt(filename);
        //        config.SelectSingleNode("/" + playerName + "Config/Audio")
        //              .Attributes["master-vol"].InnerText = value.ToString();
        //        config.Save(filename);
        //        FileMgr.I.Encrypt(filename);
        //    }
        //}

        //public float MusicVol {
        //    get {
        //        FileMgr.I.Decrypt(filename);
        //        float val = System.Convert.ToSingle(config.SelectSingleNode("/" + playerName + "Config/Audio")
        //                                                 .Attributes["music-vol"].InnerText);
        //        FileMgr.I.Encrypt(filename);
        //        return val;
        //    }
        //    set {
        //        AudioMgr.I.SetMusicLevel(value);
        //        FileMgr.I.Decrypt(filename);
        //        config.SelectSingleNode("/" + playerName + "Config/Audio")
        //              .Attributes["music-vol"].InnerText = value.ToString();
        //        config.Save(filename);
        //        FileMgr.I.Encrypt(filename);
        //    }
        //}

        //public float SoundVol {
        //    get {
        //        FileMgr.I.Decrypt(filename);
        //        float val =  System.Convert.ToSingle(config.SelectSingleNode("/" + playerName + "Config/Audio")
        //                                                   .Attributes["sfx-vol"].InnerText);
        //        FileMgr.I.Encrypt(filename);
        //        return val;
        //    }
        //    set {
        //        AudioMgr.I.SetSfxLevel(value);
        //        FileMgr.I.Decrypt(filename);
        //        config.SelectSingleNode("/" + playerName + "Config/Audio")
        //              .Attributes["sfx-vol"].InnerText = value.ToString();
        //        config.Save(filename);
        //        FileMgr.I.Encrypt(filename);
        //    }
        //}

        //public float SpeechVol {
        //    get {
        //        FileMgr.I.Decrypt(filename);
        //        float val =  System.Convert.ToSingle(config.SelectSingleNode("/" + playerName + "Config/Audio")
        //                                                   .Attributes["speech-vol"].InnerText);
        //        FileMgr.I.Encrypt(filename);
        //        return val;
        //    }
        //    set {
        //        AudioMgr.I.SetSpeechLevel(value);
        //        FileMgr.I.Decrypt(filename);
        //        config.SelectSingleNode("/" + playerName + "Config/Audio")
        //              .Attributes["speech-vol"].InnerText = value.ToString();
        //        config.Save(filename);
        //        FileMgr.I.Encrypt(filename);
        //    }
        //}

        public struct Res {
            public int width, height;
            public bool fullscreen;

            public Res(int width, int height, bool fullscreen) {
                this.width = width;
                this.height = height;
                this.fullscreen = fullscreen;
            }
        }

        public Res Resolution {
            get {
                FileMgr.I.Decrypt(filename);
                XmlNode resNode = config.SelectSingleNode("/" + playerName + "Config/Resolution");
                int width =  System.Convert.ToInt32(resNode.Attributes["width"].InnerText);
                int height =  System.Convert.ToInt32(resNode.Attributes["height"].InnerText);
                bool fullscreen =  System.Convert.ToBoolean(resNode.Attributes["fullscreen"].InnerText);
                FileMgr.I.Encrypt(filename);

                return new Res(width, height, fullscreen);
            }
            set {
                Screen.SetResolution(value.width, value.height, value.fullscreen);
                FileMgr.I.Decrypt(filename);
                XmlNode resNode = config.SelectSingleNode("/" + playerName + "Config/Resolution");
                resNode.Attributes["width"].InnerText = value.width.ToString();
                resNode.Attributes["height"].InnerText = value.height.ToString();
                resNode.Attributes["fullscreen"].InnerText = value.fullscreen.ToString();
                config.Save(filename);
                FileMgr.I.Encrypt(filename);
            }
        }

        public bool Fullscreen {
            get {
                FileMgr.I.Decrypt(filename);
                XmlNode resNode = config.SelectSingleNode("/" + playerName + "Config/Resolution");
                bool fs = System.Convert.ToBoolean(resNode.Attributes["fullscreen"].InnerText);
                FileMgr.I.Encrypt(filename);
                Screen.SetResolution(Resolution.width, Resolution.height, fs);
                return fs;
            }
            set {
                FileMgr.I.Decrypt(filename);
                XmlNode resNode = config.SelectSingleNode("/" + playerName + "Config/Resolution");
                resNode.Attributes["fullscreen"].InnerText = value.ToString();
                config.Save(filename);
                FileMgr.I.Encrypt(filename);
            }
        }

        public int Quality {
            get {
                FileMgr.I.Decrypt(filename);
                XmlNode qualityNode = config.SelectSingleNode("/" + playerName + "Config/Quality");
                int level = System.Convert.ToInt32(qualityNode.Attributes["level"].InnerText);
                QualitySettings.SetQualityLevel(level, true);
                FileMgr.I.Encrypt(filename);
                return level;
            }
            set {
                QualitySettings.SetQualityLevel(value);
                FileMgr.I.Decrypt(filename);
                XmlNode qualityNode = config.SelectSingleNode("/" + playerName + "Config/Quality");
                qualityNode.Attributes["level"].Value = value.ToString();
                config.Save(filename);
                FileMgr.I.Encrypt(filename);
            }
        }
        
        public XmlDocument config;
        string filename;
#pragma warning disable 0414
        string playerName;//, resolution;
        //int musicVol, soundVol, qualitySetting;

        public PlayerConfig(string filename, string playerName) {
            this.playerName = playerName;
            this.filename = filename;
            config = new XmlDocument();
            FileMgr.I.Decrypt(filename);
            config.Load(filename);
            FileMgr.I.Encrypt(filename);
        }

    }
}