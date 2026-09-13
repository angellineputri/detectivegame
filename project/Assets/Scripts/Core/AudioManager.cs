using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("SFX Clips")]
    [SerializeField] AudioClip sfxButtonClick;
    [SerializeField] AudioClip sfxInteract;
    [SerializeField] AudioClip sfxClueAdded;
    [SerializeField] AudioClip sfxDialogueAdvance;
    [SerializeField] AudioClip sfxDoorOpen;
    [SerializeField] AudioClip sfxFootstep;
    [SerializeField] AudioClip sfxItemSelect;

    [Header("Music Clips")]
    [SerializeField] AudioClip musMainMenu;
    [SerializeField] AudioClip musInvestigation;

    [Header("Volume")]
    [SerializeField] [Range(0f, 1f)] float sfxVolume     = 1f;
    [SerializeField] [Range(0f, 1f)] float musicVolume   = 0.6f;
    // Dialogue advance fires on every line — keep this low (0.15–0.25 recommended)
    [SerializeField] [Range(0f, 1f)] float dialogueAdvanceVolume = 0.2f;

    AudioSource _sfxSource;
    AudioSource _musicSource;
    AudioSource _footstepSource;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _sfxSource           = gameObject.AddComponent<AudioSource>();
        _sfxSource.playOnAwake = false;

        _musicSource           = gameObject.AddComponent<AudioSource>();
        _musicSource.playOnAwake = false;
        _musicSource.loop       = true;

        _footstepSource           = gameObject.AddComponent<AudioSource>();
        _footstepSource.playOnAwake = false;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        bool isMenuScene = scene.name == "MainMenu" || scene.name == "SelectCaseScreen";
        AudioClip target = isMenuScene ? musMainMenu : musInvestigation;
        SwitchMusic(target);
    }

    void SwitchMusic(AudioClip clip)
    {
        if (clip == null || _musicSource.clip == clip) return;
        _musicSource.clip   = clip;
        _musicSource.volume = musicVolume;
        _musicSource.Play();
    }

    void PlaySFX(AudioClip clip, float volume)
    {
        if (clip == null) return;
        _sfxSource.PlayOneShot(clip, volume);
    }

    public void PlayButtonClick()     => PlaySFX(sfxButtonClick,     sfxVolume);
    public void PlayInteract()        => PlaySFX(sfxInteract,         sfxVolume);
    public void PlayClueAdded()       => PlaySFX(sfxClueAdded,        sfxVolume);
    public void PlayDialogueAdvance() => PlaySFX(sfxDialogueAdvance,  dialogueAdvanceVolume);
    public void PlayDoorOpen()        => PlaySFX(sfxDoorOpen,         sfxVolume);
    public void PlayFootstep()
    {
        if (sfxFootstep == null) return;
        _footstepSource.clip   = sfxFootstep;
        _footstepSource.volume = sfxVolume;
        _footstepSource.Play();
    }

    public void StopFootstep() => _footstepSource.Stop();
    public void PlayItemSelect()      => PlaySFX(sfxItemSelect,        sfxVolume);
}
