using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace KarlsonInBaldi
{
    public class InputApi : MonoBehaviour
    {
        static public Vector2 MouseXY = Vector2.zero;
        static public PlayerMovement OldPlayerMovement = null;
        static public InputApi Inst;
        public void Update()
        {
            //This Code Sucks
            //but i need input
            //so i did this
            MouseXY = Vector2.zero; //this fixes any spin bugs
            OldPlayerMovement = GameObject.FindObjectOfType<PlayerMovement>();
            if (OldPlayerMovement != null)
            {
                Vector2 AnalogOut = Vector2.zero;
                Singleton<InputManager>.Instance.GetAnalogInput(OldPlayerMovement.cameraAnalogData, out AnalogOut, out MouseXY);
            }
        }
        static public CreatePlayer createPlayerC;
        public IEnumerator SpawnPlayerInTenSecs()
        {
            yield return new WaitForSeconds(0f);
            //GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);

            //plane.transform.localScale = new Vector3(10000f, 1f, 10000f); // 100x bigger than normal

            var CreatePlayer = new GameObject("CreatePlayer");
            createPlayerC = CreatePlayer.AddComponent<CreatePlayer>();
            CreatePlayer.transform.position = Camera.main.transform.position;
            //Creating A F L O O R
        }
        public LineRenderer line;
        public void Start() 
        { 
            
            Inst = this;
            line = gameObject.AddComponent<LineRenderer>();
            line.positionCount = 2;
            line.SetPosition(0, Vector3.zero);
            line.SetPosition(1, new Vector3(0, 1000, 1000));
            line.startWidth = 0.1f;
            line.endWidth = 0.1f;
            line.material = new Material(Shader.Find("Sprites/Default"));
            line.startColor = Color.red;
            line.endColor = Color.red;

        }


    }
}
