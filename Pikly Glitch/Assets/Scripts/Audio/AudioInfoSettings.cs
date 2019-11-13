namespace Pikl.Audio {
    public class AudioInfoSettings {
        public float volume, minDistance, maxDistance;
        public bool loop, threeD;

        public AudioInfoSettings() { }

        public AudioInfoSettings(float volume, bool loop, bool threeD, float minDistance, float maxDistance) {
            this.volume = volume;
            this.loop = loop;
            this.threeD = threeD;
            this.minDistance = minDistance;
            this.maxDistance = maxDistance;
        }
    }
}