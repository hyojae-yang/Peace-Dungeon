using UnityEngine;
using System.Linq;

/// <summary>
/// NPC의 이동 로직을 담당하는 스크립트.
/// Rigidbody를 사용하여 물리적으로 이동하며, 지정된 구역 내에서 무작위로 움직입니다.
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

    /// <summary>
    /// MonoBehaviour의 Start 메서드. 게임 시작 시 한 번 호출됩니다.
    /// </summary>
    private void Start()
    {
        // Rigidbody 컴포넌트를 가져옵니다.
        npcRigidbody = GetComponent<Rigidbody>();
        // Rigidbody를 IsKinematic으로 설정하여 물리적인 힘에 영향을 받지 않도록 합니다.
        npcRigidbody.isKinematic = true;

        // 첫 번째 목표 위치를 설정합니다.
        SetNewTargetPosition();
    }

    /// <summary>
    /// MonoBehaviour의 Update 메서드. 매 프레임 호출됩니다.
    /// </summary>
    private void Update()
    {
        // NPC가 대화 중이 아닐 때만 이동 로직을 실행합니다.
        if (!isTalking)
        {
            MoveToTarget();
        }
    }

    /// <summary>
    /// NPC를 목표 위치로 이동시킵니다.
    /// </summary>
    private void MoveToTarget()
    {
        // 현재 위치와 목표 위치의 거리가 매우 가까우면(거의 도착하면)
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
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
            // 목표 위치로 부드럽게 이동합니다.
            // Rigidbody를 사용해 물리적으로 이동합니다.
            Vector3 direction = (targetPosition - transform.position).normalized;
            npcRigidbody.MovePosition(transform.position + direction * moveSpeed * Time.deltaTime);
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
            // Rigidbody의 속도를 0으로 설정해 즉시 멈춥니다.
            npcRigidbody.linearVelocity = Vector3.zero;
            // 현재 위치를 목표로 재설정하여 MoveToTarget 메서드가 실행되지 않도록 합니다.
            targetPosition = transform.position;
        }
    }
}