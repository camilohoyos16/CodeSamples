using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PowerType { 
    BasicThrowBall
}

public class Power {
    public PowerType powerType;
    public int attackPower;    
}

public class PowersDataBase
{
    public static List<Power> powers = new List<Power> {
        new Power{
            powerType = PowerType.BasicThrowBall,
            attackPower = 10,
        }
    };

    public static Power GetPowerDataByType(PowerType powerType)
    {
        Power power = null;
        for (int i = 0; i < powers.Count; i++) {
            if (powers[i].powerType == powerType) {
                power = powers[i];
            }
        }
        return power;
    }
}
