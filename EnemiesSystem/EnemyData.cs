using System;

[Serializable]
public class EnemyData 
{
    public float seekDistance = 40;
    public float sightAngle = 80;
    public float hearDistance;
    public float walkSpeed = 1f;
    public float runSpeed = 10;
    public float wanderRadius = 20;
    public float attackDistance;
    public float walkAnimationMult = 20;
    public float runAnimationMult = 20;
}
