using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Pikl.Utils.RDS;
using Pikl.Audio;

namespace Pikl.Audio {
    public class AudioTables : Singleton<AudioTables> {
        protected AudioTables() { }
        
        static Dictionary<string, AudioTable> tables = new Dictionary<string, AudioTable>();

        public override void Awake() {
            if (tables.Count == 0) {
                TextAsset categoriesTxt = Resources.Load("Audio/Categories") as TextAsset;

                if (categoriesTxt != null) {
                    string[] lines = categoriesTxt.text.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string line in lines) {
                        tables.Add(line, new AudioTable(line));
                        Debug.Log("Table " + line + " created!");
                    }

                } else {
                    Debug.LogWarning("The Categories.txt couldn't be found in Resources/Audio! No audio will be loaded.");
                }

                base.Awake();
            }
        }

        internal void InitTable(Dictionary<string, RDSTable> tables, Dictionary<string, AudioInfoSettings> tableSettings, string tableName, string audio) {
            string audioName = audio.Split('-')[0].Split('/')[4];

            RDSTable table = new RDSTable(null, 1, audioName, 1, 1, false, true, true);

            string[] weights = null;


            TextAsset weightsTxt = Resources.Load("Audio/" + tableName + "/" + audioName + "/Weights") as TextAsset;

            if (weightsTxt != null) {
                string[] lines = weightsTxt.text.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                weights = new string[lines.Length];

                for (int i = 0; i < lines.Length; i++) {
                    weights[i] = lines[i].Split('=')[1];
                }
            }

            int j = 0;
            foreach (AudioClip clip in Resources.LoadAll("Audio/" + tableName + "/" + audioName + "/", typeof(AudioClip))) {
                double probability = 1;

                if (weights != null) {
                    try {
                        probability = Convert.ToDouble(weights[j]);
                    } catch {
                        Debug.Log(string.Format("The weight ({0}) for the ({1}) audio is not a number!", weights[j], clip.name));
                    }
                }

                table.AddEntry(new RDSAudioItem(clip, clip.name, probability));

                j++;
            }

            //Debug.HBDebug.Log("Audio : (" + audioName + ") added to table (" + tableName + ")");
            tables.Add(audioName, table);
            tableSettings.Add(audioName, ParseSettings(audio));
        }

        public AudioInfoSettings ParseSettings(string audio) {
            string[] settings = audio.Split('-')[1].Split(',');

            //print("Parsing settings for :" + audio + ", " + settings[0] + ", " + settings[1] + ", " + settings[2] + ", " + settings[3] + ", " + settings[4] + ", ");

            //                             0          1 2     3     4 5
            //Assets/Resources/Audio/Music/Music Test-1,False,False,2,10,

            float volume = Convert.ToSingle(settings[0]);
            bool loop = Convert.ToBoolean(settings[1].ToLower());
            bool threeD = Convert.ToBoolean(settings[2].ToLower());
            float minDistance = Convert.ToSingle(settings[3]);
            float maxDistance = Convert.ToSingle(settings[4]);
            
            return new AudioInfoSettings(volume, loop, threeD, minDistance, maxDistance);
        }

        public AudioTable this[string table] {
            get {
                try {
                    return tables[table];
                } catch {
                    Debug.Log(string.Format("({0}) couldn't be found in the tables!", table));
                    return null;
                }
            }
        }

        public class AudioTable {
            readonly Dictionary<string, RDSTable> tables = new Dictionary<string, RDSTable>();
            readonly Dictionary<string, AudioInfoSettings> tableSettings = new Dictionary<string, AudioInfoSettings>();

            public readonly string tableName = "";

            public AudioTable(string tableName) {
                this.tableName = tableName;

                TextAsset soundsTxt = (Resources.Load("Audio/" + this.tableName + "/Sounds") as TextAsset);

                if (soundsTxt != null) {
                    string[] sounds = soundsTxt.text.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                    //foreach (string sound in sounds) {
                    //    print("Sound " + sound + " found in file " + tableName);
                    //}

                    foreach (string sound in sounds) {
                        I.InitTable(tables, tableSettings, tableName, sound);
                    }

                } else {
                    Debug.Log(string.Format("The folder ({0}) could not be found within Resources/Audio, no audio was loaded for this table", tableName));
                }
            }

            public RDSTable this[string table] {
                get {
                    return tables[table];
                }
            }

            public AudioInfoSettings GetSettings(string table) {
                return tableSettings[table];
            }
        }
    }
}
