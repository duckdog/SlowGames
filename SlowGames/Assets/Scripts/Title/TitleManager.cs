﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TitleManager : MonoBehaviour {

    enum State
    {
        Title,
        Turtreal,
        Wait
    }

    /// <summary>
    /// シーンチェンジに必要なアイテム
    /// </summary>
    [SerializeField]
    ViveGrab[] _items;

    [SerializeField]
    private Light[] _spotLights = null;

    [SerializeField]
    private GameObject[] _gunStand = null;

    [SerializeField]
    private GameObject[] _gun = null;

    [SerializeField]
    private GameObject[] _cameraRig = null;

    [SerializeField]
    private Canvas _idCanvas = null;

    private PlayerShot[] _playerShot = null;
     
    private Dictionary<State, Action> _stateUpdate = null;
    private State _state = State.Title;

    void Start()
    {
        _stateUpdate = new Dictionary<State, Action>();
        _stateUpdate.Add(State.Title, TitleUpdate);
        _stateUpdate.Add(State.Turtreal, TurtrealUpdate);
        _stateUpdate.Add(State.Wait, () => { });
    }

    void Update()
    {
        _stateUpdate[_state]();
    }

    /// <summary>
    /// TitleのUpdate(銃を台座からとるまで)
    /// </summary>
    void TitleUpdate()
    {
        // 必要なアイテムを手に持っているか確かめる
        bool isChange = true;
        foreach (var item in _items)
        {
            // 持ってないなら
            if (!item.isPick)
            {
                isChange = false;
            }
        }

        // 持っていたらシーンを変える
        if (isChange)
        {
            _state = State.Wait;
            StartCoroutine(Authentication());
        }
    }

    void TurtrealUpdate()
    {

    }

    /// <summary>
    /// ID演出のコルーチン
    /// </summary>
    /// <returns></returns>
    IEnumerator Authentication()
    {
        //IDのキャンバスを表示
        _idCanvas.gameObject.SetActive(true);
        //アニメーションが終わるまで待つ
        while(_idCanvas.GetComponentInChildren<Animator>().GetCurrentAnimatorStateInfo(0).normalizedTime < 1)
        {
            yield return null;
        }

        //Animationを待った後、UIの演出が終わるまで待つ
        yield return new WaitForSeconds(0.6f * 5 + 2.0f);
        //消えるアニメーションに変更
        _idCanvas.GetComponentInChildren<Animator>().SetBool("End", true);
        //ちょっとだけ待つ
        yield return new WaitForSeconds(0.5f);
        //CanvasのAnimationが消えたら表示を消す
        _idCanvas.gameObject.SetActive(false);
        //チュートリアルに入るコルーチンを起動
        StartCoroutine(TurtrealProduction());
    }

    /// <summary>
    /// チュートリアルまでの演出
    /// </summary>
    /// <returns></returns>
    IEnumerator TurtrealProduction()
    {
        var time = 0.0f;
        var endTime = 2.0f; 
        //銃のObjectを消す
        for(int i = 0; i < _gun.Length; i++)
        {
            Destroy(_gun[i]);
        }

        //Cameraをの切り替え
        _cameraRig[0].SetActive(false);
        _cameraRig[1].SetActive(true);

        //弾を撃てるようにする
        foreach(var shot in FindObjectsOfType<PlayerShot>())
        {
            shot.isStart = true;
        }

        //ライトを少しずつ暗くしていく
        while (_spotLights[0].intensity != 0)
        {
            time += Time.unscaledDeltaTime;
            for(int i = 0; i < _spotLights.Length; i++)
            {
                _spotLights[i].intensity = Mathf.Lerp(_spotLights[i].intensity, 0, time / endTime);
            }
            yield return null;
        }
        //銃のスタンドを消す
        for(int i = 0; i < _gunStand.Length; i++)
        {
            Destroy(_gunStand[i]);
        }
    }

}