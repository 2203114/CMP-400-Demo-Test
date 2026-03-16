using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    public Camera cam;

    private float xRotation = 0f;

    public float xSensitivity = 30f;
    public float ySensitivity = 30f;

    private Player_Script player;


    private void Start()
    {
        player = GetComponent<Player_Script>();

    }


    public void ProcessLook(Vector2 input)
    {
        float mouseX = input.x;
        float mouseeY = input.y;

        xRotation -= (mouseeY * Time.deltaTime) * ySensitivity;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);

        cam.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);

        transform.Rotate(Vector3.up * (mouseX * Time.deltaTime) * xSensitivity);
    }

    public void StealCheck()
    {
        Debug.DrawRay(cam.transform.position, cam.transform.forward,Color.red);

        RaycastHit hit;

        if(Physics.Raycast(cam.transform.position, cam.transform.forward, out hit,2))
        {
            if(hit.transform.GetComponent<NPC_ShopKeep>()!=null)
            {
                player.Stealing(hit.transform.gameObject);
            }
        }


    }
}
