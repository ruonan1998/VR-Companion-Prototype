using UnityEngine;
using System.Collections;

public class CinematicPour : MonoBehaviour
{
    [Header("演员与台词")]
    public Animator aiAnimator;          
    public AudioSource aiAudio;          
    public string pourAnimationName = "GiveWine"; 

    [Header("环境音效")]
    public AudioSource pourSound;        
    public float soundDelay = 0.5f;      

    [Header("魔术道具")]
    public GameObject emptyGlass;        
    public GameObject fullGlass;         

    [Header("时间轴控制")]
    public float pourDuration = 3.0f;    

    [Header("🌟 AI 大脑联动 (新加入)")]
    public GeminiChat aiBrain;           // 连上它的大脑
    [Tooltip("倒完酒后，AI 的第一句内心OS/隐藏指令")]
    public string firstPrompt = "请用非常简短、随性、自然、朋友般的语气跟我打个招呼，并顺便提一下这杯刚倒好的红酒。";

    private bool hasStarted = false;

    void Start()
    {
        if (emptyGlass != null) emptyGlass.SetActive(true);
        if (fullGlass != null) fullGlass.SetActive(false);
    }

    public void StartCinematic()
    {
        if (!hasStarted)
        {
            hasStarted = true;
            Debug.Log("🎬 收到遥控指令！Action！倒酒剧情开始！");
            StartCoroutine(PlayPourSequence());
        }
    }

    private IEnumerator PlayPourSequence()
    {
        yield return new WaitForSeconds(1.0f);

        if (aiAnimator != null) aiAnimator.Play(pourAnimationName, -1, 0f);

        yield return new WaitForSeconds(soundDelay);
        if (pourSound != null) pourSound.Play();

        float remainingTime = pourDuration - soundDelay;
        if (remainingTime > 0) yield return new WaitForSeconds(remainingTime);

        if (emptyGlass != null) emptyGlass.SetActive(false);
        if (fullGlass != null) fullGlass.SetActive(true);

        if (aiAudio != null) aiAudio.Play();

        // ================= 🌟 情绪停顿与触发 =================
        Debug.Log("⏳ 倒酒动作结束，开始停顿 2 秒酝酿情绪...");
        yield return new WaitForSeconds(2.0f);

        if (aiBrain != null)
        {
            Debug.Log("🗣️ 停顿结束！命令大脑开始思考开场白！");
            aiBrain.AskGemini(firstPrompt);
        }
        else
        {
            Debug.LogError("🚨 导演！你忘了把 AI 大脑连到倒酒脚本上了！");
        }
    }
}