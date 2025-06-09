using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;


    [RequireComponent(typeof(Animator))]
    public class EnemyStateMachine: MonoBehaviour, IDamageable
    {
        [SerializeField] public EnemySO _enemySO;
        private Animator _animator;
        [SerializeField] private Hitbox hitbox;
        [SerializeField] private RangeDetection _rangPerseguir;
        [SerializeField] private RangeDetection _rangAtac;
        [SerializeField] private Knife[] knifes;
        [SerializeField] private RondaController ronda;        
        void OnEnable()
        {  
            this.cooldown = false;
            this.ChangeState(SkeletonStates.IDLE);
            this.GetComponent<SpriteRenderer>().color = _enemySO.color;
            this._hp = _enemySO.hp;
            this._rangAtac.GetComponent<CircleCollider2D>().radius = this._enemySO.rangeAttack;
            _rangPerseguir.OnEnter += FollowDetected;
            _rangPerseguir.OnStay += FollowDetected;
            _rangPerseguir.OnExit += FollowUndetected;
            _rangAtac.OnEnter += AttackDetected;
            _rangAtac.OnStay += AttackDetected;
            _rangAtac.OnExit += AttackUndetected;
            _animator.runtimeAnimatorController = _enemySO.animator;
            StartCoroutine(patrol());

        }
        private void Awake()
        {
        _animator = this.GetComponent<Animator>();
        }

    private enum SkeletonStates { NULL, IDLE, ATTACK, ATTACK2, MOVE, COMBO12, COMBO21 }
        [SerializeField] private SkeletonStates _CurrentState;
        [SerializeField] private float _StateTime;
        [SerializeField] private float _hp;

        public event Action<float> OnDamaged;


        private void Start()
        {
            ChangeState(SkeletonStates.IDLE);
        }

        private void ChangeState(SkeletonStates newState)
        {
            //tornar al mateix estat o no
            if (newState == _CurrentState)
                return;

            ExitState(_CurrentState);
            InitState(newState);
        }

        private void InitState(SkeletonStates initState)
        {
            _CurrentState = initState;
            _StateTime = 0f;

            switch (_CurrentState)
            {
                case SkeletonStates.IDLE:
                    this.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                    _animator.Play(_enemySO.clipIdle.name);
                    break;
                case SkeletonStates.MOVE:
                    _animator.Play(_enemySO.clipMove.name);
                    break;

                case SkeletonStates.ATTACK:
                    _animator.Play(_enemySO.clipAttack.name);
                    hitbox.Damage = _enemySO.dmg;
                    break;
                default:
                    break;
            }
        }

        private void UpdateState(SkeletonStates updateState)
        {
            Vector2 dir = this.GetComponent<Rigidbody2D>().velocity;
            _StateTime += Time.deltaTime;

            switch (updateState)
            {
                case SkeletonStates.IDLE:
                    if (detected)
                        ChangeState(SkeletonStates.MOVE);
                    break;

                case SkeletonStates.MOVE:
                    if (!detected)
                    {
                        ChangeState(SkeletonStates.IDLE);
                        break;
                    }

                    if (dir.x > 0f)
                    {
                        this.transform.eulerAngles = Vector3.zero;
                    }
                    else if (dir.x < 0f)
                    {
                        this.transform.eulerAngles = Vector3.up * 180;
                    }

                    this.GetComponent<Rigidbody2D>().velocity = dir * 1;
                    break;
                case SkeletonStates.ATTACK:
                    if (_StateTime >= _enemySO.clipAttack.length)
                        if(!attackDetection)
                            ChangeState(SkeletonStates.IDLE);
                        else
                         ChangeState(SkeletonStates.ATTACK);
                    break;
                default:
                    break;
            }
        }
        private void LateUpdate()
        {
        }
        private void ExitState(SkeletonStates exitState)
        {
            switch (exitState)
            {
                case SkeletonStates.MOVE:
                    this.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                    break;

                case SkeletonStates.ATTACK:
                    break;

                default:
                    break;
            }
        }
        private void OnAttack(InputAction.CallbackContext context)
        {
            switch (_CurrentState)
            {
                case SkeletonStates.IDLE:
                case SkeletonStates.MOVE:
                        ChangeState(SkeletonStates.ATTACK);
                    break;
                default:
                    break;
            }
        }


        private void Update()
        {
            UpdateState(_CurrentState);
        }
        IEnumerator DamagedColor()
        {
            this.GetComponent<SpriteRenderer>().color = Color.grey;
            yield return new WaitForSeconds(0.5f);
            this.GetComponent<SpriteRenderer>().color = _enemySO.color;
        }
        public void ReceiveDamage(float damage)
        {
            this._hp -= damage;
            StartCoroutine(DamagedColor());
            if (this._hp <= 0)
            {
                ronda.enemicsActuals--;
                this.gameObject.SetActive(false);
                _rangPerseguir.OnEnter -= FollowDetected;
                _rangPerseguir.OnStay -= FollowDetected;
                _rangPerseguir.OnExit -= FollowUndetected;
                _rangAtac.OnEnter -= AttackDetected;
                _rangAtac.OnStay -= AttackDetected;
                _rangAtac.OnExit -= AttackUndetected;
            }
        }
        private void FollowDetected(GameObject personatge)
        {
            if (personatge.layer == 7)
            {
                detected = true;
                this.GetComponent<Rigidbody2D>().velocity = (personatge.gameObject.transform.position - this.transform.position).normalized;
            }
        }
        bool detected = false;
        IEnumerator patrol()
        {
            while (!detected && this.isActiveAndEnabled)
            {
                if (this.transform.position.x < 0)
                {
                    this.GetComponent<Rigidbody2D>().velocity = new Vector2(2, 0);
                    yield return new WaitForSeconds(3);
                    this.GetComponent<Rigidbody2D>().velocity = new Vector2(-2, 0);
                }
                else
                {
                    this.GetComponent<Rigidbody2D>().velocity = new Vector2(-2, 0);
                    yield return new WaitForSeconds(3);
                    this.GetComponent<Rigidbody2D>().velocity = new Vector2(2, 0);
                }
            }
        }
        private void FollowUndetected(GameObject personatge)
        {
            detected = false;
            if (this.gameObject.activeSelf)
            {
                StartCoroutine(patrol());
            }
        }
        public bool cooldown = false;
        public bool attackDetection = false;
        private void AttackDetected(GameObject personatge)
        {
            if (personatge.layer == 7)
            {
                this.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                ChangeState(SkeletonStates.ATTACK);
            }
        }
        IEnumerator cooldownFalse()
        {
            yield return new WaitForSeconds(2f);
            cooldown = false;
        }
        private void spawnKife()
        {
            for(int x = 0; x < knifes.Length; x++)
            {
                if (!knifes[x].gameObject.activeSelf)
                {
                    knifes[x].Damage = (int)(_enemySO.dmg2);
                    knifes[x].gameObject.transform.position =  this.transform.position;
                    if (knifes[x] != null)
                        knifes[x].gameObject.SetActive(true);
                    break;
                }
            }
        }
        private void AttackUndetected(GameObject personatge)
        {

        }
    private void OnDisable()
    {
        _rangPerseguir.OnEnter -= FollowDetected;
        _rangPerseguir.OnStay -= FollowDetected;
        _rangPerseguir.OnExit -= FollowUndetected;
        _rangAtac.OnEnter -= AttackDetected;
        _rangAtac.OnStay -= AttackDetected;
        _rangAtac.OnExit -= AttackUndetected;
    }
    private void OnDestroy()
    {
        _rangPerseguir.OnEnter -= FollowDetected;
        _rangPerseguir.OnStay -= FollowDetected;
        _rangPerseguir.OnExit -= FollowUndetected;
        _rangAtac.OnEnter -= AttackDetected;
        _rangAtac.OnStay -= AttackDetected;
        _rangAtac.OnExit -= AttackUndetected;
    }
}
