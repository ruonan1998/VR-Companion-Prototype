using UnityEngine;
using System.Collections;

public class CompanionTrigger : MonoBehaviour
{
    [Header("视觉与听觉反馈")]
    public Light spotlight;              
    public AudioSource companionAudio;   
    public Animator companionAnimator;   

    [Header("唤醒设定")]
    public Color activeLightColor = new Color(0.9f, 0.8f, 0.6f); 
    public float activeLightIntensity = 5f;                      
    // 注意：这里不再是 Trigger 的名字，而是动画方块（State）的准确名字！
    public string wakeupStateName = "Wakeup";               
    public string idleStateName = "Idle";

    [Header("3D 实体进度条设置")]
    public Transform progressBar3D;       
    public float maxBarLength = 1.0f;     
    public float extraWaitTime = 1.5f;    

    [Header("转场联动")]
    public FadeAndTeleport teleportScript; 

    private bool isAwake = false;
    private bool isPlayerInZone = false;
    private bool hasConfirmed = false;
    private Coroutine fillCoroutine;  

    private Color originalLightColor;
    private float originalLightIntensity;

    private void Start()
    {
        if (spotlight != null)
        {
            originalLightColor = spotlight.color;
            originalLightIntensity = spotlight.intensity;
        }

        if (progressBar3D != null)
        {
            progressBar3D.gameObject.SetActive(false); 
            Vector3 startScale = progressBar3D.localScale;
            startScale.x = 0f;
            progressBar3D.localScale = startScale;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if ((other.CompareTag("Player") || other.name.Contains("XR") || other.name.Contains("Camera")) && !hasConfirmed)
        {
            isPlayerInZone = true;

            if (!isAwake)
            {
                ActivateCompanion();
            }

            if (fillCoroutine != null) StopCoroutine(fillCoroutine);
            fillCoroutine = StartCoroutine(StartProgressBar());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if ((other.CompareTag("Player") || other.name.Contains("XR") || other.name.Contains("Camera")) && !hasConfirmed)
        {
            isPlayerInZone = false;
            
            if (fillCoroutine != null)
            {
                StopCoroutine(fillCoroutine);
                fillCoroutine = null;
            }
            if (progressBar3D != null)
            {
                progressBar3D.gameObject.SetActive(false);
            }

            if (companionAudio != null && companionAudio.isPlaying)
            {
                companionAudio.Stop();
            }

            // ================= 暴力物理重置 =================
            if (companionAnimator != null)
            {
                // 无视一切规则，强行把动画拍回 "Idle" 方块的第 0 秒！
                companionAnimator.Play(idleStateName, -1, 0f); 
            }
            // =================================================

            if (spotlight != null)
            {
                spotlight.color = originalLightColor;
                spotlight.intensity = originalLightIntensity;
            }

            isAwake = false; 
            Debug.Log("玩家出圈！已强行切回 Idle 状态！");
        }
    }

    public void ActivateCompanion()
    {
        isAwake = true;

        if (spotlight != null)
        {
            spotlight.color = activeLightColor;
            spotlight.intensity = activeLightIntensity;
        }

        // ================= 暴力物理触发 =================
        if (companionAnimator != null)
        {
            // 无视一切连线，强行播放名为 "Wakeup" 的动画方块！
            companionAnimator.Play(wakeupStateName, -1, 0f);
        }
        // =================================================

        if (companionAudio != null && companionAudio.clip != null)
        {
            companionAudio.Play();
        }
    }

    private IEnumerator StartProgressBar()
    {
        if (progressBar3D != null)
        {
            progressBar3D.gameObject.SetActive(true);
            Vector3 initialScale = progressBar3D.localScale;
            initialScale.x = 0f;
            progressBar3D.localScale = initialScale;
        }

        float totalWaitTime = (companionAudio != null && companionAudio.clip != null) 
                              ? companionAudio.clip.length + extraWaitTime 
                              : 3f + extraWaitTime;

        float timer = 0f;

        while (timer < totalWaitTime)
        {
            timer += Time.deltaTime;
            if (progressBar3D != null)
            {
                Vector3 currentScale = progressBar3D.localScale;
                currentScale.x = Mathf.Lerp(0f, maxBarLength, timer / totalWaitTime);
                progressBar3D.localScale = currentScale;
            }
            yield return null; 
        }

        if (isPlayerInZone && !hasConfirmed)
        {
            hasConfirmed = true;
            Debug.Log("3D 读条完成！开始传送...");

            if (progressBar3D != null)
            {
                progressBar3D.gameObject.SetActive(false);
            }

            if (teleportScript != null)
            {
                teleportScript.StartTransition();
            }
        }
    }
}