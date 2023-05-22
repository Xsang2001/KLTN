
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Section4
{
    public class PlayerController : MonoBehaviour
    {
        public Action<int, int> onCurHPChanged;
        [SerializeField] private Rigidbody2D m_Rigidbody;
        [SerializeField] private Animator m_Animator;
        [SerializeField] private int m_MaxHp;
        [SerializeField] private float m_WalkingSpeed;

        //jump
        [SerializeField] private float m_JumpForce;
        [SerializeField] private LayerMask m_GourndLayerMask;
        [SerializeField] private Transform m_Foot;
        [SerializeField] private Vector2 m_GroundCastSize;

        //Climb
        [SerializeField] private LayerMask m_ClimbableLayerMask;
        [SerializeField] private float m_ClimbSpeed;

        private int m_AttackHash;
        private int m_DyingHash;
        private int m_IdelHash;
        private int m_WalkingHash;
        private PlayerInputActions m_PlayerInput;
        private Vector2 m_MovementInput;
        private bool m_OnGround;
        private int m_JumpCount;
        private bool m_AttackInput;
        private Collider2D m_Collider2D;
        private int m_CurHp;
        private bool m_GetHit;
        private bool m_Dead;
        private float m_GetHitTime;

        private void OnEnable()
        {
            if (m_PlayerInput == null)
            {
                m_PlayerInput = new PlayerInputActions();
                m_PlayerInput.Player.Movement.started += OnMovement;
                m_PlayerInput.Player.Movement.performed += OnMovement;
                m_PlayerInput.Player.Movement.canceled += OnMovement;

                m_PlayerInput.Player.Jump.started += OnJump;
                m_PlayerInput.Player.Jump.performed += OnJump;
                m_PlayerInput.Player.Jump.canceled += OnJump;

                m_PlayerInput.Player.Attack.started += OnAttack;
                m_PlayerInput.Player.Attack.performed += OnAttack;
                m_PlayerInput.Player.Attack.canceled += OnAttack;
            }
            m_PlayerInput.Enable();
        }
        private void OnDisable()
        {
            if (m_PlayerInput != null)
                m_PlayerInput.Disable();
        }

        private void Start()
        {
            TryGetComponent(out m_Collider2D);
            m_AttackHash = Animator.StringToHash("Attack");
            m_DyingHash = Animator.StringToHash("Dying");
            m_WalkingHash = Animator.StringToHash("Walking");
            m_IdelHash = Animator.StringToHash("Idle");
            m_CurHp = m_MaxHp;

            if (onCurHPChanged != null)
                onCurHPChanged(m_CurHp, m_MaxHp);
        }
        private void Update()
        {
            if (m_GetHit)
            {
                m_GetHitTime -= Time.deltaTime;
                if (m_GetHitTime <= 0)
                    m_GetHit = false;
            }
        }
        private void FixedUpdate()
        {
            if (m_GetHit || m_Dead)
                return;
            m_OnGround = Physics2D.BoxCast(m_Foot.position, m_GroundCastSize, 0, Vector3.forward, 0, m_GourndLayerMask);
            if (m_OnGround)
                m_JumpCount = 0;
            CheckMovement();
            ChekClimb();
            CheckAnimations();
        }
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (m_GetHit || m_Dead)
                return;
            if (collision.CompareTag("Enemy"))
            {
                //get hit
                m_CurHp -= 1;
                m_GetHit = true;
                m_GetHitTime = 0.5f;
                if (onCurHPChanged != null)
                    onCurHPChanged(m_CurHp, m_MaxHp);

                if (m_CurHp <=0)
                {
                    m_Dead = true;
                    AudioManager.Instance.PlaySFX_PlayerDead();
                    GamePlayManager.Instance.Gameover(false);
                    playDyingAnin();
                    return;
                }
                AudioManager.Instance.PlaySFX_EnemyGetHit();

                Vector2 knockbackDirection = transform.position - collision.transform.position;
                knockbackDirection = knockbackDirection.normalized;
                m_Rigidbody.AddForce(knockbackDirection * 10, ForceMode2D.Impulse);
                StartCoroutine(GetHitFX());
            }
        }
        private IEnumerator GetHitFX()
        {
            //Cinemachine.CinemachineImpulseSource impulseSource;
            //TryGetComponent(out impulseSource);
            //impulseSource.GenerateImpulse();

            CameraShake.Instance.Shake(0.1f);
            SpriteRenderer spt;
            TryGetComponent(out spt);
            Color transparen = Color.white;
            transparen.a = 0.25f;
            int i = 0;
            while (m_GetHitTime > 0)
            {
                if (i % 2 == 0)
                    spt.color = Color.white;
                else
                    spt.color = transparen;
                i++;
                yield return null;
            }
            spt.color = Color.white;
        }
        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("MoveablePlatform"))
            {
                transform.SetParent(collision.transform);
            }
        }
        private void OnCollisionStay2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("MoveablePlatform"))
            {
                transform.SetParent(collision.transform);
            }
        }
        private void OnCollisionExit2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("MoveablePlatform"))
            {
                transform.SetParent(null);
            }
        }
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(m_Foot.position, m_GroundCastSize);
        }
        private void CheckMovement()
        {
            if (m_AttackInput)
                return;
            m_Rigidbody.velocity = new Vector2(m_MovementInput.x *m_WalkingSpeed, m_Rigidbody.velocity.y);
            if (m_Rigidbody.velocity.x >= 0)
                transform.localScale = Vector3.one;
            else
                transform.localScale = new Vector3(-1,1,1);
        }
        private void ChekClimb()
        {
            if (m_Collider2D.IsTouchingLayers(m_ClimbableLayerMask))
            {
                Vector2 velocity = m_Rigidbody.velocity;
                velocity.y = m_ClimbSpeed * m_MovementInput.y;
                m_Rigidbody.velocity = velocity;
                m_Rigidbody.gravityScale = 0;
            }
            else
                m_Rigidbody.gravityScale = 2f;
        }
        private void CheckAnimations()
        {
            m_Animator.SetBool(m_AttackHash, m_AttackInput);
            m_Animator.SetBool(m_IdelHash, m_Rigidbody.velocity.x == 0 && !m_AttackInput);
            m_Animator.SetBool(m_WalkingHash, m_Rigidbody.velocity.x != 0 && !m_AttackInput);
        }
        private void OnMovement(InputAction.CallbackContext context)
        {
            if (context.started || context.performed)
            {
                m_MovementInput = context.ReadValue<Vector2>();
                transform.localScale = new Vector3(m_MovementInput.x >= 0 ? 1 : -1, 1, 1);
            }    
            else
                m_MovementInput = Vector2.zero;
        }
        private void OnAttack(InputAction.CallbackContext context)
        {
            if (context.started || context.performed)
                m_AttackInput = true;
            else
                m_AttackInput = false;
        }
        private void PlayAttackSFX()
        {
            AudioManager.Instance.PlaySFX_MeleeSplash();
        }
        private void OnJump(InputAction.CallbackContext context)
        {
            if (m_AttackInput || m_GetHit || m_Dead)
                return;
            if(context.started || context.performed)
            {
                if (!m_OnGround && m_JumpCount >= 2)
                    return;
          
                    m_JumpCount++;
                    m_Rigidbody.velocity = new Vector2(m_Rigidbody.velocity.x, m_JumpForce);
                 
            }    
        }


        [ContextMenu("Play Attack Anin")]
        private void playAttackAnin()
        {
            m_Animator.SetBool(m_AttackHash, true);
            m_Animator.SetBool(m_IdelHash, false);
            m_Animator.SetBool(m_WalkingHash, false);
        }
        [ContextMenu("play Idle Anin")]
        private void playIdleAnin()
        {
            m_Animator.SetBool(m_IdelHash, true);
            m_Animator.SetBool(m_WalkingHash, false);
            m_Animator.SetBool(m_AttackHash, false);
        }
        [ContextMenu("Play Walking Anin")]
        private void playWalkingAnin()
        {
            m_Animator.SetBool(m_WalkingHash, true);
            m_Animator.SetBool(m_IdelHash, false);
            m_Animator.SetBool(m_AttackHash, false);
        } 
        [ContextMenu("Play Dying Anin")]
        private void playDyingAnin()
        {
            m_Animator.SetBool(m_DyingHash, true);
        }   
        [ContextMenu("Reset Anin")]
        private void RsetAnin()
        {
            m_Animator.SetBool(m_IdelHash, true);
            m_Animator.SetBool(m_WalkingHash, false);
            m_Animator.SetBool(m_AttackHash, false);
            m_Animator.SetBool(m_DyingHash, false);
        }    
    }
}