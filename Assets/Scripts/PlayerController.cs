using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float normalSpeed;
    [SerializeField] private float dashSpeed;
    [SerializeField] private float dashCooldown;
    [SerializeField] private float dashDuration;
    [SerializeField] private float attackRadius;
    [SerializeField] private float attackDamage;
    [SerializeField] private float attackDuration;
    [SerializeField] private float maxHealth;
    [SerializeField] private float knockbackForce;
    [SerializeField] private float knockbackDuration;
    [SerializeField] private Material knockbackMaterial;
    [SerializeField] private Transform attackPosition;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private InputAction dashAction;
    [SerializeField] private InputAction horizontalMovementAction;
    [SerializeField] private InputAction verticalMovementAction;
    [SerializeField] private InputAction attackAction;
    [SerializeField] private InputAction interactAction;
    
    private Rigidbody2D _rigidbody;
    private SpriteRenderer _spriteRenderer;
    private Material _mainMaterial;
    private Vector2 _direction;
    private Vector2 _dashDirection;
    private bool _isDashing;
    private bool _isInteracting;
    private bool _disableMovement;
    private bool _isFacingRight = true;
    private float _attackTime;
    private float _dashTimer;
    private float _dashCooldownTimer;
    private Animator _animator;
    private float _currentHealth;

    // Liberar controle para o jogador (acho q é automático, mas caso eu desligue por um motivo ou outro vai estar aqui)
    void OnEnable()
    {
        EnableCharacterControl();
    }
    
    void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _currentHealth = maxHealth;
        _mainMaterial = _spriteRenderer.material;
    }
    
    void Update()
    {
        MovementProcess();
        DashProcess();
        AttackProcess();
        InteractProcess();
    }

    void FixedUpdate()
    {
        if (!_isDashing && !_disableMovement)
        {
            ApplyNormalMovement();
        }
        else if (_isDashing && !_disableMovement)
        {
            ApplyDashMovement();
        }
        
        FlipSprite();
    }

    //Debug para mostrar a área de ataque
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(attackPosition.position, attackRadius);
    }

    // Processo de movimento usando o novo sistema de input. Bem normal, exceto caso o dash seja ativado.
    private void MovementProcess()
    {
        if (!_isDashing)
        {
            _direction.x = horizontalMovementAction.ReadValue<float>();
            _direction.y = verticalMovementAction.ReadValue<float>();
            _direction = _direction.normalized;
        }

        if (dashAction.IsPressed() && CanDash() && _direction.magnitude > 0.1f)
        {
            StartDash();
        }
    }

    // Processo de ataque, tanto mecanicamente quanto de animação
    private void AttackProcess()
    {
        if (attackAction.WasPressedThisFrame())
        {
            StartCoroutine(StartAttack());
        }
        else
        {
            EndAttack();
        }
    }

    private void InteractProcess()
    {
        if (interactAction.WasPressedThisFrame())
        {
            _isInteracting = true;
        }
    }

    // Processo do dash. Basicamente cuida dos timers relacionados a ele.
    private void DashProcess()
    {
        if (_isDashing)
        {
            _dashTimer -= Time.deltaTime;
            if (_dashTimer <= 0f)
            {
                EndDash();
            }
        }

        if (_dashCooldownTimer > 0f)
        {
            _dashCooldownTimer -= Time.deltaTime;
        }
    }
    
    // Esses três cuidam dos estados dos dashs
    private bool CanDash()
    {
        return !_isDashing && _dashCooldownTimer <= 0f;
    }

    private void StartDash()
    {
        _isDashing = true;
        _dashTimer = dashDuration;
        _dashCooldownTimer = dashCooldown;
        _dashDirection = _direction;
    }

    private void EndDash()
    {
        _isDashing = false;
    }

    // Esses cuidam dos estados dos ataques
    // ReSharper disable Unity.PerformanceAnalysis
    IEnumerator StartAttack()
    {
        List<GameObject> enemies = new List<GameObject>();
        _animator.SetBool("isAttacking", true);
        _attackTime = 0f;
        
        while (_attackTime <= attackDuration)
        {
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPosition.position, attackRadius, enemyLayer);
            foreach (Collider2D enemy in hitEnemies)
            {
                if (enemies.Contains(enemy.gameObject))
                {
                    continue;
                }
                enemies.Add(enemy.gameObject);
                enemy.GetComponent<EnemyDamageController>().TakeDamage(attackDamage);
                Debug.Log("Hit!!");
            }
        
            _attackTime += Time.deltaTime;

            yield return null;
        }
    }

    private void EndAttack()
    {
        if (_animator.GetBool("isAttacking"))
        {
            _animator.SetBool("isAttacking", false);
        }
       
    }
    
    // Aplicar os diferentes tipos de movimento.
    private void ApplyNormalMovement()
    {
        _rigidbody.linearVelocity = _direction.normalized * normalSpeed;
    }

    private void ApplyDashMovement()
    {
        _rigidbody.linearVelocity = _dashDirection.normalized * dashSpeed;
    }
    
    public void TakeDamage(float damage)
    {
        _currentHealth -= damage;
        
        if (_currentHealth <= 0)
        {
            Invoke(nameof(Die), knockbackDuration);
        }
    }
    
    private void Die()
    {
        gameObject.SetActive(false);
    }

    // Gira o sprite quando ele vai pra esquerda. Talvez n seja a melhor maneira de se fazer isso, mas tudo bem!
    private void FlipSprite()
    {
        if (_isFacingRight && _direction.x < 0f)
        {
            transform.rotation = Quaternion.Euler(0, 180, 0);
            _isFacingRight = false;
        }
        else if (!_isFacingRight && _direction.x > 0f)
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
            _isFacingRight = true;
        }
    }

    // Tudo isso controla o knockback e a animação de dano. ufa.
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.collider.CompareTag("Enemy"))
        {
            DisableCharacterControl();
            StartFlashDamage();
            _rigidbody.linearVelocity = Vector2.zero;
            Vector2 knockbackDirection = (transform.position - other.collider.transform.position).normalized;
            _rigidbody.AddForce((knockbackDirection * knockbackForce), ForceMode2D.Impulse);
            Invoke(nameof(EnableCharacterControl), knockbackDuration);
            Invoke(nameof(EndFlashDamage), knockbackDuration);
        }
    }

    private void DisableCharacterControl()
    {
        horizontalMovementAction.Disable();
        verticalMovementAction.Disable();
        dashAction.Disable();
        attackAction.Disable();
        interactAction.Disable();
        _disableMovement = true;
    }

    private void EnableCharacterControl()
    {
        horizontalMovementAction.Enable();
        verticalMovementAction.Enable();
        dashAction.Enable();
        attackAction.Enable();
        interactAction.Enable();
        _disableMovement = false;
    }

    private void StartFlashDamage()
    {
        _spriteRenderer.material = knockbackMaterial;
    }
    
    private void EndFlashDamage()
    {
        _spriteRenderer.material = _mainMaterial;
    }
    
    // Um OnTriggerEnter para objetos interativos
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Interactable"))
        {
            if (_isInteracting)
            {
                other.GetComponent<InteractableController>().Interact();
                _isInteracting = false;
            }
        }
    }
}
