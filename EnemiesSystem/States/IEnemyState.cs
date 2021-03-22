using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEnemyState
{
    EnemyBehavior Behaviour { get; set; }
    StateAction CurrentStateAction { get; set; }

    void Enter();
    void Updating();
    void Exit();
    void AnimationEnd();
}

public enum EnemyState
{
    Wander,
    Attack,
    Run,
    Chase
}

public enum StateAction
{
    Enter,
    Updating,
    Exit
}
