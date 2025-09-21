using UnityEngine;
using System.Linq;
using System.Collections.Generic; // List를 사용하기 위해 추가

/// <summary>
/// NPC의 이동 로직을 담당하는 스크립트.
/// Rigidbody를 사용하여 물리적으로 이동하며, 지정된 구역 내에서 무작위로 움직입니다.
/// 충돌 시 즉시 새로운 경로를 찾아 이동을 계속합니다.
/// SOLID: 단일 책임 원칙 (물리적 이동 및 경로 탐색)
/// 개방-폐쇄 원칙 (SetIsTalking 메서드로 외부에서 이동 제어 가능)
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class NPCMovement : MonoBehaviour
{
    // NPC의 Rigidbody 컴포넌트
    private Rigidbody npcRigidbody;

    // NPC의 이동 속도입니다.
    [Header("Movement Settings")]
    [Tooltip("NPC의 이동 속도입니다.")]
    [SerializeField]
    private float moveSpeed = 2f;

    // NPC가 다음 목적지까지 도달했을 때 기다릴 시간입니다.
    [Tooltip("다음 목적지로 이동하기 전 대기 시간입니다.")]
    [SerializeField]
    private float waitTime = 3f;

    // NPC의 회전 속도입니다. 값이 높을수록 더 빠르게 회전합니다.
    [Tooltip("NPC의 회전 속도입니다. 값이 높을수록 더 빠르게 회전합니다.")]
    [SerializeField]
    private float rotationSpeed = 5f;

    // NPC가 이동할 수 있는 평면(Plane) 오브젝트들을 할당합니다.
    [Header("Movement Constraints")]
    [Tooltip("NPC가 이동할 수 있는 평면(Plane) 오브젝트들을 할당합니다. 이 오브젝트들의 Transform을 기준으로 이동합니다.")]
    [SerializeField]
    private Transform[] movementPlanes;

    // NPC의 현재 목표 지점입니다.
    private Vector3 targetPosition;
    // 다음 이동까지 남은 대기 시간을 계산합니다.
    private float waitTimer;

    // NPC가 현재 대화 중인지 상태를 추적하는 변수
    private bool isTalking = false;

    //----------------------------------------------------------------------------------------------------------------
    // MonoBehaviour 생명주기 메서드
    //----------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// MonoBehaviour의 Start 메서드. 게임 시작 시 한 번 호출됩니다.
    /// </summary>
    private void Start()
    {
        // Rigidbody 컴포넌트를 가져옵니다.
        npcRigidbody = GetComponent<Rigidbody>();
        // Rigidbody를 isKinematic = false로 설정하여 물리적 힘에 영향을 받도록 합니다.
        npcRigidbody.isKinematic = false;

        // 첫 번째 목표 위치를 설정합니다.
        SetNewTargetPosition();
    }

    /// <summary>
    /// MonoBehaviour의 FixedUpdate 메서드. 물리 연산이 이루어지는 프레임마다 호출됩니다.
    /// </summary>
    private void FixedUpdate()
    {
        // NPC가 대화 중이 아닐 때만 이동 로직을 실행합니다.
        if (!isTalking)
        {
            MoveToTarget();
        }
    }

    /// <summary>
    /// NPC의 Collider가 다른 Collider와 충돌했을 때 호출됩니다.
    /// </summary>
    /// <param name="collision">충돌에 대한 정보를 담고 있는 Collision 객체입니다.</param>
    private void OnCollisionEnter(Collision collision)
    {
        // 충돌한 오브젝트가 NPC 자신이 아닌지 확인하여 무한 충돌 루프를 방지합니다.
        // 또한 플레이어와의 충돌은 Interaction 스크립트에서 관리하므로 제외할 수 있습니다.
        if (collision.gameObject.CompareTag("NPC") || collision.gameObject.CompareTag("Player"))
        {
            return;
        }

        // 1. 충돌 시 즉시 현재 목표 위치를 무시하고 새로운 목표를 설정합니다.
        // 이를 통해 벽에 부딪혔을 때 멈추지 않고 즉시 방향을 전환하게 됩니다.
        SetNewTargetPosition();
    }

    //----------------------------------------------------------------------------------------------------------------
    // 이동 로직 메서드
    //----------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// NPC를 목표 위치로 이동시키고, 방향을 바라보게 합니다.
    /// </summary>
    private void MoveToTarget()
    {
        // 현재 위치와 목표 위치의 거리가 매우 가까우면(거의 도착하면)
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            // NPC가 멈추도록 선형 속도를 0으로 설정합니다.
            npcRigidbody.linearVelocity = Vector3.zero;

            // 대기 타이머를 줄입니다.
            waitTimer -= Time.deltaTime;
            // 대기 시간이 0보다 작거나 같아지면
            if (waitTimer <= 0)
            {
                // 새로운 목표 위치를 설정하고 대기 시간을 초기화합니다.
                SetNewTargetPosition();
            }
        }
        else
        {
            // 목표 위치로 이동할 방향 벡터를 계산합니다.
            Vector3 direction = (targetPosition - transform.position).normalized;
            // Rigidbody를 사용해 물리적으로 이동합니다.
            // 물리 연산에 더 적합한 linearVelocity를 사용합니다.
            npcRigidbody.linearVelocity = direction * moveSpeed;

            // NPC가 이동하는 방향을 바라보도록 회전합니다.
            // Y축을 기준으로 회전 방향을 설정하여 NPC가 기울어지지 않도록 합니다.
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
    }

    /// <summary>
    /// NPC가 이동할 새로운 목표 위치를 무작위로 설정합니다.
    /// 이는 선택된 발판의 실제 경계 내에 있도록 합니다.
    /// </summary>
    private void SetNewTargetPosition()
    {
        if (movementPlanes == null || movementPlanes.Length == 0)
        {
            Debug.LogError("movementPlanes 배열이 비어있습니다. 하나 이상의 Plane Transform을 할당해야 합니다.");
            return;
        }

        Transform selectedPlane = movementPlanes[Random.Range(0, movementPlanes.Length)];

        Collider planeCollider = selectedPlane.GetComponent<Collider>();
        if (planeCollider == null)
        {
            Debug.LogError("선택된 발판에 Collider 컴포넌트가 없습니다. Collider를 추가해야 합니다.");
            return;
        }

        Vector3 randomPointXZ = new Vector3(
            Random.Range(planeCollider.bounds.min.x, planeCollider.bounds.max.x),
            transform.position.y, // 수동으로 설정한 Y 좌표를 그대로 사용합니다.
            Random.Range(planeCollider.bounds.min.z, planeCollider.bounds.max.z)
        );

        targetPosition = randomPointXZ;

        // 이동하기 전 대기 시간을 초기화합니다.
        waitTimer = waitTime;
    }

    /// <summary>
    /// NPC의 이동을 제어하는 메서드. 외부 스크립트(예: DialogueManager)에서 호출합니다.
    /// </summary>
    /// <param name="talkingState">대화 상태 여부</param>
    public void SetIsTalking(bool talkingState)
    {
        isTalking = talkingState;
        // 대화가 시작되면 즉시 이동을 멈춥니다.
        if (isTalking)
        {
            npcRigidbody.linearVelocity = Vector3.zero;
            // 현재 위치를 목표로 재설정하여 MoveToTarget 메서드가 실행되지 않도록 합니다.
            targetPosition = transform.position;
        }
    }
}