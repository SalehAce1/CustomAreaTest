using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using HutongGames.PlayMaker.Actions;
using ModCommon;
using ModCommon.Util;
using UnityEngine;
using UnityEngine.SceneManagement;
using Logger = Modding.Logger;
using Modding;
using USceneManager = UnityEngine.SceneManagement.SceneManager;
using System.Linq;
using GlobalEnums;
using UnityEngine.Audio;

namespace CustomAreaTest
{
    internal class SceneLoader : MonoBehaviour
    {
        string sceneName;
        AssetBundle ab, ab2, ab3, ab4;
        public static Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();
        HeroController _target;

        private IEnumerator Start()
        {
            On.GameManager.EnterHero += GameManager_EnterHero;
            yield return new WaitWhile(() => !HeroController.instance);
            _target = HeroController.instance;
        }

        private void CreateGateway(string gateName, Vector2 pos, Vector2 size, string toScene, string entryGate,
                                   bool right, bool left, bool onlyOut, GameManager.SceneLoadVisualizations vis)
        {
            GameObject gate = new GameObject(gateName);
            gate.transform.SetPosition2D(pos);
            var tp = gate.AddComponent<TransitionPoint>();
            if (!onlyOut)
            {
                var bc = gate.AddComponent<BoxCollider2D>();
                bc.size = size;
                bc.isTrigger = true;
                tp.targetScene = toScene;
                tp.entryPoint = entryGate;
            }
            tp.alwaysEnterLeft = left;
            tp.alwaysEnterRight = right;
            GameObject rm = new GameObject("Hazard Respawn Marker");
            rm.transform.parent = tp.transform;
            rm.transform.position = new Vector2(rm.transform.position.x - 3f, rm.transform.position.y);
            var tmp = rm.AddComponent<HazardRespawnMarker>();
            tp.respawnMarker = rm.GetComponent<HazardRespawnMarker>();
            tp.sceneLoadVisualization = vis;
        }

