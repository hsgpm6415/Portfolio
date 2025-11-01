using UnityEngine;
using UnityEngine.U2D; // PixelPerfectCamera

[DefaultExecutionOrder(-1000)]
public class PixelPerfectWorldZoom : MonoBehaviour
{
    [Header("필수")]
    public PixelPerfectCamera ppc;

    [Header("타겟/기준")]
    public int baselineScreenHeight = 1080; // 1080p를 1단계로 가정
    public int tilesVertAtBaseline = 9;     // 1080p에서 세로로 보일 타일 개수 (권장: 8~12)
    public int tilePixels = 70;             // 타일 한 변의 픽셀 수
    public int assetsPPU = 100;             // 너의 PPU(Import PPU와 동일)

    [Header("제한")]
    public int minStep = 1;                 // 1 이상
    public int maxStep = 3;                 // 과도한 줌아웃 방지(필요에 맞게 조절)

    int _sw, _sh;

    void Reset() => ppc = FindObjectOfType<PixelPerfectCamera>();
    void Awake()
    {
        if (!ppc) ppc = FindObjectOfType<PixelPerfectCamera>();
        if (ppc) ppc.assetsPPU = assetsPPU;
        Apply(true);
    }

    void Update()
    {
        if (_sw != Screen.width || _sh != Screen.height) Apply();
    }

    void Apply(bool force = false)
    {
        _sw = Screen.width; _sh = Screen.height;
        if (!ppc || _sh <= 0) return;

        // 1) 기준(1080p)을 몇 단계로 나눌지: 더 작은 해상도일수록 step이 커짐(=더 작게 보임)
        int step = Mathf.Clamp(
            Mathf.CeilToInt((float)baselineScreenHeight / _sh),
            minStep, maxStep);

        // 2) 기준 가상 해상도(Ref) 계산: 세로 = (타일픽셀 * 세로타일수), 가로는 현재 화면비로 맞춤
        int baseRefH = tilePixels * tilesVertAtBaseline; // 예: 70*9 = 630
        float aspect = (float)_sw / _sh;                 // 현재 화면비(16:9 등)
        int baseRefW = RoundToMultiple(Mathf.RoundToInt(baseRefH * aspect), tilePixels);

        // 3) 단계에 따라 Ref 해상도를 정수배로 키움(=줌아웃)
        int targetRefH = baseRefH * step; // 예: 1080p(1x)=630, 720p(2x)=1260, 480p(3x)=1890
        int targetRefW = baseRefW * step;

        bool changed = (ppc.refResolutionX != targetRefW) || (ppc.refResolutionY != targetRefH);

        // 4) 적용
        ppc.refResolutionX = targetRefW;
        ppc.refResolutionY = targetRefH;

        // 픽셀퍼펙트 권장 토글(안전)
        ppc.upscaleRT = true;
        ppc.cropFrameX = true;
        ppc.cropFrameY = true;
        ppc.stretchFill = false;

        // 참고 로그
        if (changed) Debug.Log($"[PP] step:{step} ref:{targetRefW}x{targetRefH} aspect:{aspect:0.###}");
    }

    static int RoundToMultiple(int value, int multiple)
    {
        if (multiple <= 1) return value;
        int rem = value % multiple;
        if (rem == 0) return value;
        int down = value - rem;
        int up = value + (multiple - rem);
        // 가까운 배수로, 딱 중간이면 올림
        return (value - down < up - value) ? down : up;
    }
}
