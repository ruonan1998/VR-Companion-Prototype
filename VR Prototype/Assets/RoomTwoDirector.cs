using UnityEngine;
using System.Collections;

public class RoomTwoDirector : MonoBehaviour
{
    [Header("演员配置")]
    [Tooltip("把挂着 GeminiChat 的那个物体拖进来")]
    public GeminiChat aiBrain;

    [Header("时间轴控制")]
    [Tooltip("填入倒酒需要的时间 + 你想要的 2 秒停顿（比如倒酒要 3 秒，这里就填 5）")]
    public float delayBeforeSpeaking = 5.0f;

    [Header("第一句台词")]
    [TextArea]
    public string firstPrompt = "请用非常简短、随性、自然、朋友般的语气跟我打个招呼，并顺便提一下这杯刚倒好的红酒。";

    private bool hasTriggered = false;

    // 当玩家传送到这个隐形盒子里时，自动触发
    void OnTriggerEnter(Collider other)
    {
        // 确保只触发一次
        if (!hasTriggered)
        {
            hasTriggered = true;
            Debug.Log("👣 玩家落地场景二！导演打板，开始倒计时...");
            StartCoroutine(WaitAndSpeak());
        }
    }

    private IEnumerator WaitAndSpeak()
    {
        Debug.Log($"⏳ 全场安静，等待 {delayBeforeSpeaking} 秒...");
        // 等待倒酒完成和留白
        yield return new WaitForSeconds(delayBeforeSpeaking);
        
        if (aiBrain != null)
        {
            Debug.Log("🗣️ 停顿结束，命令 AI 开口！");
            aiBrain.AskGemini(firstPrompt);
        }
        else
        {
            Debug.LogError("🚨 导演，你忘了把 AI 大脑拖进面板里！");
        }
    }
}