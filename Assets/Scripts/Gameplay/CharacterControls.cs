using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterOwner))]
public class CharacterControls : MonoBehaviour {
	[Header("Prefabs and Cached Objects")]
	[SerializeField]private GameObject walkableGround;
	private CharacterOwner characterOwner;
	private Camera sharedCamera;
	
	[Header("Settings")]
	[SerializeField]private float speedInUnitsPerSec;
	
	[Header("Current Values")]
	private Coroutine movementCoroutine;
	
	// Initializing cached GameObjects and Components.
    void Start() {
        sharedCamera = Camera.main;
		characterOwner = GetComponent<CharacterOwner>();
    }
	
    void FixedUpdate() {
		// When a new command is being issued, analyze what the cursor is looking at.
		if(characterOwner.GetCursor().IsCursorPressed()) {
			// If we're looking at walkableGround, walk there.
			if(characterOwner.GetCursor().CursorOverObject(walkableGround))
				MoveToSpot(characterOwner.GetCursor().GetCursorPosition());
		}
		
		// If we use the right stick, start rotating.
		Vector2 rightStickInput = GamepadInput.GetRightStick(characterOwner.GetOwner().GetGamepad());
		if(Mathf.Abs(rightStickInput.x) > 0.2f || Mathf.Abs(rightStickInput.y) > 0.2f) {
			StopMovement();
			
			// Get the rotationg angle from input.
			float rotationAngle = Mathf.Atan2(rightStickInput.x, rightStickInput.y) * Mathf.Rad2Deg;
			transform.eulerAngles = new Vector3(transform.eulerAngles.x, rotationAngle, transform.eulerAngles.z);
		}
		
		if(GamepadInput.GetLeftShoulderDown(characterOwner.GetOwner().GetGamepad())) {
			if(characterOwner.GetOwner().GetCursor().GetCursorHidden())
				characterOwner.GetOwner().GetCursor().ShowCursor();
			else
				characterOwner.GetOwner().GetCursor().HideCursor();
		}
		
    }
	
	// Gets world position from cursor position and walks there.
	void MoveToSpot(Vector2 cursorPosition) {
		StopMovement();
		
		// Get new point from cursor position.
		Vector3 worldCursorPosition = cursorPosition;
		worldCursorPosition.z = Vector3.Distance(sharedCamera.gameObject.transform.position, walkableGround.transform.position);
		Vector3 targetPos = sharedCamera.ScreenToWorldPoint(worldCursorPosition);
		
		// Face a direction we're going.
		transform.LookAt(targetPos);
		transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
		
		movementCoroutine = StartCoroutine(SlowlyMoveToPoint(targetPos));
	}
	
	void StopMovement() {
		if(movementCoroutine != null)
			StopCoroutine(movementCoroutine);
	}
	
	IEnumerator SlowlyMoveToPoint(Vector3 targetPoint) {
		Vector3 startPos = transform.position;
		
		Vector3 movementVector = (targetPoint - startPos);
		movementVector.y = 0;
		movementVector = movementVector.normalized;
		while(Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), new Vector3(targetPoint.x, 0, targetPoint.z)) > (speedInUnitsPerSec / 100) * 2) {
			transform.position += movementVector * (speedInUnitsPerSec / 100);
			yield return new WaitForSeconds(0.01f);
		}
		
		transform.position = new Vector3(targetPoint.x, transform.position.y, targetPoint.z);
	}
}
