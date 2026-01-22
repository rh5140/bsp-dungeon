using UnityEngine;

public class BasicEnemy : MonoBehaviour
{
    void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            if (other.gameObject.GetComponent<CharacterMovement>().isAttacking)
            {
                Destroy(transform.parent.gameObject);
            }
        }
    }
}
