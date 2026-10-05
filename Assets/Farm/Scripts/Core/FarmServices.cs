using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace NongTrai
{
    // © HThinh.yy. Connection preferences and chat are independent of gameplay save22.
    public sealed class FarmServices : MonoBehaviour
    {
        [Serializable] public sealed class Connection { public string url="http://127.0.0.1:8000",key=""; }
        [Serializable] public sealed class ChatMessage { public string role,content; }
        [Serializable] sealed class ChatRequest { public string question,context;public ChatMessage[] history; }
        [Serializable] sealed class ChatResponse { public string answer,model;public string[] sources;public bool generated; }
        [Serializable] sealed class ChatHistory { public ChatMessage[] messages; }
        [Serializable] public sealed class KnowledgeSection { public string id,title,text; }
        [Serializable] public sealed class KnowledgeData { public KnowledgeSection[] sections; }
        [Serializable] sealed class Health { public bool model_ready,mqtt_connected;public string model; }
        [Serializable] sealed class Telemetry { public string session_id;public float moisture_percent,growth_percent;public bool station_built,pump_active,simulation=true; }
        [Serializable] sealed class Control { public string session_id,server_id;public int revision;public bool cloud_available,has_command,pump_enabled; }
        [Serializable] sealed class Ack { public string session_id,server_id,reason;public int revision;public bool applied,pump_active; }
        public static FarmServices Instance {get;private set;}
        public GameObject TouchMenu {get;private set;}
        public GameObject ChatPanel {get;private set;}
        public GameObject ConnectionPanel {get;private set;}
        public KnowledgeData Manual {get;private set;}
        public bool CloudConnected {get;private set;}
        public bool RemotePumpEnabled {get;private set;}=true;
        public string Status {get;private set;}="Chưa kết nối";
        public bool CloudEnabled {get;private set;}
        public Connection Config {get;private set;}=new Connection();
        public bool ChatBusy=>busy;
        public string LastReply {get;private set;}="";
        public bool LastReplyFromModel {get;private set;}
        FarmHud hud;TMP_InputField input,address,keyInput;TMP_Text chatText,statusText;RectTransform chatContent;ScrollRect chatScroll;
        readonly List<ChatMessage> history=new List<ChatMessage>();
        UnityWebRequest activeChat;Coroutine chatRoutine;string context="",session="",server="";int revision=-1;bool busy,ready;
        string PreferencesPath=>Path.Combine(Application.persistentDataPath,"farm-connection.json");
        string HistoryPath=>Path.Combine(Application.persistentDataPath,"farm-chat-history.json");
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install(){if(FindFirstObjectByType<FarmPlayer>()!=null&&FindFirstObjectByType<FarmServices>()==null)new GameObject("Farm Android AR AI Cloud • HThinh.yy").AddComponent<FarmServices>();}
        void Awake(){Instance=this;}
        IEnumerator Start()
        {
            yield return null;yield return null;hud=FindFirstObjectByType<FarmHud>();if(hud==null)yield break;
            try {if(File.Exists(PreferencesPath))Config=JsonUtility.FromJson<Connection>(File.ReadAllText(PreferencesPath));if(File.Exists(HistoryPath)){var saved=JsonUtility.FromJson<ChatHistory>(File.ReadAllText(HistoryPath));if(saved?.messages!=null)history.AddRange(saved.messages);}}
            catch(Exception e){Debug.LogWarning("Connection preferences: "+e.GetType().Name);}
            Config??=new Connection();TrimHistory();var data=Resources.Load<TextAsset>("FarmTechnology/knowledge");Manual=data==null?new KnowledgeData{sections=Array.Empty<KnowledgeSection>()}:JsonUtility.FromJson<KnowledgeData>(data.text);
            BuildUI();ready=true;if(FarmControls.Mobile){var mobile=new GameObject("Farm touch UI").AddComponent<FarmMobileUI>();mobile.Initialize(hud);}gameObject.AddComponent<FarmAR>();StartCoroutine(CloudLoop());
        }
        void BuildUI()
        {
            TouchMenu=FarmUi.Panel(hud.transform,"Công nghệ và thao tác",new Vector2(880,680));FarmUi.TmpLabel(TouchMenu.transform,"NÔNG TRẠI • MENU",new Vector2(25,-22),new Vector2(650,45),30);
            string[] labels={"Túi đồ","Cửa hàng","Chế biến","Mở đất","Chuồng","Bản đồ / Minigame","Xây dựng","Nông trại AR","Trợ lý AI","Kết nối / IO cloud","Đổi góc nhìn","Cho thú ăn","Bay sáng tạo","Lưu game","Tạm dừng","Hướng dẫn"};
            for(int i=0;i<labels.Length;i++){int item=i;FarmUi.Button(TouchMenu.transform,labels[i],new Vector2(25+(i%2)*420,-85-(i/2)*61),new Vector2(400,53),()=>MenuAction(item));}
            FarmUi.Button(TouchMenu.transform,"Tiếp tục",new Vector2(25,-592),new Vector2(830,60),Close);TouchMenu.SetActive(false);
            var quick=FarmUi.Button(hud.gameplayChrome.transform,FarmControls.Mobile?"Trợ lý / AR / Cloud":"[C] Trợ lý / AR / Cloud",new Vector2(-20,-88),new Vector2(250,48),()=>hud.ShowOverlay(TouchMenu));var qr=quick.GetComponent<RectTransform>();qr.anchorMin=qr.anchorMax=qr.pivot=Vector2.one;
            ChatPanel=FarmUi.Panel(hud.transform,"Trợ lý nông trại",new Vector2(1000,720));FarmUi.TmpLabel(ChatPanel.transform,"TRỢ LÝ NÔNG TRẠI • TIẾNG VIỆT",new Vector2(24,-18),new Vector2(810,45),28);
            var viewport=FarmUi.Panel(ChatPanel.transform,"Hội thoại",new Vector2(950,420));var vr=viewport.GetComponent<RectTransform>();vr.anchorMin=vr.anchorMax=vr.pivot=new Vector2(0,1);vr.anchoredPosition=new Vector2(24,-80);viewport.AddComponent<RectMask2D>();
            chatContent=new GameObject("Messages",typeof(RectTransform)).GetComponent<RectTransform>();chatContent.SetParent(viewport.transform,false);chatContent.anchorMin=new Vector2(0,1);chatContent.anchorMax=new Vector2(1,1);chatContent.pivot=new Vector2(.5f,1);chatContent.sizeDelta=new Vector2(0,420);
            chatText=FarmUi.TmpLabel(chatContent,"",new Vector2(12,-12),new Vector2(925,420),22);chatText.richText=false;chatText.overflowMode=TextOverflowModes.Overflow;
            chatScroll=viewport.AddComponent<ScrollRect>();chatScroll.viewport=vr;chatScroll.content=chatContent;chatScroll.horizontal=false;chatScroll.movementType=ScrollRect.MovementType.Clamped;
            input=Input(ChatPanel.transform,"Hỏi về game…",new Vector2(24,-515),new Vector2(730,62),false);FarmUi.Button(ChatPanel.transform,"Gửi",new Vector2(770,-515),new Vector2(200,62),Send);
            string[] samples={"Cách lấy và đặt nước?","Cung hỏng sửa thế nào?","Thưởng tìm số 2D?"};for(int i=0;i<3;i++){string sample=samples[i];FarmUi.Button(ChatPanel.transform,sample,new Vector2(24+i*318,-590),new Vector2(304,45),()=>{input.text=sample;Send();});}
            FarmUi.Button(ChatPanel.transform,"Kết nối",new Vector2(24,-650),new Vector2(180,48),OpenConnection);FarmUi.Button(ChatPanel.transform,"Hủy trả lời",new Vector2(217,-650),new Vector2(190,48),CancelChat);
            FarmUi.Button(ChatPanel.transform,"Hướng dẫn",new Vector2(420,-650),new Vector2(190,48),ShowManual);FarmUi.Button(ChatPanel.transform,"Xóa hội thoại",new Vector2(623,-650),new Vector2(180,48),()=>{CancelChat();history.Clear();PersistHistory();RenderChat();});FarmUi.Button(ChatPanel.transform,"Đóng",new Vector2(820,-650),new Vector2(150,48),Close);ChatPanel.SetActive(false);
            ConnectionPanel=FarmUi.Panel(hud.transform,"Kết nối PC và IO cloud",new Vector2(900,650));FarmUi.TmpLabel(ConnectionPanel.transform,"KẾT NỐI PC • CLOUD MÔ PHỎNG",new Vector2(24,-20),new Vector2(740,50),28);
            FarmUi.TmpLabel(ConnectionPanel.transform,"Địa chỉ PC cùng Wi-Fi, ví dụ http://192.168.1.10:8000",new Vector2(24,-90),new Vector2(850,50),22);address=Input(ConnectionPanel.transform,"http://192.168.1.10:8000",new Vector2(24,-145),new Vector2(850,55),false);keyInput=Input(ConnectionPanel.transform,"Mã kết nối từ backend",new Vector2(24,-218),new Vector2(850,55),true);
            FarmUi.Button(ConnectionPanel.transform,"Lưu & kiểm tra",new Vector2(24,-295),new Vector2(270,58),()=>{if(SaveConfig())StartCoroutine(CheckHealth());});FarmUi.Button(ConnectionPanel.transform,"Bật cloud",new Vector2(310,-295),new Vector2(270,58),()=>{if(SaveConfig())EnableCloud();});FarmUi.Button(ConnectionPanel.transform,"Ngắt cloud",new Vector2(595,-295),new Vector2(280,58),DisableCloud);
            statusText=FarmUi.TmpLabel(ConnectionPanel.transform,"",new Vector2(24,-380),new Vector2(850,130),22);statusText.richText=false;FarmUi.TmpLabel(ConnectionPanel.transform,"Cloud dùng độ ẩm trong game. Chỉ điều khiển trạm vùng đầu đã xây. Khóa Adafruit chỉ nhập ở PC.",new Vector2(24,-500),new Vector2(850,70),20);FarmUi.Button(ConnectionPanel.transform,"Trở lại game / AR",new Vector2(24,-585),new Vector2(850,48),Close);ConnectionPanel.SetActive(false);
        }
        void MenuAction(int action)
        {Close();switch(action){case 0:hud.interaction.inventory.Open();break;case 1:hud.interaction.shop.Open();break;case 2:FarmProcessing.Instance?.Open();break;case 3:FarmExpansion.Instance?.Open();break;case 4:hud.interaction.shop.barn?.Open();break;case 5:IslandManager.Instance?.OpenMap();break;case 6:FarmBuildingSystem.Instance?.Toggle();break;case 7:FarmAR.Instance?.Open();break;case 8:OpenChat();break;case 9:OpenConnection();break;case 10:hud.player.cameraRig.ToggleView();break;case 11:StartCoroutine(Pulse(UnityEngine.InputSystem.Key.F));break;case 12:CreativeModeManager.Instance?.ToggleFlight();break;case 13:hud.SaveNow();break;case 14:hud.player.SetPaused(true);break;case 15:OpenChat();ShowManual();break;}}
        IEnumerator Pulse(UnityEngine.InputSystem.Key key){yield return null;yield return null;FarmControls.Keys[key].Set(true);yield return null;yield return null;FarmControls.Keys[key].Set(false);}
        void Update(){if(ready&&FarmControls.Keys[UnityEngine.InputSystem.Key.C].wasPressedThisFrame&&!ChatPanel.activeSelf&&!ConnectionPanel.activeSelf&&(!hud.player.Paused||(FarmAR.Instance!=null&&FarmAR.Instance.Active)))OpenChat();}
        public void OpenChat(string topic=""){if(!ready)return;var ar=FarmAR.Instance;context=topic.Length>0?topic:ar!=null&&ar.Active?ar.ChatContext:CurrentContext();ar?.HideForChat();hud.ShowOverlay(ChatPanel);RenderChat();}
        public void OpenConnection(){if(!ready)return;address.text=Config.url;keyInput.text=Config.key;hud.ShowOverlay(ConnectionPanel);SetStatus(Status);}
        public bool HandleEscape(){if(!ready)return false;if(ChatPanel.activeSelf||ConnectionPanel.activeSelf||TouchMenu.activeSelf){Close();return true;}if(FarmAR.Instance!=null&&FarmAR.Instance.Active){FarmAR.Instance.Close();return true;}return false;}
        public void Close(){CancelChat();if(ChatPanel!=null)ChatPanel.SetActive(false);if(ConnectionPanel!=null)ConnectionPanel.SetActive(false);if(TouchMenu!=null)TouchMenu.SetActive(false);if(FarmAR.Instance!=null&&FarmAR.Instance.Active)FarmAR.Instance.ReturnFromChat();else hud.Resume();}
        string CurrentContext(){var plot=hud.interaction.Plot;return plot!=null?plot.Description:AdventureBag.Instance==null?"":"Đang cầm: "+AdventureBag.Instance.Name(AdventureBag.Instance.Item);}
        public void Send(){if(busy||string.IsNullOrWhiteSpace(input.text))return;string question=input.text.Trim();if(question.Length>800){RenderChat("Câu hỏi tối đa 800 ký tự.");return;}input.text="";chatRoutine=StartCoroutine(Chat(question));}
        IEnumerator Chat(string question)
        {
            busy=true;LastReply="";LastReplyFromModel=false;var payload=new ChatRequest{question=question,context=context,history=history.ToArray()};history.Add(new ChatMessage{role="user",content=question});RenderChat("Đang trả lời…");
            using(var request=Request("/v1/chat",JsonUtility.ToJson(payload))){activeChat=request;yield return request.SendWebRequest();activeChat=null;if(request.result==UnityWebRequest.Result.Success){var answer=JsonUtility.FromJson<ChatResponse>(request.downloadHandler.text);if(answer!=null&&!string.IsNullOrEmpty(answer.answer)){LastReply=answer.answer;LastReplyFromModel=answer.generated;history.Add(new ChatMessage{role="assistant",content=answer.answer+((answer.sources?.Length??0)>0?"\nTham khảo: "+string.Join(" • ",answer.sources):"")});}}else history.Add(new ChatMessage{role="assistant",content="Chưa kết nối được trợ lý. "+Error(request)+" Bạn vẫn có thể đọc Hướng dẫn."});}
            busy=false;chatRoutine=null;TrimHistory();PersistHistory();RenderChat();
        }
        void TrimHistory(){while(history.Count>6)history.RemoveAt(0);foreach(var m in history)if(m.content.Length>1500)m.content=m.content.Substring(0,1500);}
        public void CancelChat(){if(activeChat!=null){activeChat.Abort();activeChat=null;}if(chatRoutine!=null){StopCoroutine(chatRoutine);chatRoutine=null;}if(busy){busy=false;if(history.Count>0&&history[history.Count-1].role=="user")history.RemoveAt(history.Count-1);RenderChat("Đã hủy.");}}
        void RenderChat(string notice=""){if(chatText==null)return;var b=new StringBuilder();if(history.Count==0)b.Append("Hỏi trợ lý về trồng cây, nước, cung, rèn và nhà hàng.\n\n");foreach(var m in history)b.Append(m.role=="user"?"Bạn: ":"Trợ lý: ").Append(m.content).Append("\n\n");b.Append(notice);ShowText(b.ToString(),false);}
        void ShowText(string text,bool top){chatText.text=text;chatText.rectTransform.sizeDelta=new Vector2(925,Mathf.Max(420,chatText.preferredHeight+24));chatContent.sizeDelta=new Vector2(0,chatText.rectTransform.sizeDelta.y);Canvas.ForceUpdateCanvases();chatScroll.verticalNormalizedPosition=top?1:0;}
        void ShowManual(){CancelChat();var b=new StringBuilder();foreach(var s in Manual.sections)b.Append(s.title).Append(":\n").Append(s.text).Append("\n\n");ShowText(b.ToString(),true);}
        bool SaveConfig()
        {if(!Uri.TryCreate(address.text.Trim(),UriKind.Absolute,out var uri)||(uri.Scheme!="http"&&uri.Scheme!="https")||!string.IsNullOrEmpty(uri.UserInfo)||uri.AbsolutePath!="/"){SetStatus("Nhập địa chỉ http(s) của PC, không thêm đường dẫn API.");return false;}if(string.IsNullOrWhiteSpace(keyInput.text)){SetStatus("Nhập mã kết nối của backend.");return false;}DisableCloud();Config=new Connection{url=address.text.Trim().TrimEnd('/'),key=keyInput.text.Trim()};try{File.WriteAllText(PreferencesPath,JsonUtility.ToJson(Config));return true;}catch(Exception){SetStatus("Không lưu được cấu hình kết nối.");return false;}}
        IEnumerator CheckHealth(){using(var paired=Request("/v1/pair")){paired.timeout=8;yield return paired.SendWebRequest();if(paired.result!=UnityWebRequest.Result.Success){SetStatus(Error(paired));yield break;}}using(var request=Request("/health")){yield return request.SendWebRequest();if(request.result!=UnityWebRequest.Result.Success){SetStatus(Error(request));yield break;}var health=JsonUtility.FromJson<Health>(request.downloadHandler.text);SetStatus("Mã kết nối đúng • Model "+(health.model_ready?"sẵn sàng":"chưa tải")+" • Adafruit "+(health.mqtt_connected?"đã kết nối":"chưa cấu hình / mất mạng"));}}
        public void EnableCloud(){session=Guid.NewGuid().ToString("N");server="";revision=-1;RemotePumpEnabled=true;CloudEnabled=true;SetStatus("Đang nối cloud mô phỏng…");}
        public void DisableCloud(){string old=session;CloudEnabled=CloudConnected=false;RemotePumpEnabled=true;session="";SetStatus("Cloud tắt • tưới cục bộ");if(!string.IsNullOrEmpty(old))StartCoroutine(Disconnect(old));}
        IEnumerator Disconnect(string id){using(var request=Request("/v1/control?session_id="+id,null,"DELETE")){request.timeout=5;yield return request.SendWebRequest();}}
        IEnumerator CloudLoop()
        {
            float nextTelemetry=0;string previous="";
            while(true)
            {
                if(!CloudEnabled){yield return new WaitForSecondsRealtime(1);continue;}string current=session;if(previous!=current){previous=current;nextTelemetry=0;}
                if(Time.realtimeSinceStartup>=nextTelemetry){nextTelemetry=Time.realtimeSinceStartup+20;using(var request=Request("/v1/telemetry",JsonUtility.ToJson(ReadTelemetry(current)))){request.timeout=8;yield return request.SendWebRequest();if(current!=session)continue;if(request.result!=UnityWebRequest.Result.Success){Fallback(Error(request));yield return new WaitForSecondsRealtime(5);continue;}}}
                using(var request=Request("/v1/control?session_id="+current))
                {request.timeout=8;yield return request.SendWebRequest();if(current!=session)continue;if(request.result!=UnityWebRequest.Result.Success)Fallback(Error(request));else{var control=JsonUtility.FromJson<Control>(request.downloadHandler.text);if(control==null||control.session_id!=current||!control.cloud_available)Fallback("Adafruit chưa kết nối • tưới cục bộ");else{CloudConnected=true;SetStatus("Cloud mô phỏng đã kết nối • Trạm vùng 1");if(!control.has_command){RemotePumpEnabled=true;server=control.server_id;revision=control.revision;}else if(control.server_id!=server||control.revision!=revision){bool built=FarmWaterSystem.Instance!=null&&FarmWaterSystem.Instance.StationBuilt[0];RemotePumpEnabled=!built||control.pump_enabled;server=control.server_id;revision=control.revision;var ack=new Ack{session_id=current,server_id=server,revision=revision,applied=built,pump_active=built&&RemotePumpEnabled,reason=built?"":"Trạm vùng 1 chưa được xây"};using(var response=Request("/v1/control/ack",JsonUtility.ToJson(ack))){response.timeout=8;yield return response.SendWebRequest();}}}}}
                yield return new WaitForSecondsRealtime(5);
            }
        }
        Telemetry ReadTelemetry(string id){float water=0,growth=0;int count=0;foreach(var plot in FindObjectsByType<FarmPlot>(FindObjectsSortMode.None))if(FarmExpansion.Instance.RegionFor(plot)==0&&plot.Crop!=null){water+=plot.Moisture;growth+=plot.Growth;count++;}bool built=FarmWaterSystem.Instance!=null&&FarmWaterSystem.Instance.StationBuilt[0];return new Telemetry{session_id=id,moisture_percent=count==0?0:water/count*100,growth_percent=count==0?0:growth/count*100,station_built=built,pump_active=built&&(!CloudConnected||RemotePumpEnabled)};}
        void Fallback(string message){CloudConnected=false;RemotePumpEnabled=true;SetStatus(message);}
        void SetStatus(string value){Status=value;if(statusText!=null)statusText.text=value;}
        UnityWebRequest Request(string path,string body=null,string method=null){var request=new UnityWebRequest(Config.url+path,method??(body==null?"GET":"POST"));request.downloadHandler=new DownloadHandlerBuffer();if(body!=null){request.uploadHandler=new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));request.SetRequestHeader("Content-Type","application/json");}request.SetRequestHeader("X-Farm-Key",Config.key??"");request.timeout=95;return request;}
        static string Error(UnityWebRequest request){if(request.responseCode==401)return "Mã kết nối không đúng.";if(request.responseCode==409)return "Phiên khác đang kết nối hoặc lệnh đã cũ.";if(request.responseCode==429)return "Trợ lý đang bận; hãy thử lại.";return "Kiểm tra địa chỉ PC, backend và Wi-Fi (HTTP "+request.responseCode+").";}
        void PersistHistory(){try{TrimHistory();File.WriteAllText(HistoryPath,JsonUtility.ToJson(new ChatHistory{messages=history.ToArray()}));}catch(Exception){Debug.LogWarning("Chat history could not be saved");}}
        void OnDestroy(){CancelChat();CloudConnected=CloudEnabled=false;RemotePumpEnabled=true;if(Instance==this)Instance=null;}
        public static TMP_InputField Input(Transform parent,string placeholder,Vector2 position,Vector2 size,bool password)
        {var box=FarmUi.Panel(parent,"Nhập",size);var rect=box.GetComponent<RectTransform>();rect.anchorMin=rect.anchorMax=rect.pivot=new Vector2(0,1);rect.anchoredPosition=position;var field=box.AddComponent<TMP_InputField>();var area=new GameObject("Text area",typeof(RectTransform),typeof(RectMask2D)).GetComponent<RectTransform>();area.SetParent(box.transform,false);area.anchorMin=Vector2.zero;area.anchorMax=Vector2.one;area.offsetMin=new Vector2(12,6);area.offsetMax=new Vector2(-12,-6);var text=FarmUi.TmpLabel(area,"",Vector2.zero,size-new Vector2(24,12),24);var hint=FarmUi.TmpLabel(area,placeholder,Vector2.zero,size-new Vector2(24,12),22);hint.color=new Color(.7f,.75f,.7f);field.textViewport=area;field.textComponent=text;field.placeholder=hint;field.characterLimit=password?200:800;field.contentType=password?TMP_InputField.ContentType.Password:TMP_InputField.ContentType.Standard;return field;}
    }
}
