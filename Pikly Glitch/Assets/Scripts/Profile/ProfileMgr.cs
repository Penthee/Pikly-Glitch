#define AUTO_LOG_IN

using UnityEngine;
using System.IO;
using System.Collections;
using System.Text.RegularExpressions;
//using Pikl.Audio;

namespace Pikl.Profile {
	public class ProfileMgr : Singleton<ProfileMgr> {
        protected ProfileMgr() { }
        
        public enum ProfileExistsType {
            /// <summary>
            /// This is the first profile to be created.
            /// </summary>
            FirstProfile,
            /// <summary>
            /// This profile did not exist, and so it was created.
            /// </summary>
            DoesntExist,
            /// <summary>
            /// This profile couldn't get created because it already exists.
            /// </summary>
            Exists,
            /// <summary>
            /// Something went terribly wrong.
            /// </summary>
            SomeError
        }

        public string CurrentProfileName {
            get {
                if (profile == null)
                    return "N/A";

                return profile.Name;
            }
        }

        public PlayerProfile profile;

        public override void Awake() {
            if (FileMgr.I.config.LastPlayer == "")
                CreateProfile("Pikl");
#if AUTO_LOG_IN
            if (FileMgr.I.config.LastPlayer != "" && profile == null) {
                SetProfile(FileMgr.I.config.LastPlayer);
                Debug.Log("Profile " + CurrentProfileName + " Auto Log-in");
            }
#endif
            //profile = new PlayerProfile("", "", "");
            base.Awake();
        }

        /// <summary>
        /// Creates a new profile, input and config files for the given profile name, and sets the profile as logged in.
        /// </summary>
        public ProfileExistsType CreateProfile(string profileName) {
            string pattern = @"\s+";
            string replacement = "";
            Regex rgx = new Regex(pattern);
            profileName = rgx.Replace(profileName, replacement);

            if (string.IsNullOrEmpty(profileName))
                return ProfileExistsType.SomeError;

#if !UNITY_WEBPLAYER
            ProfileExistsType pet = ProfileExists(profileName);

            switch (pet) {
                case ProfileExistsType.FirstProfile:
                case ProfileExistsType.DoesntExist:
                    FileMgr.I.CreateNewProfile(profileName);
                    //FileMgr.I.CreateNewPlayerConfig(profileName);
                    FileMgr.I.CreateNewInputFile(profileName);
                    
                    SetProfile(profileName);

                    //TableMgr.I.StackStat("ProfilesCreated");
                    //TableMgr.I.PostStats();
                    break;
            }

            return pet;
#endif
#if UNITY_WEBPLAYER
            profile = new PlayerProfile("", "", profileName);
            SetProfile(profileName);
            return ProfileExistsType.FirstProfile;
#endif
        }

        /// <summary>
        /// Sets the given profile name as the logged in profile.<para/>
        /// An empty string will clear the logged in profile.
        /// </summary>
        public void SetProfile(string profileName) {
            FileMgr.I.config.LastPlayer = profileName;
#if !UNITY_WEBPLAYER
            if (ProfileExists(profileName) == ProfileExistsType.Exists) {
                if (profileName == "")
                    profile = null;
                else {
                    profile = new PlayerProfile(Path.Combine(FileMgr.I.profilePath, profileName + ".xml"), profileName);
                    profile.InitAttributes();
                    profile.OnResChange(0);
                    profile.OnFullscreenChange(false);
                    //AudioMgr.I.SetLevels();
                }
            } else {
                Debug.Log("Profile needs to be created before it can be set!");
            }
#endif
        }

        /// <summary>
        /// Deletes the currently logged in profile.
        /// </summary>
        public void DeleteProfile() {
#if !UNITY_WEBPLAYER
            DeleteProfile(profile.Name);
#endif
        }

        /// <summary>
        /// Deletes the specified profile.
        /// </summary>
        public void DeleteProfile(string name) {
#if !UNITY_WEBPLAYER
            if (FileMgr.I.config.LastPlayer == name)
                FileMgr.I.config.LastPlayer = "";

            if (profile != null && name == profile.Name)
                profile = null;

            FileMgr.I.DeleteProfileFiles(name);
#endif
        }
        public string[] GetProfiles() {
#if !UNITY_WEBPLAYER
            return FileMgr.I.GetProfiles();
#endif
#if UNITY_WEBPLAYER
            return new string[] { "" };
#endif
        }

        public ProfileExistsType ProfileExists(string profileName) {
#if !UNITY_WEBPLAYER
            int i = 0;
            foreach (string profile in GetProfiles()) {
                if (profile == profileName)
                    return ProfileExistsType.Exists;
                i++;
            }

            if (i == 0)
                return ProfileExistsType.FirstProfile;

#endif
            return ProfileExistsType.DoesntExist;
        }
        
    }
}