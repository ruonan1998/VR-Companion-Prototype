using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class GeminiChat : MonoBehaviour
{
    [Header("🧠 谷歌 Gemini 大脑设置")]
    public string geminiApiKey = "YOUR_GEMINI_API_KEY"; 
    private const string GEMINI_URL = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent";

    [Header("🗣️ ElevenLabs 嘴巴设置")]
    public string elevenLabsApiKey = "YOUR_ELEVENLABS_API_KEY";
    public string voiceId = "ErXwobaYiN019PkySvjV"; 
    public AudioSource aiAudioSource;

    [HideInInspector] 
    public bool isThinking = false; 

    void Start()
    {
        Debug.Log("🧠 Gemini 大脑已开机，防刷屏+防标点符号报错版！");
    }

    public void StopSpeaking()
    {
        StopAllCoroutines(); 
        if (aiAudioSource != null && aiAudioSource.isPlaying)
        {
            aiAudioSource.Stop();
        }
        isThinking = false;
        Debug.Log("🛑 玩家打断了 AI 的施法！AI 瞬间闭嘴听讲。");
    }

    public void AskGemini(string userText)
    {
        if (isThinking) return;
        StartCoroutine(SendRequestToGemini(userText));
    }

    private IEnumerator SendRequestToGemini(string prompt)
    {
        isThinking = true; 

        string jsonRequestBody = "{\"contents\": [{\"parts\":[{\"text\": \"" + prompt + "\"}]}]}";
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonRequestBody);
        string requestUrl = GEMINI_URL + "?key=" + geminiApiKey.Trim();

        using (UnityWebRequest request = UnityWebRequest.Put(requestUrl, bodyRaw))
        {
            request.method = "POST"; 
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("🚨 大脑连接失败: " + request.responseCode);
                isThinking = false; 
            }
            else
            {
                string cleanAnswer = ExtractTextFromJson(request.downloadHandler.text);
                Debug.Log("🤖 大脑想好了台词: " + cleanAnswer);
                StartCoroutine(GetAudioFromElevenLabs(cleanAnswer));
            }
        }
    }

    private IEnumerator GetAudioFromElevenLabs(string textToSpeak)
    {
        // 🌟 终极净化法：把所有可能引发 422 报错的标点符号、换行、回车、制表符统统碾碎！
        string safeText = textToSpeak.Replace("\\", "")
                                     .Replace("\"", "\\\"")
                                     .Replace("\n", " ")
                                     .Replace("\r", " ")
                                     .Replace("\t", " ");
                                     
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

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("🚨 语音生成失败: " + request.error);
                isThinking = false; 
            }
            else
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(request);
                if (aiAudioSource != null && clip != null)
                {
                    aiAudioSource.clip = clip;
                    aiAudioSource.Play();
                    StartCoroutine(UnlockAfterAudio(clip.length));
                }
                else
                {
                    isThinking = false;
                }
            }
        }
    }

    private IEnumerator UnlockAfterAudio(float audioLength)
    {
        yield return new WaitForSeconds(audioLength);
        isThinking = false;
        Debug.Log("✅ AI 发言完毕，可以进行下一次对话了！");
    }

    private string ExtractTextFromJson(string json)
    {
        try
        {
            string searchString = "\"text\": \"";
            int startIndex = json.IndexOf(searchString);
            if (startIndex == -1) return "没听清，请再说一遍。";
            startIndex += searchString.Length;
            int endIndex = json.IndexOf("\"", startIndex);
            // 这里也做一次基础清理
            return json.Substring(startIndex, endIndex - startIndex).Replace("\\n", " ").Replace("\\\"", "\"").Replace("\\t", " ");
        }
        catch { return "解析异常。"; }
    }
}