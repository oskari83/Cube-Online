using RiptideNetworking;
using RiptideNetworking.Utils;
using System.Collections.Generic;
using System;
using UnityEngine;

public enum ServerToClientId : ushort {
    playerSpawned = 1,
    playerMovement = 2,
    playerMovementPos = 3,
}

public enum ClientToServerId : ushort{
    name = 1,
    input = 2,
}

public class NetworkManager : MonoBehaviour
{
    private static NetworkManager _singleton;

    public static NetworkManager Singleton{
        get => _singleton;
        
        private set{
            if(_singleton == null)
                _singleton = value;
            else if (_singleton != value){
                Debug.Log($"{nameof(NetworkManager)} instance already exists, destroying duplicate!");
                Destroy(value);
            }
        }
    }

    public Server Server { get; private set; }

    [SerializeField] private ushort port;
    [SerializeField] private ushort maxClientCount;

    public Queue<object[]> InputSendBatchQueue = new Queue<object[]>();
    public Queue<object[]> PositionSendBatchQueue = new Queue<object[]>();

    public int serverTick {get; private set;}

    private void Awake(){
        Singleton = this;
    }

    private void Start(){
        Application.targetFrameRate = 60;
        RiptideLogger.Initialize(Debug.Log, Debug.Log, Debug.LogWarning, Debug.LogError, false);
        Server = new Server();
        Server.Start(port,maxClientCount);
        Server.ClientDisconnected += PlayerLeft;
        serverTick = 1;
    }

    private void FixedUpdate(){
        Server.Tick();
        serverTick++;
        SendMovement();
        SendMovementPos();
    }

    private void OnApplicationQuit(){
        Server.Stop();
    }

    private void PlayerLeft(object sender, ClientDisconnectedEventArgs e){
        Destroy(Player.list[e.Id].gameObject);
    }

    private void SendMovement(){
        Message message = Message.Create(MessageSendMode.unreliable, ServerToClientId.playerMovement);
        message.AddInt(NetworkManager.Singleton.serverTick);
        message.AddByte((byte)InputSendBatchQueue.Count);
        foreach(object[] item in InputSendBatchQueue){
            message.AddUShort((ushort)item[0]);
            message.AddVector3((Vector3)item[1]);
            message.AddBytes((byte[])item[2],false);
        }
        //message.AddUShort(player.Id);
        //message.AddVector3(transform.position);
        //message.AddBytes(inputs,false);
        //message.AddInt(NetworkManager.Singleton.serverTick);
        Server.SendToAll(message);
        InputSendBatchQueue.Clear();
    }

    private void SendMovementPos(){
        Message message = Message.Create(MessageSendMode.unreliable, ServerToClientId.playerMovementPos);
        message.AddInt(NetworkManager.Singleton.serverTick);
        message.AddByte((byte)PositionSendBatchQueue.Count);
        foreach(object[] item in PositionSendBatchQueue){
            message.AddUShort((ushort)item[0]);
            message.AddVector3((Vector3)item[1]);
            message.AddVector3((Vector3)item[2]);
        }
        //message.AddUShort(player.Id);
        //message.AddVector3(transform.position);
        //message.AddVector3(rb.velocity);
        //message.AddInt(NetworkManager.Singleton.serverTick);
        Server.SendToAll(message);
        PositionSendBatchQueue.Clear();
    }    
}
