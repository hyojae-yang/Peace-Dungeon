using UnityEngine;
using System.Collections.Generic;
using System; // Serializable을 사용하기 위해 필요합니다.
using Random = UnityEngine.Random; // Random.Range 충돌 방지

/// <summary>
/// 플레이어 레벨에 따라 장비 아이템 목록 중 하나를 선택하고, 무작위 등급을 부여하여 지급하는 보물 상자 스크립트입니다.
/// MonsterLoot와 유사하게 ScriptableObject를 직접 참조하는 방식을 사용합니다.
/// 던전 진입 이벤트(DungeonManager.OnDungeonEnter)에 반응하여 스스로 상태를 초기화합니다.
/// </summary>
[RequireComponent(typeof(Animator))]
public class TreasureChest : MonoBehaviour
{
    // === 이템 데이터 구조 및 등급 가중치 (기존 유지) ===
    [System.Serializable]
    public class ItemReward
    {
        [Tooltip("지급할 EquipmentItemSO 템플릿을 직접 연결합니다. (필수)")]
        public EquipmentItemSO itemData;
    }

    [Serializable]
    public struct GradeDropWeight
    {
        public ItemGrade grade;
        public int weight; // 해당 등급이 드롭될 가중치입니다.
    }

    [Header("아이템 등급 드롭 확률 설정")]
    public List<GradeDropWeight> gradeDropWeights = new List<GradeDropWeight>();

    // === 레벨 구간 및 아이템 드롭 설정 (기존 유지) ===
    [Header("레벨 구간 정의")]
    [Tooltip("각 레벨 구간의 상한선입니다. 총 4개의 경계값을 설정합니다.")]
    public int[] levelTiers = { 10, 20, 25, 30 };

    [Header("아이템 드롭 설정 (5단계)")]
    public List<ItemReward> lootTier1 = new List<ItemReward>();
    public List<ItemReward> lootTier2 = new List<ItemReward>();
    public List<ItemReward> lootTier3 = new List<ItemReward>();
    public List<ItemReward> lootTier4 = new List<ItemReward>();
    public List<ItemReward> lootTier5 = new List<ItemReward>();

    // === 상호작용 및 애니메이션 변수 (기존 유지) ===
    [Header("상호작용 설정")]
    public float interactionRange = 3.0f;
    public KeyCode interactionKey = KeyCode.E;

    [Header("애니메이션 설정")]
    public string openTriggerName = "Open";
    [Tooltip("상자가 닫힌 기본 상태의 애니메이션 클립 이름입니다. 초기화 시 사용됩니다.")]
    public string idleAnimationStateName = "close"; // 닫힌 상태 애니메이션 이름을 인스펙터로 분리

    [Header("사운드 및 알림 설정")]
    public SFXType lootSFXType = SFXType.Item_Goodpickup;

    // === 내부 상태 관리 및 참조 ===
    private bool hasBeenOpened = false;
    private bool isInteractable = false;
    private Animator chestAnimator;
    private Transform playerTransform;
    private PlayerCharacter playerCharacter;

    // === 초기화 및 이벤트 구독 ===
    void Start()
    {
        chestAnimator = GetComponent<Animator>();
        playerCharacter = PlayerCharacter.Instance;

        if (playerCharacter != null)
        {
            playerTransform = playerCharacter.transform;
        }
        else
        {
            Debug.LogError("PlayerCharacter 인스턴스를 찾을 수 없습니다. 상호작용이 불가능합니다.", this);
            enabled = false;
        }

        // 몬스터 루트 방식의 핵심: DungeonManager 이벤트에 구독
        // 던전 진입 시 (재진입 포함) 상자 상태를 초기화합니다.
        DungeonManager.OnDungeonEnter += ResetChestState;

        // (옵션) 게임 시작 시 상자가 이미 열린 상태로 저장되어 있을 경우를 대비하여 초기화
        // 이 상자는 저장되지 않는 오브젝트라고 가정하고, 던전 진입 시에만 초기화합니다.
    }

    // === 이벤트 구독 해제 ===
    private void OnDestroy()
    {
        // 메모리 누수 방지: 오브젝트 파괴 시 이벤트 구독을 해제합니다.
        DungeonManager.OnDungeonEnter -= ResetChestState;
    }

    // === 핵심 추가: 상자 초기화 메서드 ===
    /// <summary>
    /// DungeonManager.OnDungeonEnter 이벤트에 의해 호출됩니다.
    /// 상자의 상태를 '닫힌' 상태로 되돌리고 재상호작용을 가능하게 합니다.
    /// </summary>
    public void ResetChestState()
    {
        // 1. 상태 변수 초기화
        hasBeenOpened = false;
        isInteractable = false; // Update()에서 다시 계산됨

        // 2. 비주얼 (애니메이션) 초기화
        if (chestAnimator != null)
        {
            // 애니메이터 상태를 닫힌 기본 상태(Idle)로 강제 설정합니다.
            // (0f는 애니메이션 재생 시작 시간, 0은 레이어 인덱스)
            chestAnimator.SetTrigger(idleAnimationStateName);
        }
    }

