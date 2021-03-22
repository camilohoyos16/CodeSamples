using UnityEngine;
using DG.Tweening;
using System;

public enum DoorType
{
    Enter,
    Exit
}

public class DoorRoomController : MonoBehaviour
{
    public DoorType doorType;
    public Collider2D doorCollider;
    public SpriteRenderer doorSprite;
    public Color lockColor;
    public Color unlockColor;

    [Space (10)]

    public GameObject insideRoomPoint;
    public GameObject outsideRoomPoint;

    private Vector2 initialPosition;
    private Vector2 finalPosition;

    private void Awake() {
        this.LockDoor();
    }

    public void LockDoor() {
        this.doorCollider.enabled = true;
        this.doorSprite.color = this.lockColor;
        this.doorCollider.isTrigger = false;
    }

    public void UnlockDoor() {
        this.doorCollider.enabled = true;
        this.doorSprite.color = this.unlockColor;
        this.doorCollider.isTrigger = true;
    }

    public void EnterPlayer(GameObject player, float time, Action action) {
        this.initialPosition = this.outsideRoomPoint.transform.position;
        this.finalPosition = this.insideRoomPoint.transform.position;
        this.doorCollider.enabled = false;

        player.transform.DOMove(this.finalPosition, time).From(this.initialPosition).OnComplete(delegate() {
            this.LockDoor();
            action?.Invoke();
        });
    }

    public void ExitPlayer(GameObject player, float time, Action action) {
        this.initialPosition = this.insideRoomPoint.transform.position;
        this.finalPosition = this.outsideRoomPoint.transform.position;
        this.doorCollider.enabled = false;

        player.transform.DOMove(this.finalPosition, time).From(this.initialPosition).OnComplete(() => action?.Invoke());
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if(collision.CompareTag("Player")){
            if(this.doorType == DoorType.Exit) {
                EventManager.Instance.Trigger(new OnPlayerChangeRoomEvent());
            }
        }
    }
}
