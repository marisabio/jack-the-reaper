using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    // Essas são todas as variáveis que cuidam do movimento da Dorotéia. O pulo dela tem muita variáveis! É para parecer um pouco mais natural e responsivo
    // Tudo pode ser mudado no inspector dentro da Unity em si
    [Header ("Movement Settings")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float jumpForce;
    [SerializeField] private float jumpSpeed;
    [SerializeField] private float jumpAcceleration;
    [SerializeField] private float jumpMaxAcceleration;
    [SerializeField] private float fallMultiplier;
    [SerializeField] private float lowJumpMultiplier;
    [SerializeField] private float coyoteTime;
    [SerializeField] private float jumpBufferTime;
    public Transform groundCheck;
    public float groundCheckRadius;
    public LayerMask groundLayer;

    // Variáveis de combate, ainda em desenvolvimento
    [Header ("Combat Settings")]
    [SerializeField] private float maxHealth;
    [SerializeField] private float knockbackForce;
    [SerializeField] private float knockbackDuration;
    [SerializeField] private Material knockbackMaterial;
    [SerializeField] private float dyingDuration;
    [SerializeField] private float attackRadius;
    [SerializeField] private float attackDamage;
    [SerializeField] private float attackDuration;
    [SerializeField] private Transform attackPosition;
    [SerializeField] private LayerMask enemyLayer;

    // Variáveis de input usando o novo sistema de inputs da Unity. Qualquer coisa, elas também podem ser mudadas no inspector.
    [Header ("Input Settings")] 
    [SerializeField] private InputAction jumpAction;
    [SerializeField] private InputAction movementAction;
    [SerializeField] private InputAction attackAction;

    // Variáveis aleatórias 
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Material mainMaterial;
    private Collider2D col;
    private bool enableHorizontalControl;
    private bool enableVerticalControl;
    private bool isFacingRight = false;
    private bool isJumping;
    private bool isGrounded;
    private bool isAlive = true;
    private float currentHealth;
    private float horizontalInput;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private float attackTimeCounter;

    // O novo sistema de input da Unity exige que os inputs sejam ativados no código antes de serem usados. 
    // Isso é útil pq permite que a gente desative eles facilmente durante diálogos e custscenes, se necessário.
    void OnEnable()
    {
        EnableCharacterControl();
    }

    // No start a gente instanceia os componentes do Player para serem usados no código.
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        currentHealth = maxHealth;
        mainMaterial = spriteRenderer.material;
    }

    // No update vamos deixar os "processos" relacionados a diferentes elementos de gameplay da Dorotéia,
    // assim como outros elementos de física que precisam ser checados pro frame
    void Update()
    {
        // Isso aqui checa se tem um chão embaixo da Dorotéia se ela estiver viva. Bem importante!
        if (isAlive)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        }

        PlayerPrefs.SetFloat("health", currentHealth);

        MovementProcess();
        JumpingProcess();
        AttackProcess();
    }

    // Esse trecho de código cuida do processo de movimento da Dorotéia. No caso, o movimento de direita pra esquerda!
    void MovementProcess()
    {
        if (enableHorizontalControl)
        {
            // O input da direção será lido aqui e colocado numa variável.
            horizontalInput = movementAction.ReadValue<float>();

            // A velocidade da Dorotéia vai aumentar dependendo da direção do input.
            rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocityY);
        }
        
        // Código pra flipar o sprite.
        if (isFacingRight && horizontalInput > 0)
        {
            isFacingRight = false;
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else if (!isFacingRight && horizontalInput < 0)
        {
            isFacingRight = true;
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }

        // Aqui controla a animação de andar da Dorotéia. O input tem que ser maior que zero e ela precisar estar no chão.
        if (horizontalInput != 0 && isGrounded)
        {
            animator.SetBool("isWalking", true);
        }
        else
        {
            animator.SetBool("isWalking", false);
        }
    }

    // Esse trecho aqui cuida do processo de pulo. É meio complexo e tomou um tico mais do meu tempo do que eu gostaria,
    // mas acho que o resultado final ficou um pouco melhor do que só seguir o tutorial mais básico na web de pulo de platformer.
    void JumpingProcess()
    {
        if (enableVerticalControl)
        {
            //Esse trechinho aqui cuida do "coyote jump" da Dorotéia.
            if (isGrounded)
            {
                coyoteTimeCounter = coyoteTime;
            }
            else
            {
                coyoteTimeCounter -= Time.deltaTime;
            }

            // Esse trecho cuida do processo do pulo no momento em que o jogador aperta o botão de pulo.
            // Também cuida do buffer do pulo, um tempinho a mais de reação pro pulo pro jogo ficar um pouco mais responsivo.
            if (jumpAction.WasPressedThisFrame())
            {
                animator.SetBool("isLanding", false);
                jumpBufferCounter = jumpBufferTime;
                
                if (coyoteTimeCounter > 0f && jumpBufferCounter > 0f) 
                {
                    isJumping = true;
                    animator.Play("Jumping");
                    coyoteTimeCounter = 0f;
                    jumpBufferCounter = 0f;

                    // Isso cuida da física do pulo em si. A aceleração do pulo da Dorotéia e tudo mais.
                    if (isJumping)
                    {
                        rb.linearVelocity = new Vector2(rb.linearVelocityX, jumpForce);
                        float velocityRatio = rb.linearVelocityY / jumpSpeed;
                        jumpAcceleration = jumpMaxAcceleration * (1 - velocityRatio);
                        rb.linearVelocityY += jumpAcceleration * Time.deltaTime;
                        animator.SetBool("isFalling", true);
                    }
                }        
            }
            // Esse trechinho faz com que ela entre no ciclo de animação e física do pulo mesmo caso ela não pule,
            // mas esteja caindo de uma plataforma ou coisa assim. 
            else if (!isGrounded)
            {
                animator.SetBool("isLanding", false);
                jumpBufferCounter = jumpBufferTime;
                
                if (!isJumping) 
                {
                    animator.Play("Fall Jumping");
                    jumpBufferCounter = 0f;
                }        
            }
            else
            {
                jumpBufferCounter -= Time.deltaTime;
            }
            
            // A aceleração do pulo é interrompida caso o jogador solte o botão antes do ápice do pulo.
            if (jumpAction.WasReleasedThisFrame())
            {
                isJumping = false;
                animator.SetBool("isFalling", true);
            }

            // Esses dois trechinhos do código aceleram a queda do jogador dependendo do ápice do pulo.
            // Pode parecer meio esquisito mas muito platformer usa isso pra deixar a queda um pouco mas responsiva.
            if (rb.linearVelocityY < 0)
            {
                rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
                isJumping = false;
            }
            else if (rb.linearVelocityY > 0 && !isJumping)
            {         
                rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
                isJumping = false;
            }

            // Esse restinho do código determina o resto da animação de pulo, constando a queda e a aterrissagem.
            if (!isJumping)
            {
                animator.SetBool("isFalling", true);
            }

            if (animator.GetBool("isFalling") == true && isGrounded)
            {
                animator.SetBool("isFalling", false);
                animator.SetBool("isLanding", true);
            }
        }
    }

