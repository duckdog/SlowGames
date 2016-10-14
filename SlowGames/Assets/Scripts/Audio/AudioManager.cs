﻿using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// Audioを管理するクラス
/// </summary>
public class AudioManager : SingletonMonoBegaviour<AudioManager>
{
    enum Type
    {
        MASTER,
        BGM,
        SE
    }

    /// <summary>
    /// Seの最大音数
    /// </summary>
    const uint SE_CHANNEL = 15;

    [SerializeField, Tooltip("同じSEが鳴るときの最大音数")]
    uint LIMIT_SE_COUNT = 3;

    [SerializeField]
    AudioClip[] _bgmClips = null;

    [SerializeField]
    AudioClip[] _seClips = null;

    AudioSource _bgmSource = null;

    AudioSource[] _seSources = new AudioSource[SE_CHANNEL];

    List<AudioMixerGroup> _audioMixerGroup = new List<AudioMixerGroup>();

    //=====================================================================================================

    /// <summary>
    /// BGMのAudioSourceを取得
    /// </summary>
    public AudioSource getBgmSource
    {
        get
        {
            return _bgmSource;
        }
    }

    /// <summary>
    /// index番号のclipを入れ、AudioSourceを取得
    /// </summary>
    /// <param name="index">bgm番号</param>
    /// <returns></returns>
    public AudioSource getBgm(int index)
    {
        _bgmSource.clip = _bgmClips[index];
        return _bgmSource;
    }

    /// <summary>
    /// nameのclipを入れ、AudioSourceを取得
    /// </summary>
    /// <param name="name">bgm名</param>
    /// <returns></returns>
    public AudioSource getBgm(AudioName.BgmName name)
    {
        return getBgm((int)name);
    }

    /// <summary>
    /// index番号のBGMを再生
    /// </summary>
    /// <param name="index">bgm番号</param>
    /// <returns></returns>
    public AudioSource playBgm(int index)
    {
        _bgmSource.clip = _bgmClips[index];
        _bgmSource.Play();
        return _bgmSource;
    }

    /// <summary>
    /// nameのBGMを再生
    /// </summary>
    /// <param name="name">bgm名</param>
    /// <returns></returns>
    public AudioSource playBgm(AudioName.BgmName name)
    {
        return playBgm((int)name);
    }

    /// <summary>
    /// bgmを停止
    /// </summary>
    /// <returns></returns>
    public AudioSource stopBgm()
    {
        _bgmSource.Stop();
        return _bgmSource;
    }

    /// <summary>
    /// index番号のclipを入れ、AudioSourceを取得
    /// </summary>
    /// <param name="index">se番号</param>
    /// <returns>Seのチャンネル数が最大数使用していた場合nullが返る</returns>
    public AudioSource getSe(int index)
    {
        int sourceIndex = -1;
        for (int i = 0; i < _seSources.Length; ++i)
        {
            if (_seSources[i].clip != null) continue;
            sourceIndex = i;
            break;
        }

        if (sourceIndex == -1) return null;

        var source = _seSources[sourceIndex];
        source.clip = _seClips[index];
        return source;
    }

    /// <summary>
    /// nameのclipを入れ、AudioSourceを取得
    /// </summary>
    /// <param name="name">se名</param>
    /// <returns>Seのチャンネル数が最大数使用していた場合nullが返る</returns>
    public AudioSource getSe(AudioName.SeName name)
    {
        return getSe((int)name);
    }

    /// <summary>
    /// SeのAudioSourceを取得
    /// </summary>
    /// <param name="channel">チャンネル番号</param>
    /// <returns></returns>
    public AudioSource getSeSource(uint channel)
    {
        if (!(0 <= channel && channel < SE_CHANNEL)) return null;
        return _seSources[channel];
    }

