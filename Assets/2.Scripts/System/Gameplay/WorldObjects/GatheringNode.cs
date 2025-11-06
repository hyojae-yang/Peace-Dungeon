using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;

/// <summary>
/// 필드에 배치되어 플레이어와 상호작용하여 아이템을 지급하는 채집 노드 스크립트입니다.
/// 단일 책임 원칙(SRP)에 따라 상호작용 및 아이템 지급 로직을 담당합니다.
/// **아이템 수량은 항상 1개로 고정됩니다.**
/// </summary>
public class GatheringNode : MonoBehaviour
{
    // === 아이템 설정 구조체 ===
    [System.Serializable]
    public class GatherableItem
    {
        [Tooltip("지급할 BaseItemSO 템플릿을 직접 연결합니다. (필수)")]
        public BaseItemSO itemData;

        // 수정: 최소/최대 수량 필드를 제거하고 항상 1개를 지급하도록 로직 변경

        [Tooltip("이 아이템이 드롭될 가중치입니다. 높을수록 확률이 높습니다.")]
        [Range(0, 100)] public int dropWeight = 10;
    }

    // === 인스펙터 설정 ===
    [Header("아이템 드롭 설정")]
    [Tooltip("이 채집 노드에서 획득할 수 있는 아이템 목록입니다.")]
    public List<GatherableItem> availableItems = new List<GatherableItem>();

    [Header("상호작용 설정")]
    [Tooltip("플레이어의 상호작용 가능 거리입니다.")]
    public float interactionRange = 2.0f;
    [Tooltip("상호작용에 사용할 키입니다.")]
    public KeyCode interactionKey = KeyCode.E;

    [Header("재생성 설정")]
    [Tooltip("채집 후 오브젝트를 즉시 파괴할지(true) 재생성 대기 상태로 만들지(false) 결정합니다.")]
    public bool destroyAfterGathering = true;
    [Tooltip("destroyAfterGathering이 false일 때, 재생성까지 걸리는 시간(초)입니다. (최소 1초)")]
    public float respawnTime = 30f;

    [Header("사운드 설정")]
    public SFXType gatheringSFXType = SFXType.Item_Pickup;

    // === 내부 상태 및 참조 ===
    private bool isGathered = false; // 이미 채집되었는지 여부
    private bool isInteractable = false;
    private Collider nodeCollider;
    private Transform playerTransform;
    private PlayerCharacter playerCharacter;

    // [SOLID: Single Responsibility Principle]
    // 이 클래스는 GatherableItem에 정의된 아이템과 수량을 지급하는 책임만 가집니다.

    // === 초기화 및 설정 ===
    void Start()
    {
        nodeCollider = GetComponent<Collider>();

        playerCharacter = PlayerCharacter.Instance;

        if (playerCharacter != null)
        {
            playerTransform = playerCharacter.transform;
        }
        else
        {
            Debug.LogError($"[GatheringNode] PlayerCharacter 인스턴스를 찾을 수 없습니다. 상호작용 불가: {gameObject.name}", this);
            enabled = false;
            return;
        }

        if (availableItems == null || availableItems.Count == 0)
        {
            Debug.LogWarning($"[GatheringNode] '{gameObject.name}'에 할당된 아이템 목록이 없습니다. 이 노드는 작동하지 않습니다.");
            enabled = false;
        }
    }

    // === 거리 및 상호작용 체크 ===
    void Update()
    {
        if (isGathered || playerCharacter == null) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);

        bool wasInteractable = isInteractable;
        isInteractable = distance <= interactionRange;

        // 상호작용 프롬프트 표시/숨김
        if (isInteractable && !wasInteractable)
        {
            NotificationManager.Instance?.ShowInteractionPrompt($"E 키를 눌러 채집하기", this.gameObject);
        }
        else if (!isInteractable && wasInteractable)
        {
            NotificationManager.Instance?.HideInteractionPrompt(this.gameObject);
        }

        // 상호작용 키 입력 처리
        if (isInteractable && Input.GetKeyDown(interactionKey))
        {
            GatherResource();
        }
    }

    // === 채집 로직 ===
    private void GatherResource()
    {
        // 1. 상태 변경
        isGathered = true;
        isInteractable = false;
        NotificationManager.Instance?.HideInteractionPrompt(this.gameObject);

        // 2. 사운드
        SoundManager.Instance?.PlaySFX(gatheringSFXType, 0.5f);

        // 3. 아이템 지급
        GrantRandomItem();

        // 4. 후처리: 파괴 또는 재생성 시작
        if (destroyAfterGathering)
        {
            Destroy(gameObject, 0.5f);
        }
        else
        {
            // 재생성 로직 시작 (노드를 비활성화하고 코루틴 시작)
            gameObject.SetActive(false);
            Invoke(nameof(RespawnNode), respawnTime);
        }
    }

    /// <summary>
    /// 설정된 가중치에 따라 아이템 목록 중 하나를 무작위로 선택하고,
    /// 수량은 항상 1개로 고정하여 지급합니다.
    /// </summary>
    private void GrantRandomItem()
    {
        GatherableItem chosenItem = SelectItemByWeight();

        if (chosenItem == null || chosenItem.itemData == null)
        {
            Debug.LogWarning($"[GatheringNode] 선택된 아이템의 ItemDataSO가 할당되지 않았습니다. 지급 실패.");
            return;
        }

        // 수정: 수량을 무조건 1로 고정하여 Random.Range를 사용하지 않습니다.
        const int quantity = 1;

        // 인벤토리에 추가
        if (playerCharacter?.inventoryManager != null)
        {
            playerCharacter.inventoryManager.AddItem(chosenItem.itemData, quantity);

            // 알림 메시지 (quantity는 항상 1이므로 안전합니다.)
            NotificationManager.Instance?.ShowNotification(
                $"{chosenItem.itemData.itemName} {quantity}개 획득하였습니다.",
                NotificationType.General
            );
        }
    }

    /// <summary>
    /// 설정된 가중치에 따라 무작위 아이템을 선택하는 메서드입니다.
    /// </summary>
    /// <returns>무작위로 선택된 GatherableItem</returns>
    private GatherableItem SelectItemByWeight()
    {
        int totalWeight = 0;
        foreach (var item in availableItems)
        {
            if (item.dropWeight > 0)
            {
                totalWeight += item.dropWeight;
            }
        }

        if (totalWeight <= 0)
        {
            if (availableItems.Count > 0)
            {
                return availableItems[0];
            }
            return null;
        }

        int dropPoint = Random.Range(0, totalWeight);
        int currentWeight = 0;

        foreach (var item in availableItems)
        {
            if (item.dropWeight <= 0) continue;

            currentWeight += item.dropWeight;

            if (dropPoint < currentWeight)
            {
                return item;
            }
        }

        return availableItems[0];
    }

    // === 재생성 로직 (DestroyAfterGathering=false일 때만 호출됨) ===
    private void RespawnNode()
    {
        isGathered = false;
        isInteractable = false;
        gameObject.SetActive(true);
        Debug.Log($"[GatheringNode] '{gameObject.name}'이(가) 재생성되었습니다. 다시 채집 가능합니다.");
    }

    // === 디버그 시각화 ===
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}