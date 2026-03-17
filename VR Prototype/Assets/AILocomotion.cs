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
    }

    void Update()
    {
        // 持续把双脚的真实移动速度，汇报给肌肉（控制 Idle 和 Walk 切换）
        if (animator != null && agent != null)
        {
            animator.SetFloat("Speed", agent.velocity.magnitude);
        }

        if (isGivingAttention)
        {
            // 🌟 终极护身符：只要剧情锁没解开（还在倒酒阶段），绝对不转身，防止酒杯穿模！
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

        if (!canWalkFreely) return; 

        timer += Time.deltaTime;
        if (timer >= wanderTimer)
        {
            Vector3 newPos = RandomNavSphere(transform.position, wanderRadius, -1);
            if (agent != null && agent.isOnNavMesh) agent.SetDestination(newPos);
            timer = 0;
        }
    }

    public void StopAndFacePlayer()
    {
        isGivingAttention = true;
        if (agent != null && agent.isOnNavMesh) agent.isStopped = true;
        Debug.Log("👀 接收到打断指令，AI 停下动作准备听讲...");
    }

    public void ResumeWandering()
    {
        isGivingAttention = false;
        canWalkFreely = true; // 砸碎剧情锁
        
        // 🌟🌟🌟 终极闭环！倒酒结束，通知肌肉强行切换到站姿！
        if (animator != null)
        {
            animator.SetTrigger("FinishPour"); 
        }
        
        // 🌟 终极障眼法：AI 开步走的同时，灭掉手里的酒杯，点亮桌上的酒杯！
        if (handWine != null) handWine.SetActive(false);
        if (tableWine != null) tableWine.SetActive(true);

        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = false;
            timer = wanderTimer; 
        }
        Debug.Log("🚶‍♂️ 剧情锁解除，酒杯已放下，肌肉状态切换，AI 开始自由漫步...");
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