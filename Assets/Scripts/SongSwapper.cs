using UnityEngine;
using System.Collections;

public class SeamlessSongSwapper : MonoBehaviour
{
    [SerializeField] private AudioSource redSongSource;
    [SerializeField] private AudioSource blueSongSource;
    [SerializeField] private float crossfadeDuration = 0.5f;
    [SerializeField] private AnimationCurve crossfadeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    public enum ActiveSong { Red, Blue, None }

    private ActiveSong currentActiveSong = ActiveSong.None;
    private Coroutine crossfadeCoroutine;

    public System.Action<ActiveSong> OnSongSwapped;

    public ActiveSong CurrentActiveSong => currentActiveSong;
    public bool IsSwapping => crossfadeCoroutine != null;
    public float RedVolume => redSongSource != null ? redSongSource.volume : 0f;
    public float BlueVolume => blueSongSource != null ? blueSongSource.volume : 0f;

    void Start()
    {
        InitializeAudioSources();
    }

    private void InitializeAudioSources()
    {
        if (redSongSource == null || blueSongSource == null) return;

        redSongSource.volume = 0f;
        blueSongSource.volume = 0f;
        redSongSource.loop = false;
        blueSongSource.loop = false;

        redSongSource.Play();
        blueSongSource.Play();
    }

    void Update()
    {
        if (!redSongSource.isPlaying && !blueSongSource.isPlaying)
        {
            redSongSource.Play();
            blueSongSource.Play();
        }
    }

    public void SwapToRed()
    {
        if (currentActiveSong == ActiveSong.Red && !IsSwapping) return;
        StartCrossfade(ActiveSong.Red);
    }

    public void SwapToBlue()
    {
        if (currentActiveSong == ActiveSong.Blue && !IsSwapping) return;
        StartCrossfade(ActiveSong.Blue);
    }

    public void SwapToRedInstant()
    {
        StopCurrentCrossfade();
        SetInstantVolumes(1f, 0f);
        currentActiveSong = ActiveSong.Red;
        OnSongSwapped?.Invoke(currentActiveSong);
    }

    public void SwapToBlueInstant()
    {
        StopCurrentCrossfade();
        SetInstantVolumes(0f, 1f);
        currentActiveSong = ActiveSong.Blue;
        OnSongSwapped?.Invoke(currentActiveSong);
    }

    public void MuteBoth()
    {
        StartCrossfade(ActiveSong.None);
    }

    public float GetCurrentPlaybackTime()
    {
        return redSongSource != null ? redSongSource.time : 0f;
    }

    public void SetPlaybackTime(float timeInSeconds)
    {
        if (redSongSource != null) redSongSource.time = timeInSeconds;
        if (blueSongSource != null) blueSongSource.time = timeInSeconds;
    }

    public void PauseBoth()
    {
        if (redSongSource != null) redSongSource.Pause();
        if (blueSongSource != null) blueSongSource.Pause();
    }

    public void ResumeBoth()
    {
        if (redSongSource != null) redSongSource.UnPause();
        if (blueSongSource != null) blueSongSource.UnPause();
    }

    public void SetCrossfadeDuration(float duration)
    {
        crossfadeDuration = Mathf.Max(0.1f, duration);
    }

    private void StartCrossfade(ActiveSong targetSong)
    {
        StopCurrentCrossfade();
        crossfadeCoroutine = StartCoroutine(CrossfadeCoroutine(targetSong));
    }

    private void StopCurrentCrossfade()
    {
        StopAllCoroutines();
        crossfadeCoroutine = null;
    }

    private IEnumerator CrossfadeCoroutine(ActiveSong targetSong)
    {
        float startRedVolume = redSongSource.volume;
        float startBlueVolume = blueSongSource.volume;

        float targetRedVolume = (targetSong == ActiveSong.Red) ? 1f : 0f;
        float targetBlueVolume = (targetSong == ActiveSong.Blue) ? 1f : 0f;

        float elapsed = 0f;

        while (elapsed < crossfadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / crossfadeDuration;
            float curveValue = crossfadeCurve.Evaluate(t);

            redSongSource.volume = Mathf.Lerp(startRedVolume, targetRedVolume, curveValue);
            blueSongSource.volume = Mathf.Lerp(startBlueVolume, targetBlueVolume, curveValue);

            yield return null;
        }

        redSongSource.volume = targetRedVolume;
        blueSongSource.volume = targetBlueVolume;

        currentActiveSong = targetSong;
        crossfadeCoroutine = null;

        OnSongSwapped?.Invoke(currentActiveSong);
    }

    private void SetInstantVolumes(float redVolume, float blueVolume)
    {
        if (redSongSource != null) redSongSource.volume = redVolume;
        if (blueSongSource != null) blueSongSource.volume = blueVolume;
    }

    void OnValidate()
    {
        crossfadeDuration = Mathf.Max(0.1f, crossfadeDuration);
    }
}