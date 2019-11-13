using UnityEngine;
using System.Collections;
using System.Xml;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using TeamUtility.IO;
//using Pikl.Audio;

namespace Pikl.Profile {
	public class PlayerProfile {
        
//        public string ID {
//            get {
//                FileMgr.I.Decrypt(filename);
//                string val = profile.SelectSingleNode("/" + Name).Attributes["id"].InnerText;
//                FileMgr.I.Encrypt(filename);
//                return val;
//            }
//        }
                
        public XmlAttribute<int> deaths, timesLoggedIn, highestLevel;
        public XmlAttribute<long> highscore, hardcoreHighscore, endlessHighscore, endlessHardcoreHighscore, bossRushHighscore, bossRushHardcoreHighscore;
        public XmlAttribute<bool> aimAssist, stickSmoothing;
        public XmlAttribute<float> lStickDeadzone, rStickDeadzone, lTriggerDeadzone, rTriggerDeadzone;
        public XmlAttribute<string> ID, version, controllerConfig;

        public XmlAttribute<float> masterVol, musicVol, soundVol, speechVol;
        public XmlAttribute<int> quality;
        public Res resolution;
        //public PlayerConfig config;
        //public PlayerInput input;

        /// <summary>The name of the player.</summary>
        public string Name {
            get;
            private set;
        }

#if !UNITY_WEBPLAYER
        internal XmlDocument profile;
#endif
#if UNITY_WEBPLAYER
        internal Dictionary<string, string> profile = new Dictionary<string, string>();
#endif
        internal string filename;

        public PlayerProfile(string filename, string profileName) {
            Name = profileName;

#if !UNITY_WEBPLAYER
            this.filename = filename;
            profile = new XmlDocument();
            FileMgr.I.Decrypt(filename);
            profile.Load(filename);
            FileMgr.I.Encrypt(filename);

            //configFilename = Path.Combine(configFilename, Name + ".xml");
            ////FileManager.Instance.profileConfigPath = configFilename;
            //config = new PlayerConfig(configFilename, Name);
#endif
        }

        /// <summary>
        /// Must be called after creating a new PlayerProfile!
        /// </summary>
        public void InitAttributes() {
#if UNITY_WEBPLAYER
            profile.Add("id", System.Guid.NewGuid().ToString());
            profile.Add("times-logged-in", "0");
            profile.Add("deaths", "0");

            config = new PlayerConfig(configFilename, Name);
#endif
            version = new XmlAttribute<string>(Name, "version", filename, "0.4.2.0", null);
            if (version.Value != FileMgr.profileVersion) {
                Debug.LogError(string.Format("Profile version ({0}) is below the current version ({1})! Delete & create a new profile!", version.Value, FileMgr.profileVersion));
                return;
            }

            ID = new XmlAttribute<string>(Name, "id", filename, "null", null);
            deaths = new XmlAttribute<int>(Name + "/Info", "deaths", filename, 0, null);
            highestLevel = new XmlAttribute<int>(Name + "/Info", "highest-level", filename, 1, null);
            timesLoggedIn = new XmlAttribute<int>(Name + "/Info", "times-logged-in", filename, 0, null);
            timesLoggedIn.Value++;

            highscore = new XmlAttribute<long>(Name + "/Highscores", "highscore", filename, 0, null);
            hardcoreHighscore = new XmlAttribute<long>(Name + "/Highscores", "hardcore-highscore", filename, 0, null);
            endlessHighscore = new XmlAttribute<long>(Name + "/Highscores", "endless-highscore", filename, 0, null);
            endlessHardcoreHighscore = new XmlAttribute<long>(Name + "/Highscores", "endless-hardcore-highscore", filename, 0, null);
            bossRushHighscore = new XmlAttribute<long>(Name + "/Highscores", "bossrush-highscore", filename, 0, null);
            bossRushHardcoreHighscore = new XmlAttribute<long>(Name + "/Highscores", "bossrush-hardcore-highscore", filename, 0, null);

            aimAssist = new XmlAttribute<bool>(Name + "/Settings/Controller", "aim-assist", filename, true, null);
            stickSmoothing = new XmlAttribute<bool>(Name + "/Settings/Controller", "stick-smoothing", filename, true, null);
            
            lStickDeadzone = new XmlAttribute<float>(Name + "/Settings/Controller/Deadzones", "l-stick", filename, 0.2f, OnLSDeadzoneChange);
            rStickDeadzone = new XmlAttribute<float>(Name + "/Settings/Controller/Deadzones", "r-stick", filename, 0.2f, OnRSDeadzoneChange);
            lTriggerDeadzone = new XmlAttribute<float>(Name + "/Settings/Controller/Deadzones", "l-trigger", filename, 0.2f, OnLTDeadzoneChange);
            rTriggerDeadzone = new XmlAttribute<float>(Name + "/Settings/Controller/Deadzones", "r-trigger", filename, 0.2f, OnRTDeadzoneChange);

            controllerConfig = new XmlAttribute<string>(Name + "/Settings/Controller", "config", filename, "Default", null);

            masterVol = new XmlAttribute<float>(Name + "/Settings/Audio", "master-vol", filename, 1f, OnMasterVolChange);
            musicVol = new XmlAttribute<float>(Name + "/Settings/Audio", "music-vol", filename, 1f, OnMusicVolChange);
            soundVol = new XmlAttribute<float>(Name + "/Settings/Audio", "sound-vol", filename, 1f, OnSoundVolChange);
            speechVol = new XmlAttribute<float>(Name + "/Settings/Audio", "speech-vol", filename, 1f, OnSpeechVolChange);

            quality = new XmlAttribute<int>(Name + "/Settings/Quality", "level", filename, 5, OnQualityChange);

            resolution = new Res(new XmlAttribute<int>(Name + "/Settings/Resolution", "width", filename, Screen.currentResolution.width, OnResChange), 
                                 new XmlAttribute<int>(Name + "/Settings/Resolution", "height", filename, Screen.currentResolution.height, OnResChange),
                                 new XmlAttribute<bool>(Name + "/Settings/Resolution", "fullscreen", filename, Screen.fullScreen, OnFullscreenChange));

            
        }

#if !UNITY_WEBPLAYER
        ///<summary>TODO - Check and fix this - it's not working correctly</summary>
        public void ChangeName(string newName) {
            FileMgr.I.Decrypt(filename);
            
            string newFilename = Path.Combine(Path.GetDirectoryName(filename), newName + ".xml");

            using (FileStream fs = new FileStream(newFilename, FileMode.Create)) {
                using (StreamWriter sw = new StreamWriter(fs)) {
                    using (XmlTextWriter xw = new XmlTextWriter(sw)) {
                        xw.Settings.Indent = true;
                        xw.Formatting = Formatting.Indented;
                        xw.Indentation = 4;
                        xw.WriteStartDocument();

                        xw.WriteStartElement(newName);
                        xw.WriteAttributeString("id", profile.DocumentElement.Attributes["id"].InnerText);
                        xw.WriteRaw(profile.DocumentElement.InnerText);

                        xw.WriteEndElement();
                        xw.WriteEndDocument();
                        xw.Close();
                    }
                }
            }

            profile.Load(newFilename);
            File.Delete(filename);

            filename = newFilename;

            FileMgr.I.Encrypt(filename);
        }
#endif

