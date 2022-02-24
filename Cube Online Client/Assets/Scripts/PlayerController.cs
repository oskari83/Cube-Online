using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RiptideNetworking;

public class PlayerController : MonoBehaviour
{
    private bool[] inputs;

    private void Start(){
        inputs = new bool[5];
    }

    private void Update(){
        if(Input.GetKey(KeyCode.W))
            inputs[0] = true;
        if(Input.GetKey(KeyCode.A))
            inputs[1] = true;
        if(Input.GetKey(KeyCode.S))
            inputs[2] = true;
        if(Input.GetKey(KeyCode.D))
            inputs[3] = true;
        if(Input.GetKey(KeyCode.Space))
            inputs[4] = true;
    }

    private void FixedUpdate(){
        SendInput();
        for(int i=0; i<inputs.Length; i++){
            inputs[i]=false;
        }
    }

    private void SendInput(){
        Message message = Message.Create(MessageSendMode.unreliable, ClientToServerId.input);
        message.AddBools(inputs, false);
        NetworkManager.Singleton.Client.Send(message);
    }
}