    /// <summary>
    /// index番号のSeを再生
    /// Seのチャンネル数が最大数使用していた場合、再生できない
    /// </summary>
    /// <param name="index">se番号</param>
    /// <param name="loop">ループするか</param>
    /// <returns>Seのチャンネル数が最大数使用していた場合nullが返る</returns>
    public AudioSource playSe(int index, bool loop = false)
    {
        int sourceIndex = -1;
        for (int i = 0; i < _seSources.Length; ++i)
        {
            var tempSource = _seSources[i];
            if (tempSource.clip != null) continue;

            sourceIndex = i;
            break;
        }

        var count = 0;

        for (int i = 0; i < _seSources.Length; ++i)
        {
            var tempSource = _seSources[i];
            if (tempSource.clip == null) continue;

            for (int j = 0; j < _seClips.Length; ++j)
            {
                if (tempSource.clip.name == _seClips[j].name)
                {
                    count++;
                }
            }
        }

        if (count > LIMIT_SE_COUNT)
        {
            return null;
        }

        if (sourceIndex == -1) return null;

        var source = _seSources[sourceIndex];
        source.clip = _seClips[index];
        source.Play();
        source.loop = loop;
        return source;
    }

    /// <summary>
    /// index番号のSeを再生
    /// Seのチャンネル数が最大数使用していた場合、再生できない
    /// </summary>
    /// <param name="name">se名</param>    
    /// <param name="loop">ループするか</param>
    /// <returns>Seのチャンネル数が最大数使用していた場合nullが返る</returns>
    public AudioSource playSe(AudioName.SeName name, bool loop = false)
    {
        return playSe((int)name, loop);
    }

    /// <summary>
    /// すべてのSeを停止
    /// </summary>
    public void stopAllSe()
    {
        for (int i = 0; i < _seSources.Length; ++i)
        {
            var source = _seSources[i];
            if (source.clip == null) continue;
            source.Stop();
            source.loop = false;
            source.clip = null;
        }
    }

    /// <summary>
    /// 音全体の音量を変更(デシベル単位)
    /// デシベルの参考URL：http://macasakr.sakura.ne.jp/decibel.html
    /// </summary>
    /// <param name="db">20から-80(0がAudioSourceのVolumeの1に当たる)</param>
    /// <returns></returns>
    public AudioManager changeMasterVolume(float db)
    {
        int type = (int)Type.MASTER;
        _audioMixerGroup[type].audioMixer.SetFloat("MasterVolume", db);
        return this;
    }

    /// <summary>
    /// BGMの音量を変更(デシベル単位)
    /// デシベルの参考URL：http://macasakr.sakura.ne.jp/decibel.html
    /// </summary>
    /// <param name="db">20から-80(0がAudioSourceのVolumeの1に当たる)</param>
    /// <returns></returns>
    public AudioManager changeBGMVolume(float db)
    {
        int type = (int)Type.BGM;
        _audioMixerGroup[type].audioMixer.SetFloat("BGMVolume", db);
        return this;
    }

    /// <summary>
    /// SEの音量を変更(デシベル単位)
    /// デシベルの参考URL：http://macasakr.sakura.ne.jp/decibel.html
    /// </summary>
    /// <param name="db">20から-80(0がAudioSourceのVolumeの1に当たる)</param>
    /// <returns></returns>
    public AudioManager changeSEVolume(float db)
    {
        int type = (int)Type.SE;
        _audioMixerGroup[type].audioMixer.SetFloat("SEVolume", db);
        return this;
    }

    /// <summary>
    /// BGMのピッチを変更
    /// </summary>
    /// <param name="value">-3 to +3</param>
    /// <returns></returns>
    public AudioManager changeBGMPitch(float value)
    {
        _bgmSource.pitch = value;
        return this;
    }

    /// <summary>
    /// フェードインしながらBGMを再生
    /// 現在のボリュームからMaxVolume(1.0f)までフェードする
    /// </summary>
    /// <param name="name">鳴らすBGM</param>
    /// <param name="fade_time">フェードする時間</param>
    /// <returns></returns>
    public AudioManager fadeInBGM(AudioName.BgmName name, float fade_time)
    {
        StopAllCoroutines();
        playBgm(name);
        StartCoroutine(fade(fade_time, _bgmSource.volume, 1.0f));
        return this;
    }

