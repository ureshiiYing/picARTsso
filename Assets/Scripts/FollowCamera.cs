using UnityEngine;

public class FollowCamera : MonoBehaviour
{

    // Update is called once per frame
    void Update()
    {
        if (Input.mousePosition != null)
        {
            Vector3 temp = Input.mousePosition;
            temp.z = 0f;
            this.transform.position = Camera.main.ScreenToWorldPoint(temp);
        }
    }
}
