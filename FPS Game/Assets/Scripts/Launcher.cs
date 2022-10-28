using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using Photon.Realtime;
using System.Linq;

public class Launcher : MonoBehaviourPunCallbacks
{

    public static Launcher instance;
    [SerializeField] TMP_InputField roomNameInputField;
    [SerializeField] Transform roomListContent;
    [SerializeField] TMP_Text roomNameText;
    [SerializeField] GameObject roomNameListItem;
    [SerializeField] GameObject playerNameListItem;
    [SerializeField] Transform playerListContent;

    [SerializeField] GameObject LaunchGameBtn;





    void Awake()
    {
        instance = this;
    }
    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master");

        PhotonNetwork.JoinLobby();
        PhotonNetwork.AutomaticallySyncScene = true;
    }
    public override void OnJoinedLobby()
    {
        Debug.Log("Joined Lobby");
        MenuManager.instance.OpenMenu("title");
        PhotonNetwork.NickName = "Player"+ Random.Range(0,1000).ToString("0000");
        
    }

    public void CreateRoom()
    {
        if(string.IsNullOrEmpty(roomNameInputField.text))
        {
            return;
        }
        Debug.Log("test");
        PhotonNetwork.CreateRoom(roomNameInputField.text);
        MenuManager.instance.OpenMenu("loading");
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        MenuManager.instance.OpenMenu("loading");
        Debug.Log("Left Room");
    }

    public void JoinRoom(RoomInfo info)
    {
        Debug.Log("test"); 
        PhotonNetwork.JoinRoom(info.Name);
        MenuManager.instance.OpenMenu("loading");

    }
    public override void OnLeftRoom()
    {

        MenuManager.instance.OpenMenu("title");
    }

    public override void OnJoinedRoom()
    {
        MenuManager.instance.OpenMenu("room");
        roomNameText.text = PhotonNetwork.CurrentRoom.Name;
        
        Player[] players = PhotonNetwork.PlayerList;

        foreach(Transform child in playerListContent)
        {
            Destroy(child.gameObject);
        }

        for(int i = 0; i < players.Count(); i++)
        {
            Instantiate(playerNameListItem, playerListContent).GetComponent<PlayerListItem>().SetUp(players[i]);
        }

        LaunchGameBtn.SetActive(PhotonNetwork.IsMasterClient);

    }
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        LaunchGameBtn.SetActive(PhotonNetwork.IsMasterClient);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach(Transform trans in roomListContent)
        {
            Destroy(trans);
        }
        for(int i = 0; i < roomList.Count; i++)
        {
            if(roomList[i].RemovedFromList)
            {
                continue;
            }
            Instantiate(roomNameListItem, roomListContent).GetComponent<RoomListItem>().SetUp(roomList[i]);
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Instantiate(playerNameListItem, playerListContent).GetComponent<PlayerListItem>().SetUp(newPlayer);

    }

    public void StartGame()
    {
        PhotonNetwork.LoadLevel(1);
    }
}
