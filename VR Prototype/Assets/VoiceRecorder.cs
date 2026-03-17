using System;
using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.InputSystem;

public class VoiceRecorder : MonoBehaviour
{
    [Header("👂 Wit.ai 语音识别设置")]
    public string witAccessToken = "YOUR_WIT_TOKEN";
    public GeminiChat aiBrain;

    [Header("🎵 交互音效")]
    public AudioSource dingSound; 

    private string micName;
    private AudioClip recordedClip;
    private bool isRecording = false;

    void Start()
    {
        if (Microphone.devices.Length > 0)
        {
            micName = Microphone.devices[0];
            Debug.Log("🎤 找到麦克风: " + micName);
        }
    }

    void Update()
    {
        if (Keyboard.current == null) return;

        // 🌟 核心修改：我们彻底去掉了那行阻止玩家按键的代码！玩家现在随时可以按空格！
        if (Keyboard.current.spaceKey.wasPressedThisFrame && !isRecording)
        {
            StartRecording();
        }
        if (Keyboard.current.spaceKey.wasReleasedThisFrame && isRecording)
        {
            StopRecording();
        }
    }

    private void StartRecording()
    {
        // 1. 播放“叮”提示音
        if (dingSound != null) dingSound.Play();

        // 🌟 2. 联动打断：一旦玩家按下空格键准备说话，立刻命令 AI 闭嘴并清空大脑！
        if (aiBrain != null)
        {
            aiBrain.StopSpeaking();
        }

        // 3. 开始录音
        isRecording = true;
        recordedClip = Microphone.Start(micName, false, 10, 16000);
        Debug.Log("🔴 正在录音... (听到叮声后请说话)");
    }

    private void StopRecording()
    {
        isRecording = false;
        Microphone.End(micName);
        Debug.Log("⏹️ 录音结束，正在发送...");
        StartCoroutine(SendAudioToWitAi(recordedClip));
    }

    private IEnumerator SendAudioToWitAi(AudioClip clip)
    {
        byte[] wavData = ConvertClipToWav(clip);
        
        string url = "https://api.wit.ai/speech?v=20240317";
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(wavData);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Authorization", "Bearer " + witAccessToken.Trim());
            request.SetRequestHeader("Content-Type", "audio/wav");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("🚨 语音识别失败: " + request.error);
            }
            else
            {
                string recognizedText = ExtractTextFromWit(request.downloadHandler.text);
                Debug.Log("🗣️ 你说: " + recognizedText);
                
                if (!string.IsNullOrEmpty(recognizedText) && aiBrain != null)
                {
                    aiBrain.AskGemini(recognizedText);
                }
            }
        }
    }

    private string ExtractTextFromWit(string json)
    {
        try
        {
            int lastIndex = json.LastIndexOf("\"text\": \"");
            if (lastIndex == -1) return "没听清，请再说一遍。";

            lastIndex += 9;
            int endIndex = json.IndexOf("\"", lastIndex);
            return json.Substring(lastIndex, endIndex - lastIndex);
        }
        catch { return ""; }
    }

    private byte[] ConvertClipToWav(AudioClip clip)
    {
        MemoryStream stream = new MemoryStream();
        using (BinaryWriter writer = new BinaryWriter(stream))
        {
            writer.Write(Encoding.UTF8.GetBytes("RIFF"));
            writer.Write(0); 
            writer.Write(Encoding.UTF8.GetBytes("WAVE"));
            writer.Write(Encoding.UTF8.GetBytes("fmt "));
            writer.Write(16);
            writer.Write((short)1);
            writer.Write((short)clip.channels);
            writer.Write(clip.frequency);
            writer.Write(clip.frequency * clip.channels * 2);
            writer.Write((short)(clip.channels * 2));
            writer.Write((short)16);
            writer.Write(Encoding.UTF8.GetBytes("data"));
            writer.Write(clip.samples * clip.channels * 2);

            float[] samples = new float[clip.samples * clip.channels];
            clip.GetData(samples, 0);
            int intData = 0;
            foreach (float sample in samples)
            {
                intData = (int)(sample * 32767);
                if (intData > 32767) intData = 32767;
                if (intData < -32768) intData = -32768;
                writer.Write((short)intData);
            }
            
            writer.Seek(4, SeekOrigin.Begin);
            writer.Write((int)(stream.Length - 8));
        }
        return stream.ToArray();
    }
}