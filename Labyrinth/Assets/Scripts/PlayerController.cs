using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float speed = 4f;
 
    private Rigidbody2D rb;
    private Vector2 moveInput;
 
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        float xInput = Input.GetAxis("Horizontal");
        float yInput = Input.GetAxis("Vertical");
 
        moveInput = new Vector2(xInput, yInput);
        
        if (moveInput.sqrMagnitude > 1f)
        {
            moveInput.Normalize();
        }
    }
    
    void FixedUpdate()
    {
        rb.linearVelocity = moveInput * speed;
    }
}
