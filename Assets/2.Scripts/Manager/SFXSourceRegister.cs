using UnityEngine;

/// <summary>
/// SFXSourceRegister는 자신이 부착된 AudioSource 컴포넌트를 SoundManager의 SFX 리스트에 등록하고,
/// 파괴 시(씬 전환 등) 리스트에서 자신을 해제하는 역할을 담당합니다.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class SFXSourceRegister : MonoBehaviour
{
    private AudioSource _audioSource; // OnDestroy에서도 접근하기 위해 멤버 변수로 저장

    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();

        // 1. 등록 (Start 단계에서 진행하여 안정성 확보)
        if (SoundManager.Instance != null && _audioSource != null)
        {
            SoundManager.Instance.RegisterSFXSource(_audioSource);
        }
        else
        {
            Debug.LogError($"[SFXSourceRegister] {gameObject.name}의 AudioSource 등록 실패. SoundManager 인스턴스 또는 AudioSource 컴포넌트가 null입니다.");
        }
    }

    /// <summary>
    /// 이 게임 오브젝트가 파괴될 때(예: 씬 전환 시) 호출됩니다.
    /// SoundManager 리스트에서 자신을 제거하여 잔해 누적을 방지합니다. (메모리 관리)
    /// </summary>
    private void OnDestroy()
    {
        // 2. 해제 (파괴 직전에 진행)
        // SoundManager 인스턴스가 존재하고 등록했던 AudioSource가 있다면 해제를 요청합니다.
        if (SoundManager.Instance != null && _audioSource != null)
        {
            SoundManager.Instance.UnregisterSFXSource(_audioSource);
        }
        // *주의: SoundManager 자체가 먼저 파괴되면 Instance가 null일 수 있으며, 이 경우는 무시합니다.
    }
}