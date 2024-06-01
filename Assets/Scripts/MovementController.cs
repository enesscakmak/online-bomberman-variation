using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(Rigidbody2D))]
public class MovementController : MonoBehaviour
{
    PhotonView view;
    private Rigidbody2D rb;
    private Vector2 direction = Vector2.down;
    public float speed = 5f;

    [Header("Input")]
    public KeyCode inputUp = KeyCode.W;
    public KeyCode inputDown = KeyCode.S;
    public KeyCode inputLeft = KeyCode.A;
    public KeyCode inputRight = KeyCode.D;

    [Header("Sprites")]
    public AnimatedSpriteRenderer spriteRendererUp;
    public AnimatedSpriteRenderer spriteRendererDown;
    public AnimatedSpriteRenderer spriteRendererLeft;
    public AnimatedSpriteRenderer spriteRendererRight;
    public AnimatedSpriteRenderer spriteRendererDeath;
    private AnimatedSpriteRenderer activeSpriteRenderer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        activeSpriteRenderer = spriteRendererDown;
        view = GetComponent<PhotonView>();
    }

    private void Start()
    {
        
    }
    
    private void Update()
    {
        if (view.IsMine)
        {
            if (Input.GetKey(inputUp)) {
                SetDirection(Vector2.up, spriteRendererUp);
            } else if (Input.GetKey(inputDown)) {
                SetDirection(Vector2.down, spriteRendererDown);
            } else if (Input.GetKey(inputLeft)) {
                SetDirection(Vector2.left, spriteRendererLeft);
            } else if (Input.GetKey(inputRight)) {
                SetDirection(Vector2.right, spriteRendererRight);
            } else {
                SetDirection(Vector2.zero, activeSpriteRenderer);
            }
        }
        
        
    }

    private void FixedUpdate()
    {
        Vector2 position = rb.position;
        Vector2 translation = speed * Time.fixedDeltaTime * direction;

        rb.MovePosition(position + translation);
    }

    private void SetDirection(Vector2 newDirection, AnimatedSpriteRenderer spriteRenderer)
    {
        direction = newDirection;

        spriteRendererUp.enabled = spriteRenderer == spriteRendererUp;
        spriteRendererDown.enabled = spriteRenderer == spriteRendererDown;
        spriteRendererLeft.enabled = spriteRenderer == spriteRendererLeft;
        spriteRendererRight.enabled = spriteRenderer == spriteRendererRight;

        activeSpriteRenderer = spriteRenderer;
        activeSpriteRenderer.idle = direction == Vector2.zero;

		view.RPC("RPC_RotatePlayer", RpcTarget.All, newDirection);
    }

	[PunRPC]
	private void RPC_RotatePlayer(Vector2 newDirection){
	    direction = newDirection;

        spriteRendererUp.enabled = newDirection == Vector2.up;
        spriteRendererDown.enabled = newDirection == Vector2.down;
        spriteRendererLeft.enabled = newDirection == Vector2.left;
        spriteRendererRight.enabled = newDirection == Vector2.right;

        activeSpriteRenderer = newDirection == Vector2.up ? spriteRendererUp : newDirection == Vector2.down ? spriteRendererDown : newDirection == Vector2.left ? spriteRendererLeft : spriteRendererRight;
        activeSpriteRenderer.idle = newDirection == Vector2.zero;

}

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Explosion")) {
            DeathSequence();
        }
    }

    private void DeathSequence()
    {
        enabled = false;
        GetComponent<BombController>().enabled = false;

        spriteRendererUp.enabled = false;
        spriteRendererDown.enabled = false;
        spriteRendererLeft.enabled = false;
        spriteRendererRight.enabled = false;
        spriteRendererDeath.enabled = true;

        Invoke(nameof(OnDeathSequenceEnded), 1.25f);
    }

    private void OnDeathSequenceEnded()
    {
        gameObject.SetActive(false);
        GameManager.Instance.CheckWinState();
    }

}
