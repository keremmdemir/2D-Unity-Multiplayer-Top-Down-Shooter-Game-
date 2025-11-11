using UnityEngine;
using Mirror;
using System.Security.Cryptography;
using System.Threading;
using Telepathy;

public class PlayerMovement : NetworkBehaviour
{

   
    public float defaultSpeed = 3f; 
    private float currentSpeed; 
    
    private Rigidbody2D rb;

    [SyncVar] private Vector2 syncedPosition;

    [SerializeField] private float syInterval = 0.1f; 
    private float syncTimer = 0f;


    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        EventBus.Subscribe<PlayerDiedEvent>(ZeroMoveSpeed);
        EventBus.Subscribe<PlayerRespawnedEvent>(SetMoveSpeed);
        
        
    }

    public override void OnStopLocalPlayer()
    {
        EventBus.Unsubscribe<PlayerDiedEvent>(ZeroMoveSpeed);
        EventBus.Unsubscribe<PlayerRespawnedEvent>(SetMoveSpeed);
    }

    private void SetMoveSpeed(PlayerRespawnedEvent evt)
    {
        currentSpeed = defaultSpeed; 
    }
    private void ZeroMoveSpeed(PlayerDiedEvent evt)
    {
        currentSpeed = 0;
        
        
        if (rb != null)
        {
            rb.velocity = Vector2.zero; 
        }
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        currentSpeed = defaultSpeed; 
    }

    void Update()
    {
       
        if (isLocalPlayer)
        {
            HandleLocalMovement(); 
            HandleNetworkThrottling();
        }
        else
        {
            ClientMovement();
        }
    }
    private void HandleLocalMovement() 
    {
       
        Vector2 move = InputDirection() * currentSpeed;
        rb.velocity = move;
    }

    private void HandleNetworkThrottling()
    {
        
        syncTimer += Time.deltaTime;

        if (syncTimer >= syInterval)
        {
           
            CmdSendPosition(rb.position);
            
            syncTimer = 0f;
        }
    }
    

    private void ClientMovement()
    {
        if (isLocalPlayer) return;
        
        
        transform.position = Vector2.Lerp(transform.position, syncedPosition, Time.deltaTime * 15f);
    }

    Vector2 InputDirection()
    {
        return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
    }

    [Command(channel = Channels.Unreliable)]
    void CmdSendPosition(Vector2 pos)
    {
        syncedPosition = pos;
    }
}