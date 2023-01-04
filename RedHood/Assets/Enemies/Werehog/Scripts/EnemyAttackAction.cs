using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "A.I. Actions/Attack Action")]

public class EnemyAttackAction : EnemyAction
{
    public int _attackScore = 3;
    public float _recoveryTime = 2;

    public float maxAttackAngle = 35;
    public float minAttackAngle = -35;

    public float minAttackDistance = 0;
    public float maxAttackDistance = 3;
}
