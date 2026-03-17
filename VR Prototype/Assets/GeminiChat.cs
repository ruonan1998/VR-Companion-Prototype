using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class GeminiChat : MonoBehaviour
{
    [Header("🧠 谷歌 Gemini 设置")]
    public string geminiApiKey = "在这里填入API_KEY"; 
    // 🌟 终极修正：如果 1.5-flash 报 404，换成这个 gemini-pro 绝对能通
    private const string GEMINI_URL = "https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent";
    [Header("🗣️ ElevenLabs 设置")]
    public string elevenLabsApiKey = "在这里填入API_KEY";
    public string voiceId = "ErXwobaYiN019PkySvjV"; 
    public AudioSource aiAudioSource;

    [Header("🎭 联动组件")]
    public Animator animator;
    public AILocomotion aiLegs; 

    [Header("🛡️ 安全锁 (防止宕机/429)")]
    public float requestCooldown = 2.0f; 
    private float lastRequestTime = -5f;
    [HideInInspector] public bool isThinking = false; 

    void Start() { 
        Debug.Log("🧠 Jack 艺术版大脑：识别即注视 + 5s思考/留白逻辑就绪。");
    }

    public void StopSpeaking()
    {
        StopAllCoroutines(); 
        if (aiAudioSource != null && aiAudioSource.isPlaying) aiAudioSource.Stop();
        isThinking = false;
        if (aiLegs != null) aiLegs.StopAndFacePlayer(); 
    }

    // 🌟 核心改动：当 VoiceRecorder 识别到说话时，先调用这个
    public void AskGemini(string userText)
    {
        if (Time.time - lastRequestTime < requestCooldown) return;
        if (isThinking) return;

        lastRequestTime = Time.time;
        StartCoroutine(SendRequestToGemini(userText));
    }

    private IEnumerator SendRequestToGemini(string prompt)
    {
        isThinking = true; 
        
        // 1. 🌟 识别到说话，立刻停下并盯着你 (作为思考的开始)
        if (aiLegs != null) aiLegs.StopAndFacePlayer();
        Debug.Log("⏳ Jack 正在倾听并思考...");

        string jsonRequestBody = "{\"contents\": [{\"parts\":[{\"text\": \"" + prompt + "\"}]}]}";
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonRequestBody);

        using (UnityWebRequest request = UnityWebRequest.Put(GEMINI_URL + "?key=" + geminiApiKey.Trim(), bodyRaw))
        {
            request.method = "POST"; 
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"🚨 大脑报错: {request.responseCode}");
                ResetState(); 
            }
            else
            {
                string cleanAnswer = ExtractTextFromJson(request.downloadHandler.text);
                StartCoroutine(GetAudioFromElevenLabs(cleanAnswer));
            }
        }
    }

    private IEnumerator GetAudioFromElevenLabs(string textToSpeak)
    {
        string safeText = textToSpeak.Replace("\\", "").Replace("\"", "\\\"").Replace("\n", " ").Replace("\r", " ").Replace("\t", " ");
        string url = "https://api.elevenlabs.io/v1/text-to-speech/" + voiceId;
        string jsonRequestBody = "{\"text\": \"" + safeText + "\", \"model_id\": \"eleven_multilingual_v2\"}";
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonRequestBody);

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerAudioClip(url, AudioType.MPEG);
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("xi-api-key", elevenLabsApiKey.Trim());
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(request);
                if (aiAudioSource != null && clip != null)
                {
                    aiAudioSource.clip = clip;
                    aiAudioSource.Play();
                    
                    // 2. 话说完之前，保持注视
                    yield return new WaitForSeconds(clip.length);
                    
                    // 3. 🌟 话说完了，再额外原地对视 5 秒（介绍自己后的沉淀）
                    Debug.Log("⏳ 话说完了，原地站立 5 秒...");
                    yield return new WaitForSeconds(5.0f);
                }
            }
            ResetState(); 
        }
    }

    private void ResetState()
    {
        isThinking = false;
        if (aiLegs != null) aiLegs.ResumeWandering(); 
    }

    private string ExtractTextFromJson(string json)
    {
        try
        {
            string searchString = "\"text\": \"";
            int startIndex = json.IndexOf(searchString);
            if (startIndex == -1) return "...";
            startIndex += searchString.Length;
            int endIndex = json.IndexOf("\"", startIndex);
            return json.Substring(startIndex, endIndex - startIndex).Replace("\\n", " ").Replace("\\\"", "\"");
        }
        catch { return "..."; }
    }
}