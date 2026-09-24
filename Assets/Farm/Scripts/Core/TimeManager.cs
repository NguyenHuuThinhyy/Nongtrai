using System;
using UnityEngine;

namespace NongTrai
{
    public enum FarmSeason { Spring, Summer, Autumn, Winter }
    public enum FarmWeather { Sunny, Rain, Fog, Storm }

    public sealed class TimeManager : MonoBehaviour
    {
        public static TimeManager Instance { get; private set; }
        public const float DayLengthSeconds = 1080f;
        public FarmPlayer player;
        public FieldManager field;
        public Light sun;
        public Camera viewCamera;
        public Material grassSource,leavesSource;
        public DisasterPuzzleManager disaster;
        public int Day { get; private set; } = 1;
        public float NormalizedTime { get; private set; } = .25f;
        public FarmWeather Weather { get; private set; } = FarmWeather.Sunny;
        public FarmSeason Season => (FarmSeason)(((Day-1)/28)%4);
        public int Year => (Day-1)/112+1;
        public int SeasonDay => (Day-1)%28+1;
        public int Hour => Mathf.FloorToInt(NormalizedTime*24f)%24;
        public int Minute => Mathf.FloorToInt(NormalizedTime*1440f)%60;
        public string SeasonName => new[]{"Xuân","Hạ","Thu","Đông"}[(int)Season];
        public string WeatherName => new[]{"Nắng","Mưa","Sương mù","Bão"}[(int)Weather];
        public string ClockText => "Năm "+Year+" • "+SeasonName+" "+SeasonDay+"/28 • Ngày "+Day
            +" • "+Hour.ToString("00")+":"+Minute.ToString("00")+" • "+WeatherName;
        Material grassRuntime,leavesRuntime;
        ParticleSystem rain;
        AudioSource rainSound;
        float rainTick;
        public float WeatherRemaining { get; private set; }
        void Awake() => Instance=this;
        void Start()
        {
            CloneSeasonMaterials();
            CreateRain();
            ApplySeason();ApplyWeather();ApplyLighting();
        }
        void OnDestroy() { if(Instance==this) Instance=null; }
        void CloneSeasonMaterials()
        {
            if(grassSource==null || leavesSource==null) return;
            grassRuntime=new Material(grassSource);grassRuntime.name="Seasonal grass";
            leavesRuntime=new Material(leavesSource);leavesRuntime.name="Seasonal leaves";
            foreach(var renderer in FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None))
            {
                if(renderer.sharedMaterial==grassSource) renderer.sharedMaterial=grassRuntime;
                else if(renderer.sharedMaterial==leavesSource) renderer.sharedMaterial=leavesRuntime;
            }
        }
        void CreateRain()
        {
            var go=new GameObject("Rain around player");
            rain=go.AddComponent<ParticleSystem>();
            var main=rain.main;main.loop=true;main.startLifetime=1.4f;main.startSpeed=13;
            main.startSize=.045f;main.startColor=new Color(.66f,.79f,1,.65f);main.simulationSpace=ParticleSystemSimulationSpace.World;
            main.maxParticles=1000;
            var shape=rain.shape;shape.shapeType=ParticleSystemShapeType.Box;shape.scale=new Vector3(15,.1f,15);
            var emission=rain.emission;emission.rateOverTime=380;
            var velocity=rain.velocityOverLifetime;velocity.enabled=true;velocity.space=ParticleSystemSimulationSpace.World;
            velocity.y=-14;
            var shader=Shader.Find("Universal Render Pipeline/Unlit");
            if(shader==null)shader=Shader.Find("Universal Render Pipeline/Lit");
            if(shader!=null)
            {var rainMaterial=new Material(shader);rainMaterial.color=new Color(.31f,.69f,1f,.85f);
             rainMaterial.SetColor("_BaseColor",new Color(.31f,.69f,1f,.85f));
             go.GetComponent<ParticleSystemRenderer>().material=rainMaterial;}
            rainSound=go.AddComponent<AudioSource>();rainSound.loop=true;rainSound.playOnAwake=false;
            rainSound.clip=RainClip();rainSound.volume=.25f;
            rain.Stop();
        }
        static AudioClip RainClip()
        {
            const int rate=16000,length=rate*3;
            var samples=new float[length];var random=new System.Random(3907);
            float smooth=0;
            for(int i=0;i<length;i++)
            { smooth=Mathf.Lerp(smooth,(float)random.NextDouble()*2-1,.14f);samples[i]=smooth*.19f; }
            var clip=AudioClip.Create("Rain ambience",length,1,rate,false);clip.SetData(samples,0);return clip;
        }
        void Update()
        {
            bool shopping=player!=null&&player.Paused&&FarmShop.ClockRunningInShop&&Application.isFocused;
            if(player==null || player.Paused&&!shopping) return;
            float elapsed=shopping?Time.unscaledDeltaTime:Time.deltaTime;
            float fraction=elapsed/DayLengthSeconds;
            NormalizedTime+=fraction;
            if(WeatherRemaining>0)
            {WeatherRemaining=Mathf.Max(0,WeatherRemaining-elapsed);if(WeatherRemaining<=0&&(Weather==FarmWeather.Rain||Weather==FarmWeather.Storm))SetWeather(FarmWeather.Sunny);}
            if(!shopping)foreach(var animal in FindObjectsByType<FarmAnimal>(FindObjectsSortMode.None)) animal.AdvanceCare(fraction);
            if(NormalizedTime>=1f) { NormalizedTime-=1f;NewDay(); }
            if(Weather==FarmWeather.Rain || Weather==FarmWeather.Storm)
            {
                rain.transform.position=player.transform.position+Vector3.up*10;
                rainTick+=elapsed;
                if(rainTick>=1f) { WaterFields(.045f*rainTick);rainTick=0; }
            }
            ApplyLighting();
        }
        void NewDay()
        {
            Day++;ApplySeason();
            FarmCraftOrders.Instance?.OnNewDay(Day);
            var random=new System.Random(Day*7919+Year*373);
            int roll=random.Next(100);
            SetWeather(roll<67?FarmWeather.Sunny:roll<82?FarmWeather.Rain:roll<96?FarmWeather.Fog:FarmWeather.Storm,true);
        }
        public void SetWeather(FarmWeather value,bool triggerPuzzle=false)
        {
            Weather=value;WeatherRemaining=value==FarmWeather.Rain||value==FarmWeather.Storm?DayLengthSeconds*.25f:0;ApplyWeather();
            if((value==FarmWeather.Rain || value==FarmWeather.Storm)) WaterFields(1);
            if(value==FarmWeather.Storm && triggerPuzzle) disaster?.BeginStorm();
        }
        void WaterFields(float amount)
        { foreach(var plot in FindObjectsByType<FarmPlot>(FindObjectsSortMode.None)) plot.AddMoisture(amount); }
        void ApplySeason()
        {
            Color[] grassColors={new Color(.56f,.76f,.34f),new Color(.29f,.57f,.20f),new Color(.73f,.56f,.25f),new Color(.79f,.84f,.84f)};
            Color[] leafColors={new Color(.42f,.73f,.29f),new Color(.19f,.50f,.20f),new Color(.90f,.43f,.12f),new Color(.75f,.80f,.83f)};
            if(grassRuntime!=null) grassRuntime.color=grassColors[(int)Season];
            if(leavesRuntime!=null) leavesRuntime.color=leafColors[(int)Season];
        }
        void ApplyWeather()
        {
            bool wet=Weather==FarmWeather.Rain || Weather==FarmWeather.Storm;
            if(rain!=null)
            { if(wet && !rain.isPlaying) rain.Play();if(!wet && rain.isPlaying) rain.Stop(); }
            if(rainSound!=null)
            { if(wet && !rainSound.isPlaying) rainSound.Play();if(!wet && rainSound.isPlaying) rainSound.Stop(); }
            RenderSettings.fog=true;RenderSettings.fogMode=FogMode.Linear;
            RenderSettings.fogStartDistance=Weather==FarmWeather.Fog?12:Weather==FarmWeather.Storm?20:65;
            RenderSettings.fogEndDistance=Weather==FarmWeather.Fog?48:Weather==FarmWeather.Storm?65:140;
            RenderSettings.fogColor=Weather==FarmWeather.Storm?new Color(.30f,.36f,.45f):
                Weather==FarmWeather.Fog?new Color(.75f,.78f,.80f):new Color(.68f,.82f,.85f);
        }
        void ApplyLighting()
        {
            float hour=NormalizedTime*24;
            float daylight=Mathf.Clamp01(Mathf.Sin((NormalizedTime-.25f)*Mathf.PI*2));
            float twilight=Mathf.Clamp01(1-Mathf.Abs(hour-6)/2)+Mathf.Clamp01(1-Mathf.Abs(hour-18)/2);
            Color sky=Color.Lerp(new Color(.04f,.08f,.19f),new Color(.62f,.80f,.91f),daylight);
            Color warm=hour<12?new Color(1f,.49f,.25f):new Color(1f,.30f,.18f);
            sky=Color.Lerp(sky,warm,twilight*.55f);
            float cloud=Weather==FarmWeather.Storm ? .45f : Weather==FarmWeather.Rain ? .70f : 1f;
            sun.transform.rotation=Quaternion.Euler(NormalizedTime*360-90,-35,0);
            sun.color=Color.Lerp(new Color(.24f,.38f,.72f),Color.Lerp(Color.white,warm,twilight*.75f),daylight);
            sun.intensity=(.28f+daylight*1.95f+twilight*.65f)*cloud;
            RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor=sky*cloud;
            RenderSettings.ambientEquatorColor=sky*.75f*cloud;
            RenderSettings.ambientGroundColor=sky*.40f*cloud;
            if(viewCamera!=null) viewCamera.backgroundColor=sky*cloud;
            bool exploring=player!=null&&player.transform.position.y>500;
            RenderSettings.fogStartDistance=exploring?18:Weather==FarmWeather.Fog?12:Weather==FarmWeather.Storm?20:65;
            RenderSettings.fogEndDistance=exploring?32:Weather==FarmWeather.Fog?48:Weather==FarmWeather.Storm?65:140;
            RenderSettings.fogColor=exploring?sky*cloud:Weather==FarmWeather.Storm?new Color(.30f,.36f,.45f):
                Weather==FarmWeather.Fog?new Color(.75f,.78f,.80f):new Color(.68f,.82f,.85f);

        }
        public void Restore(int day,float time,FarmWeather weather,float remaining=-1)
        {
            Day=Mathf.Max(1,day);NormalizedTime=Mathf.Repeat(time,1f);
            Weather=weather;WeatherRemaining=remaining<0?(weather==FarmWeather.Rain||weather==FarmWeather.Storm?Mathf.Max(0,(.25f-NormalizedTime)*DayLengthSeconds):0):remaining;
            if(WeatherRemaining<=0&&(Weather==FarmWeather.Rain||Weather==FarmWeather.Storm))Weather=FarmWeather.Sunny;
            ApplySeason();ApplyWeather();ApplyLighting();
        }
        public float SleepUntilMorning()
        {
            float skippedDays=NormalizedTime<.25f ? .25f-NormalizedTime : 1.25f-NormalizedTime;
            float skippedSeconds=skippedDays*DayLengthSeconds;
            foreach(var plot in FindObjectsByType<FarmPlot>(FindObjectsSortMode.None)) plot.Tick(skippedSeconds);
            foreach(var animal in FindObjectsByType<FarmAnimal>(FindObjectsSortMode.None))
            { animal.AdvanceCare(skippedDays);animal.AdvanceCooldown(skippedSeconds); }
            foreach(var pen in FindObjectsByType<AnimalPen>(FindObjectsSortMode.None)) pen.Advance(skippedSeconds);
            foreach(var tree in FindObjectsByType<FruitTree>(FindObjectsSortMode.None))
            {float grow=tree.planted?Mathf.Min(skippedSeconds,Mathf.Max(0,240-tree.age)):0;tree.age+=grow;tree.remaining=Mathf.Max(0,tree.remaining-(skippedSeconds-grow));}
            FarmProcessing.Instance?.Advance(skippedSeconds);
            bool crossedDay=NormalizedTime>=.25f;
            if(crossedDay)NewDay();
            NormalizedTime=.25f;
            WeatherRemaining=Mathf.Max(0,WeatherRemaining-(crossedDay?DayLengthSeconds*.25f:skippedSeconds));
            if(WeatherRemaining<=0&&(Weather==FarmWeather.Rain||Weather==FarmWeather.Storm))SetWeather(FarmWeather.Sunny);
            ApplyLighting();
            return skippedSeconds;
        }
    }
}
