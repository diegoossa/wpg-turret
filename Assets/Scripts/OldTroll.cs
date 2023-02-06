using UnityEngine;

public class OldTroll : MonoBehaviour
{
    // Troll state
    public enum State
    {
        Moving,
        Dying,
        Winning,
    };

    private Animator anim;
    private CharacterController controller;
    private State state = State.Moving;

    // Speed property.
    public float Speed { get; set; }

    // Troll Id property.
    public int TrollId { get; set; }

    // Set troll state.
    public void SetState(State newState)
    {
        if (state != newState)
        {
            state = newState;

            switch (state)
            {
                case State.Moving:
                    anim.SetInteger("moving", 1);
                    break;

                case State.Dying:
                    Speed = 0;
                    var animIndex = Random.Range(0, 2) == 0 ? 9 : 12;
                    anim.SetInteger("moving", animIndex);
                break;

                case State.Winning:
                    anim.SetInteger("moving", 6);
                    break;

                default:
                    break;
            };
        }
    }

    // Start is called before the first frame update
    private void Start()
    {
        anim = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        state = State.Moving;
    }

    // Update is called once per frame.
    private void Update()
    {
        // Move troll.
        var moveDirection = transform.forward * Speed;
        controller.Move(moveDirection * Time.deltaTime);
    }
}
