using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 카메라와 타겟(플레이어) 사이에 있는 오브젝트를 감지하여 반투명하게 만듭니다.
/// 최적화를 위해 코루틴을 사용하여 레이캐스트 감지 주기를 늦추고, 페이딩은 Update에서 부드럽게 진행합니다.
/// </summary>
public class CameraObstructorFader : MonoBehaviour
{
    [Header("타겟 설정")]
    [Tooltip("카메라가 바라보는 타겟(보통 플레이어)의 Transform입니다.")]
    [SerializeField]
    private Transform target;

    [Tooltip("장애물 감지에 사용할 레이어 마스크입니다. 벽, 건물 등 가려지는 오브젝트만 선택하세요.")]
    [SerializeField]
    private LayerMask obstractionLayer;

    [Header("투명화 설정")]
    [Tooltip("오브젝트가 투명화될 최종 알파 값입니다 (0.0f ~ 1.0f).")]
    [SerializeField, Range(0.01f, 0.9f)]
    private float fadedAlpha = 0.3f;

    [Tooltip("투명화/불투명화가 진행되는 속도입니다.")]
    [SerializeField]
    private float fadeSpeed = 5f;

    [Header("최적화 설정")]
    [Tooltip("장애물 감지 및 목록 업데이트 주기 (초). 레이캐스트 부하를 줄입니다.")]
    [SerializeField, Range(0.05f, 1.0f)]
    private float checkInterval = 0.2f;

    // 현재 투명화 상태인 오브젝트 관리 (InstanceID를 Key로 사용)
    private Dictionary<int, FadeManager> currentlyFadedObjects = new Dictionary<int, FadeManager>();
    // 이전 주기 또는 프레임에 투명화 상태였으나, 현재 감지되지 않아 복구 대기 중인 오브젝트 목록
    private List<FadeManager> previousFadedObjects = new List<FadeManager>();


    /// <summary>
    /// 초기화 시, 주기적인 레이캐스트 감지 코루틴을 시작합니다.
    /// </summary>
    private void Start()
    {
        if (target != null)
        {
            StartCoroutine(CheckForObstructionsCoroutine());
        }
    }

    /// <summary>
    /// 페이딩(알파값 변경)은 부드러움을 위해 매 프레임 실행되어야 합니다.
    /// 코루틴에서 상태만 업데이트하고, 실제 페이딩은 여기서 처리합니다.
    /// </summary>
    private void Update()
    {
        // 1. 현재 감지되어 투명화 상태를 유지해야 하는 오브젝트 페이드 인 진행
        foreach (FadeManager manager in currentlyFadedObjects.Values)
        {
            // manager.IsFadingIn은 CheckAndManageObstructions에서 true로 설정됩니다.
            manager.UpdateFade(manager.IsFadingIn);
        }

        // 2. 복구 대기 중인 오브젝트의 페이드 아웃 진행
        // 이 리스트에 있는 오브젝트는 레이캐스트에서 감지되지 않았기 때문에 shouldBeFaded를 false로 설정합니다.
        for (int i = previousFadedObjects.Count - 1; i >= 0; i--)
        {
            FadeManager managerToRestore = previousFadedObjects[i];
            managerToRestore.UpdateFade(false); // Fade Out 진행

            if (managerToRestore.IsRestoreComplete())
            {
                // 복구 완료 시, 현재 목록(Dictionary)에서 제거하고 Cleanup을 호출합니다.
                int idToRemove = -1;
                foreach (var kvp in currentlyFadedObjects)
                {
                    if (kvp.Value == managerToRestore)
                    {
                        idToRemove = kvp.Key;
                        break;
                    }
                }

                if (idToRemove != -1)
                {
                    currentlyFadedObjects.Remove(idToRemove);
                    previousFadedObjects.RemoveAt(i); // List에서도 제거
                    managerToRestore.Cleanup();
                }
            }
        }
    }


    /// <summary>
    /// 지정된 간격(checkInterval)마다 카메라 장애물을 체크하고 목록을 업데이트하는 코루틴입니다.
    /// 레이캐스트 및 목록 관리 로직을 주기적으로 실행하여 CPU 부하를 줄입니다.
    /// </summary>
    private IEnumerator CheckForObstructionsCoroutine()
    {
        // 코루틴 종료 시까지 무한 반복
        while (true)
        {
            // 실제 감지 및 상태 업데이트 로직을 호출합니다.
            CheckAndManageObstructions();

            // 지정된 시간만큼 대기합니다. (레이캐스트 연산 주기 조절)
            yield return new WaitForSeconds(checkInterval);
        }
    }