    // === 거리 및 상호작용 체크 (기존 유지) ===
    void Update()
    {
        if (hasBeenOpened || playerCharacter == null) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);

        bool wasInteractable = isInteractable;
        isInteractable = distance <= interactionRange;

        if (isInteractable && !wasInteractable)
        {
            NotificationManager.Instance?.ShowInteractionPrompt("E 키를 눌러 상자열기", this.gameObject);
        }
        else if (!isInteractable && wasInteractable)
        {
            NotificationManager.Instance?.HideInteractionPrompt(this.gameObject);
        }

        if (isInteractable && Input.GetKeyDown(interactionKey))
        {
            OpenChest();
        }
    }

    // === 상자 열기 로직 (기존 유지) ===
    private void OpenChest()
    {
        // 상태 설정
        hasBeenOpened = true;
        isInteractable = false;
        NotificationManager.Instance?.HideInteractionPrompt(this.gameObject);

        // 애니메이션 재생 및 아이템 지급
        chestAnimator?.SetTrigger(openTriggerName);
        GrantItemBasedOnLevel();
    }

    // === 아이템 지급 로직 (기존 유지) ===
    private void GrantItemBasedOnLevel()
    {
        if (playerCharacter?.playerStats == null || ItemGenerator.Instance == null)
        {
            Debug.LogError("필요한 컴포넌트가 부족하여 아이템 지급이 불가능합니다.");
            return;
        }

        int playerLevel = playerCharacter.playerStats.level;
        List<ItemReward> lootList = GetLootListByLevel(playerLevel);
        int tierIndex = GetTierIndexByLevel(playerLevel);

        if (lootList != null && lootList.Count > 0)
        {
            int randomIndex = Random.Range(0, lootList.Count);
            ItemReward chosenItem = lootList[randomIndex];

            EquipmentItemSO equipmentItemTemplate = chosenItem.itemData;

            if (equipmentItemTemplate != null)
            {
                ItemGrade randomGrade = GetRandomItemGrade();
                EquipmentItemSO generatedItem = ItemGenerator.Instance.GenerateItem(equipmentItemTemplate, randomGrade);

                PlayerCharacter.Instance.inventoryManager.AddItem(generatedItem, 1);

                NotificationManager.Instance?.ShowNotification($"{generatedItem.itemName}를(을) 획득하였습니다.", NotificationType.General);
                SoundManager.Instance?.PlaySFX(lootSFXType, 0.5f);
            }
            else
            {
                Debug.LogError($"ItemReward 목록의 {tierIndex} Tier, {randomIndex}번째 슬롯에 EquipmentItemSO 템플릿이 할당되지 않았습니다. 인스펙터 설정을 확인하세요.");
            }
        }
        else
        {
            Debug.LogWarning($"Tier {tierIndex} 아이템 목록이 비어 있습니다. 레벨 {playerLevel}에 지급할 아이템이 없습니다.");
        }
    }

    // === 유틸리티 메서드 (기존 유지) ===
    private List<ItemReward> GetLootListByLevel(int playerLevel)
    {
        if (playerLevel <= levelTiers[0]) return lootTier1;
        if (playerLevel <= levelTiers[1]) return lootTier2;
        if (playerLevel <= levelTiers[2]) return lootTier3;
        if (playerLevel <= levelTiers[3]) return lootTier4;
        return lootTier5;
    }

    private int GetTierIndexByLevel(int playerLevel)
    {
        if (playerLevel <= levelTiers[0]) return 1;
        if (playerLevel <= levelTiers[1]) return 2;
        if (playerLevel <= levelTiers[2]) return 3;
        if (playerLevel <= levelTiers[3]) return 4;
        return 5;
    }

    private ItemGrade GetRandomItemGrade()
    {
        int totalWeight = 0;
        foreach (var dropWeight in gradeDropWeights)
        {
            if (dropWeight.weight > 0)
            {
                totalWeight += dropWeight.weight;
            }
        }

        if (totalWeight <= 0)
        {
            Debug.LogWarning("아이템 등급 드롭 가중치 설정이 유효하지 않습니다. 기본 등급을 반환합니다.");
            return (ItemGrade)0;
        }

        int dropPoint = Random.Range(0, totalWeight);
        int currentWeight = 0;

        foreach (var dropWeight in gradeDropWeights)
        {
            if (dropWeight.weight <= 0) continue;

            currentWeight += dropWeight.weight;

            if (dropPoint < currentWeight)
            {
                return dropWeight.grade;
            }
        }

        return (ItemGrade)0;
    }

    // === 디버그 시각화 (기존 유지) ===
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}