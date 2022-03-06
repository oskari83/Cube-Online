using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RiptideNetworking;

public class PlayerController : MonoBehaviour{

    [SerializeField] private bool latencyOn = true;
    [SerializeField] private float minimumLatency = 0.2f;
    [SerializeField] private float maximumLatency = 0.2f;
    [SerializeField] private float packetLossChance = 0.2f;

    private bool[] inputs;
    private LagSimulator latSim;
    private IEnumerator coroutine;

    private void Start(){
        latSim = new LagSimulator(minimumLatency,maximumLatency,packetLossChance);
        inputs = new bool[5];
    }

    private void Update(){
        if(Input.GetKeyDown(KeyCode.W))
            inputs[0] = true;
        if(Input.GetKeyDown(KeyCode.A))
            inputs[1] = true;
        if(Input.GetKeyDown(KeyCode.S))
            inputs[2] = true;
        if(Input.GetKeyDown(KeyCode.D))
            inputs[3] = true;
        if(Input.GetKeyDown(KeyCode.Space))
            inputs[4] = true;

        if(Input.GetKeyUp(KeyCode.W))
            inputs[0] = false;
        if(Input.GetKeyUp(KeyCode.A))
            inputs[1] = false;
        if(Input.GetKeyUp(KeyCode.S))
            inputs[2] = false;
        if(Input.GetKeyUp(KeyCode.D))
            inputs[3] = false;
        if(Input.GetKeyUp(KeyCode.Space))
            inputs[4] = false;
    }

    private void FixedUpdate(){
        if(latencyOn){
            if(!latSim.CheckPacketLoss()){
                SendInput();
            }
        }else{
            SendInput();
        }
    }

    private void SendInput(){
        Message message = Message.Create(MessageSendMode.unreliable, ClientToServerId.input);
        Debug.Log(Converter.BoolsToString(inputs));
        message.AddBools(inputs, false);
        NetworkManager.Singleton.Client.Send(message);
    }
}
