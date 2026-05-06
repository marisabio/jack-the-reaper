using UnityEngine;

public class EnemyPatrolController : MonoBehaviour
{
    [SerializeField] private float moveSpeed;
    [SerializeField] private float attackDamage;
    [SerializeField] private float knockbackForce;
    [SerializeField] private float knockbackDuration;
    [SerializeField] private float flashDuration;
    [SerializeField] private Material knockbackMaterial;
    [SerializeField] private Transform player;

    private Vector2 playerPosition;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private bool isActive = true;
    private float direction = 1;
    private Animator animator;
    private bool isFacingRight;
    private Material mainMaterial;
    
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        mainMaterial = spriteRenderer.material;
    }

    void Update()
    {
        playerPosition = (player.transform.position - rb.transform.position).normalized;

        if (isActive)
        {
            if (isFacingRight)
            {
                rb.linearVelocity = new Vector2(-direction * moveSpeed, rb.linearVelocityY);
            }
            else if (!isFacingRight)
            {
                rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocityY);
            }
        }
    }

    public void KnockbackProcess()
    {
        StartFlashDamage();
        isActive = false;
        animator.SetBool("takingDamage", true);
        rb.linearVelocity = Vector2.zero;
        Vector2 knockbackDirection = (transform.position - player.transform.position).normalized;
        rb.AddForce((knockbackDirection * knockbackForce), ForceMode2D.Impulse);
        Invoke(nameof(EndFlashDamage), flashDuration);
    }
    
    private void StartFlashDamage()
    {
        spriteRenderer.material = knockbackMaterial;
    }
    
    private void EndFlashDamage()
    {
        spriteRenderer.material = mainMaterial;
    }

    private void OnTriggerEnter2D(Collider2D other) {

        if (other.CompareTag("Patrol Zone") && isActive)
        {
            if (isFacingRight)
            {
                isFacingRight = false;
                transform.rotation = Quaternion.Euler(0, 0, 0);
            }
            else if (!isFacingRight)
            {
                isFacingRight = true;
                transform.rotation = Quaternion.Euler(0, 180, 0);
            }
        }
    }

    // Causa dano pro jogador
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            other.gameObject.GetComponent<PlayerController>().TakeDamage(attackDamage);
            Debug.Log("Enemy hit!!");
        }
    }
}