    /// <summary>
    /// フェードインしながらBGMを再生
    /// </summary>
    /// <param name="name">鳴らすBGM</param>
    /// <param name="fade_time">フェードする時間</param>
    /// <param name="start_volume">最初のボリューム</param>
    /// <param name="end_volume">最後のボリューム</param>
    /// <returns></returns>
    public AudioManager fadeInBGM(AudioName.BgmName name, float fade_time, float start_volume, float end_volume)
    {
        StopAllCoroutines();
        playBgm(name);
        StartCoroutine(fade(fade_time, start_volume, end_volume));
        return this;
    }

    /// <summary>
    /// フェードアウトでBGMのボリュームを下げる
    /// 現在のボリュームからMinVolume(0.0f)までフェードする
    /// </summary>
    /// <param name="fade_time">フェードする時間</param>
    /// <returns></returns>
    public AudioManager fadeOutBGM(float fade_time)
    {
        StopAllCoroutines();
        StartCoroutine(fade(fade_time, _bgmSource.volume, 0.0f));
        return this;
    }

    /// <summary>
    /// フェードアウトでBGMのボリュームを下げる
    /// </summary>
    /// <param name="fade_time">フェードする時間</param>
    /// <param name="start_volume">最初のボリューム</param>
    /// <param name="end_volume">最後のボリューム</param>
    /// <returns></returns>
    public AudioManager fadeOutBGM(float fade_time, float start_volume, float end_volume)
    {
        StopAllCoroutines();
        StartCoroutine(fade(fade_time, start_volume, end_volume));
        return this;
    }

    /// <summary>
    /// フェードしながらBGMを変更する(Out→In)
    /// 現在のボリュームからフェードする
    /// </summary>
    /// <param name="next_name">次鳴らすBGMの名前</param>
    /// <param name="fade_time">フェードする時間</param>
    /// <returns></returns>
    public AudioManager fadeChangeBGM(AudioName.BgmName next_name, float fade_time)
    {
        StopAllCoroutines();
        StartCoroutine(fadeOutIn(next_name, fade_time, _bgmSource.volume, 0.0f));
        return this;
    }

    /// <summary>
    /// フェードしながらBGMを変更する(Out→In)
    /// </summary>
    /// <param name="next_name">次鳴らすBGMの名前</param>
    /// <param name="fade_time">フェードする時間</param>
    /// <param name="start_volume">最初のボリューム</param>
    /// <param name="end_volume">最後のボリューム</param>
    /// <returns></returns>
    public AudioManager fadeChangeBGM(AudioName.BgmName next_name, float fade_time, float start_volume, float end_volume)
    {
        StopAllCoroutines();
        StartCoroutine(fadeOutIn(next_name, fade_time, start_volume, end_volume));
        return this;
    }

    /// <summary>
    /// Seを検索
    /// </summary>
    /// <param name="name">Seの名前</param>
    /// <returns>検索がヒットしなかったらnull</returns>
    public AudioSource findSeSource(AudioName.SeName name)
    {
        AudioSource source = null;
        for (int i = 0; i < _seSources.Length; ++i)
        {
            if (_seSources[i].clip == null) continue;
            if (_seSources[i].clip.name != name.ToString()) continue;
            source = _seSources[i];
            break;
        }
        return source;
    }

    /// <summary>
    /// Seを検索(複数)
    /// </summary>
    /// <param name="name">Seの名前</param>
    /// <returns>検索がヒットしなかったら空</returns>
    public IEnumerable<AudioSource> findSeSources(AudioName.SeName name)
    {
        List<AudioSource> sources = new List<AudioSource>();
        for (int i = 0; i < _seSources.Length; ++i)
        {
            if (_seSources[i].clip == null) continue;
            if (_seSources[i].clip.name != name.ToString()) continue;
            sources.Add(_seSources[i]);
        }
        return sources;
    }

    /// <summary>
    /// 特定のSeを止める
    /// </summary>
    /// <param name="name">Seの名前</param>
    /// <returns></returns>
    public AudioManager stopSe(AudioName.SeName name)
    {
        var sources = findSeSources(name).GetEnumerator();
        while (sources.MoveNext())
        {
            var source = sources.Current;
            source.Stop();
            source.loop = false;
            source.clip = null;
        }
        return this;
    }

