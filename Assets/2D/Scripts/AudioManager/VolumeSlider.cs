using UnityEngine;
using UnityEngine.UI;

// 슬라이더 하나를 BGM 또는 SFX 볼륨에 연결하는 컴포넌트.
// Slider 오브젝트에 붙이고 Type을 Inspector에서 선택한다.
public class VolumeSlider : MonoBehaviour
{
    public enum VolumeType { BGM, SFX }
    public VolumeType type;

    Slider slider;

    void Start()
    {
        slider = GetComponent<Slider>();

        // AudioManager에 저장된 현재 볼륨으로 슬라이더 초기화
        if (AudioManager.instance == null) return;

        // 키보드/패드 입력으로 슬라이더가 움직이지 않도록 Navigation 비활성화
        slider.navigation = new Navigation { mode = Navigation.Mode.None };

        slider.value = type == VolumeType.BGM
            ? AudioManager.instance.bgmVolume
            : AudioManager.instance.sfxVolume;

        // 값 변경 이벤트 등록
        slider.onValueChanged.AddListener(OnValueChanged);
    }

    void OnValueChanged(float value)
    {
        if (AudioManager.instance == null) return;

        if (type == VolumeType.BGM)
            AudioManager.instance.SetBgmVolume(value);
        else
            AudioManager.instance.SetSfxVolume(value);
    }

    void OnDestroy()
    {
        if (slider != null)
            slider.onValueChanged.RemoveListener(OnValueChanged);
    }
}
