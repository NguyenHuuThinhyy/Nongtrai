using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace NongTrai.Editor
{
    // An explicit request file lets the already-open editor import without closing the user's session.
    [InitializeOnLoad]
    public static class FarmRedesignBridge
    {
        const string Request="Library/FarmRedesign.build-request";
        static FarmRedesignBridge(){EditorApplication.update+=Tick;}
        static void Tick()
        {
            if(EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.isPlayingOrWillChangePlaymode)return;
            const string buildRequest="Library/FarmRedesign.windows-request";
            if(File.Exists(buildRequest)) {
                File.Delete(buildRequest);
                try {
                    Environment.SetEnvironmentVariable("FARM_BUILD_OUTPUT","Builds/Windows-VisualRedesign");
                    FarmProjectBuilder.BuildWindowsCurrentScene();
                    File.WriteAllText("Logs/VisualRedesign-BuildResult.txt","OK");
                } catch(Exception e){File.WriteAllText("Logs/VisualRedesign-BuildResult.txt",e.ToString());Debug.LogException(e);}
                finally{Environment.SetEnvironmentVariable("FARM_BUILD_OUTPUT",null);}
                return;
            }
            if(!File.Exists(Request))return;
            File.Delete(Request);
            try{FarmRedesignBuilder.Build();File.WriteAllText("Logs/VisualRedesign-Result.txt","OK "+DateTime.UtcNow.ToString("O"));}
            catch(Exception e){File.WriteAllText("Logs/VisualRedesign-Result.txt",e.ToString());Debug.LogException(e);}
        }
    }
}