    /// <summary>
    /// 레이캐스트 감지 및 투명화 관리 상태 로직을 수행합니다. (0.2초마다 실행)
    /// </summary>
    private void CheckAndManageObstructions()
    {
        if (target == null)
        {
            return;
        }

        Vector3 cameraPosition = transform.position;
        Vector3 targetPosition = target.position;
        Vector3 direction = targetPosition - cameraPosition;
        float distance = direction.magnitude;

        // 현재 투명화 목록을 복구 대기 목록(previousFadedObjects)으로 옮깁니다.
        // 다음 레이캐스트에서 감지되면 이 목록에서 제거됩니다.
        previousFadedObjects.Clear();
        foreach (var manager in currentlyFadedObjects.Values)
        {
            previousFadedObjects.Add(manager);
            // 다음 레이캐스트가 감지될 때까지는 FadeManager의 상태가 FadingIn을 유지하도록 합니다.
            // Update()에서 이 상태를 보고 Fade In을 계속 진행합니다.
            manager.SetFadingInStatus(true);
        }

        // 레이캐스트를 이용해 장애물 감지
        RaycastHit[] hits = Physics.RaycastAll(cameraPosition, direction.normalized, distance, obstractionLayer);

        foreach (RaycastHit hit in hits)
        {
            GameObject hitObject = hit.collider.gameObject;
            int instanceID = hitObject.GetInstanceID();

            // 현재 투명화 목록에 이미 있는지 확인
            if (!currentlyFadedObjects.TryGetValue(instanceID, out FadeManager manager))
            {
                // 없으면 새로 생성하여 목록에 추가
                manager = new FadeManager(hitObject, fadedAlpha, fadeSpeed);
                currentlyFadedObjects.Add(instanceID, manager);
            }

            // 현재 감지된 오브젝트이므로, 복구 대기 목록에서 제거합니다.
            previousFadedObjects.Remove(manager);

            // 현재 감지된 오브젝트는 투명화 상태를 유지해야 함을 명시적으로 설정
            manager.SetFadingInStatus(true);
        }

        // 레이캐스트에서 감지되지 않아 previousFadedObjects에 남아있는 매니저들은
        // Update()에서 isFadingIn 상태가 false인 것으로 간주되어 Fade Out을 시작합니다.
        foreach (FadeManager managerToRestore in previousFadedObjects)
        {
            managerToRestore.SetFadingInStatus(false);
        }
    }


    /// <summary>
    /// 오브젝트의 머티리얼 배열을 관리하고 투명화/복구 페이딩을 처리하는 내부 클래스입니다.
    /// SOLID 원칙: 모든 머티리얼에 대한 페이딩 로직을 단일 책임 원칙에 따라 관리합니다.
    /// </summary>
    private class FadeManager
    {
        private const int OPAQUE = 0;
        private const int FADE = 2; // 투명화 모드

        private Renderer targetRenderer;
        // 각 머티리얼의 원본/투명화 복사본 쌍을 저장하는 리스트
        private List<MaterialInfo> materialInfos;

        private float targetAlpha;
        private float fadeSpeed;
        private bool isFadingIn = false; // 현재 페이드 인(투명화) 중인지 여부 상태
        private bool isInitialized = false;

        // IsFadingIn 속성을 외부에서 읽을 수 있도록 추가
        public bool IsFadingIn => isFadingIn;

        /// <summary>
        /// 다중 머티리얼 관리를 위한 내부 구조체
        /// </summary>
        private class MaterialInfo
        {
            public Material originalMaterial;
            public Material fadedMaterial;
        }

        public FadeManager(GameObject obj, float alpha, float speed)
        {
            targetRenderer = obj.GetComponent<Renderer>();

            if (targetRenderer == null)
            {
                return;
            }

            targetAlpha = alpha;
            fadeSpeed = speed;

            // **여기서 모든 머티리얼을 복사하고 초기화합니다.**
            Material[] originalMaterials = targetRenderer.sharedMaterials;
            materialInfos = new List<MaterialInfo>(originalMaterials.Length);

            Material[] newFadedMaterials = new Material[originalMaterials.Length];

            for (int i = 0; i < originalMaterials.Length; i++)
            {
                Material original = originalMaterials[i];
                if (original == null) continue;

                Material faded = new Material(original);

                SetMaterialRenderingMode(faded, FADE);

                if (faded.HasProperty("_ZWrite"))
                {
                    faded.SetInt("_ZWrite", 0);
                }

                materialInfos.Add(new MaterialInfo
                {
                    originalMaterial = original,
                    fadedMaterial = faded
                });

                newFadedMaterials[i] = faded;
            }

            targetRenderer.sharedMaterials = newFadedMaterials;

            isInitialized = true;
            isFadingIn = true; // 새로 생성되면 바로 투명화 시작
        }

