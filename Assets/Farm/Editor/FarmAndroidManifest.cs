using System.IO;
using System.Xml;
using UnityEditor.Android;

namespace NongTrai.Editor
{
    // © HThinh.yy. The classroom backend is reached by HTTP on a private Wi-Fi LAN.
    // Patch Unity's generated manifest, keeping its activity/ARCore declarations.
    public sealed class FarmAndroidManifest : IPostGenerateGradleAndroidProject
    {
        public int callbackOrder => 1000;
        public void OnPostGenerateGradleAndroidProject(string path)
        {
            string file=Path.Combine(path,"src/main/AndroidManifest.xml");
            var xml=new XmlDocument();xml.Load(file);
            const string android="http://schemas.android.com/apk/res/android";
            var application=(XmlElement)xml.SelectSingleNode("/manifest/application");
            application.SetAttribute("usesCleartextTraffic",android,"true");
            var feature=xml.CreateElement("uses-feature");
            feature.SetAttribute("name",android,"android.hardware.camera");
            feature.SetAttribute("required",android,"false");
            xml.DocumentElement.AppendChild(feature);xml.Save(file);
        }
    }
}
