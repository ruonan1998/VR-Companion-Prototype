using UnityEngine;
using UnityEngine.UI; // 引入 UI 组件
using UnityEngine.InputSystem; // 引入新版输入系统

public class DebugChatUI : MonoBehaviour
{
    [Header("核心连接")]
    [Tooltip("把挂着 GeminiChat 的大脑拖进来")]
    public GeminiChat aiBrain;
    
    [Tooltip("把你在屏幕上创建的 InputField (输入框) 拖进来")]
    public InputField inputField; 

    void Update()
    {
        // 防错检测
        if (Keyboard.current == null || inputField == null || aiBrain == null) return;

        // 如果按下了回车键 (Enter)，并且输入框里有字
        if (Keyboard.current.enterKey.wasPressedThisFrame && !string.IsNullOrEmpty(inputField.text))
        {
            SendTextToBrain();
        }
    }

    // 也可以绑定给 UI 上的“发送”按钮
    public void SendTextToBrain()
    {
        if (inputField != null && !string.IsNullOrEmpty(inputField.text))
        {
            string textToSend = inputField.text;
            Debug.Log("⌨️ 导演打字发送: " + textToSend);
            
            // 直接把文字喂给大脑！
            aiBrain.AskGemini(textToSend);
            
            // 发送完自动清空输入框，方便下次打字
            inputField.text = ""; 
            inputField.ActivateInputField(); // 自动让输入框重新获得焦点
        }
    }
}