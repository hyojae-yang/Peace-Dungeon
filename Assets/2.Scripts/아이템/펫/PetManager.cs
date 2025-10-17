// PetManager.cs
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Collections;
using System;

/// <summary>
/// 펫 시스템의 중앙 관리자입니다.
/// ISavable 인터페이스를 구현하여 SaveManager와 통신하며, 펫의 영구 데이터를 저장하고 로드합니다.
/// SOLID: OCP (SaveManager를 수정하지 않고 기능 확장)
/// SOLID: SRP (펫 데이터 저장/로드 및 인스턴스 관리에 대한 책임)
/// </summary>
public class PetManager : MonoBehaviour, ISavable
{
    // === 싱글톤 인스턴스 ===
    public static PetManager Instance { get; private set; }

    // === 필드 ===
    [Header("설정")]
    [Tooltip("망치 펫 프리팹입니다. 인스펙터에 반드시 할당해야 합니다.")]
    [SerializeField] private GameObject mangChiPrefab;

    // 로드된 데이터를 임시로 보관하는 변수
    private PetSystemSaveData loadedData;

    // === 유니티 라이프사이클 ===
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // 1. 씬 로드 시 SaveManager에 자신을 등록하고 데이터를 요청합니다.
        if (SaveManager.Instance != null)
        {
            // SaveManager.RegisterSavable(this) 호출 시 -> LoadData(로드된 데이터)가 즉시 호출됩니다.
            SaveManager.Instance.RegisterSavable(this);

            // --------------------------------------------------------
            // [핵심 추가 1] MainSceneManager.OnGameOver 이벤트 구독
            // --------------------------------------------------------
            // MainSceneManager는 씬 내에 존재한다고 가정합니다.
            if (MainSceneManager.Instance != null)
            {
                // 플레이어 사망 시 SavePetDeathState 메서드를 호출하도록 등록
                MainSceneManager.OnGameOver += SavePetDeathState;
            }
        }
        else
        {
            Debug.LogError("[PetManager] SaveManager 인스턴스를 찾을 수 없습니다! 저장/로드 불가.");
        }
    }

    // === ISavable 구현 메서드 (저장 및 로드의 핵심) ===

    /// <summary>
    /// ISavable 인터페이스 구현: 현재 펫 시스템의 데이터를 SaveManager에 전달합니다.
    /// </summary>
    /// <returns>소환 여부(bool)를 담는 PetSystemSaveData 객체</returns>
    public object SaveData()
    {
        // 펫의 인스턴스가 현재 씬에 존재하고 활성화되어 있다면, 펫에게 데이터를 요청합니다.
        if (MangChi.Instance != null && MangChi.Instance.gameObject.activeInHierarchy)
        {
            // MangChi 객체 자체가 GetSaveData()를 호출하여 소환 여부를 보고합니다.
            return MangChi.Instance.GetSaveData();
        }

        // 펫이 씬에 존재하지 않는다면, 소환되지 않은 상태로 저장합니다.
        return new PetSystemSaveData { isMangChiSummoned = false };
    }

    /// <summary>
    /// ISavable 인터페이스 구현: SaveManager로부터 로드된 데이터를 받아 임시 저장합니다.
    /// </summary>
    /// <param name="data">SaveManager로부터 전달받은 로드 데이터</param>
    public void LoadData(object data)
    {
        // 데이터를 PetSystemSaveData 타입으로 캐스팅합니다.
        if (data is PetSystemSaveData petData)
        {
            // 로드 시점에 즉시 펫 재생성을 시도합니다.
            if (petData.isMangChiSummoned)
            {
                RecreatePetIfSummoned(petData);
            }
        }
    }

    // === 펫 생성 및 관리 로직 ===

    /// <summary>
    /// 로드된 데이터를 바탕으로 펫 객체를 재생성합니다.
    /// 플레이어 주변에 펫을 생성하고 Initialize를 호출합니다.
    /// </summary>
    /// <param name="saveData">로드된 펫 시스템 데이터</param>
    private void RecreatePetIfSummoned(PetSystemSaveData saveData)
    {
        // 1. 소환 상태 확인 및 유효성 검사
        if (!saveData.isMangChiSummoned || mangChiPrefab == null || PlayerCharacter.Instance == null)
        {
            if (saveData.isMangChiSummoned) Debug.LogError("[PetManager] 펫을 로드해야 하지만, 프리팹 또는 플레이어 인스턴스가 누락되었습니다.");
            return;
        }

        // 2. 소환 위치 계산 (위치 저장 로직이 없으므로 항상 플레이어 주변에 생성)
        float followDistance = 4f; // MangChi 스크립트의 followDistance 값과 일치해야 합니다.
        Vector3 playerPos = PlayerCharacter.Instance.transform.position;
        Vector3 playerForward = PlayerCharacter.Instance.transform.forward;
        Vector3 spawnPosition = playerPos - playerForward * followDistance;

        // 3. 프리팹을 인스턴스화합니다.
        GameObject petObject = Instantiate(
            mangChiPrefab,
            spawnPosition,
            PlayerCharacter.Instance.transform.rotation
        );

        // 4. MangChi 스크립트를 가져와 초기화 및 로드 상태 복원
        if (petObject.TryGetComponent<MangChi>(out var petController))
        {
            // 초기화: 주인 정보 설정 및 기본 상태 설정
            petController.Initialize(PlayerCharacter.Instance);

            // 로드: 소환 여부 외의 상태 복원 (현재는 isDead=false 설정만 함)
            petController.LoadData(saveData);
        }
        else
        {
            Destroy(petObject);
            Debug.LogError($"[PetManager] 소환된 펫 프리팹에 MangChi 스크립트가 없습니다.");
        }
    }

    // === 기타 공용 메서드 (SummonItemSO에서 호출됨) ===

    /// <summary>
    /// MainSceneManager.OnGameOver 이벤트 발생 시 호출됩니다.
    /// 현재 펫의 상태(isSummoned=false)만 파일에 저장하도록 SaveManager에 요청합니다.
    /// </summary>
    private void SavePetDeathState()
    {
        if (SaveManager.Instance != null)
        {
            // 1. SaveManager에 단일 저장 요청 (이때 SaveManager의 로그가 먼저 찍힘)
            SaveManager.Instance.SaveSingleSavable(this);
        }
        else
        {
            Debug.LogError("[PetManager] SaveManager 인스턴스를 찾을 수 없어 펫 상태를 기록할 수 없습니다.");
        }
    }
    // ==========================================================
    // [필수 추가] 이벤트 구독 해제를 위한 OnDestroy 메서드 추가
    // ==========================================================
    private void OnDestroy()
    {
        // 싱글톤 인스턴스 해제 로직 (Awake에서 DontDestroyOnLoad를 썼다면 필요)
        if (Instance == this)
        {
            Instance = null;
        }

        // --------------------------------------------------------
        // [핵심 추가 2] MainSceneManager.OnGameOver 이벤트 구독 해제
        // --------------------------------------------------------
        if (MainSceneManager.Instance != null)
        {
            // 메모리 누수를 막기 위해 객체가 파괴될 때 반드시 해제해야 합니다.
            MainSceneManager.OnGameOver -= SavePetDeathState;
        }
        // --------------------------------------------------------
    }
}