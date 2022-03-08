using RiptideNetworking;
using RiptideNetworking.Utils;
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
    }

    private void OnApplicationQuit(){
        Server.Stop();
    }

    private void PlayerLeft(object sender, ClientDisconnectedEventArgs e){
        Destroy(Player.list[e.Id].gameObject);
    }
}
