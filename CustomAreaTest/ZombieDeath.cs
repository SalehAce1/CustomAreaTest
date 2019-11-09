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
    internal class ZombieDeath : MonoBehaviour
    {
        Animator _anim;
        Rigidbody2D _rb;
        DamageHero _dmg;
        BoxCollider2D _bc;
        HealthManager _hm;
        AudioSource _aud;
        SkinnedMeshRenderer _skin;

        private void Awake()
        {
            _rb = gameObject.GetComponent<Rigidbody2D>();
            _aud = gameObject.GetComponent<AudioSource>();
            _anim = gameObject.GetComponent<Animator>();
            _skin = gameObject.GetComponent<SkinnedMeshRenderer>();
        }

        private void Start()
        {
            StartCoroutine(DoDeath());
        }

        private IEnumerator DoDeath()
        {
            FaceHero();
            _anim.Play("death");
            _rb.gravityScale = 1f;
            yield return new WaitForSeconds(0.75f);
            Destroy(gameObject);
        }

        private float FaceHero(bool opposite = false)
        {
            HeroController _target = HeroController.instance;
            float heroSignX = Mathf.Sign(_target.transform.GetPositionX() - gameObject.transform.GetPositionX());
            heroSignX = opposite ? -1f * heroSignX : heroSignX;
            Vector3 pScale = gameObject.transform.localScale;
            gameObject.transform.localScale = new Vector3(pScale.x, pScale.y, Mathf.Abs(pScale.z) * heroSignX);
            return heroSignX;
        }

        public static void Log(object o)
        {
            Logger.Log("[Zombie Death] " + o);
        }
    }
}