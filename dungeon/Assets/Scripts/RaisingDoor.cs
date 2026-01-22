using UnityEngine;

public class RaisingDoor : MonoBehaviour
{
    Vector3 startPosition;
    Vector3 endPosition;

    void Start()
    {
        startPosition = transform.position;
        endPosition = new Vector3(startPosition.x, startPosition.y + 3, startPosition.z);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            gameObject.transform.parent.transform.position = endPosition;
        }
    }
}
