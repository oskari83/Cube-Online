using RiptideNetworking;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour{

    public static Dictionary<ushort, Player> list = new Dictionary<ushort, Player>();

    public ushort Id {get; private set; }
    public bool IsLocal {get; private set; }
    
    [SerializeField] private int lerpAmount = 15;
    [SerializeField] private float movementSpeed;
    private string username;

    private Transform normalCube;
    private Transform lerpCube;
    private ushort clientTick = 10;
    private ushort newestTick = 10;
    private bool newQueueRequired = true;

    private Queue<(byte,ushort)> MyQueue = new Queue<(byte,ushort)>();
    private Vector3 latencyStorePos;
    private Vector3 latencyStoreVel;
    private ushort executeOnTick;

    private void Start(){
        normalCube = this.gameObject.transform.GetChild(0);
        lerpCube = this.gameObject.transform.GetChild(1);
    }

    private void OnDestroy(){
        list.Remove(Id);
    }

    private void FixedUpdate(){
        if(newQueueRequired==false){
            clientTick++;
        }
        if(clientTick==executeOnTick){
            MoveWithPosLatencyIncluded();
        }

        if(MyQueue.Count!=0){
            bool[] executingInputs = GetFromQueue();
            Vector3 inputDirection = Vector3.zero;
            if(executingInputs[0])
                inputDirection.z += 1;
            if(executingInputs[1])
                inputDirection.x += 1;
            if(executingInputs[2])
                inputDirection.z -= 1;
            if(executingInputs[3])
                inputDirection.x -= 1;
            inputDirection *= (movementSpeed * Time.fixedDeltaTime);
            lerpCube.gameObject.GetComponent<Rigidbody>().AddForce(inputDirection);
        }
    }

    private void AddToQueue(byte[] inputss, ushort t){
        Debug.Log("adding on server-tick: " + t.ToString() +"  client-tick: " + clientTick.ToString());
        if(newQueueRequired){
            MyQueue = new Queue<(byte,ushort)>();
            clientTick = (ushort)(t-4-1);
            newestTick = t;
            for(int i=4; i>-1; i--){
                MyQueue.Enqueue((inputss[i],(ushort)(t-i)));
            }
            newQueueRequired = false;
        }else{
            if(t>newestTick+1){
                ushort delta = (ushort)(t-newestTick);
                if (delta>5){
                    return;
                }else{
                    for(int i=delta-1; i>-1; i--){
                        MyQueue.Enqueue((inputss[i],(ushort)(t-i)));
                    }
                }
            }else if (t==newestTick+1){
                MyQueue.Enqueue((inputss[0],t));
                newestTick = t;
            }else{
                return;
            }
        }
    }

    private bool[] GetFromQueue(){
        Debug.Log("getting from queue on client-tick: "+ clientTick.ToString()  +"  bottomOfQueueTick: " + MyQueue.Peek().Item2.ToString());
        if(MyQueue.Peek().Item2==clientTick){
            return Tools.ByteToBools(MyQueue.Dequeue().Item1);
        }
        newQueueRequired = true;
        Debug.Log("need New Queue");
        return new bool[] {false,false,false,false};
    }

    private void MoveWithInputs(Vector3 newPosition, byte[] inputss, ushort tick_){
        AddToQueue(inputss,tick_);
        
        normalCube.position = newPosition;
        /*
        if(inputss[4]){
            gameObject.GetComponent<Rigidbody>().AddForce(Vector3.up * jumpSpeed);
        }
        */
    }

    private void MoveWithPos(Vector3 pos, Vector3 vel,ushort tck){
        latencyStorePos = pos;
        latencyStoreVel = vel;
        executeOnTick = tck;
    }

    private void MoveWithPosLatencyIncluded(){
        lerpCube.gameObject.transform.position = latencyStorePos;
        lerpCube.gameObject.GetComponent<Rigidbody>().velocity = latencyStoreVel;
    }

    public static void Spawn(ushort id, string username, Vector3 position){
        Player player;
        if(id==NetworkManager.Singleton.Client.Id){
            player = Instantiate(GameLogic.Singleton.LocalPlayerPrefab, position, Quaternion.identity).GetComponent<Player>();
            player.IsLocal = true;
        }else{
            player = Instantiate(GameLogic.Singleton.PlayerPrefab, position, Quaternion.identity).GetComponent<Player>();
            player.IsLocal = false;
        }

        player.name = $"Player {id} ({(string.IsNullOrEmpty(username) ? "Guest" : username)})";
        player.Id = id;
        player.username = username;

        list.Add(id, player);
    }

    [MessageHandler((ushort)ServerToClientId.playerSpawned)]
    private static void SpawnPlayer(Message message){
        Spawn(message.GetUShort(), message.GetString(), message.GetVector3());
    }

    [MessageHandler((ushort)ServerToClientId.playerMovement)]
    private static void PlayerMovement(Message message){
        if(list.TryGetValue(message.GetUShort(), out Player player)){
            Vector3 _position = message.GetVector3();
            byte[] _inp = message.GetBytes(5);
            ushort _tick = message.GetUShort();
            player.MoveWithInputs(_position,_inp, _tick);
        }
    }

    [MessageHandler((ushort)ServerToClientId.playerMovementPos)]
    private static void PlayerMovementPos(Message message){
        if(list.TryGetValue(message.GetUShort(), out Player player)){
            Vector3 _position = message.GetVector3();
            Vector3 _velocity = message.GetVector3();
            ushort _tick = message.GetUShort();
            player.MoveWithPos(_position, _velocity,_tick);
            Debug.Log($"(0.5s pos update T: {_tick}");
        }
    }
}
