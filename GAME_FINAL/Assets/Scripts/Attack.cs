using UnityEngine;

public class Attack : MonoBehaviour
{
    [SerializeField]
    private Transform shootPoint;
    [SerializeField]
    private float distance;
    [SerializeField]
    private float damage;
    [SerializeField]
    private float maxCoolDown;
    private float coolDown;
    [SerializeField]
    private KeyCode attackButton;
    
    public NetworkClient networkClient;
    
    [SerializeField]
    private Hero hero;


    private void Update()
    {
        if (coolDown > 0)
            coolDown -= Time.deltaTime;
        else
            coolDown = 0;

        if (coolDown > 0)
            return;

        if (Input.GetKeyDown(attackButton))
        {
            hero.Animator.SetTrigger("IsAttack");
            int direction = hero.IsRight ? 1 : -1;
            float heroDeltaX = shootPoint.position.x - hero.transform.position.x;

            Vector2 correctPosition = shootPoint.position;
            if (!hero.IsRight)
            {
                correctPosition.x -= heroDeltaX * 2;
            }
            var hit = Physics2D.Raycast(correctPosition, shootPoint.right * direction, distance);
            if (hit.collider != null)
            {
                float attackX = correctPosition.x;
                float attackY = correctPosition.y;
                networkClient.SendMessageToServer($"Attack:{attackX};{attackY};{damage};{hero.IsRight};{distance}");
                hit.collider.GetComponent<Health>()?.TakeDamage(damage);
                  
            }
            coolDown = maxCoolDown;
        }

    }
}
