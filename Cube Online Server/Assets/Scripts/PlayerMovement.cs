using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RiptideNetworking;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private float movementSpeed;
    [SerializeField] private float jumpSpeed = 500f;

    private float moveSpeed;

    private bool[] inputs;
    private float yVelocity;

    private void OnValidate(){
        if(player == null){
            player = GetComponent<Player>();
        }

        Initialize();
    }

    private void Start(){
        Initialize();
        inputs = new bool[5];
    }

    private void FixedUpdate(){
        Vector3 inputDirection = Vector3.zero;
        if(inputs[0])
            inputDirection.z += 1;
        if(inputs[1])
            inputDirection.x += 1;
        if(inputs[2])
            inputDirection.z -= 1;
        if(inputs[3])
            inputDirection.x -= 1;
        Move(inputDirection, inputs[4]);
    }

    private void Initialize(){
        moveSpeed = movementSpeed * Time.fixedDeltaTime;
    }

    private void Move(Vector3 inputDirection, bool jump){
        inputDirection *= moveSpeed;

        if(jump){
            gameObject.GetComponent<Rigidbody>().AddForce(Vector3.up * jumpSpeed);
        }

        transform.Translate(inputDirection);

        SendMovement();
    }

    public void SetInput(bool[] inputs){
        this.inputs = inputs;
    }

    private void SendMovement(){
        Message message = Message.Create(MessageSendMode.unreliable, ServerToClientId.playerMovement);
        message.AddUShort(player.Id);
        message.AddVector3(transform.position);
        NetworkManager.Singleton.Server.SendToAll(message);
    }
}
