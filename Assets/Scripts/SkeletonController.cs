using UnityEngine;
 
public class SkeletonController : MonoBehaviour
{
    [SerializeField] private float moveSpeed;
    [SerializeField] private float attackDamage;
    [SerializeField] private float minDistance;
    [SerializeField] private float maxDistance;
    [SerializeField] private float knockbackForce;
    [SerializeField] private float knockbackDuration;
    [SerializeField] private float flashDuration;
    [SerializeField] private Material knockbackMaterial;
    [SerializeField] private Transform player;
    
    private Vector2 playerPosition;
    private Vector2 skeletonPosition;
    private bool disableMovement = false;
    private float spriteDirection;
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Material mainMaterial;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        mainMaterial = spriteRenderer.material;
    }
    
    void Update()
    {
        playerPosition = (player.transform.position - rb.transform.position).normalized;
        skeletonPosition = transform.position;

        // Se lembre de estudar sobre Vector2.Dot depois. Importante!!!
        spriteDirection = Vector2.Dot(Vector2.left, skeletonPosition - (Vector2)player.transform.position);

        FlipSprite();
    }
 
    private void FixedUpdate()
    {
        if (!disableMovement)
        {
            MoveToPlayer();
            animator.SetBool("isMoving", true);
        }
        else
        {
            animator.SetBool("isMoving", false);
        }
    }
 
    // Segue o jogador
    private void MoveToPlayer()
    { 
        if (Vector2.Distance(player.transform.position, skeletonPosition) > minDistance)
        {
            rb.MovePosition(skeletonPosition + playerPosition * (moveSpeed * Time.fixedDeltaTime));
        }
        else if (Vector2.Distance(player.transform.position, skeletonPosition) < minDistance)
        {
            rb.MovePosition(skeletonPosition - playerPosition * (moveSpeed * Time.fixedDeltaTime));
        }

    }
 
    // Dá dano pro jogador
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            other.gameObject.GetComponent<PlayerController>().TakeDamage(attackDamage);
            Debug.Log("Enemy hit!!");
        }
    }

    // Flipa o sprite
    private void FlipSprite()
    {
        if (spriteDirection < 0f)
        {
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }
        else if (spriteDirection > 0f)
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
    }

    // Tudo isso controla a animação de dano e knockback. vixe
    public void KnockbackProcess()
    {
        StartFlashDamage();
        disableMovement = true;
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
}