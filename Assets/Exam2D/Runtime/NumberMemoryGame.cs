using UnityEngine;
using UnityEngine.UI;

namespace Midterm2D
{
    public struct NumberMemoryReward
    {
        public int coins, stones;
        public string message;
        public NumberMemoryReward(int coins, int stones, string message)
        { this.coins=coins; this.stones=stones; this.message=message; }
    }

    public sealed class NumberMemoryGame : MonoBehaviour
    {
        public const int TotalQuestions = 5;
        public const float RoundSeconds = 5f;
        public static readonly Color[] NumberColors = {
            new Color(.42f,.22f,.09f), new Color(.12f,.48f,.23f),
            new Color(.07f,.15f,.43f), new Color(.48f,.16f,.62f) };
        [SerializeField] private Button[] cells;
        [SerializeField] private Text[] numbers;
        [SerializeField] private Text questionLabel, targetLabel, scoreLabel, timerLabel, completedLabel, resultLabel;
        [SerializeField] private Image timerBar;
        [SerializeField] private GameObject resultPanel;
        private int[] values = new int[49];
        private int[] colorIndices = new int[49];
        private float timeoutPause;
        public int Question { get; private set; }
        public int Completed { get; private set; }
        public int Correct { get; private set; }
        public int Target { get; private set; }
        public float Remaining { get; private set; }
        public bool Finished { get; private set; }
        public bool TimedOut { get; private set; }
        public int[] Values => (int[])values.Clone();
        public int[] ColorIndices => (int[])colorIndices.Clone();
        public Button[] Cells => cells;
        // Farm supplies its wallet; standalone play keeps a session wallet only.
        public System.Func<int,NumberMemoryReward> RewardProvider { get; set; }
        public NumberMemoryReward LastReward { get; private set; }
        public int SessionCoins { get; private set; }
        public int SessionStones { get; private set; }

        public void Configure(Button[] buttons, Text[] labels, Text question, Text target, Text score,
            Text timer, Text completed, Text result, Image bar, GameObject panel)
        {
            cells=buttons; numbers=labels; questionLabel=question; targetLabel=target; scoreLabel=score;
            timerLabel=timer; completedLabel=completed; resultLabel=result; timerBar=bar; resultPanel=panel;
        }

        private void Awake()
        {
            for(int i=0;i<cells.Length;i++)
            { int index=i; cells[i].onClick.AddListener(()=>SelectCell(index)); }
        }
        private void Start() { StartNewGame(); }
        private void Update() { AdvanceClock(Time.unscaledDeltaTime); }

        public void StartNewGame()
        {
            Question=1; Completed=Correct=0; Finished=TimedOut=false; timeoutPause=0;
            LastReward=default;
            resultPanel.SetActive(false); BeginRound();
        }

        private void BeginRound()
        {
            var pool=new int[100]; for(int i=0;i<pool.Length;i++)pool[i]=i;
            for(int i=0;i<49;i++)
            {
                int j=Random.Range(i,pool.Length); int swap=pool[i];pool[i]=pool[j];pool[j]=swap;
                values[i]=pool[i]; colorIndices[i]=Random.Range(0,NumberColors.Length);
                numbers[i].text=values[i].ToString(); numbers[i].color=NumberColors[colorIndices[i]];
                cells[i].interactable=true;
            }
            Target=values[Random.Range(0,values.Length)]; Remaining=RoundSeconds; TimedOut=false;
            RefreshLabels();
        }

        public void SelectCell(int index)
        {
            if(Finished || TimedOut || index<0 || index>=values.Length || values[index]!=Target) return;
            Correct++; CompleteRound();
        }

        public void AdvanceClock(float elapsed)
        {
            if(Finished || elapsed<=0) return;
            if(TimedOut)
            {
                timeoutPause-=elapsed;
                if(timeoutPause<=0) CompleteRound();
                return;
            }
            Remaining=Mathf.Max(0,Remaining-elapsed); RefreshLabels();
            if(Remaining<=0)
            {
                TimedOut=true; timeoutPause=.15f;
                foreach(var cell in cells)cell.interactable=false;
            }
        }

        private void CompleteRound()
        {
            Completed++;
            if(Completed>=TotalQuestions)
            {
                Finished=true; TimedOut=false;
                foreach(var cell in cells)cell.interactable=false;
                LastReward=RewardProvider!=null ? RewardProvider(Correct) :
                    new NumberMemoryReward(Correct*20+(Correct==5?100:0),Correct==5?1:0,"Ví thưởng trong phiên chơi 2D");
                SessionCoins+=LastReward.coins; SessionStones+=LastReward.stones;
                resultLabel.text=$"HOÀN THÀNH • {Correct}/{TotalQuestions}\n+{LastReward.coins} xu • +{LastReward.stones} đá nâng cấp\n{LastReward.message}";
                resultPanel.SetActive(true); RefreshLabels();return;
            }
            Question=Completed+1; BeginRound();
        }

        private void RefreshLabels()
        {
            questionLabel.text=$"Câu: {Question}/{TotalQuestions}";
            completedLabel.text=$"Đã làm: {Completed}/{TotalQuestions}";
            targetLabel.text=Target.ToString(); scoreLabel.text=Correct.ToString();
            timerLabel.text=Mathf.CeilToInt(Remaining).ToString("00");
            timerBar.fillAmount=Remaining/RoundSeconds;
            timerBar.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,588f*timerBar.fillAmount);
        }
    }
}
