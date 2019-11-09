using System.Collections;
using System.Reflection;
using System.IO;
using HutongGames.PlayMaker.Actions;
using ModCommon;
using ModCommon.Util;
using UnityEngine;
using UnityEngine.SceneManagement;
using Logger = Modding.Logger;
using USceneManager = UnityEngine.SceneManagement.SceneManager;
using System.Linq;
using GlobalEnums;
using System;

namespace CustomAreaTest
{
    internal class MusicControl : MonoBehaviour
    {
        AudioSource _aud;
        private bool isPlaying;
        public void Start()
        {
            _aud = gameObject.GetComponent<AudioSource>();
            USceneManager.activeSceneChanged += LastScene;
        }

        private void LastScene(Scene arg0, Scene arg1)
        {
            Log("Check1");
            if (!isPlaying && arg1.name == "zombie-scene")
            {
                Log("Check2");
                _aud.Play();
                Log("Check3");
                isPlaying = true;
            }
            if (isPlaying && !arg1.name.Contains("zombie"))
            {
                Log("Check4");
                _aud.Stop();
                isPlaying = false;
            }
        }

        public static void Log(object o)
        {
            Logger.Log("[Music] " + o);
        }
    }
}