        private void GameManager_EnterHero(On.GameManager.orig_EnterHero orig, GameManager self, bool additiveGateSearch)
        {
            self.UpdateSceneName();
            if (self.sceneName == "zombie-scene")
            {
                foreach (var i in FindObjectsOfType<AudioSource>().Where(x => x.name == "Main")) Destroy(i);
                sceneName = "zombie-scene";
                CreateGateway("left test2", new Vector2(42.7f, 3.8f), new Vector2(1f, 4f), 
                              "GG_Workshop", "left test", false, true, false, GameManager.SceneLoadVisualizations.Default);
                CreateGateway("right test3", new Vector2(146.8f, 3.8f), new Vector2(2.4f, 5.2f),
                              "zombie-scene2", "left test3", true, false, true, GameManager.SceneLoadVisualizations.Default);
                orig(self, false);
                StartCoroutine(DoorControl());
                StartCoroutine(CameraControl());
                return;
            }
            else if (self.sceneName == "zombie-scene2")
            {
                sceneName = "zombie-scene2";
                CreateGateway("left test3", new Vector2(89.45f, 4.3f), new Vector2(1f, 4f),
                              "zombie-scene", "right test3", false, true, false, GameManager.SceneLoadVisualizations.Default);
                Log("made gateway");
                orig(self, false);
                StartCoroutine(CameraControl2());
                return;
            }
            else if (self.sceneName == "GG_Workshop")
            {
                if (ab == null)
                {
                    ab = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "zombiebundle"));
                    ab3 = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "zombiebundle2"));
                    ab2 = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "zombscene"));
                    ab4 = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "zombscene2"));
                    GameObject _aud = new GameObject();
                    var f = _aud.AddComponent<AudioSource>();
                    f.loop = true;
                    f.clip = ab.LoadAsset<AudioClip>("audMusic");
                    _aud.AddComponent<MusicControl>();
                    DontDestroyOnLoad(_aud);
                    foreach (AudioClip i in ab.LoadAllAssets<AudioClip>())
                    {
                        clips.Add(i.name, i);
                    }
                }
                sceneName = "GG_Workshop";
                CreateGateway("left test", new Vector2(7.6f, 36.4f), new Vector2(1f, 4f),
                              "zombie-scene", "left test2", false, true, false, GameManager.SceneLoadVisualizations.Default);
                orig(self, false);
                return;
            }

            orig(self, additiveGateSearch);
        }

        IEnumerator CameraControl()
        {
            yield return new WaitWhile(() => !GameObject.Find("zombie10"));
            foreach (var i in FindObjectsOfType<GameObject>())
            {
                Log(i.name);
                if (i.name.Contains("bottom") || i.name.Contains("top") || i.name.Contains("overlay"))
                {
                    try
                    {
                        i.GetComponent<SpriteRenderer>().material = new Material(Shader.Find("Sprites/Default"));
                    }
                    catch (System.Exception e)
                    {
                        Log(e);
                    }
                }
                if (i.name.Contains("zombie"))
                {
                    try
                    {
                        i.AddComponent<ZombieControl>();
                    }
                    catch (System.Exception e)
                    {
                        Log(e);
                    }
                }

            }

            yield return new WaitWhile(() => !GameCameras.instance);
            while (sceneName == "zombie-scene")
            {
                float tarX = _target.transform.position.x;
                if (tarX < 57f)
                {
                    GameCameras.instance.mainCamera.transform.position = new Vector3(57f, 8.3f, -38.1f);
                    GameCameras.instance.cameraController.SetMode(CameraController.CameraMode.FROZEN);
                }
                else if (tarX > 142f)
                {
                    GameCameras.instance.mainCamera.transform.position = new Vector3(142f, 8.3f,-38.1f);
                    GameCameras.instance.cameraController.SetMode(CameraController.CameraMode.FROZEN);
                }
                else
                {
                    GameCameras.instance.cameraController.SetMode(CameraController.CameraMode.FOLLOWING);
                }
                yield return new WaitForEndOfFrame();
            }
        }

        IEnumerator DoorControl()
        {
            InputHandler ih = GameManager.instance.GetComponent<InputHandler>();
            yield return new WaitWhile(() => HeroController.instance.transform.GetPositionX() < 144f || HeroController.instance.transform.GetPositionX() > 148f || !ih.inputActions.up.WasPressed);            tk2dSpriteAnimator anim = HeroController.instance.gameObject.GetComponent<tk2dSpriteAnimator>();
            _target.RelinquishControl();
            _target.StopAnimationControl();
            anim.Play("Enter");
            GameManager.instance.playerData.disablePause = true;
            PlayMakerFSM pm = GameCameras.instance.tk2dCam.gameObject.LocateMyFSM("CameraFade");
            pm.SendEvent("FADE OUT");
            yield return new WaitForSeconds(0.5f);
            GameManager.instance.BeginSceneTransition(new GameManager.SceneLoadInfo
            {
                SceneName = "zombie-scene2",
                EntryGateName = "left test3",
                Visualization = GameManager.SceneLoadVisualizations.Default,
                WaitForSceneTransitionCameraFade = false,
                
            });
        }

        IEnumerator CameraControl2() //18.5f
        {
            yield return new WaitWhile(() => !GameObject.Find("top2"));
            foreach (var i in FindObjectsOfType<CameraLockArea>())
            {
                Log("SSASA " + i.name);
                GameCameras.instance.cameraController.ReleaseLock(i);
            }
            foreach (var i in FindObjectsOfType<GameObject>())
            {
                Log(i.name);
                if (i.name.Contains("bottom2") || i.name.Contains("top2") || i.name.Contains("overlay2") || i.name.Contains("plat"))
                {
                    try
                    {
                        i.GetComponent<SpriteRenderer>().material = new Material(Shader.Find("Sprites/Default"));
                    }
                    catch (System.Exception e)
                    {
                        Log(e);
                    }
                }
                if (i.name.Contains("zombie"))
                {
                    try
                    {
                        i.AddComponent<ZombieControl>();
                    }
                    catch (System.Exception e)
                    {
                        Log(e);
                    }
                }
            }

            yield return new WaitWhile(() => !GameCameras.instance);
            while (sceneName == "zombie-scene2")
            {
                float tarX = _target.transform.position.x;
                if (tarX < 103.4f)
                {
                    GameCameras.instance.mainCamera.transform.position = new Vector3(103.4f, 8.3f, -38.1f);
                    GameCameras.instance.cameraController.SetMode(CameraController.CameraMode.FROZEN);
                }
                else
                {
                    GameCameras.instance.cameraController.SetMode(CameraController.CameraMode.FOLLOWING);
                }
                yield return new WaitForEndOfFrame();
            }
        }

        public static void Log(object o)
        {
            Logger.Log("[Area Loader] " + o);
        }
    }
}