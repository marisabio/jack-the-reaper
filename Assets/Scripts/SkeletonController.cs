using System;
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
    
    private Vector2 _playerPosition;
    private Vector2 _skeletonPosition;
    private bool _isFacingRight =  true;
    private bool _disableMovement = false;
    private float _spriteDirection;
    private Rigidbody2D _rigidbody;
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;
    private Material _mainMaterial;
    
    void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _mainMaterial = _spriteRenderer.material;
    }
    
    void Update()
    {
        _playerPosition = (player.transform.position - _rigidbody.transform.position).normalized;
        _skeletonPosition = _rigidbody.transform.position;
        _spriteDirection = Vector2.Dot(_playerPosition, _skeletonPosition.normalized);
        FlipSprite();
    }
 
    private void FixedUpdate()
    {
        if (!_disableMovement)
        {
            MoveToPlayer();
        }
    }
 
    // Segue o jogador
    private void MoveToPlayer()
    {
        float distance = Vector2.Distance(player.transform.position, _skeletonPosition);
 
        if (distance < maxDistance && distance > minDistance)
        {
            _rigidbody.MovePosition(_skeletonPosition + _playerPosition * (moveSpeed * Time.fixedDeltaTime));
            _animator.SetBool("isMoving", true);
        }
 
        else if (distance < minDistance)
        {
            _rigidbody.MovePosition(_skeletonPosition - _playerPosition * (moveSpeed * Time.fixedDeltaTime));
            _animator.SetBool("isMoving", true);
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
    // Flipa o sprite. Dar uma mexida depois, ainda tem bugs
    private void FlipSprite()
    {
        if (_isFacingRight && _spriteDirection < 0f)
        {
            transform.rotation = Quaternion.Euler(0, 180, 0);
            _isFacingRight = false;
        }
        else if (!_isFacingRight && _spriteDirection > 0f)
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
            _isFacingRight = true;
        }
    }
    // Tudo isso controla a animação de dano e knockback. vixe
    public void KnockbackProcess()
    {
       StartFlashDamage();
       _disableMovement = true;
       _animator.SetBool("takingDamage", true);
       _rigidbody.linearVelocity = Vector2.zero;
      Vector2 knockbackDirection = (transform.position - player.transform.position).normalized;
      _rigidbody.AddForce((knockbackDirection * knockbackForce), ForceMode2D.Impulse);
       Invoke(nameof(EndFlashDamage), flashDuration);
   }
    private void StartFlashDamage()
    {
        _spriteRenderer.material = knockbackMaterial;
    }
    private void EndFlashDamage()
    {
        _spriteRenderer.material = _mainMaterial;
    }
}