    /// <summary>
    /// 3D音SE再生
    /// </summary>
    /// <param name="gameobject"></param>
    /// <param name="index">BGM番号</param>
    /// <param name="loop">ループするか</param>
    /// <returns></returns>
    public AudioSource play3DSe(GameObject gameobject, int index, bool loop = false)
    {
        AudioSource audioSource = null;

        var audioSources = gameobject.GetComponents<AudioSource>();

        if (audioSources.Length != 0)
        {
            foreach (var tempAudioSource in audioSources)
            {
                if (tempAudioSource.isPlaying) continue;
                if (tempAudioSource.outputAudioMixerGroup.name != "SE") continue;
                audioSource = tempAudioSource;
                break;
            }
        }

        if (audioSource == null)
        {
            audioSource = gameobject.AddComponent<AudioSource>();

            audioSource.dopplerLevel = 0.0f;
            audioSource.spatialBlend = 1.0f;

            int typeIndex = (int)Type.SE;
            audioSource.outputAudioMixerGroup = _audioMixerGroup[typeIndex];
        }

        audioSource.clip = _bgmClips[index];
        audioSource.loop = loop;

        audioSource.Play();

        return audioSource;
    }

    /// <summary>
    /// 3D音BGM再生
    /// </summary>
    /// <param name="gameobject"></param>
    /// <param name="index">BGM番号</param>
    /// <param name="loop">ループするか</param>
    /// <returns></returns>
    public AudioSource play3DBgm(GameObject gameobject, int index, bool loop = true)
    {
        AudioSource audioSource = null;

        var audioSources = gameobject.GetComponents<AudioSource>();

        if (audioSources.Length != 0)
        {
            foreach (var tempAudioSource in audioSources)
            {
                if (tempAudioSource.isPlaying) continue;
                if (tempAudioSource.outputAudioMixerGroup.name != "BGM") continue;
                audioSource = tempAudioSource;
                break;
            }
        }

        if (audioSource == null)
        {
            audioSource = gameobject.AddComponent<AudioSource>();

            audioSource.dopplerLevel = 0.0f;
            audioSource.spatialBlend = 1.0f;

            int typeIndex = (int)Type.BGM;
            audioSource.outputAudioMixerGroup = _audioMixerGroup[typeIndex];
        }

        audioSource.clip = _bgmClips[index];
        audioSource.loop = loop;

        audioSource.Play();

        return audioSource;
    }

    /// <summary>
    /// 特定のオブジェクト内の3D音SE再生
    /// </summary>
    /// <param name="gameobject"></param>
    /// <param name="name">SEの名前</param>
    /// <param name="loop">ループするか</param>
    /// <returns></returns>
    public AudioSource play3DSe(GameObject gameobject, AudioName.SeName name, bool loop = false)
    {
        var index = (int)name;
        return play3DSe(gameobject, index, loop);
    }

    /// <summary>
    /// 特定のオブジェクト内の3D音BGM再生
    /// </summary>
    /// <param name="gameobject"></param>
    /// <param name="name">BGMの名前</param>
    /// <param name="loop">ループするか</param>
    /// <returns></returns>
    public AudioSource play3DBgm(GameObject gameobject, AudioName.BgmName name, bool loop = false)
    {
        var index = (int)name;
        return play3DSe(gameobject, index, loop);
    }

    /// <summary>
    /// 特定のオブジェクト内の3D音Seを止める
    /// </summary>
    /// <param name="gameobject"></param>
    /// <param name="name">SEの名前</param>
    /// <returns></returns>
    public AudioManager stop3DSe(GameObject gameobject, AudioName.SeName name)
    {
        var audioSources = gameobject.GetComponents<AudioSource>();

        foreach (var audioSource in audioSources)
        {
            if (audioSource.clip.name != name.ToString()) continue;
            audioSource.Stop();
        }
        return this;
    }

