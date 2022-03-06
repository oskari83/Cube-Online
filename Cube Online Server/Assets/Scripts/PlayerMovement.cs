using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using RiptideNetworking;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private float movementSpeed;
    [SerializeField] private float jumpSpeed = 500f;
    [SerializeField] bool latencyOn = true;

    private float moveSpeed;

    private bool[] inputs;
    private bool[] bufferInputs;
    private float yVelocity;

    private Buffer buffer = new Buffer(5);
    private LagSimulator lagSim = new LagSimulator(0f,0f,0.25f);

    private void OnValidate(){
        if(player == null){
            player = GetComponent<Player>();
        }

        Initialize();
    }

    private void Start(){
        Initialize();
        inputs = new bool[5];
        bufferInputs = new bool[5];
    }

    private void FixedUpdate(){
        buffer.incrementTick();
        buffer.setInputs(inputs);
        if(buffer.getTick()==5){
            bufferInputs = buffer.getInputs();
        }
        Move(bufferInputs);

        SendMovement();
    }

    private void Initialize(){
        moveSpeed = movementSpeed * Time.fixedDeltaTime;
    }

    private void Move(bool[] inputss){
        Vector3 inputDirection = Vector3.zero;
        if(inputss[0])
            inputDirection.z += 1;
        if(inputss[1])
            inputDirection.x += 1;
        if(inputss[2])
            inputDirection.z -= 1;
        if(inputss[3])
            inputDirection.x -= 1;
        inputDirection *= moveSpeed;

        if(inputss[4]){
            gameObject.GetComponent<Rigidbody>().AddForce(Vector3.up * jumpSpeed);
        }

        transform.Translate(inputDirection);
    } 

    public void SetInput(bool[] inputs){
        Debug.Log(Converter.BoolsToString(inputs));
        this.inputs = inputs;
    }

    private void SendMovement(){
        Message message = Message.Create(MessageSendMode.unreliable, ServerToClientId.playerMovement);
        message.AddUShort(player.Id);
        message.AddVector3(transform.position);
        message.AddInt(NetworkManager.Singleton.tickN);
        message.AddUShort(buffer.getTick());
        message.AddUShort(buffer.getBatchTick());
        NetworkManager.Singleton.Server.SendToAll(message);
    }
}
