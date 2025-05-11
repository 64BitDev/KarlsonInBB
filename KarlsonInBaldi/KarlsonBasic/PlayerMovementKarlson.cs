using System;

using UnityEngine;
using Rewired.UI;
using Rewired.Utils;
using Rewired.Integration.UnityUI;
using KarlsonInBaldi;

[DefaultExecutionOrder(1000)]
public class PlayerMovementKarlson : MonoBehaviour
{
	public float Scale = 1f; //this is scale to game world good if you have to change the player size
	private float sensitivity = 50f;

	private float sensMultiplier = 1f;

	public Transform playerCam;

	public Transform orientation;

	private float xRotation;

	public Rigidbody rb;

	private float moveSpeed = 4500f;

	private float walkSpeed = 20f;

	private float runSpeed = 10f;

	public bool grounded;
	
	private bool readyToJump;

	private float jumpCooldown = 0.25f;

	private float jumpForce = 550f;

	private float Movementx;

	private float Movementy;

	private bool jumping;

	private bool sprinting;

	private bool crouching;

	private Vector3 normalVector;

	private Vector3 wallNormalVector;

	private bool wallRunning;

	private Vector3 wallRunPos;

	private ParticleSystem.EmissionModule psEmission;

	private Collider playerCollider;

	private Rigidbody objectGrabbing;

	private Vector3 previousLookdir;

	private Vector3 grabPoint;

	private float dragForce = 700000f;

	private SpringJoint grabJoint;

	private LineRenderer grabLr;

	private Vector3 myGrabPoint;

	private Vector3 myHandPoint;

	private Vector3 endPoint;

	private Vector3 grappleVel;

	private float offsetMultiplier;

	private float offsetVel;

	private float distance;

	private float slideSlowdown = 0.2f;

	private float actualWallRotation;

	private float wallRotationVel;

	private float desiredX;

	private bool cancelling;

	private bool readyToWallrun = true;

	private float wallRunGravity = 1f;

	private float maxSlopeAngle = 35f;

	private float wallRunRotation;

	private bool airborne;

	private int nw;

	private bool onWall;

	private bool onGround;

	private bool surfing;

	private bool cancellingGrounded;

	private bool cancellingWall;

	private bool cancellingSurf;

//	public LayerMask whatIsHittable;

	private float desiredTimeScale = 1f;

	private float timeScaleVel;

	private float actionMeter;

	private float vel;

	public static PlayerMovementKarlson Instance { get; private set; }

	private void Awake()
	{
	}

	private void Start()
	{
        Instance = this;
        rb = GetComponent<Rigidbody>();
        Time.timeScale = 1;

        playerCollider = GetComponent<Collider>();
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		readyToJump = true;
		wallNormalVector = Vector3.up;
	}
	private void LateUpdate()
	{
		DrawGrabbing();
		WallRunning();
        InputApi.OldPlayerMovement.transform.position = playerCam.transform.position;
    }

	private void FixedUpdate()
	{
        grounded = false;

        if (transform.position.y < transform.localScale.y)
		{
			rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            transform.position = new Vector3(transform.position.x, transform.localScale.y, transform.position.z);
			grounded = true;
        }
        Movement();
    }

	private void Update()
	{
		UpdateActionMeter();
		MyInput();
		Look();
		DrawGrabbing();
        InputApi.OldPlayerMovement.transform.rotation = playerCam.rotation;
        
    }

	private void MyInput()
	{
		Movementx = 0; //these are movement idk why they are not movementx and movementy
		Movementy = 0;


		if(Input.GetKey(KeyCode.W))
		{
			Movementy += 1;
		}
        if (Input.GetKey(KeyCode.S))
        {
            Movementy -= 1;
        }

        if (Input.GetKey(KeyCode.D))
        {
            Movementx += 1;
        }
        if (Input.GetKey(KeyCode.A))
        {
            Movementx -= 1;
        }
        jumping = Input.GetKey(KeyCode.Space);
		crouching = Input.GetKey(KeyCode.LeftControl);
		if (Input.GetKeyDown(KeyCode.LeftControl))
		{
			StartCrouch();
		}
		if (Input.GetKeyUp(KeyCode.LeftControl))
		{
			StopCrouch();
		}
	}

