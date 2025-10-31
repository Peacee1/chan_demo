using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sfs2X;
using Sfs2X.Core;
using Sfs2X.Util;
using Sfs2X.Requests;
using Sfs2X.Entities;

public class Connectorsfs : MonoBehaviour
{
    public string defaultHost = "127.0.0.1";
    public int defaultTcpPort = 9933;
    private SmartFox sfs;
    public InputField hostInput;
    public InputField portInput;
    public Toggle isToggleDebug;

    void Update()
    {
        if (sfs != null)
            sfs.ProcessEvents();
    }

    public void Login_Click()
    {
        if (sfs == null || !sfs.IsConnected)
        {
            sfs = new SmartFox();
            sfs.ThreadSafeMode = true;

            sfs.AddEventListener(SFSEvent.CONNECTION, OnConnection);
            sfs.AddEventListener(SFSEvent.CONNECTION_LOST, OnConnectionLost);
            sfs.AddEventListener(SFSEvent.LOGIN, OnLogin);
            sfs.AddEventListener(SFSEvent.ROOM_JOIN, OnRoomJoin);
            sfs.AddEventListener(SFSEvent.PUBLIC_MESSAGE, OnPublicMessage);
            sfs.AddEventListener(SFSEvent.USER_ENTER_ROOM, OnUserEnterRoom);
            sfs.AddEventListener(SFSEvent.USER_EXIT_ROOM, OnUserExitRoom);
            // sfs.AddEventListener(SFSEvent.ROOM_ADD, OnRoomAdd);
            // sfs.AddEventListener(SFSEvent.ROOM_REMOVE, OnRoomRemove);
            

            ConfigData cfg = new ConfigData
            {
                Host = defaultHost,
                Port = defaultTcpPort,
                Zone = "Khu_1",
                Debug = true
            };

            sfs.Connect(cfg);
        }
        else
        {
            sfs.Disconnect();
        }
    }

    void OnConnection(BaseEvent evt)
    {
        if ((bool)evt.Params["success"])
        {
            Debug.Log("Connected to SmartFoxServer 2X!");
            sfs.Send(new LoginRequest("Player_" + Random.Range(1000, 9999), "", "Khu_1"));
        }
        else
        {
            Debug.LogError("Connection failed!");
        }
    }

    private void OnLogin(BaseEvent evt)
    {
        User user = (User)evt.Params["user"];
        Debug.Log("Login successfully! " + user.Name);

        List<Room> rooms = sfs.RoomList;
        populateRoomList(rooms);

        if (rooms.Count == 0)
        {
            Debug.Log("No rooms found, creating Room_1...");
            sfs.Send(new CreateRoomRequest(new RoomSettings("Room_1")));
        }
        else
        {
            Debug.Log("Joining room: " + rooms[0].Name);
            sfs.Send(new JoinRoomRequest(rooms[0].Name));
        }
    }

    private void OnRoomAdd(BaseEvent evt)
    {
        Room room = (Room)evt.Params["room"];
        Debug.Log("New room added: " + room.Name);
        populateRoomList(sfs.RoomList);
    }

    private void OnRoomRemove(BaseEvent evt)
    {
        Room room = (Room)evt.Params["room"];
        Debug.Log("Room removed: " + room.Name);
        populateRoomList(sfs.RoomList);
    }

    private void populateRoomList(List<Room> rooms)
    {
        Debug.Log("populateRoomList called. Rooms count: " + (rooms != null ? rooms.Count.ToString() : "null"));
        if (rooms != null)
        {
            foreach (Room r in rooms)
                Debug.Log(" - Room: " + r.Name);
        }
    }

    private void OnRoomJoin(BaseEvent evt)
    {
        Room room = (Room)evt.Params["room"];
        Debug.Log("Join room successfully! " + room.Name);
    }

    private void OnPublicMessage(BaseEvent evt)
    {
        User sender = (User)evt.Params["sender"];
        string message = (string)evt.Params["message"];
        Debug.Log(sender.Name + ": " + message);
    }

    private void OnUserEnterRoom(BaseEvent evt)
    {
        User user = (User)evt.Params["user"];
        Room room = (Room)evt.Params["room"];
        Debug.Log("User " + user.Name + " entered room " + room.Name);
    }

    private void OnUserExitRoom(BaseEvent evt)
    {
        User user = (User)evt.Params["user"];
        if (user != sfs.MySelf)
        {
            Room room = (Room)evt.Params["room"];
            Debug.Log("User " + user.Name + " left room " + room.Name);
        }
    }

    void OnConnectionLost(BaseEvent evt)
    {
        Debug.LogWarning("Connection lost: " + (string)evt.Params["reason"]);
        reset();
    }

    private void reset()
    {
        if (sfs != null)
        {
            sfs.RemoveAllEventListeners();
            if (sfs.IsConnected)
                sfs.Disconnect();
            sfs = null;
        }
    }

    void OnApplicationQuit()
    {
        if (sfs != null && sfs.IsConnected)
        {
            Debug.Log("Disconnecting (OnApplicationQuit)...");
            sfs.Disconnect();
        }
    }

    void OnDestroy()
    {
        if (sfs != null && sfs.IsConnected)
        {
            Debug.Log("Disconnecting (OnDestroy)...");
            sfs.Disconnect();
        }
    }
}
