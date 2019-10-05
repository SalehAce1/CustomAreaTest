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

namespace CustomAreaTest
{
    internal class SceneLoader : MonoBehaviour
    {
        string sceneName;
        Rigidbody2D _rb;
        HeroController _target;
        private IEnumerator Start()
        {
            yield return new WaitWhile(() => !HeroController.instance);

            yield return new WaitWhile(() => !Input.GetKey(KeyCode.T));

            AssetBundle ab = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "texture"));
            Object[] resources = ab.LoadAllAssets();
            AssetBundle ab2 = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "scene"));
            sceneName = ab2.GetAllScenePaths()[0];
            GameManager unsafeInstance = GameManager.UnsafeInstance;
            unsafeInstance.BeginSceneTransition(new GameManager.SceneLoadInfo
            {
                SceneName = sceneName,
                EntryGateName = "door1",
                EntryDelay = 0f,
                Visualization = GameManager.SceneLoadVisualizations.GrimmDream,
                PreventCameraFadeOut = false,
                WaitForSceneTransitionCameraFade = false,
                AlwaysUnloadUnusedAssets = false
            });
            yield return new WaitWhile(() => !GameObject.Find("bg"));
            _target = HeroController.instance;
            _target.transform.SetPosition2D(14f, 14f);
            foreach (var i in FindObjectsOfType<GameObject>())
            {
                if (i.name.Contains("block"))
                {
                    i.GetComponent<SpriteRenderer>().material = new Material(Shader.Find("Sprites/Default"));
                }
                else if (i.name.Contains("bg"))
                {
                    i.transform.SetPosition3D(i.transform.position.x, i.transform.position.y, 1f);
                    i.GetComponent<SpriteRenderer>().material = new Material(Shader.Find("Sprites/Default"));
                }
                else if (i.name.Contains("PK"))
                {
                    i.GetComponent<SpriteRenderer>().material = new Material(Shader.Find("Sprites/Default"));
                }
                else if (i.name == "bg")
                {
                    i.GetComponent<SpriteRenderer>().material = new Material(Shader.Find("Sprites/Default"));
                }
            }
            _rb = _target.GetComponent<Rigidbody2D>();
            On.HeroController.CheckTouchingGround += HeroController_CheckTouchingGround;
            On.HeroController.Jump += HeroController_Jump;
            On.HeroController.JumpReleased += HeroController_JumpReleased;
            On.HeroController.DoubleJump += HeroController_DoubleJump;

            while (true)
            {
                if (Input.GetKeyDown(KeyCode.R))
                {
                    _target.spellControl.GetAction<SetVelocity2d>("Quake1 Down", 6).y.Value *= -1f;
                    _target.spellControl.GetAction<SetVelocity2d>("Quake2 Down", 6).y.Value *= -1f;
                    Physics2D.gravity = new Vector2(Physics2D.gravity.x, Physics2D.gravity.y * -1f);
                    _target.transform.localScale = new Vector2(_target.transform.localScale.x, _target.transform.localScale.y * -1f);
                }
                yield return new WaitForEndOfFrame();
            }
        }

        private void GameManager_BeginSceneTransition(On.GameManager.orig_BeginSceneTransition orig, GameManager self, GameManager.SceneLoadInfo info)
        {
            /*Log("---------------------------------");
            Log(info.SceneName);
            Log(info.EntryGateName);
            Log(info.EntryDelay);
            Log(info.Visualization);
            Log(info.PreventCameraFadeOut);
            Log(info.WaitForSceneTransitionCameraFade);
            Log(info.AlwaysUnloadUnusedAssets);*/
            if (info.SceneName== "GG_Workshop")
            {
                AssetBundle ab = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "texture"));
                Object[] resources = ab.LoadAllAssets();
                AssetBundle ab2 = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "scene"));
                sceneName = ab2.GetAllScenePaths()[0];
                Log("oof");
                info.SceneName = sceneName;
                //info.EntryGateName = "left1";
                //info.EntryDelay = 0.2f;
                //info.Visualization = GameManager.SceneLoadVisualizations.Dream;
                //info.PreventCameraFadeOut = true;
                //info.WaitForSceneTransitionCameraFade = true;
                info.AlwaysUnloadUnusedAssets = false;
            }
            orig(self, info);
            //StartCoroutine(WaitForHero());
        }

        void LateUpdate()
        {
            if (_rb.velocity.y > _target.MAX_FALL_VELOCITY && !_target.inAcid && !_target.controlReqlinquished && !_target.cState.shadowDashing && !_target.cState.spellQuake)
            {
                _rb.velocity = new Vector2(_rb.velocity.x, _target.MAX_FALL_VELOCITY);
            }
        }

        private bool CheckTouchingGround(HeroController self)
        {
            Collider2D col2d = self.GetComponent<Collider2D>();
            Vector2 vector = new Vector2(col2d.bounds.min.x, col2d.bounds.center.y);
            Vector2 vector2 = col2d.bounds.center;
            Vector2 vector3 = new Vector2(col2d.bounds.max.x, col2d.bounds.center.y);
            float distance = col2d.bounds.extents.y + 0.16f;
            UnityEngine.Debug.DrawRay(vector, Vector2.down, Color.yellow);
            UnityEngine.Debug.DrawRay(vector2, Vector2.down, Color.yellow);
            UnityEngine.Debug.DrawRay(vector3, Vector2.down, Color.yellow);
            RaycastHit2D raycastHit2D = Physics2D.Raycast(vector, Vector2.up, distance, 256);
            RaycastHit2D raycastHit2D2 = Physics2D.Raycast(vector2, Vector2.up, distance, 256);
            RaycastHit2D raycastHit2D3 = Physics2D.Raycast(vector3, Vector2.up, distance, 256);
            return raycastHit2D.collider != null || raycastHit2D2.collider != null || raycastHit2D3.collider != null; 
        }

        private void HeroController_JumpReleased(On.HeroController.orig_JumpReleased orig, HeroController self)
        {
            if (Physics2D.gravity.y > 0f)
            {
                int jumped_steps = Modding.ReflectionHelper.GetAttr<HeroController, int>(self, "jumped_steps");
                if (_rb.velocity.y < 0f && jumped_steps >= _target.JUMP_STEPS_MIN && !_target.inAcid && !_target.cState.shroomBouncing)
                {
                    bool jumpReleaseQueueingEnabled = Modding.ReflectionHelper.GetAttr<HeroController, bool>(self, "jumpReleaseQueueingEnabled");
                    bool jumpReleaseQueuing = Modding.ReflectionHelper.GetAttr<HeroController, bool>(self, "jumpReleaseQueuing");
                    int jumpReleaseQueueSteps = Modding.ReflectionHelper.GetAttr<HeroController, int>(self, "jumpReleaseQueueSteps");
                    if (jumpReleaseQueueingEnabled)
                    {
                        if (jumpReleaseQueuing && jumpReleaseQueueSteps <= 0)
                        {
                            _rb.velocity = new Vector2(_rb.velocity.x, 0f);
                            self.cState.jumping = false;
                            Modding.ReflectionHelper.SetAttr(self, "jumpReleaseQueuing", false);
                            Modding.ReflectionHelper.SetAttr(self, "jump_steps", 0);
                        }
                    }
                    else
                    {
                        _rb.velocity = new Vector2(_rb.velocity.x, 0f);
                        self.cState.jumping = false;
                        Modding.ReflectionHelper.SetAttr(self, "jumpReleaseQueuing", false);
                        Modding.ReflectionHelper.SetAttr(self, "jump_steps", 0);
                    }
                }
                Modding.ReflectionHelper.SetAttr(self, "jumpQueuing", false);
                Modding.ReflectionHelper.SetAttr(self, "doubleJumpQueuing", false);
                if (_target.cState.swimming)
                {
                    _target.cState.swimming = false;
                }
            }
            else
            {
                orig(self);
            }
        }

        private void HeroController_DoubleJump(On.HeroController.orig_DoubleJump orig, HeroController self)
        {
            if (Physics2D.gravity.y > 0f)
            {
                int doubleJump_steps = Modding.ReflectionHelper.GetAttr<HeroController, int>(_target, "doubleJump_steps");
                if (doubleJump_steps <= _target.DOUBLE_JUMP_STEPS)
                {
                    if (doubleJump_steps > 3)
                    {
                        _rb.velocity = new Vector2(_rb.velocity.x, _target.JUMP_SPEED * -1.1f);
                    }
                    Modding.ReflectionHelper.SetAttr(_target, "doubleJump_steps", doubleJump_steps + 1);
                }
                else
                {
                    _target.cState.doubleJumping = false;
                    Modding.ReflectionHelper.SetAttr(_target, "doubleJump_steps", 0);
                }
                if (_target.cState.onGround)
                {
                    _target.cState.doubleJumping = false;
                    Modding.ReflectionHelper.SetAttr(_target, "doubleJump_steps", 0);
                }
            }
            else
            {
                orig(self);
            }
        }

        private void HeroController_Jump(On.HeroController.orig_Jump orig, HeroController self)
        {
            if (Physics2D.gravity.y > 0f)
            {
                int jump_steps = Modding.ReflectionHelper.GetAttr<HeroController, int>(self, "jump_steps");
                int jumped_steps = Modding.ReflectionHelper.GetAttr<HeroController, int>(self, "jumped_steps");
                if (jump_steps <= self.JUMP_STEPS)
                {
                    if (self.inAcid)
                    {
                        _rb.velocity = new Vector2(_rb.velocity.x, -1f * self.JUMP_SPEED_UNDERWATER);
                    }
                    else
                    {
                        _rb.velocity = new Vector2(_rb.velocity.x, -1f * self.JUMP_SPEED);
                    }
                    Modding.ReflectionHelper.SetAttr(self, "jump_steps", jump_steps + 1);
                    Modding.ReflectionHelper.SetAttr(self, "jumped_steps", jumped_steps + 1);
                    Modding.ReflectionHelper.SetAttr(self, "ledgeBufferSteps", 0);
                }
                else
                {
                    self.cState.jumping = false;
                    Modding.ReflectionHelper.SetAttr(self, "jumpReleaseQueuing", false);
                    Modding.ReflectionHelper.SetAttr(self, "jump_steps", 0);
                }

            }
            else
            {
                orig(self);
            }
        }

        private bool HeroController_CheckTouchingGround(On.HeroController.orig_CheckTouchingGround orig, HeroController self)
        {
            if (Physics2D.gravity.y > 0f)
            {
                return CheckTouchingGround(self);
            }
            return orig(self);
        }

        public static void Log(object o)
        {
            Logger.Log("[Area Loader] " + o);
        }
    }
}