	private void GrabObject()
	{
		if (objectGrabbing == null)
		{
			StartGrab();
		}
		else
		{
			HoldGrab();
		}
	}

	private void DrawGrabbing()
	{
		if ((bool)objectGrabbing)
		{
			myGrabPoint = Vector3.Lerp(myGrabPoint, objectGrabbing.position, Time.deltaTime * 45f);
			myHandPoint = Vector3.Lerp(myHandPoint, grabJoint.connectedAnchor, Time.deltaTime * 45f);
			grabLr.SetPosition(0, myGrabPoint);
			grabLr.SetPosition(1, myHandPoint);
		}
	}

	private void StartGrab()
	{
		RaycastHit[] array = Physics.RaycastAll(playerCam.transform.position, playerCam.transform.forward, 8f);
		if (array.Length < 1)
		{
			return;
		}
		for (int i = 0; i < array.Length; i++)
		{
			MonoBehaviour.print("testing on: " + array[i].collider.gameObject.layer);
			if ((bool)array[i].transform.GetComponent<Rigidbody>())
			{
				objectGrabbing = array[i].transform.GetComponent<Rigidbody>();
				grabPoint = array[i].point;
				grabJoint = objectGrabbing.gameObject.AddComponent<SpringJoint>();



				grabJoint.autoConfigureConnectedAnchor = false;

                grabJoint.connectedAnchor = playerCam.transform.position + playerCam.transform.forward * 5.5f;

                grabJoint.minDistance = 0f;
				grabJoint.maxDistance = 0f;
				grabJoint.damper = 4f;
				grabJoint.spring = 40f;
				grabJoint.massScale = 5f;
				objectGrabbing.angularDrag = 5f;
				objectGrabbing.drag = 1f;
				previousLookdir = playerCam.transform.forward;
				grabLr = objectGrabbing.gameObject.AddComponent<LineRenderer>();
				grabLr.positionCount = 2;
				grabLr.startWidth = 0.05f;
				grabLr.material = new Material(Shader.Find("Sprites/Default"));
				grabLr.numCapVertices = 10;
				grabLr.numCornerVertices = 10;
				break;
			}
		}
	}

	private void HoldGrab()
	{
		grabJoint.connectedAnchor = playerCam.transform.position + playerCam.transform.forward * 5.5f;
		grabLr.startWidth = 0f;
		grabLr.endWidth = 0.0075f * objectGrabbing.velocity.magnitude;
		previousLookdir = playerCam.transform.forward;
	}

	private void StopGrab()
	{
		UnityEngine.Object.Destroy(grabJoint);
		UnityEngine.Object.Destroy(grabLr);
		objectGrabbing.angularDrag = 0.05f;
		objectGrabbing.drag = 0f;
		objectGrabbing = null;
	}

	private void StartCrouch()
	{
		float num = 400f;
		base.transform.localScale = new Vector3(1f, 0.5f, 1f) * Scale;
		base.transform.position = new Vector3(base.transform.position.x, base.transform.position.y - (0.5f * Scale), base.transform.position.z);
		if (( rb.velocity.magnitude / Scale) > 0.1f && grounded)
		{
			rb.AddForce((orientation.transform.forward * num) * Scale);
		}
	}

	private void StopCrouch()
	{
		base.transform.localScale = new Vector3(1f, 1.5f, 1f) * Scale;
		base.transform.position = new Vector3(base.transform.position.x, base.transform.position.y + (0.5f * Scale), base.transform.position.z);
	}

	private void FootSteps()
	{
		if (!crouching && (grounded || wallRunning))
		{
			float num = 1.2f;
			float num2 = rb.velocity.magnitude / Scale;
			if (num2 > 20f)
			{
				num2 = 20f;
			}
			distance += num2;
			if (distance > 300f / num)
			{
				distance = 0f;
			}
		}
	}

