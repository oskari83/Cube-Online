using RiptideNetworking;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Dictionary<ushort, Player> list = new Dictionary<ushort, Player>();

    public ushort Id {get; private set; }
    public bool IsLocal {get; private set; }
    public int lerpAmount = 15;
    private string username;

    private Transform normalCube;
    private Transform lerpCube;

    private void Start(){
        normalCube = this.gameObject.transform.GetChild(0);
        lerpCube = this.gameObject.transform.GetChild(1);
    }

    private void OnDestroy(){
        list.Remove(Id);
    }

    private void Move(Vector3 newPosition){
        normalCube.position = newPosition;
        lerpCube.position = Vector3.Lerp(lerpCube.position,newPosition, lerpAmount * Time.deltaTime);
        //transform.position = newPosition;
    }

    public static void Spawn(ushort id, string username, Vector3 position){
        Debug.Log("here");
        Player player;
        if(id==NetworkManager.Singleton.Client.Id){
            player = Instantiate(GameLogic.Singleton.LocalPlayerPrefab, position, Quaternion.identity).GetComponent<Player>();
            player.IsLocal = true;
        }else{
            player = Instantiate(GameLogic.Singleton.PlayerPrefab, position, Quaternion.identity).GetComponent<Player>();
            player.IsLocal = false;
        }

        Debug.Log("here");

        player.name = $"Player {id} ({(string.IsNullOrEmpty(username) ? "Guest" : username)})";
        player.Id = id;
        player.username = username;

        list.Add(id, player);
    }

    [MessageHandler((ushort)ServerToClientId.playerSpawned)]
    private static void SpawnPlayer(Message message){
        Debug.Log("here");
        Spawn(message.GetUShort(), message.GetString(), message.GetVector3());
    }

    [MessageHandler((ushort)ServerToClientId.playerMovement)]
    private static void PlayerMovement(Message message){
        if(list.TryGetValue(message.GetUShort(), out Player player)){
            player.Move(message.GetVector3());
        }
    }
}
