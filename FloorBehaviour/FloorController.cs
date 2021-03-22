using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorController : MonoBehaviour
{
    [Header("General")]
    public int roomsQuantity;

    [HideInInspector] public int floorNumber;
    private int roomNumber = 0;
    private bool isFloorCompleted;

    [HideInInspector] public RoomController currentRoom;
    private RoomController nextRoom;

    // Start is called before the first frame update
    void Start()
    {
    }

    public void ActiveFloor() {
        EventManager.Instance.AddListener<OnPlayerChangeRoomEvent>(this.OnPlayerChangeRoomListener);
        EventManager.Instance.AddListener<OnPlayerFloorCompletedEvent>(this.OnPlayerFloorCompletedListener);
    }

    public void RecycleFloor() {
        EventManager.Instance.RemoveListener<OnPlayerChangeRoomEvent>(this.OnPlayerChangeRoomListener);
        EventManager.Instance.RemoveListener<OnPlayerFloorCompletedEvent>(this.OnPlayerFloorCompletedListener);
        this.isFloorCompleted = false;
    }

    private void OnDestroy() {
        if (EventManager.HasInstance()) {
            EventManager.Instance.RemoveListener<OnPlayerChangeRoomEvent>(this.OnPlayerChangeRoomListener);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SpawnRoom() {
        if(this.currentRoom != null) {
            this.currentRoom.RecycleRoom();
        }

        if(this.floorNumber == 1 && this.roomNumber == 0) {
            this.currentRoom = ResourcesManager.Instance.GetRoom(Env.INITIAL_ROOM_POOL_PATH);
            this.currentRoom.DrawRoom();
            this.currentRoom.roomNumber = this.roomNumber;
        } else {
            this.currentRoom = this.nextRoom;
        }

        if(this.roomNumber == this.roomsQuantity) {
            this.nextRoom = ResourcesManager.Instance.GetRoom(Env.STATS_ROOM_POOL_PATH);
            this.nextRoom.DrawRoom();
            this.nextRoom.ChangeRoomPosition(this.currentRoom.transform.position + Vector3.up * 10);
            this.nextRoom.roomNumber = this.roomNumber + 1;
        } else {
            if (this.roomNumber < this.roomsQuantity) {
                this.nextRoom = ResourcesManager.Instance.GetRoom(Env.ROOM_POOL_PATH.GetRandomEntry());
                this.nextRoom.DrawRoom();
                this.nextRoom.ChangeRoomPosition(this.currentRoom.transform.position + Vector3.up * 10);
                this.nextRoom.roomNumber = this.roomNumber + 1;
            }
        }
        this.nextRoom.ChangeRoomPosition(this.currentRoom.transform.position + Vector3.up * 10);
        this.nextRoom.roomNumber = this.roomNumber + 1;

        this.roomNumber ++;
    }

    private void OnPlayerFloorCompletedListener(OnPlayerFloorCompletedEvent e) {
        this.isFloorCompleted = true;
    }

    private void OnPlayerChangeRoomListener(OnPlayerChangeRoomEvent e) {
        this.ChangeRoom();
    }

    public void ChangeRoom() {
        if (this.isFloorCompleted) {

        } else {
            this.currentRoom.PlayerExitRoom(() => 
                this.nextRoom.PlayerEnterRoom(delegate () {
                    this.SpawnRoom();
                })
            );
        }
    }
}