	private void Movement()
	{
        //FakeGrav
        // Apply custom gravity manually
        Vector3 customGravity = new Vector3(0, -30f, 0); // your (0, 100, 0) gravity

        rb.AddForce((customGravity * rb.mass) * Scale, ForceMode.Force);


        rb.AddForce((Vector3.down * Time.deltaTime * 10f) * Scale);
		Vector2 VelRelToLook = FindVelRelativeToLook();
		float VelRelToLookX = VelRelToLook.x;
		float VelRelToLookY = VelRelToLook.y;
		FootSteps();
		CounterMovement(Movementx, Movementy, VelRelToLook);
		if (readyToJump && jumping)
		{
			Jump();
		}
		float WantSpeed = walkSpeed;
		if (sprinting)
		{
			WantSpeed = runSpeed;
		}
		if (crouching && grounded && readyToJump)
		{
			rb.AddForce((Vector3.down * Time.deltaTime * 3000f) * Scale);
			return;
		}
		if (Movementx > 0f && VelRelToLookX > WantSpeed)
		{
			Movementx = 0f;
		}
		if (Movementx < 0f && VelRelToLookX < 0f - WantSpeed)
		{
			Movementx = 0f;
		}
		if (Movementy > 0f && VelRelToLookY > WantSpeed)
		{
			Movementy = 0f;
		}
		if (Movementy < 0f && VelRelToLookY < 0f - WantSpeed)
		{
			Movementy = 0f;
		}
		float num4 = 1f;
		float num5 = 1f;
		if (!grounded)
		{
			num4 = 0.5f;
			num5 = 0.5f;
		}
		if (grounded && crouching)
		{
			num5 = 0f;
		}
		if (wallRunning)
		{
			num5 = 0.3f;
			num4 = 0.3f;
		}
		if (surfing)
		{
			num4 = 0.7f;
			num5 = 0.3f;
		}
		rb.AddForce((orientation.transform.forward * Movementy * moveSpeed * Time.deltaTime * num4 * num5) * Scale);
		rb.AddForce((orientation.transform.right * Movementx * moveSpeed * Time.deltaTime * num4) * Scale);
		SpeedLines();
	}

	private void SpeedLines()
	{
		float num = Vector3.Angle(rb.velocity / Scale, playerCam.transform.forward) * 0.15f;
		if (num < 1f)
		{
			num = 1f;
		}
		float rateOverTimeMultiplier = (rb.velocity.magnitude / Scale) / num;
		if (grounded && !wallRunning)
		{
			rateOverTimeMultiplier = 0f;
		}
	}

	private void CameraShake()
	{
		float num = (rb.velocity.magnitude / Scale) / 9f;
		Invoke("CameraShake", 0.2f);
	}

	private void ResetJump()
	{
		readyToJump = true;
	}

	private void Jump()
	{
		if ((grounded || wallRunning || surfing) && readyToJump)
		{
			MonoBehaviour.print("jumping");
			Vector3 velocity = rb.velocity / Scale;
			readyToJump = false;
			rb.AddForce((Vector2.up * jumpForce * 1.5f) * Scale);
			rb.AddForce(normalVector * jumpForce * 0.5f);
			if (rb.velocity.y < 0.5f)
			{
				rb.velocity = new Vector3(velocity.x, 0f, velocity.z) * Scale;
			}
			else if (rb.velocity.y > 0f)
			{
				rb.velocity = new Vector3(velocity.x, velocity.y / 2f, velocity.z) * Scale;
			}
			if (wallRunning)
			{
				rb.AddForce((wallNormalVector * jumpForce * 3f) * Scale);
			}
			Invoke("ResetJump", jumpCooldown);
			if (wallRunning)
			{
				wallRunning = false;
			}
		}
	}

