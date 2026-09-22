using UnityEngine;

namespace NongTrai
{
    public sealed class FarmAudio : MonoBehaviour
    {
        public enum Cue { Hoe, Water, Harvest, Buy, Sell, Level }
        public static FarmAudio Instance { get; private set; }
        public float MusicVolume { get; private set; } = .28f;
        public float EffectsVolume { get; private set; } = .65f;
        AudioSource music,effects;
        AudioClip[] clips;
        void Awake()
        {
            Instance=this;
            music=gameObject.AddComponent<AudioSource>();music.loop=true;music.playOnAwake=false;
            effects=gameObject.AddComponent<AudioSource>();effects.playOnAwake=false;
            clips=new AudioClip[6];
            for(int i=0;i<clips.Length;i++) clips[i]=Tone("Farm "+(Cue)i,220+i*90,.14f+i*.025f,i==1);
            music.clip=Background();music.volume=MusicVolume;music.Play();
            effects.volume=EffectsVolume;
        }
        void OnDestroy() { if(Instance==this) Instance=null; }
        public void SetMusic(float value) { MusicVolume=Mathf.Clamp01(value);if(music!=null) music.volume=MusicVolume; }
        public void SetEffects(float value) { EffectsVolume=Mathf.Clamp01(value);if(effects!=null) effects.volume=EffectsVolume; }
        public void Play(Cue cue) { if(effects!=null) effects.PlayOneShot(clips[(int)cue]); }
        static AudioClip Tone(string title,float frequency,float seconds,bool noise)
        {
            int rate=22050,length=Mathf.CeilToInt(seconds*rate);
            float[] samples=new float[length];
            for(int i=0;i<length;i++)
            {
                float time=i/(float)rate, envelope=(1f-time/seconds)*(1f-time/seconds);
                float sound=Mathf.Sin(time*frequency*Mathf.PI*2)+.24f*Mathf.Sin(time*frequency*2*Mathf.PI*2);
                if(noise) sound+=.15f*Mathf.Sin(i*91.7f);
                samples[i]=sound*envelope*.25f;
            }
            var clip=AudioClip.Create(title,length,1,rate,false);clip.SetData(samples,0);return clip;
        }
        static AudioClip Background()
        {
            const int rate=22050,length=rate*16;
            float[] samples=new float[length];
            int[] notes={262,330,392,330,294,349,440,349,262,330,392,523,440,392,330,294};
            for(int i=0;i<length;i++)
            {
                float t=i/(float)rate;int beat=Mathf.FloorToInt(t);float phase=t-beat;
                float envelope=Mathf.Min(1,phase*12)*Mathf.Min(1,(1-phase)*3);
                float melody=Mathf.Sin(2*Mathf.PI*notes[beat]*phase)*.1f*envelope;
                float bass=Mathf.Sin(2*Mathf.PI*(notes[beat]/2f)*t)*.035f;
                samples[i]=melody+bass;
            }
            var clip=AudioClip.Create("Farm background",length,1,rate,false);clip.SetData(samples,0);return clip;
        }
    }
}
