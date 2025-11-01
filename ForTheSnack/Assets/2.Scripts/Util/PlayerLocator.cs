/*
 PlayerLocator
  - 목적: 씬 로드 직후 '플레이어 준비 완료' 타이밍을 안정적으로 알리기 위한
          경량 레지스트리. FindObjectOfType/실행 순서 운에 의존하지 않기 위함.
  - 원리: PlayerSaveProxy가 OnEnable/OnDisable에서 Register/Unregister 하고,
          소비자는 Current != null을 기다리거나 OnReady 이벤트를 구독한다.
  - 보장:
      * 한 번에 0개 또는 1개의 플레이어만 추적(싱글 플레이 기준).
      * 메인 스레드(유니티 스레드)에서만 접근.
  - 사용법(권장 패턴):
      1) 씬 로드 완료 후 1프레임 유예: yield return ao; yield return null;
      2) 플레이어 준비 대기: yield return new WaitUntil(() => PlayerLocator.Current != null);
      3) 그 다음 PlayerLocator.Current를 사용하여 위치/속도 복원 등 수행.
  - 하지 말 것:
      * 멀티플레이/분신에 이 레지스트리를 재사용(그땐 컬렉션/ID 기반 매니저가 필요).
      * DDOL로 영구 보관된 참조를 계속 쥐고 있기(씬 갈 때마다 다시 조회 권장).
  - 대안:
      * 씬별 SceneBootstrap에서 Start() 시점에 GameProgressService.RestoreIfPending() 호출.
      * LoadSceneManager가 onActivated 콜백을 제공하고, 그 내부에서 Current 대기.
*/

using System;

public static class PlayerLocator
{
    /// <summary>
    /// 현재 씬에서 활성화된 <see cref="PlayerSaveProxy"/> 인스턴스.
    /// 씬에 아직 플레이어가 없으면 <c>null</c> 입니다.
    /// </summary>

    public static PlayerSaveProxy Current {  get; private set; }

    /// <summary>
    /// 플레이어가 등록되어 사용 가능해지는 즉시(보통 씬 활성화 후 첫 프레임) 한 번 호출됩니다.
    /// GameProgressService 같은 오케스트레이터가 여기서 복원 로직을 이어갈 수 있습니다.
    /// </summary>
    public static event Action<PlayerSaveProxy> OnReady;

    /// <summary>
    /// 현재 추적 중인 플레이어가 파괴/비활성화되어 더 이상 유효하지 않을 때 호출됩니다.
    /// 씬 전환 시 클린업 로직을 붙일 수 있습니다.
    /// </summary>
    public static event Action OnCleared;

    // 같은 어셈블리 내부에서만 호출되도록 하고 싶으면 internal로 두세요.
    public static void Registry(PlayerSaveProxy proxy)
    {
        if (proxy == null || Current == proxy) return;
        Current = proxy;
        OnReady?.Invoke(proxy);
    }

    public static void Unregistry(PlayerSaveProxy proxy)
    {
        if (proxy != null && Current == proxy)
        {
            Current = null;
            OnCleared?.Invoke();
        }
    }
}