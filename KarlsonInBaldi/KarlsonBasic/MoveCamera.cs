using UnityEngine;

public class MoveCamera : MonoBehaviour
{
	public Transform player;

	private Vector3 offset;

	private Camera cam;

	public static MoveCamera Instance { get; private set; }

	private void Start()
	{
		Instance = this;
		cam = base.transform.GetChild(0).GetComponent<Camera>();
		cam.fieldOfView = 90;
		offset = base.transform.position - player.transform.position;
	}

	private void Update()
	{
		base.transform.position = player.transform.position;
	}
}
