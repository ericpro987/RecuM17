using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

    [RequireComponent(typeof(Animator))]
    public class PJStateMachine: MonoBehaviour, IDamageable
    {
        [SerializeField] Hitbox hitbox;
        private Animator _Animator;
        private InputSystem_Actions _inputAction;
        private InputAction _MovementAction;

        [SerializeField] private AnimationClip _AttackClip;
        [SerializeField] private AnimationClip _Attack2Clip;
        [SerializeField] private AnimationClip _IdleClip;
        [SerializeField] private AnimationClip _MoveClip;
        [SerializeField] private AnimationClip _jumpClip;
        [SerializeField] private AnimationClip _dashClip;
        [SerializeField] private float _hp;
        [SerializeField] private Slider slider;
        [SerializeField] private Camera _camera;
        private int _atack1dmg = 1;
        private int _atack2dmg = 2;
        private Rigidbody2D _rigidbody;
        private void Awake()
        {
            this.slider.maxValue = _hp;
            this.slider.value = _hp;
            _Animator = GetComponent<Animator>();
            _inputAction = new InputSystem_Actions();
            _rigidbody = this.GetComponent<Rigidbody2D>();
            _MovementAction = _inputAction.Player.Move;
            _inputAction.Player.Dash.performed += OnDash;
            _inputAction.Player.Attack.performed += OnAttack;
            _inputAction.Player.Attack2.performed += OnAttackStrong;
            _inputAction.Player.Jump.performed += OnJump;
            _inputAction.Player.Enable();
            _jumpAvailable = true;
            _dashAvailable = true;
            _invincible = false;
        }

        private enum SkeletonStates { NULL, IDLE, ATTACK, ATTACK2, JUMP, DASH, MOVE, COMBO12, COMBO21 }
        [SerializeField] private SkeletonStates _CurrentState;
        [SerializeField] private float _StateTime;
        private bool _ComboAvailable;
        private bool _jumpAvailable;
        private bool _dashAvailable;
        [SerializeField]
        private bool _invincible;

        public void StartCombo()
        {
            _ComboAvailable = true;
        }

        public void EndCombo()
        {
            _ComboAvailable = false;
        }

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
                    _Animator.Play(_IdleClip.name);
                    break;
                case SkeletonStates.MOVE:
                    _Animator.Play(_MoveClip.name);
                    break;

                case SkeletonStates.ATTACK:
                    _Animator.Play(_AttackClip.name);
                    hitbox.Damage = _atack1dmg;
                    break;
                case SkeletonStates.ATTACK2:
                    _Animator.Play(_Attack2Clip.name);
                    hitbox.Damage = _atack2dmg;
                    break;
                case SkeletonStates.JUMP:
                    _rigidbody.AddForce(new Vector2(0, 0.15f));
                    if (_jumpAvailable)
                        _Animator.Play(_jumpClip.name);
                    break;
                case SkeletonStates.DASH:
                    break;
                case SkeletonStates.COMBO12:
                    _Animator.Play(_Attack2Clip.name);
                    hitbox.Damage = (int)(_atack2dmg / 0.4f);
                    break;
                case SkeletonStates.COMBO21:
                    _Animator.Play(_AttackClip.name);
                    hitbox.Damage = (int)(_atack2dmg / 0.2f);
                    break;
                default:
                    break;
            }
        }

        private void UpdateState(SkeletonStates updateState)
        {
            Vector2 dir = _MovementAction.ReadValue<Vector2>();
            _StateTime += Time.deltaTime;

            switch (updateState)
            {
                case SkeletonStates.IDLE:
                    if (dir != Vector2.zero)
                        ChangeState(SkeletonStates.MOVE);
                    break;

                case SkeletonStates.MOVE:
                    if (dir == Vector2.zero)
                    {
                        ChangeState(SkeletonStates.IDLE);
                        break;
                    }

                    if (dir.x > 0)
                    {
                        this.transform.eulerAngles = Vector3.zero;
                   //     this.transform.Find("Canvas").eulerAngles = Vector3.zero;
                    }
                    else if (dir.x < 0)
                    {
                        this.transform.localEulerAngles = Vector3.up * 180;
              //          this.transform.Find("Canvas").localEulerAngles = Vector3.up*180;
                //        slider.gameObject.transform.localEulerAngles = Vector3.up * 180;
                    }

                    _rigidbody.velocity = dir * 2;
                    break;
                case SkeletonStates.ATTACK:
                case SkeletonStates.COMBO21:
                    if (_StateTime >= _AttackClip.length)
                        ChangeState(SkeletonStates.IDLE);
                    break;
                case SkeletonStates.ATTACK2:
                    if (_StateTime >= _Attack2Clip.length)
                        ChangeState(SkeletonStates.IDLE);
                    break;
                case SkeletonStates.JUMP:
                    if (dir.x > 0)
                    {
                        this.transform.eulerAngles = Vector3.zero;
                        //     this.transform.Find("Canvas").eulerAngles = Vector3.zero;
                    }
                    else if (dir.x < 0)
                    {
                        this.transform.localEulerAngles = Vector3.up * 180;
                        //          this.transform.Find("Canvas").localEulerAngles = Vector3.up*180;
                        //        slider.gameObject.transform.localEulerAngles = Vector3.up * 180;
                    }

                    _rigidbody.velocity = dir * 2;
                    if (_jumpAvailable)
                    {
                        Debug.Log("Entro con el available: " + _jumpAvailable);
                        _jumpAvailable = false;
                    }
                    break;
                case SkeletonStates.DASH:
                    if (_dashAvailable)
                    {
                        if(this.transform.eulerAngles == Vector3.zero)
                            _rigidbody.AddForce(new Vector2(0.1f, 0));
                        else
                            _rigidbody.AddForce(new Vector2(-0.1f, 0));
                        _invincible = true;
                        _dashAvailable = false;
                        _Animator.Play(_dashClip.name);
                    }
                    if (_StateTime >= _dashClip.length)
                        ChangeState(SkeletonStates.IDLE);
                    break;
                case SkeletonStates.COMBO12:
                    if (_StateTime >= _Attack2Clip.length)
                        ChangeState(SkeletonStates.IDLE);
                    break;

                default:
                    break;
            }
        }

        private void ExitState(SkeletonStates exitState)
        {
            switch (exitState)
            {
                case SkeletonStates.MOVE:
                    this.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                    break;

                case SkeletonStates.ATTACK:
                case SkeletonStates.JUMP:
                case SkeletonStates.COMBO12:
                case SkeletonStates.COMBO21:
                    _ComboAvailable = false;
                    break;
                case SkeletonStates.DASH:
                    _invincible = false;
                    StartCoroutine(DashCooldown());
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

                case SkeletonStates.ATTACK2:
                    if (_ComboAvailable)
                        ChangeState(SkeletonStates.COMBO21);
                    break;
                default:
                    break;
            }
        }
        private void OnAttackStrong(InputAction.CallbackContext context)
        {
            switch (_CurrentState)
            {
                case SkeletonStates.IDLE:
                case SkeletonStates.MOVE:
                    ChangeState(SkeletonStates.ATTACK2);
                    break;

                case SkeletonStates.ATTACK:
                    if (_ComboAvailable)
                        ChangeState(SkeletonStates.COMBO12);
                    break;
                default:
                    break;
            }
        }    
        private void OnJump(InputAction.CallbackContext context)
        {
            switch (_CurrentState)
            {
                case SkeletonStates.IDLE:
                case SkeletonStates.MOVE:
                    ChangeState(SkeletonStates.JUMP);
                    break;
            }
        }
        private void OnDash(InputAction.CallbackContext context)
        {
            switch (_CurrentState)
            {
                case SkeletonStates.IDLE:
                case SkeletonStates.MOVE:
                case SkeletonStates.JUMP:
                    if(_dashAvailable)
                        ChangeState(SkeletonStates.DASH);
                    break;
            };
        }
        private void Update()
        {
            slider.transform.position = new Vector3(this.transform.position.x, this.transform.position.y + 0.5f, slider.transform.position.z);
            _camera.transform.position = new Vector3(this.transform.position.x, _camera.transform.position.y, _camera.transform.position.z);
            UpdateState(_CurrentState);
        }

        IEnumerator DashCooldown()
        {
            yield return new WaitForSeconds(5);
            _dashAvailable = true;
        }
        public void ReceiveDamage(float damage)
        {
            if (!_invincible)
            {
                this._hp -= damage;
                this.slider.value = _hp;
                if (this._hp <= 0)
                {
                    SceneManager.LoadScene("GameOver");
                    _inputAction.Player.Attack.performed -= OnAttack;
                    _inputAction.Player.Attack.performed -= OnAttackStrong;
                }
            }
        }
        private void OnCollisionEnter2D(Collision2D collision)
        {
            if(collision.gameObject.layer == 8)
            {
                _jumpAvailable = true;
                ChangeState(SkeletonStates.IDLE);
            }
        }
    private void OnDisable()
    {
        _inputAction.Player.Dash.performed -= OnDash;
        _inputAction.Player.Attack.performed -= OnAttack;
        _inputAction.Player.Attack2.performed-= OnAttackStrong;
        _inputAction.Player.Jump.performed -= OnJump;
    }
    private void OnDestroy()
    {
        _inputAction.Player.Dash.performed -= OnDash;
        _inputAction.Player.Attack.performed -= OnAttack;
        _inputAction.Player.Attack2.performed-= OnAttackStrong;
        _inputAction.Player.Jump.performed -= OnJump;
    }
}
