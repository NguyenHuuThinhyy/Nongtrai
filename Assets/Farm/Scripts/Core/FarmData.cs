using System.IO;
using UnityEngine;

namespace NongTrai
{
    // Android StreamingAssets live inside the APK and cannot be read as ordinary files.
    public static class FarmData
    {
        public static string ReadJson(string file)
        {
            if(Application.platform!=RuntimePlatform.Android)
            {string path=Path.Combine(Application.streamingAssetsPath,file);if(File.Exists(path))return File.ReadAllText(path);}
            var resource=Resources.Load<TextAsset>("FarmTechnology/Data/"+Path.GetFileNameWithoutExtension(file));
            if(resource==null)throw new FileNotFoundException("Packaged farm data missing",file);
            return resource.text;
        }
    }
}
