using UnityEngine;

public class Hero : MonoBehaviour
{
    [SerializeField] 
    private float speed = 3f;
    
    [SerializeField] 
    private int lives = 5;
    
    [SerializeField] 
    private float jumpForce = 0.3f;

    [SerializeField]
    private Animator animator;

    [SerializeField]
    private Collider2D feetColider;



    private bool isRight;
    private bool isGrounded = false;

    private Rigidbody2D rb;
    private SpriteRenderer sprite;

    public bool IsRight { get => isRight;}
    public Animator Animator { get => animator;}

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponentInChildren<SpriteRenderer>();
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        isGrounded = true;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        isGrounded = false;
    }



    private void Update()
    {
        if (Input.GetButton("Horizontal"))
        { 
            Run(); 
        }
        if (isGrounded && Input.GetButton("Jump"))
        {
            Jump();
        }
        feetColider.enabled = !(Input.GetKey(KeyCode.S) || rb.linearVelocityY > 0);
        
        animator.SetBool("IsMove", Input.GetAxis("Horizontal") != 0);
    }

    private void Run()
    {
        Vector3 dir = transform.right * Input.GetAxis("Horizontal");
        transform.position = Vector3.MoveTowards(transform.position, transform.position + dir, speed * Time.deltaTime);
        sprite.flipX = dir.x < 0.0f;
        isRight = dir.x > 0.0f;
    }

    private void Jump()
    {
        rb.AddForce(transform.up * jumpForce, ForceMode2D.Impulse);
        isGrounded = false;
    }
}
