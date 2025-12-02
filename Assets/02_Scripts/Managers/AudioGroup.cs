using UnityEngine;
using UnityEngine.Audio;

namespace CoffeeCat.FrameWork {
    public class AudioGroup : MonoBehaviour {
        [field: SerializeField] public AudioListener Listener { get; private set; } = null;
        [field: SerializeField] public AudioSource ChannelBgm { get; private set; } = null;
        [field: SerializeField] public AudioSource ChannelSE { get; private set; } = null;
        [field: SerializeField] public AudioSource ChannelAmbient { get; private set; } = null;
        [field: SerializeField] public AudioMixer MainAudioMixer { get; private set; } = null;
    }
}