    /// <summary>
    /// 特定のオブジェクト内の3D音Bgmを止める
    /// </summary>
    /// <param name="gameobject"></param>
    /// <param name="name">BGMの名前</param>
    /// <returns></returns>
    public AudioManager stop3DBgm(GameObject gameobject, AudioName.BgmName name)
    {
        var audioSources = gameobject.GetComponents<AudioSource>();

        foreach (var audioSource in audioSources)
        {
            if (audioSource.clip.name != name.ToString()) continue;
            audioSource.Stop();
        }
        return this;
    }

    /// <summary>
    /// 特定のオブジェクト内の3D音Seを全て止める
    /// </summary>
    /// <param name="gameobject"></param>
    /// <returns></returns>
    public AudioManager stopAll3DSe(GameObject gameobject)
    {
        var audioSources = gameobject.GetComponents<AudioSource>();
        if (audioSources == null) return this;

        foreach (var audioSource in audioSources)
        {
            if (audioSource.outputAudioMixerGroup.name != "SE") continue;
            audioSource.Stop();
        }

        return this;
    }

    /// <summary>
    /// 特定のオブジェクト内の3D音Bgmを全て止める
    /// </summary>
    /// <param name="gameobject"></param>
    /// <returns></returns>
    public AudioManager stopAll3DBgm(GameObject gameobject)
    {
        var audioSources = gameobject.GetComponents<AudioSource>();
        if (audioSources == null) return this;

        foreach (var audioSource in audioSources)
        {
            if (audioSource.outputAudioMixerGroup.name != "BGM") continue;
            audioSource.Stop();
        }
        return this;
    }

    /// <summary>
    /// 特定のオブジェクトのAudioSourceを取得
    /// </summary>
    /// <param name="gameobject"></param>
    /// <returns></returns>
    public IEnumerable<AudioSource> get3DAudioSources(GameObject gameobject)
    {
        return gameobject.GetComponents<AudioSource>();
    }


    //============================================================================================

    override protected void Awake()
    {
        base.Awake();

        var audioMixer = Resources.Load<AudioMixer>("Audio/AudioMixer");

        var audioMixerGroup = audioMixer.FindMatchingGroups(string.Empty);

        _audioMixerGroup.AddRange(audioMixerGroup);

        _bgmSource = gameObject.AddComponent<AudioSource>();
        _bgmSource.loop = true;

        int bgmType = (int)Type.BGM;

        _bgmSource.outputAudioMixerGroup = _audioMixerGroup[bgmType];

        int seType = (int)Type.SE;

        for (int i = 0; i < _seSources.Length; ++i)
        {
            _seSources[i] = gameObject.AddComponent<AudioSource>();
            _seSources[i].outputAudioMixerGroup = _audioMixerGroup[seType];
        }

        _bgmClips = Resources.LoadAll<AudioClip>("Audio/BGM");
        _seClips = Resources.LoadAll<AudioClip>("Audio/SE");
    }

    void Update()
    {
        for (int i = 0; i < _seSources.Length; ++i)
        {
            var source = _seSources[i];
            if (source.clip == null) continue;
            if (source.isPlaying) continue;
            source.clip = null;
        }
    }

    IEnumerator fade(float fade_time, float start_volume, float end_volume)
    {
        float time = 0.0f;

        while (1.0f >= time)
        {
            time += Time.deltaTime / fade_time;
            _bgmSource.volume = Mathf.Lerp(start_volume, end_volume, time);
            yield return null;
        }
    }

    IEnumerator fadeOutIn(AudioName.BgmName name, float fade_time, float start_volume, float end_volume)
    {
        float time = 0.0f;

        while (1.0f >= time)
        {
            time += Time.deltaTime / fade_time;
            _bgmSource.volume = Mathf.Lerp(start_volume, end_volume, time);
            yield return null;
        }

        time = 0.0f;
        playBgm(name);

        while (1.0f >= time)
        {
            time += Time.deltaTime / fade_time;
            _bgmSource.volume = Mathf.Lerp(end_volume, start_volume, time);
            yield return null;
        }
    }
}