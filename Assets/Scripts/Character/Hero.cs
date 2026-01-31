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
        }
    }
}
