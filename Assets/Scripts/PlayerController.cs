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
    
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Material mainMaterial;
    private Vector2 direction;
    private Vector2 dashDirection;
    private bool isDashing;
    private bool isInteracting;
    private bool disableMovement;
    private bool isFacingRight = true;
    private float attackTime;
    private float dashTimer;
    private float dashCooldownTimer;
    private Animator animator;
    private float currentHealth;

    // Liberar controle para o jogador (acho q é automático, mas caso eu desligue por um motivo ou outro vai estar aqui)
    void OnEnable()
    {
        EnableCharacterControl();
    }
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentHealth = maxHealth;
        mainMaterial = spriteRenderer.material;
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
        if (!isDashing && !disableMovement)
        {
            ApplyNormalMovement();
        }
        else if (isDashing && !disableMovement)
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
        if (!isDashing)
        {
            direction.x = horizontalMovementAction.ReadValue<float>();
            direction.y = verticalMovementAction.ReadValue<float>();
            direction = direction.normalized;
        }

        if (dashAction.IsPressed() && CanDash() && direction.magnitude > 0.1f)
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
            isInteracting = true;
        }
    }

    // Processo do dash. Basicamente cuida dos timers relacionados a ele.
    private void DashProcess()
    {
        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0f)
            {
                EndDash();
            }
        }

        if (dashCooldownTimer > 0f)
        {
            dashCooldownTimer -= Time.deltaTime;
        }
    }
    
    // Esses três cuidam dos estados dos dashs
    private bool CanDash()
    {
        return !isDashing && dashCooldownTimer <= 0f;
    }

    private void StartDash()
    {
        isDashing = true;
        dashTimer = dashDuration;
        dashCooldownTimer = dashCooldown;
        dashDirection = direction;
    }

    private void EndDash()
    {
        isDashing = false;
    }

    // Esses cuidam dos estados dos ataques
    // ReSharper disable Unity.PerformanceAnalysis
    IEnumerator StartAttack()
    {
        List<GameObject> enemies = new List<GameObject>();
        animator.SetBool("isAttacking", true);
        attackTime = 0f;
        
        while (attackTime <= attackDuration)
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
        
            attackTime += Time.deltaTime;

            yield return null;
        }
    }

    private void EndAttack()
    {
        if (animator.GetBool("isAttacking"))
        {
            animator.SetBool("isAttacking", false);
        }
       
    }
    
    // Aplicar os diferentes tipos de movimento.
    private void ApplyNormalMovement()
    {
        rb.linearVelocity = direction.normalized * normalSpeed;
    }

    private void ApplyDashMovement()
    {
        rb.linearVelocity = dashDirection.normalized * dashSpeed;
    }
    
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        
        if (currentHealth <= 0)
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
        if (isFacingRight && direction.x < 0f)
        {
            transform.rotation = Quaternion.Euler(0, 180, 0);
            isFacingRight = false;
        }
        else if (!isFacingRight && direction.x > 0f)
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
            isFacingRight = true;
        }
    }

    // Tudo isso controla o knockback e a animação de dano. ufa.
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.collider.CompareTag("Enemy"))
        {
            DisableCharacterControl();
            StartFlashDamage();
            rb.linearVelocity = Vector2.zero;
            Vector2 knockbackDirection = (transform.position - other.collider.transform.position).normalized;
            rb.AddForce((knockbackDirection * knockbackForce), ForceMode2D.Impulse);
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
        disableMovement = true;
    }

    private void EnableCharacterControl()
    {
        horizontalMovementAction.Enable();
        verticalMovementAction.Enable();
        dashAction.Enable();
        attackAction.Enable();
        interactAction.Enable();
        disableMovement = false;
    }

    private void StartFlashDamage()
    {
        spriteRenderer.material = knockbackMaterial;
    }
    
    private void EndFlashDamage()
    {
        spriteRenderer.material = mainMaterial;
    }
    
    // Um OnTriggerEnter para objetos interativos
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Interactable"))
        {
            if (isInteracting)
            {
                other.GetComponent<InteractableController>().Interact();
                isInteracting = false;
            }
        }
    }
}
