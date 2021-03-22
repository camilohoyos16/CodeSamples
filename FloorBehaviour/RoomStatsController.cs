using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomStatsController : RoomController
{
    public StatsManager statsManager;

    protected void Start() {
        base.Start();
        EventManager.Instance.AddListener<OnUpgradePlayerStatsEvent>(this.OnUpgradePlayerStatsListener);
    }

    public override void DrawRoom() {
        this.statsManager.AppearEntries();
    }

    private void OnDestroy() {
        if (EventManager.HasInstance()) {
            EventManager.Instance.RemoveListener<OnUpgradePlayerStatsEvent>(this.OnUpgradePlayerStatsListener);
        }
    }

    protected override void ActiveRoom() {
        base.ActiveRoom();
        EventManager.Instance.Trigger(new OnPlayerFloorCompletedEvent());
    }

    private void OnUpgradePlayerStatsListener(OnUpgradePlayerStatsEvent e) {
        this.RoomIsDone();
    }

    protected override void CheckCondition() {
        
    }
}