    private void Look()
    {
        float num = InputApi.MouseXY.x * sensitivity * Time.fixedDeltaTime * sensMultiplier;
        float num2 = InputApi.MouseXY.y * sensitivity * Time.fixedDeltaTime * sensMultiplier;
        desiredX = playerCam.transform.localRotation.eulerAngles.y + num;
        xRotation -= num2;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        FindWallRunRotation();
        actualWallRotation = Mathf.SmoothDamp(actualWallRotation, wallRunRotation, ref wallRotationVel, 0.2f);
        playerCam.transform.localRotation = Quaternion.Euler(xRotation, desiredX, actualWallRotation);
        orientation.transform.localRotation = Quaternion.Euler(0f, desiredX, 0f);
    }


    private void CounterMovement(float x, float y, Vector2 mag)
    {
        if (!grounded || jumping)
        {
            return;
        }
        float num = 0.16f * Scale;
        float num2 = 0.01f * Scale;

        if (crouching)
        {
            rb.AddForce((moveSpeed * Time.deltaTime * -rb.velocity.normalized * slideSlowdown) * Scale);
            return;
        }
        if ((Math.Abs(mag.x) > num2 && Math.Abs(x) < 0.05f) || (mag.x < -num2 && x > 0f) || (mag.x > num2 && x < 0f))
        {
            rb.AddForce((moveSpeed * orientation.transform.right * Time.deltaTime * (0f - mag.x) * num) * Scale);
        }
        if ((Math.Abs(mag.y) > num2 && Math.Abs(y) < 0.05f) || (mag.y < -num2 && y > 0f) || (mag.y > num2 && y < 0f))
        {
            rb.AddForce((moveSpeed * orientation.transform.forward * Time.deltaTime * (0f - mag.y) * num) * Scale);
        }

        if (Mathf.Sqrt(Mathf.Pow(rb.velocity.x, 2f) + Mathf.Pow(rb.velocity.z, 2f)) > walkSpeed * Scale)
        {
            float num3 = rb.velocity.y / Scale;
            Vector3 vector = rb.velocity.normalized * walkSpeed * Scale;
            rb.velocity = new Vector3(vector.x, num3, vector.z);
        }
    }

    public Vector2 FindVelRelativeToLook()
	{
		float current = orientation.transform.eulerAngles.y;
		float target = Mathf.Atan2(rb.velocity.x / Scale, rb.velocity.z / Scale) * 57.29578f;
		float num = Mathf.DeltaAngle(current, target);
		float num2 = 90f - num;
		float magnitude = rb.velocity.magnitude / Scale;
		return new Vector2(y: magnitude * Mathf.Cos(num * ((float)Math.PI / 180f)), x: magnitude * Mathf.Cos(num2 * ((float)Math.PI / 180f)));
	}

	private void FindWallRunRotation()
	{
		if (!wallRunning)
		{
			wallRunRotation = 0f;
			return;
		}
		_ = new Vector3(0f, playerCam.transform.rotation.y, 0f).normalized;
		new Vector3(0f, 0f, 1f);
		float num = 0f;
		float current = playerCam.transform.rotation.eulerAngles.y;
		if (Math.Abs(wallNormalVector.x - 1f) < 0.1f)
		{
			num = 90f;
		}
		else if (Math.Abs(wallNormalVector.x - -1f) < 0.1f)
		{
			num = 270f;
		}
		else if (Math.Abs(wallNormalVector.z - 1f) < 0.1f)
		{
			num = 0f;
		}
		else if (Math.Abs(wallNormalVector.z - -1f) < 0.1f)
		{
			num = 180f;
		}
		num = Vector3.SignedAngle(new Vector3(0f, 0f, 1f), wallNormalVector, Vector3.up);
		float num2 = Mathf.DeltaAngle(current, num);
		wallRunRotation = (0f - num2 / 90f) * 15f;
		if (!readyToWallrun)
		{
			return;
		}
		if ((Mathf.Abs(wallRunRotation) < 4f && Movementy > 0f && Math.Abs(Movementx) < 0.1f) || (Mathf.Abs(wallRunRotation) > 22f && Movementy < 0f && Math.Abs(Movementx) < 0.1f))
		{
			if (!cancelling)
			{
				cancelling = true;
				CancelInvoke("CancelWallrun");
				Invoke("CancelWallrun", 0.2f);
			}
		}
		else
		{
			cancelling = false;
			CancelInvoke("CancelWallrun");
		}
	}

