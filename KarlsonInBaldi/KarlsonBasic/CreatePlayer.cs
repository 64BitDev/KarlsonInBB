using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;

public class CreatePlayer : MonoBehaviour
{
    // Start is called before the first frame update
    GameObject PlayerAndCamera;
    GameObject PlayerAndCamera_Camera;
    GameObject PlayerAndCamera_Camera_Main_Camera;

    GameObject PlayerAndCamera_Player;
    GameObject PlayerAndCamera_Player_Orientation;
    GameObject PlayerAndCamera_Player_Head;
    PlayerMovementKarlson playerMovement;
    MoveCamera moveCamera;
    float PlayerScale = 2;
    public void Start()
    {
        PlayerAndCamera = new GameObject("PlayerAndCamera");
        PlayerAndCamera_Camera = new GameObject("Camera");
        PlayerAndCamera_Camera_Main_Camera = new GameObject("Main Camera");
        PlayerAndCamera_Player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        PlayerAndCamera_Player.name = "Player";
        PlayerAndCamera_Player_Orientation = new GameObject("Orientation");
        PlayerAndCamera_Player_Head = new GameObject("Head");
        PlayerAndCamera_Player.layer = LayerMask.NameToLayer("Player");
        PlayerAndCamera_Camera.transform.SetParent(PlayerAndCamera.transform);
        PlayerAndCamera_Camera_Main_Camera.transform.SetParent(PlayerAndCamera_Camera.transform);


        PlayerAndCamera_Player.transform.SetParent(PlayerAndCamera.transform);
        PlayerAndCamera_Player_Orientation.transform.SetParent(PlayerAndCamera_Player.transform);
        PlayerAndCamera_Player_Head.transform.SetParent(PlayerAndCamera_Player.transform);

        PlayerAndCamera_Camera.transform.position = new Vector3(0, 1.11f,0);
        PlayerAndCamera_Player.transform.position = new Vector3(0, 0.17f, 0);
        PlayerAndCamera_Player_Head.transform.position = new Vector3(0, 0.835f,0);
        playerMovement = PlayerAndCamera_Player.AddComponent<PlayerMovementKarlson>();
        playerMovement.playerCam = PlayerAndCamera_Camera.transform;
        playerMovement.orientation = PlayerAndCamera_Player_Orientation.transform;
        playerMovement.rb = PlayerAndCamera_Player.AddComponent<Rigidbody>();
        playerMovement.rb.constraints = RigidbodyConstraints.FreezeRotation;
        moveCamera = PlayerAndCamera_Camera.AddComponent<MoveCamera>();
        PlayerAndCamera_Camera_Main_Camera.AddComponent<Camera>();
        moveCamera.player = PlayerAndCamera_Player_Head.transform; //idk why its called player

        PhysicMaterial physicMaterial = PlayerAndCamera_Player.GetComponent<Collider>().material;
        physicMaterial.staticFriction = 0;
        physicMaterial.dynamicFriction = 0;

        PlayerAndCamera_Player.transform.localScale = new Vector3(1.1f, 1.7f, 1.1f) * PlayerScale;
        playerMovement.Scale = PlayerScale;
        PlayerAndCamera_Player.transform.position = gameObject.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