        public void OnMasterVolChange(float val) {
            //AudioMgr.I.SetMasterLevel(val);
        }

        public void OnMusicVolChange(float val) {
            //AudioMgr.I.SetMusicLevel(val);
        }

        public void OnSoundVolChange(float val) {
            //AudioMgr.I.SetSfxLevel(val);
        }

        public void OnSpeechVolChange(float val) {
            //AudioMgr.I.SetSpeechLevel(val);
        }

        public void OnQualityChange(int val) {
            QualitySettings.SetQualityLevel(val);
        }

        public void OnResChange(int val) {
            Screen.SetResolution(resolution.width.Value, resolution.height.Value, resolution.fullscreen.Value);
        }
        
        public void OnFullscreenChange(bool val) {
            Screen.SetResolution(resolution.width.Value, resolution.height.Value, resolution.fullscreen.Value);
        }

        //public void OnMouseSensitivityChange(float val) {
        //    AxisConfiguration axisConfig = InputMgr.GetAxisConfiguration("KeyboardAndMouse", "mouse_axis_0");
        //    axisConfig.sensitivity = val;
        //    axisConfig = InputMgr.GetAxisConfiguration("KeyboardAndMouse", "mouse_axis_1");
        //    axisConfig.sensitivity = val;
        //    FileMgr.I.CreateNewInputFile(Name);
        //}

        public void OnLSDeadzoneChange(float val) {
            AxisConfiguration axisConfig = InputMgr.GetAxisConfiguration("Windows_Gamepad", "LeftStickHorizontal");
            axisConfig.deadZone = val;
            axisConfig = InputMgr.GetAxisConfiguration("Windows_Gamepad", "LeftStickVertical");
            axisConfig.deadZone = val;
            FileMgr.I.CreateNewInputFile(Name);
        }

        public void OnRSDeadzoneChange(float val) {
            AxisConfiguration axisConfig = InputMgr.GetAxisConfiguration("Windows_Gamepad", "RightStickHorizontal");
            axisConfig.deadZone = val;
            axisConfig = InputMgr.GetAxisConfiguration("Windows_Gamepad", "RightStickVertical");
            axisConfig.deadZone = val;
            FileMgr.I.CreateNewInputFile(Name);
        }

        public void OnLTDeadzoneChange(float val) {
            AxisConfiguration axisConfig = InputMgr.GetAxisConfiguration("Windows_Gamepad", "LeftTrigger");
            axisConfig.deadZone = val;
            FileMgr.I.CreateNewInputFile(Name);
        }

        public void OnRTDeadzoneChange(float val) {
            AxisConfiguration axisConfig = InputMgr.GetAxisConfiguration("Windows_Gamepad", "RightTrigger");
            axisConfig.deadZone = val;
            FileMgr.I.CreateNewInputFile(Name);
        }
    }

 }