	private void CancelWallrun()
	{
		MonoBehaviour.print("cancelled");
		Invoke("GetReadyToWallrun", 0.1f);
		rb.AddForce((wallNormalVector * 600f) * Scale);
		readyToWallrun = false;
	}

	private void GetReadyToWallrun()
	{
		readyToWallrun = true;
	}

	private void WallRunning()
	{
		if (wallRunning)
		{
			rb.AddForce((-wallNormalVector * Time.deltaTime * moveSpeed) * Scale);
			rb.AddForce((Vector3.up * Time.deltaTime * rb.mass * 100f * wallRunGravity) * Scale);
		}
	}

	private bool IsFloor(Vector3 v)
	{
		return Vector3.Angle(Vector3.up, v) < maxSlopeAngle;
	}

	private bool IsSurf(Vector3 v)
	{
		float num = Vector3.Angle(Vector3.up, v);
		if (num < 89f)
		{
			return num > maxSlopeAngle;
		}
		return false;
	}

	private bool IsWall(Vector3 v)
	{
		return Math.Abs(90f - Vector3.Angle(Vector3.up, v)) < 0.1f;
	}

	private bool IsRoof(Vector3 v)
	{
		return v.y == -1f;
	}

	private void StartWallRun(Vector3 normal)
	{
		if (!grounded && readyToWallrun)
		{
			wallNormalVector = normal;
			float num = 20f;
			if (!wallRunning)
			{
				rb.velocity = new Vector3(rb.velocity.x / Scale, 0f, rb.velocity.z / Scale) * Scale;
				rb.AddForce((Vector3.up * num) * Scale, ForceMode.Impulse);
			}
			wallRunning = true;
		}
	}



	private void OnCollisionExit(Collision other)
	{
	}

	private void OnCollisionStay(Collision other)
	{
		for (int i = 0; i < other.contactCount; i++)
		{
			Vector3 normal = other.contacts[i].normal;
			if (IsFloor(normal))
			{
				if (wallRunning)
				{
					wallRunning = false;
				}
				grounded = true;
				normalVector = normal;
				cancellingGrounded = false;
				CancelInvoke("StopGrounded");
			}
			if (IsWall(normal))
			{
				if (!onWall)
				{
				}
				StartWallRun(normal);
				onWall = true;
				cancellingWall = false;
				CancelInvoke("StopWall");
			}
			if (IsSurf(normal))
			{
				surfing = true;
				cancellingSurf = false;
				CancelInvoke("StopSurf");
			}
			IsRoof(normal);
		}
		float num = 3f;
		if (!cancellingGrounded)
		{
			cancellingGrounded = true;
			Invoke("StopGrounded", Time.deltaTime * num);
		}
		if (!cancellingWall)
		{
			cancellingWall = true;
			Invoke("StopWall", Time.deltaTime * num);
		}
		if (!cancellingSurf)
		{
			cancellingSurf = true;
			Invoke("StopSurf", Time.deltaTime * num);
		}
	}

	private void StopGrounded()
	{
		grounded = false;
	}

	private void StopWall()
	{
		onWall = false;
		wallRunning = false;
	}

	private void StopSurf()
	{
		surfing = false;
	}
	
	public Vector3 HitPoint()
	{
		RaycastHit[] array = Physics.RaycastAll(playerCam.transform.position, playerCam.transform.forward);
		if (array.Length < 1)
		{
			return playerCam.transform.position + playerCam.transform.forward * 100f;
		}
		if (array.Length > 1)
		{
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].transform.gameObject.layer == LayerMask.NameToLayer("Enemy"))
				{
					return array[i].point;
				}
			}
		}
		return array[0].point;
	}

	private void UpdateActionMeter()
	{
		float target = 0.09f;
		if (rb.velocity.magnitude / Scale > 15f)
		{
			target = 1f;
		}
		actionMeter = Mathf.SmoothDamp(actionMeter, target, ref vel, 0.7f);
	}
}