        /// <summary>
        /// 외부(CameraObstructorFader)에서 이 매니저의 의도된 투명화 상태를 설정합니다.
        /// </summary>
        public void SetFadingInStatus(bool status)
        {
            isFadingIn = status;
        }

        /// <summary>
        /// 모든 머티리얼의 알파 값을 목표치로 부드럽게 변경합니다.
        /// </summary>
        /// <param name="shouldBeFaded">true: 투명화(Fade In)를 목표로 함, false: 복구(Fade Out)를 목표로 함</param>
        public void UpdateFade(bool shouldBeFaded)
        {
            if (!isInitialized) return;

            float target = shouldBeFaded ? targetAlpha : 1.0f;

            // 모든 머티리얼 정보를 순회하며 알파 값 업데이트
            foreach (var info in materialInfos)
            {
                Material fadedMaterial = info.fadedMaterial;

                float currentAlpha = fadedMaterial.color.a;
                Color currentColor = fadedMaterial.color;

                // Time.deltaTime을 사용하여 프레임 속도에 독립적인 부드러운 전환 구현
                float newAlpha = Mathf.Lerp(currentAlpha, target, Time.deltaTime * fadeSpeed);
                Color newColor = new Color(currentColor.r, currentColor.g, currentColor.b, newAlpha);

                // 색상 프로퍼티 업데이트
                if (fadedMaterial.HasProperty("_Color"))
                {
                    fadedMaterial.SetColor("_Color", newColor);
                }
                if (fadedMaterial.HasProperty("_BaseColor"))
                {
                    fadedMaterial.SetColor("_BaseColor", newColor);
                }
                fadedMaterial.color = newColor;
            }
        }

        /// <summary>
        /// 복구(Fade Out) 과정이 완료되었는지 확인합니다.
        /// </summary>
        public bool IsRestoreComplete()
        {
            // isFadingIn이 true이면 아직 투명화 상태를 유지해야 하므로 복구 완료가 아님
            if (!isInitialized || isFadingIn) return false;

            // 모든 머티리얼이 복구되었는지 확인 (알파값 1.0f에 근접)
            foreach (var info in materialInfos)
            {
                if (Mathf.Abs(info.fadedMaterial.color.a - 1.0f) >= 0.01f)
                {
                    return false;
                }
            }

            // 모든 머티리얼이 복구되었으면 복구 완료
            return true;
        }

        /// <summary>
        /// 복구 완료 후 원본 머티리얼로 되돌리고 복사된 머티리얼 인스턴스들을 파괴합니다.
        /// </summary>
        public void Cleanup()
        {
            if (!isInitialized) return;

            // 원본 머티리얼 배열로 Renderer를 복구
            if (targetRenderer != null)
            {
                targetRenderer.sharedMaterials = GetOriginalMaterialsArray();
            }

            // 복사된 머티리얼 인스턴스 파괴
            foreach (var info in materialInfos)
            {
                if (info.fadedMaterial != null)
                {
                    Object.Destroy(info.fadedMaterial);
                }
            }
            materialInfos.Clear();
            isInitialized = false;
        }

        /// <summary>
        /// 원본 머티리얼 배열을 반환합니다.
        /// </summary>
        private Material[] GetOriginalMaterialsArray()
        {
            Material[] originals = new Material[materialInfos.Count];
            for (int i = 0; i < materialInfos.Count; i++)
            {
                originals[i] = materialInfos[i].originalMaterial;
            }
            return originals;
        }


        /// <summary>
        /// 쉐이더의 렌더링 모드를 투명 모드로 전환합니다.
        /// </summary>
        private void SetMaterialRenderingMode(Material material, int mode)
        {
            // 1. URP 호환성
            if (material.HasProperty("_Surface"))
            {
                if (mode == FADE)
                {
                    material.SetInt("_Surface", 1); // Transparent
                    material.SetOverrideTag("RenderType", "Transparent");
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    material.SetInt("_ZWrite", 0);
                    material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                    material.renderQueue = 3000;
                }
            }
            // 2. Standard 쉐이더 호환성
            else if (material.HasProperty("_Mode"))
            {
                if (mode == FADE)
                {
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    material.SetInt("_ZWrite", 0);
                    material.EnableKeyword("_ALPHABLEND_ON");
                    material.SetInt("_Mode", FADE);
                    material.renderQueue = 3000;
                }
            }

            // 초기 알파 값을 1.0f로 설정
            if (material.HasProperty("_Color"))
            {
                Color col = material.GetColor("_Color");
                material.SetColor("_Color", new Color(col.r, col.g, col.b, 1.0f));
            }
        }
    }
}