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
    internal class ZombieControl : MonoBehaviour
    {
        Animator _anim;
        Rigidbody2D _rb;
        DamageHero _dmg;
        BoxCollider2D _bc;
        HealthManager _hm;
        AudioSource _aud;
        SkinnedMeshRenderer[] _skin;
        bool isHit;
        bool isClose;

        private void Awake()
        {
            _rb = gameObject.GetComponent<Rigidbody2D>();
            _dmg = gameObject.AddComponent<DamageHero>();
            _bc = gameObject.GetComponent<BoxCollider2D>();
            _hm = gameObject.AddComponent<HealthManager>();
            _aud = gameObject.AddComponent<AudioSource>();
            _anim = gameObject.GetComponent<Animator>();
            _skin = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
        }

        private void Start()
        {
            _hm.hp = name.Contains("X") ? 30 : name.Contains("L") ? 75 : 10;
            _dmg.damageDealt = name.Contains("X") ? 10 : 1;
            _hm.OnDeath += _hm_OnDeath;
            On.HealthManager.Hit += HealthManager_Hit;
            StartCoroutine(Live());
        }

        private void HealthManager_Hit(On.HealthManager.orig_Hit orig, HealthManager self, HitInstance hitInstance)
        {
            if (self.name.Contains("zomb") && !isHit)
            {
                isHit = true;
                StartCoroutine(OnHit());
            }
            orig(self, hitInstance);
        }

        private void _hm_OnDeath()
        {
            _rb.velocity = new Vector2(0f, 0f);
            Destroy(_bc);
            Destroy(_hm);
            Destroy(_dmg);
            foreach (var i in _skin)
            {
                i.material.SetFloat("_Metallic", 0f);
            }
            gameObject.AddComponent<ZombieDeath>();
            Destroy(this);
        }

        private IEnumerator Live()
        {
            while (DistToPl() > 15f)
            {
                yield return new WaitForEndOfFrame();
            }

            if (!isClose)
            {
                isClose = true;
                StartCoroutine(MakeNoise());
            }

            _anim.Play("run");
            float dir = FaceHero();
            while (DistToPl() > 3f)
            {
                _rb.velocity = new Vector2(dir * 10f, 0f);
                yield return new WaitForEndOfFrame();
            }
            _rb.velocity = new Vector2(0f, 0f);
            _anim.Play("punch");
            dir = FaceHero();
            yield return null;
            yield return new WaitWhile(() => _anim.IsPlaying());
            _anim.Play("kick");
            dir = FaceHero();
            yield return null;
            yield return new WaitWhile(() => _anim.IsPlaying());
            _anim.Play("dodge");
            dir = FaceHero();
            _rb.velocity = new Vector2(dir * -8f, 0f);
            yield return new WaitWhile(() => _anim.IsPlaying());
            yield return null;
            StartCoroutine(Live());
        }

        private float DistToPl()
        {
            return Mathf.Abs(HeroController.instance.transform.GetPositionX() - gameObject.transform.GetPositionX());
        }

        private float FaceHero(bool opposite = false)
        {
            HeroController _target = HeroController.instance;
            float heroSignX = Mathf.Sign(_target.transform.GetPositionX()- gameObject.transform.GetPositionX());
            heroSignX = opposite ? -1f * heroSignX : heroSignX;
            Vector3 pScale = gameObject.transform.localScale;
            gameObject.transform.localScale = new Vector3(pScale.x, pScale.y, Mathf.Abs(pScale.z) * heroSignX);
            return heroSignX;
        }

        private IEnumerator MakeNoise()
        {
            while (true)
            {
                if (!isHit)
                {
                    string rnd = UnityEngine.Random.Range(1, 4).ToString();
                    _aud.PlayOneShot(SceneLoader.clips["aud" + rnd]);
                    yield return new WaitForSeconds(UnityEngine.Random.Range(3,5));
                }
                yield return new WaitForEndOfFrame();
            }
        }

        private IEnumerator OnHit()
        {
            _aud.PlayOneShot(SceneLoader.clips["audHurt"]);
            foreach (var j in _skin)
            {
                for (float i = 0f; i <= 1f; i += 0.05f)
                {
                    j.material.SetFloat("_Metallic", i);
                    yield return null;
                }
            }

            foreach (var j in _skin)
            {
                for (float i = 1f; i >= 0f; i -= 0.05f)
                {
                    j.material.SetFloat("_Metallic", i);
                    yield return null;
                }
            }
            isHit = false;
        }

        public static void Log(object o)
        {
            Logger.Log("[Zombie] " + o);
        }
    }
}