// Esse método cuida de evocar a rotina de ataque quando o botão for apertado. 
// Por ser um IEnumerator, ele vai precisar ser chamado no update por outro método. 
private void AttackProcess()
    {
        if (attackAction.WasPressedThisFrame())
        {
            StartCoroutine(StartAttack());
        }
    }
// Isso aqui cuida do começo do ataque em si. 
// Esse outro comentário abaixo e pra Unity não encher o saco sobre performance. 
// Não se preocupe! A lista vai ter pouco elementos pra ser um problema.
IEnumerator StartAttack()
    {
        List<GameObject> enemies = new List<GameObject>();
        // animator.Play("Attack");
        attackTimeCounter = 0f;
        
        while (attackTimeCounter <= attackDuration)
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
        
            attackTimeCounter += Time.deltaTime;

            yield return null;
        }
    }

    // Método que diminui o HP da Dorotéia ao levar dano. Precisa ser evocado pelo script dos inimigos e obstáculos!
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            DisableCharacterControl();
            StartCoroutine("Die");
        }
    }

    // Esses dois métodos cuidam da animação de flash da Dorotéia mudando o seu shader.
    // Queria que fosse mais elegante mas foi como eu consegui fazer pra evocar os métodos com o CollisionEnter
    private void StartFlashDamage()
    {
        spriteRenderer.material = knockbackMaterial;
        animator.Play("Damage");
    }
    
    private void EndFlashDamage()
    {
        spriteRenderer.material = mainMaterial;
    }
    
    // Esse método cuida da animação e processo de derrota da Dorotéia e faz um reload na cena. 
    private IEnumerator Die()
    {
        isAlive = false;
        isGrounded = true;
        animator.Play("Dying");
        animator.SetBool("isDying", true);
        Invoke(nameof(DisableCharacterControl), knockbackDuration + 0.1f);
        yield return new WaitForSeconds(knockbackDuration - 0.1f);
        col.isTrigger = true;
        rb.constraints = RigidbodyConstraints2D.FreezePosition;
        yield return new WaitForSeconds(dyingDuration);     
        gameObject.SetActive(false);

        // Temporário. O jogo final deve precisar de algo mais elegante que isso.
        int currentScene = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentScene);
    }

    // Esses dois métodos cuidam da ativação e desativação dos controles da Dorotéia.
    private void DisableCharacterControl()
    {
        jumpAction.Disable();
        movementAction.Disable();
        enableHorizontalControl = false;
        enableVerticalControl = false;
    }

    private void EnableCharacterControl()
    {
        jumpAction.Enable();
        movementAction.Enable();
        enableHorizontalControl = true;
        enableVerticalControl = true;

    }

    // Tudo relacionado a colliders!
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.collider.CompareTag("Enemy"))
        {
            // Tudo isso aqui podia ser evocado num método a parte, agora que notei. Se blotar demais, depois faço isso.
            DisableCharacterControl();
            StartFlashDamage();
            Debug.Log("Damage!");
            rb.linearVelocity = Vector2.zero;
            Vector2 knockbackDirection = (transform.position - other.collider.transform.position).normalized;
            rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
            Invoke(nameof(EndFlashDamage), knockbackDuration);
            Invoke(nameof(EnableCharacterControl), knockbackDuration);
        }
    }

    // Isso aqui é só pra ser possível ver o raio do detector de ataque e chão no editor da Unity.
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPosition.position, attackRadius);
    }   

}