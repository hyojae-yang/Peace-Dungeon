using UnityEngine;

public class TestSenser : MonoBehaviour
{
    // 스크립트가 동작해야 할 때 외부에서 활성화하는 플래그
    public static bool tt = false;

    // 센서의 현재 활성화 상태를 추적하는 변수
    private bool _isSensorActive = false;

        // 감지할 오브젝트가 속한 레이어 마스크
    public LayerMask serchLayerMask;

    // 이 오브젝트의 콜라이더 컴포넌트
    private Collider _collider;
    // 이 스크립트가 붙은 오브젝트의 메시 렌더러
    private MeshRenderer _ownMeshRenderer;

    // 감지된 오브젝트의 콜라이더
    private Collider _serchedCollider;

    // 감지된 오브젝트의 메시 렌더러와 자식 오브젝트 리스트
    private MeshRenderer _serchedMeshRenderer;
    private Transform[] _serchedChildren;

    

    // 감지 범위를 x축 방향으로만 조절할 값 (1.0f는 원래 크기, 0.5f는 절반 크기)
    [SerializeField]
    private float _detectionXScale = 0.005f;

    /// <summary>
    /// 스크립트 시작 시 필요한 컴포넌트들을 초기화합니다.
    /// </summary>
    private void Awake()
    {
        _collider = GetComponent<Collider>();
        _ownMeshRenderer = GetComponent<MeshRenderer>();
        //SerchAndDeactivateOnce();
    }

    /// <summary>
    /// 매 프레임마다 업데이트되는 함수
    /// tt 플래그 상태 변화에 따라 로직을 실행합니다.
    /// </summary>
    private void Update()
    {
        // tt가 true가 되고 센서가 비활성화 상태일 경우
        if (tt && !_isSensorActive)
        {
            if (SerchDoor())
            {
                _isSensorActive = true;
            }
        }
        // tt가 false로 바뀌고 센서가 활성화 상태일 경우
        else if (!tt&& _isSensorActive)
        {
            ReactivateComponents();
            _isSensorActive = false;
        }
    }

