using UnityEngine;

public class CharAnimation : MonoBehaviour
{
    private Character character;

    private void Start()
    {
        character = GetComponent<Character>();
    }

    private void Update()
    {
        ChooseAnimation(character);
    }

    private void ChooseAnimation(Character c)
    {
        c.Anim.SetBool("IsIdle", false);
        c.Anim.SetBool("IsWalk", false);

        switch (c.State)
        {
            case CharState.Idle:
                c.Anim.SetBool("IsIdle", true);
                break;
            case CharState.Walk:
            case CharState.WalkToEnemy:
            case CharState.WalkToMagicCast:
            case CharState.WalkToNPC:
                c.Anim.SetBool("IsWalk", true);
                break;
        }
    }
}
