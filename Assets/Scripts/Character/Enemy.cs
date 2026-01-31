using UnityEngine;

public class Enemy : Character
{
    private void Update()
    {
        switch (state)
        {
            case CharState.Walk:
                WalkUpdate();
                break;
        }
    }

}
