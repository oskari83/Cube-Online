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
    private Rigidbody rb;

    private Buffer buffer = new Buffer();
    private LagSimulator lagSim = new LagSimulator(0f,0f,0.1f);

    private void OnValidate(){
        if(player == null){
            player = GetComponent<Player>();
        }
    }

    private void Start(){
        if(rb == null){
            rb = gameObject.GetComponent<Rigidbody>();
        }
        inputs = new bool[4];
        bufferInputs = new bool[4];
    }

    private void FixedUpdate(){
        buffer.addInput(Tools.BoolsToByte(inputs));
        Move(inputs);
        if(latencyOn){
            if(!lagSim.CheckPacketLoss()){
                SendMovement(buffer.getBuffer());
            }
        }else{
            SendMovement(buffer.getBuffer());
        }
        if(NetworkManager.Singleton.serverTick%25==0){
            SendMovementPos();
        }
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
        inputDirection *= (movementSpeed * Time.fixedDeltaTime);

        /*
        if(inputss[4]){
            rb.AddForce(Vector3.up * jumpSpeed);
        }
        */

        rb.AddForce(inputDirection);
    } 

    public void SetInput(bool[] inputs){
        //Debug.Log(Tools.BoolsToString(inputs));
        this.inputs = inputs;
    }

    private void SendMovement(byte[] inputs){
        NetworkManager.Singleton.InputSendBatchQueue.Enqueue(new object [] {player.Id,transform.position,inputs});
    }

    private void SendMovementPos(){
        NetworkManager.Singleton.PositionSendBatchQueue.Enqueue(new object [] {player.Id,transform.position,rb.velocity});
    }
}
