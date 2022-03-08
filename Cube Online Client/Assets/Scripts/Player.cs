using RiptideNetworking;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour{

    public static Dictionary<ushort, Player> list = new Dictionary<ushort, Player>();

    public ushort Id {get; private set; }
    public bool IsLocal {get; private set; }
    
    [SerializeField] private float movementSpeed = 500f;
    [SerializeField] private bool lerpConstantly = true;
    [SerializeField] private int lerpAmount = 15;

    private string username;

    private Transform normalCube;
    private Transform lerpCube;

    private int clientTick = 0;
    private int newestTick = 0;
    private bool newQueueRequired = true;
    private Queue<(byte,int)> MyQueue = new Queue<(byte,int)>();

    private Vector3 latencyStorePos;
    private Vector3 latencyStoreVel;
    private int executeOnTick;
    private bool needToMoveToPos = true;

    private int ticksBehind;
    private Queue<int> TickSum = new Queue<int>();
    private float averageTicksBehind;
    private int currentTickSum;

    private void Start(){
        normalCube = this.gameObject.transform.GetChild(0);
        lerpCube = this.gameObject.transform.GetChild(1);
    }

    private void OnDestroy(){
        list.Remove(Id); 
    }

    private void FixedUpdate(){
        if (newQueueRequired)
            needToMoveToPos=true;
        clientTick++;

        if(clientTick==executeOnTick){
            if(lerpConstantly){
                MoveWithPosLatencyIncluded();
            }else if(needToMoveToPos){
                MoveWithPosLatencyIncluded();
                needToMoveToPos=false;
            }
        }
        MovePlayerOnScreen();
    }

    private void MovePlayerOnScreen(){
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

    private void AddToTickSum(int behind){
        int adjustedBehind = behind -5;
        if(TickSum.Count<50){
            currentTickSum += adjustedBehind;
            TickSum.Enqueue(adjustedBehind);
        }else{
            currentTickSum -= TickSum.Peek();
            TickSum.Dequeue();
            currentTickSum += adjustedBehind;
            TickSum.Enqueue(adjustedBehind);
        }
    }

    private float getAverageTickSum(){
        return (currentTickSum / 50f);
    }

    private void AddToQueue(byte[] inputss, int t){
        //Debug.Log("adding on server-tick: " + t.ToString() +"  newest-tick: " + newestTick.ToString() +"  client-tick: " + clientTick.ToString());
        if(newQueueRequired){
            MyQueue = new Queue<(byte,int)>();
            clientTick = (t-5);
            newestTick = t;
            for(int i=4; i>-1; i--){
                MyQueue.Enqueue((inputss[i],(t-i)));
            }
            newQueueRequired = false;
        }else{
            int delta = (t-newestTick);
            if(delta>5 || delta <1){
                return;
            }else if (delta==1){
                MyQueue.Enqueue((inputss[0],t));
                newestTick = t;
            }else{
                for(int i=delta-1; i>-1; i--){
                    MyQueue.Enqueue((inputss[i],(t-i)));
                }
                newestTick = t;
            }
        }

        //so if we are running on average more than 7 ticks behind (2 behind), or only 3 ticks behind (so 2 forward), adjust again
        ticksBehind = t-clientTick;
        AddToTickSum(ticksBehind);
        if(getAverageTickSum()>2.0f || getAverageTickSum() < -2.0f){
            newQueueRequired=true;
        }
        //Debug.Log("ticks behind:" + ticksBehind.ToString() + " average: " + getAverageTickSum().ToString("0.0"));
    }

    private bool[] GetFromQueue(){
        if(MyQueue.Count!=0 && MyQueue.Peek().Item2==clientTick){
            return Tools.ByteToBools(MyQueue.Dequeue().Item1);
        }
        Debug.Log("need New Queue");
        newQueueRequired = true;
        return new bool[] {false,false,false,false};
    }

    private void MoveWithInputs(Vector3 newPosition, byte[] inputss, int tick_){
        AddToQueue(inputss,tick_);
        //reference cube is the current position
        normalCube.position = newPosition;
        //we can also lerp reference cube
        //normalCube.position = Vector3.Lerp(normalCube.position,newPosition,lerpAmount * Time.fixedDeltaTime);
        /*
        if(inputss[4]){
            gameObject.GetComponent<Rigidbody>().AddForce(Vector3.up * jumpSpeed);
        }
        */
    }

    private void MoveWithPos(Vector3 pos, Vector3 vel,int tck){
        latencyStorePos = pos;
        latencyStoreVel = vel;
        executeOnTick = tck;
    }

    private void MoveWithPosLatencyIncluded(){
        //Moves it instantly = more accurate
        //lerpCube.position = latencyStorePos;
        //Lerps it, smoother, but inaccuracy between simulation and whats shown
        lerpCube.position = Vector3.Lerp(lerpCube.position,latencyStorePos,lerpAmount * Time.fixedDeltaTime);
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
            int _tick = message.GetInt();
            player.MoveWithInputs(_position,_inp, _tick);
        }
    }

    [MessageHandler((ushort)ServerToClientId.playerMovementPos)]
    private static void PlayerMovementPos(Message message){
        if(list.TryGetValue(message.GetUShort(), out Player player)){
            Vector3 _position = message.GetVector3();
            Vector3 _velocity = message.GetVector3();
            int _tick = message.GetInt();
            player.MoveWithPos(_position, _velocity,_tick);
        }
    }
}