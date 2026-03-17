using UnityEngine;
using UnityEngine.AI;

public class AILocomotion : MonoBehaviour
{
    [Header("核心组件")]
    public NavMeshAgent agent;
    [Tooltip("把主角摄像机（Main Camera）拖进来")]
    public Transform player; 
    [Tooltip("把 AI 身上的 Animator 组件拖进来")]
    public Animator animator; 

    [Header("漫步设置")]
    public float wanderRadius = 5f; 
    public float wanderTimer = 6f;  
    
    [Header("剧情锁")]
    [Tooltip("如果打勾，开局就能乱跑；如果不勾，必须等对完第一次话才解锁。")]
    public bool canWalkFreely = false; 

    [Header("🍷 道具控制 (障眼法)")]
    [Tooltip("拖入 AI 手里拿着的酒杯")]
    public GameObject handWine;  
    [Tooltip("拖入吧台桌上放着的酒杯")]
    public GameObject tableWine; 

    private float timer;
    private bool isGivingAttention = false; 

    void Start()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        timer = wanderTimer;

        // 初始状态：确保桌上的酒杯是关闭的，手里的是打开的
        if(tableWine != null) tableWine.SetActive(false);
        if(handWine != null) handWine.SetActive(true);
    }

    void Update()
    {
        // 实时同步移动速度到动画机参数 Speed
        if (animator != null && agent != null)
        {
            animator.SetFloat("Speed", agent.velocity.magnitude);
        }

        if (isGivingAttention)
        {
            // 只有在可以自由行动（即倒酒动作已结束）的情况下才转身看向玩家，防止倒酒动作穿模
            if (canWalkFreely && player != null)
            {
                Vector3 direction = (player.position - transform.position).normalized;
                direction.y = 0; 
                if (direction != Vector3.zero)
                {
                    Quaternion lookRotation = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
                }
            }
            return; 
        }

        // 如果剧情锁没解开，就原地待命
        if (!canWalkFreely) return; 

        // 自由漫步逻辑
        timer += Time.deltaTime;
        if (timer >= wanderTimer)
        {
            Vector3 newPos = RandomNavSphere(transform.position, wanderRadius, -1);
            if (agent != null && agent.isOnNavMesh) agent.SetDestination(newPos);
            timer = 0;
        }
    }

    // 🧠 由 GeminiChat 发起对话时调用
    public void StopAndFacePlayer()
    {
        isGivingAttention = true;
        if (agent != null && agent.isOnNavMesh) 
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }
        Debug.Log("👀 Jack：正在倾听，暂时停止动作。");
    }

    // 🧠 由 GeminiChat 话说完后调用：触发倒酒结束、切换杯子、开始走路
    public void ResumeWandering()
    {
        // 1. 解除专注和剧情锁
        isGivingAttention = false;
        canWalkFreely = true; 
        
        // 2. 🌟 触发倒酒结束动画
        if (animator != null)
        {
            animator.SetTrigger("FinishPour"); 
        }
        
        // 3. 🍷 障眼法切换：手里杯子灭，桌上杯子亮
        if (handWine != null) handWine.SetActive(false);
        if (tableWine != null) tableWine.SetActive(true);

        // 4. 恢复双腿导航
        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = false;
            timer = wanderTimer; 
        }
        Debug.Log("🚶‍♂️ Jack：酒已斟好，我先失陪去走走。");
    }

    public static Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
    {
        Vector3 randDirection = Random.insideUnitSphere * dist;
        randDirection += origin;
        NavMeshHit navHit;
        NavMesh.SamplePosition(randDirection, out navHit, dist, layermask);
        return navHit.position;
    }
}