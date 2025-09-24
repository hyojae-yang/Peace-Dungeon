using UnityEngine;
using System.Collections;

/// <summary>
/// '뿌리 묶기' 공격 효과를 담당하는 스크립트입니다.
/// 트리거 영역에 진입한 플레이어의 이동을 일정 시간 동안 제어하고, 스스로 사라집니다.
/// </summary>
public class RootTrap : MonoBehaviour
{
    [Tooltip("플레이어의 이동이 제약될 시간입니다.")]
    [SerializeField] private float immobilizeDuration = 3.0f;
    private bool isPlayerImmobilized = false; // 플레이어에게 효과를 적용했는지 확인하는 플래그
    private float originalSpeed; // 플레이어의 원래 이동 속도를 저장할 변수

    private void Start()
    {
        // 일정 시간이 지나면 스스로 파괴되도록 코루틴 시작
        StartCoroutine(SelfDestructRoutine());
    }

    /// <summary>
    /// 트리거 영역에 다른 콜라이더가 진입했을 때 호출됩니다.
    /// </summary>
    /// <param name="other">영역에 진입한 콜라이더</param>
    private void OnTriggerEnter(Collider other)
    {
        // 이미 효과를 적용했거나, 진입한 오브젝트가 플레이어가 아니면 함수를 종료합니다.
        if (isPlayerImmobilized || !other.CompareTag("Player"))
        {
            return;
        }

        // 플레이어 캐릭터와 컨트롤러 컴포넌트가 모두 유효한지 확인합니다.
        if (PlayerCharacter.Instance != null && PlayerCharacter.Instance.playerController != null)
        {
            isPlayerImmobilized = true;

            // 플레이어의 현재 이동 속도를 저장합니다.
            originalSpeed = PlayerCharacter.Instance.playerController.walkSpeed;

            // 이동 제약 코루틴을 시작합니다.
            StartCoroutine(ImmobilizePlayerRoutine(PlayerCharacter.Instance.playerController));
        }
    }

    /// <summary>
    /// 플레이어의 이동을 일정 시간 동안 제어하는 코루틴입니다.
    /// </summary>
    /// <param name="controller">플레이어 컨트롤러 컴포넌트</param>
    private IEnumerator ImmobilizePlayerRoutine(PlayerController controller)
    {
        // 플레이어 이동 속도를 0으로 설정하여 이동 불가 상태로 만듭니다.
        controller.walkSpeed = 0f;


        // 설정된 시간만큼 기다립니다.
        yield return new WaitForSeconds(immobilizeDuration);

        // 이동 제약을 해제합니다.
        // 이때, 원래 속도를 PlayerStats에서 다시 가져오도록 하여, 
        // 런타임에 속도가 변경되었더라도 최신 값을 적용합니다.
        controller.walkSpeed = PlayerCharacter.Instance.playerStats.moveSpeed;

        isPlayerImmobilized = false;
    }

    /// <summary>
    /// 일정 시간 후 오브젝트를 파괴하는 코루틴입니다.
    /// </summary>
    private IEnumerator SelfDestructRoutine()
    {
        // 이펙트가 지속될 시간만큼 기다립니다.
        yield return new WaitForSeconds(immobilizeDuration + 0.5f);
        Destroy(gameObject);
    }
}