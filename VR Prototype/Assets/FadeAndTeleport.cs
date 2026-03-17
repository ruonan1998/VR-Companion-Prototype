using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FadeAndTeleport : MonoBehaviour
{
    [Header("转场视觉组件")]
    public Image fadeImage;          
    public float fadeDuration = 1.5f;

    [Header("空间传送设置")]
    public Transform playerRig;      
    public Transform targetLocation; 

    [Header("场景管理 (解决幽灵触发)")]
    public GameObject oldRoom;       // 传送后要被无情关掉的房间一

    [Header("房间二联动")]
    public CinematicPour room2Cinematic; // ⭐️ 新增：房间二的场记脚本

    public void StartTransition()
    {
        StartCoroutine(TransitionRoutine());
    }

    private IEnumerator TransitionRoutine()
    {
        // 1. 闭眼：白屏渐渐出现
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            Color c = fadeImage.color;
            float timer = 0f;
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                c.a = Mathf.Lerp(0f, 1f, timer / fadeDuration);
                fadeImage.color = c;
                yield return null; 
            }
            c.a = 1f;
            fadeImage.color = c;
        }

        // 2. 乾坤大挪移 (瞬间传送)
        if (playerRig != null && targetLocation != null)
        {
            playerRig.position = targetLocation.position;
            playerRig.rotation = targetLocation.rotation; 
        }

        // 🌟 核心触发：传送过去的瞬间，直接按下房间二的遥控器开演！
        if (room2Cinematic != null)
        {
            room2Cinematic.StartCinematic();
        }

        // 🌟 过河拆桥：传送完瞬间把旧房间彻底关掉
        if (oldRoom != null)
        {
            oldRoom.SetActive(false);
            Debug.Log("已彻底关闭房间一！");
        }

        yield return new WaitForSeconds(0.5f);

        // 3. 睁眼：白屏渐渐消失
        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            float timer = 0f;
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                c.a = Mathf.Lerp(1f, 0f, timer / fadeDuration);
                fadeImage.color = c;
                yield return null;
            }
            c.a = 0f;
            fadeImage.color = c;
            fadeImage.gameObject.SetActive(false);
        }
    }
}