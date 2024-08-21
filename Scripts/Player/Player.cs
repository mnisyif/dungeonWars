using Godot;
using System;
using System.Numerics;
using Vector3 = Godot.Vector3;

public partial class Player : CharacterBody3D {
	[Export] private float gravity = 9.8f;
	[Export] private int jumpForce = 9;
	[Export] private int walkSpeed = 3;
	[Export] private int runSpeed = 10;

	// animation node names
	private const string idleNodeName = "Idle";
	private const string walkNodeName = "Walk";
	private const string runNodeName = "Run";
	private const string jumpNodeName = "Jump";
	private const string attack1NodeName = "Attack01";
	private const string deathNodeName = "Death";

	// state machine conditions
	private bool isAttacking;
	private bool isWalking;
	private bool isRunning;
	private bool isDead;

	// physics values
	private Vector3 direction;
	private Vector3 horizontalVelocity;
	private float aimTurn;
	private Vector3 movement;
	private Vector3 verticalVelocity;
	private int movementSpeed;
	private int angularAcceleration;
	private int acceleration;
	private bool justHit;

	private Node3D camrotH;
	private Node3D playerMesh;

	public override void _Ready() {
		camrotH = GetNode<Node3D>("camroot/h");
		playerMesh = GetNode<Node3D>("Knight");
	}

	public override void _Input(InputEvent @event) {
		if (@event is InputEventMouseMotion mouseMotion) {
			aimTurn = (float)(-mouseMotion.Relative.X * 0.015);
		}

		if (@event.IsActionPressed("aim")) {
			direction = camrotH.GlobalTransform.Basis.Z;
		}
	}
	public override void _PhysicsProcess(double delta) {
		bool onFloor = IsOnFloor();

		if (!isDead) {
			if (!onFloor) {
				verticalVelocity += Vector3.Down * gravity * 2 * (float)delta;

			} else {
				verticalVelocity = Vector3.Up * gravity / 10;
			}

			if (Input.IsActionJustPressed("jump") && !isAttacking && onFloor) {
				verticalVelocity = Vector3.Up * jumpForce;
			}

			angularAcceleration = 10;
			movementSpeed = 0;
			acceleration = 15;

			float hRot = camrotH.GlobalTransform.Basis.GetEuler().Y;

			if (Input.IsActionPressed("forward") || Input.IsActionPressed("backward") || Input.IsActionPressed("left") || Input.IsActionPressed("right")) {
				direction = new Vector3(
					Input.GetActionStrength("left") - Input.GetActionStrength("right"),
					0,
					Input.GetActionStrength("forward") - Input.GetActionStrength("backward")
				);
				direction = direction.Rotated(Vector3.Up, hRot).Normalized();
				if (Input.IsActionPressed("spring") && isWalking) {
					movementSpeed = runSpeed;
					isRunning = true;

				} else {
					isWalking = true;
					movementSpeed = walkSpeed;
				}

			} else {
				isWalking = false;
				isRunning = false;
			}

			// update player rotatoin using vector3
			if (Input.IsActionPressed("aim")) {
				Vector3 targetRot = new Vector3(
					playerMesh.Rotation.X,
					camrotH.Rotation.Y,
					playerMesh.Rotation.Z
				);
				playerMesh.Rotation = playerMesh.Rotation.Lerp(targetRot, (float)delta * angularAcceleration);

			} else {
				float targetYRot = Mathf.Atan2(-direction.X, -direction.Z);
				Vector3 targetRot = new Godot.Vector3(
					playerMesh.Rotation.X,
					targetYRot,
					playerMesh.Rotation.Z
				);
				playerMesh.Rotation = playerMesh.Rotation.Lerp(targetRot, (float)delta * angularAcceleration);
			}

			if (isAttacking) {
				horizontalVelocity = horizontalVelocity.Lerp(direction * 0.1f, (float)delta * acceleration);

			} else {
				horizontalVelocity = horizontalVelocity.Lerp(direction * movementSpeed, (float)delta * acceleration);
			}

			Velocity = new Vector3(
				horizontalVelocity.X + verticalVelocity.X,
				verticalVelocity.Y,
				horizontalVelocity.Z + verticalVelocity.Z
			);

			MoveAndSlide();
		}
	}
}