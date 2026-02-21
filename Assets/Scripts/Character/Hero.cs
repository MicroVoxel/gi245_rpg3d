using UnityEngine;

public class Hero : Character
{
    private void Update()
    {
        switch (state)
        {
            case CharState.Walk:
                WalkUpdate(); 
                break;
            case CharState.WalkToEnemy:
                WalkToEnemyUpdate();
                break;
            case CharState.Attack:
                AttackUpdate();
                break;
            case CharState.WalkToMagicCast:
                WalkToMagicCastUpadate();
                break;
        }
    }
}
