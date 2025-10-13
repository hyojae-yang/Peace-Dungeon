// SummonItemSO.cs
using UnityEngine;

[CreateAssetMenu(fileName = "New Pet Summon Item", menuName = "Item/Consumable/Pet Summon Item")]
public class SummonItemSO : ConsumableItemSO
{
    [Header("소환 속성")]
    [Tooltip("소환할 펫 프리팹입니다. 이 프리팹에는 PetController가 붙어 있어야 합니다.")]
    public GameObject petPrefab;

    [Tooltip("펫이 플레이어 주변에 소환될 최대 반경(거리)입니다.")]
    public float spawnRadius = 3f;

    [Tooltip("펫을 소환할 수 있는 지면의 레이어 마스크입니다. 3D 소환에 사용됩니다.")]
    public LayerMask groundLayerMask; // ⭐ 논의를 통해 추가된 유연성 요소

    // 펫 스크립트가 아직 없으므로, 편의상 PetController 대신 가상의 클래스 이름을 사용합니다.
    private const string PetControllerClassName = "PetController";

    public override void Use(PlayerCharacter player)
    {
        if (player == null || petPrefab == null)
        {
            Debug.LogError("플레이어 또는 소환할 프리팹이 설정되지 않았습니다.");
            return;
        }

        // 1. 기존 펫 파괴 (유일성 보장)
        // PetController.Instance는 PetController 스크립트에 싱글톤으로 정의되어 있다고 가정합니다.
        // 현재 논의 범위 밖이므로, 이 부분은 주석으로 처리하고 넘어가겠습니다.
        /*
        if (PetController.Instance != null)
        {
            Debug.Log($"기존 펫 ({PetController.Instance.name})을 파괴하고 새로 소환합니다.");
            // Unity.Object를 명시적으로 사용합니다.
            Object.Destroy(PetController.Instance.gameObject); 
        }
        */

        // 2. 소환 위치 계산 (플레이어 주변 랜덤 위치 + 지면 검색)
        Vector3 spawnPosition = CalculateSpawnPosition(player.transform.position);

        // 3. 새 펫 소환
        GameObject newPet = Object.Instantiate(petPrefab, spawnPosition, Quaternion.identity);

        // 4. 소환된 펫 초기화 명령 (펫 스크립트의 책임)
        
        if (newPet.TryGetComponent<MangChi>(out var petController))
        {
            petController.Initialize(player); // 펫 스크립트에게 주인 정보를 넘겨줍니다.
        }
    }

    /// <summary>
    /// 플레이어 주변의 랜덤한 위치를 계산하고, 지면이 있다면 그 위의 위치를 반환합니다.
    /// </summary>
    /// <param name="playerOrigin">플레이어의 현재 위치</param>
    /// <returns>최종 소환 위치</returns>
    private Vector3 CalculateSpawnPosition(Vector3 playerOrigin)
    {
        // 1. 플레이어 주변에 무작위 구(Sphere) 오프셋을 생성합니다.
        Vector3 randomOffset = Random.insideUnitSphere * spawnRadius;

        // 2. 소환 시작점 (플레이어 기준)
        Vector3 startPosition = playerOrigin + randomOffset;

        // 3. 지면 검색을 위한 레이캐스트 준비
        // 시작점을 공중에 두고 아래로 쏴서 지면을 찾습니다.
        Vector3 rayStart = startPosition + Vector3.up * 5f;
        float rayDistance = 10f;

        // 4. 레이캐스트 실행
        if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, rayDistance, groundLayerMask))
        {
            // 지면을 찾았습니다! 지면의 위치(hit.point)를 반환합니다.
            return hit.point;
        }
        else
        {
            // 지면을 찾지 못했습니다. 안전을 위해 플레이어의 높이에서 소환하거나 경고를 표시합니다.
            // 여기서는 시작점의 Y좌표를 플레이어 Y좌표로 고정하여 공중에 뜨는 것을 방지합니다.
            startPosition.y = playerOrigin.y;
            Debug.LogWarning("소환 반경 내에서 유효한 지면을 찾지 못했습니다. 플레이어 높이에 소환합니다.");
            return startPosition;
        }
    }
}