    /// <summary>
    /// 특정 레이어의 콜라이더를 감지하고, 해당 콜라이더와 메시 렌더러, 자식들을 비활성화합니다.
    /// </summary>
    /// <returns>콜라이더 감지 성공 시 true, 실패 시 false</returns>
    private bool SerchDoor()
    {
        // 레이어 마스크가 설정되지 않았을 경우 경고 메시지를 출력하고 false 반환
        if (serchLayerMask == 0)
        {
            Debug.LogWarning("레이어 마스크가 설정되지 않았습니다. [TestSenser]");
            return false;
        }

        // 로컬 스케일을 기반으로 새로운 halfExtents를 계산합니다.
        Vector3 localScale = transform.localScale;
        Vector3 newHalfExtents = new Vector3(localScale.x * _detectionXScale, localScale.y, localScale.z) * 0.5f;

        // Physics.OverlapBox를 사용하여 콜라이더를 감지합니다.
        Collider[] hitColliders = Physics.OverlapBox(transform.position, newHalfExtents, transform.rotation, serchLayerMask, QueryTriggerInteraction.Ignore);

        // 감지된 콜라이더가 있을 경우
        if (hitColliders.Length > 0)
        {
            // 감지된 콜라이더 배열을 순회하며 자기 자신이 아닌 오브젝트가 있는지 확인합니다.
            foreach (var collider in hitColliders)
            {
                if (collider.gameObject != this.gameObject)
                {
                    _serchedCollider = collider;

                    // --- 본인의 콜라이더, 메시 렌더러, 자식 비활성화 로직 (유지) ---
                    // 자기 자신의 콜라이더와 메시 렌더러만 비활성화
                    _collider.enabled = false;
                    if (_ownMeshRenderer != null)
                    {
                        _ownMeshRenderer.enabled = false;
                    }
                    // 모든 자식 오브젝트들을 비활성화합니다.
                    foreach (Transform child in transform)
                    {
                        child.gameObject.SetActive(false);
                    }

                    // --- 감지된 오브젝트의 컴포넌트 비활성화 로직 (추가) ---
                    _serchedMeshRenderer = _serchedCollider.GetComponent<MeshRenderer>();
                    _serchedChildren = _serchedCollider.GetComponentsInChildren<Transform>(true);

                    _serchedCollider.enabled = false;
                    if (_serchedMeshRenderer != null)
                    {
                        _serchedMeshRenderer.enabled = false;
                    }

                    foreach (Transform child in _serchedChildren)
                    {
                        if (child.gameObject != _serchedCollider.gameObject)
                        {
                            child.gameObject.SetActive(false);
                        }
                    }

                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// 비활성화된 콜라이더와 메시 렌더러를 다시 활성화합니다.
    /// </summary>
    private void ReactivateComponents()
    {
        // --- 본인의 컴포넌트 활성화 로직 (유지) ---
        if (_collider != null && _ownMeshRenderer != null)
        {
            _collider.enabled = true;
            _ownMeshRenderer.enabled = true;

            // 모든 자식 오브젝트들을 다시 활성화합니다.
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(true);
            }
        }

        // --- 감지된 오브젝트의 컴포넌트 활성화 로직 (추가) ---
        if (_serchedCollider != null)
        {
            _serchedCollider.enabled = true;

            if (_serchedMeshRenderer != null)
            {
                _serchedMeshRenderer.enabled = true;
            }

            if (_serchedChildren != null)
            {
                foreach (Transform child in _serchedChildren)
                {
                    child.gameObject.SetActive(true);
                }
            }
        }
    }
    /// <summary>
    /// 스크립트 시작 시 한 번만 실행되는 감지 및 비활성화 로직입니다.
    /// </summary>
    private void SerchAndDeactivateOnce()
    {
        // SerchDoor()와 동일한 로직을 재사용합니다.
        if (serchLayerMask == 0)
        {
            Debug.LogWarning("레이어 마스크가 설정되지 않았습니다. [TestSenser]");
            return;
        }
        Vector3 localScale = transform.localScale;
        Vector3 newHalfExtents = new Vector3(localScale.x * _detectionXScale, localScale.y, localScale.z) * 0.5f;
        Collider[] hitColliders = Physics.OverlapBox(transform.position, newHalfExtents, transform.rotation, serchLayerMask, QueryTriggerInteraction.Ignore);
        if (hitColliders.Length > 0)
        {
            foreach (var collider in hitColliders)
            {
                if (collider.gameObject != this.gameObject)
                {
                    _serchedCollider = collider;
                    if (_collider != null) _collider.enabled = false;
                    if (_ownMeshRenderer != null) _ownMeshRenderer.enabled = false;
                    foreach (Transform child in transform)
                    {
                        child.gameObject.SetActive(false);
                    }
                    _serchedMeshRenderer = _serchedCollider.GetComponent<MeshRenderer>();
                    _serchedChildren = _serchedCollider.GetComponentsInChildren<Transform>(true);
                    if (_serchedCollider != null) _serchedCollider.enabled = false;
                    if (_serchedMeshRenderer != null) _serchedMeshRenderer.enabled = false;
                    if (_serchedChildren != null)
                    {
                        foreach (Transform child in _serchedChildren)
                        {
                            if (child.gameObject != _serchedCollider.gameObject)
                            {
                                child.gameObject.SetActive(false);
                            }
                        }
                    }
                    return; // 한 번만 비활성화
                }
            }
        }
    }

    /// <summary>
    /// 유니티 에디터에서 오브젝트가 선택되었을 때 감지 범위를 시각적으로 보여줍니다.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (_collider == null)
        {
            _collider = GetComponent<Collider>();
        }

        if (_collider != null)
        {
            Gizmos.color = Color.yellow;

            // 로컬 스케일을 기반으로 기즈모 크기를 계산합니다.
            Vector3 localSize = new Vector3(transform.localScale.x * _detectionXScale, transform.localScale.y, transform.localScale.z);

            // Gizmos.matrix를 사용하여 오브젝트의 위치와 회전을 기즈모에 적용합니다.
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, localSize);
        }
    }
}