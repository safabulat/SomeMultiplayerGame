using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInfoSO : ScriptableObject
{
    public string playerName;
    public enum AttackType
    {
        None,
        Ranged,
        Melee
    }
    public AttackType type = AttackType.None;

    public float Damage;

    public float MovementSpeed;
    public float SprintSpeed;

    public float AttackRange;
    public float AttackSpeed;

    public float MaxHP;

    public GameObject weapon;

}
