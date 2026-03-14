using System.Collections;
using UnityEngine;
using UnityEngine.UI; // 必须引入UI库

public class FadeAndTeleport : MonoBehaviour
{
    [Header("转场视觉组件")]
    public Image fadeImage;          // 刚才做的纯白Image
    public float fadeDuration = 1.5f;// 白屏渐变的时间（秒）

    [Header("空间传送设置")]
    public Transform playerRig;      // 你的整体玩家 (EZPZ XR Origin Rig)
    public Transform targetLocation; // 虚拟家的目标点 (放一个空物体定位置)

    // 这个方法可以被外界调用，启动传送
    public void StartTransition()
    {
        StartCoroutine(TransitionRoutine());
    }

    private IEnumerator TransitionRoutine()
    {
        // 确保图片是激活状态
        fadeImage.gameObject.SetActive(true);
        Color c = fadeImage.color;

        // 1. 闭眼：逐渐变白 (Alpha从 0 变 1)
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            c.a = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            fadeImage.color = c;
            yield return null; // 等待下一帧
        }
        c.a = 1f;
        fadeImage.color = c;

        // 2. 乾坤大挪移：瞬间把玩家移动到目标点！
        playerRig.position = targetLocation.position;
        playerRig.rotation = targetLocation.rotation; // 同步转身方向

        // 在全白状态下停顿一小会儿，给玩家心理缓冲时间
        yield return new WaitForSeconds(0.5f);

        // 3. 睁眼：逐渐恢复清晰 (Alpha从 1 变 0)
        timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            c.a = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            fadeImage.color = c;
            yield return null;
        }
        c.a = 0f;
        fadeImage.color = c;
        
        // 彻底隐藏白板，避免挡住后续操作
        fadeImage.gameObject.SetActive(false);
    }
}