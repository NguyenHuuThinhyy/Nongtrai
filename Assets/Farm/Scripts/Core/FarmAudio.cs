using UnityEngine;

namespace NongTrai
{
    public sealed class FarmAudio : MonoBehaviour
    {
        public enum Cue { Hoe, Water, Harvest, Buy, Sell, Level, GolemImpact, FishBite, RunnerCoin }
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
            clips=new AudioClip[9];
            for(int i=0;i<clips.Length;i++) clips[i]=Tone("Farm "+(Cue)i,220+i*90,.14f+i*.025f,i==1);
            clips[(int)Cue.GolemImpact]=GolemImpact();
            clips[(int)Cue.FishBite]=FishBiteSound();
            clips[(int)Cue.RunnerCoin]=RunnerCoinSound();
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
        static AudioClip GolemImpact()
        {
            const int rate=22050;const float seconds=.52f;int length=Mathf.CeilToInt(rate*seconds);
            var samples=new float[length];float[] hits={0,.17f};
            for(int i=0;i<length;i++)
            {
                float time=i/(float)rate,sound=0;
                for(int hit=0;hit<hits.Length;hit++)
                {
                    float t=time-hits[hit];if(t<0)continue;
                    float envelope=Mathf.Exp(-t*10f)*Mathf.Min(1,t*300f);
                    float phase=2*Mathf.PI*(72*t-28*t*t);
                    float grit=Mathf.Sin(2*Mathf.PI*195*t)+.65f*Mathf.Sin(2*Mathf.PI*327*t);
                    float noise=Mathf.Sin(i*127.1f)*Mathf.Sin(i*311.7f);
                    float crack=Mathf.Exp(-t*48f)*noise;
                    sound+=envelope*(Mathf.Sin(phase)*.95f+grit*.17f+noise*.18f)+crack*.45f;
                }
                samples[i]=Mathf.Clamp(sound*1.05f,-1,1);
            }
            var clip=AudioClip.Create("Golem • đùng đùng",length,1,rate,false);clip.SetData(samples,0);return clip;
        }
        static AudioClip FishBiteSound()
        {
            const int rate=22050;const float seconds=.34f;int length=Mathf.CeilToInt(rate*seconds);
            var samples=new float[length];float[] splashes={0,.105f};
            for(int i=0;i<length;i++)
            {
                float time=i/(float)rate,sound=0;
                for(int hit=0;hit<splashes.Length;hit++)
                {
                    float t=time-splashes[hit];if(t<0)continue;
                    float envelope=Mathf.Exp(-t*22f)*Mathf.Min(1,t*180f);
                    float phase=2*Mathf.PI*(310*t-210*t*t);
                    float ripple=Mathf.Sin(2*Mathf.PI*(690*t-360*t*t));
                    float waterNoise=Mathf.Sin(i*173.3f)*Mathf.Sin(i*47.9f);
                    sound+=envelope*(Mathf.Sin(phase)*.42f+ripple*.14f+waterNoise*.12f);
                }
                samples[i]=Mathf.Clamp(sound*.8f,-1,1);
            }
            var clip=AudioClip.Create("Cá cắn câu • tõm",length,1,rate,false);clip.SetData(samples,0);return clip;
        }
        static AudioClip RunnerCoinSound()
        {
            const int rate=22050;const float seconds=.24f;int length=Mathf.CeilToInt(rate*seconds);
            var samples=new float[length];
            for(int i=0;i<length;i++)
            {
                float t=i/(float)rate;
                float envelope=Mathf.Exp(-t*17f)*(1-Mathf.Exp(-t*420f));
                float fundamental=2*Mathf.PI*(1320*t-360*t*t);
                float shimmer=2*Mathf.PI*(1980*t-520*t*t);
                float ping=Mathf.Sin(fundamental)+.42f*Mathf.Sin(shimmer)+.16f*Mathf.Sin(2*Mathf.PI*2640*t);
                float click=Mathf.Exp(-t*95f)*Mathf.Sin(2*Mathf.PI*3600*t);
                samples[i]=Mathf.Clamp((ping*envelope+click*.2f)*.34f,-1,1);
            }
            var clip=AudioClip.Create("Farm Runner • xu leng keng",length,1,rate,false);clip.SetData(samples,0);return clip;
        }
    }
}
