using Godot;

public partial class Player : CharacterBody3D {
	[Export] public float Gravity = 9.8f;
	[Export] public int JumpForce = 9;
	[Export] public int WalkSpeed = 3;
	[Export] public int RunSpeed = 10;

	// Animation node names
	private const string IdleNodeName = "Idle";
	private const string WalkNodeName = "Walk";
	private const string RunNodeName = "Run";
	private const string JumpNodeName = "Jump";
	private const string Attack1NodeName = "Attack01";
	private const string DeathNodeName = "Death";

	// State machine conditions
	private bool isAttacking;
	private bool isWalking;
	private bool isRunning;
	private bool isDying;

	// Physics values
	private Vector3 direction;
	private Vector3 horizontalVelocity;
	private Vector3 verticalVelocity;
	private float aimTurn;
	private int movementSpeed;
	private int angularAcceleration;
	private int acceleration;
	private bool justHit;

	// OnReady load
	private Node3D camrotH;
	private Node3D playerMesh;
	private AnimationTree animationTree;
	private AnimationNodeStateMachinePlayback playBack;

	public override void _Ready() {
		camrotH = GetNode<Node3D>("camroot/h");
		playerMesh = GetNode<Node3D>("Knight");
		animationTree = GetNode<AnimationTree>("AnimationTree");
		playBack = (AnimationNodeStateMachinePlayback)animationTree.Get("parameters/playback");
	}

	public override void _Input(InputEvent @event) {
		if (@event is InputEventMouseMotion mouseMotion) {
			aimTurn = -mouseMotion.Relative.X * 0.015f;
		}

		if (@event.IsActionPressed("aim")) {
			direction = camrotH.GlobalTransform.Basis.Z;
		}
	}

	public override void _PhysicsProcess(double delta) {
		if (isDying) return;

		bool isOnFloor = IsOnFloor();

		Attack01();
		HandleGravity(delta, isOnFloor);
		HandleAttack();
		HandleMovement(delta);
		UpdateVelocity();
		MoveAndSlide();
		SetAnimationParameters(isOnFloor);
	}

	private void HandleGravity(double delta, bool isOnFloor) {
		if (!isOnFloor) {
			verticalVelocity += Vector3.Down * Gravity * 2 * (float)delta;

		} else {
			verticalVelocity = Vector3.Down * Gravity / 10; // Small downward velocity when on floor
		}

		if (Input.IsActionJustPressed("jump") && !isAttacking && isOnFloor) {
			verticalVelocity = Vector3.Up * JumpForce;
			// GD.Print("Jump initiated!"); // debugging
		}
	}

	private void HandleAttack() {
		string currentNodeStatus = playBack.GetCurrentNode();
		if (currentNodeStatus.Contains(Attack1NodeName)) {
			isAttacking = true;
		} else {
			isAttacking = false;
		}
	}

	private void HandleMovement(double delta) {
		float hRot = camrotH.GlobalTransform.Basis.GetEuler().Y;

		movementSpeed = 0;
		acceleration = 15;

		if (Input.IsActionPressed("forward") || Input.IsActionPressed("backward") ||
			Input.IsActionPressed("left") || Input.IsActionPressed("right")) {

			direction = new Vector3(
				Input.GetActionStrength("left") - Input.GetActionStrength("right"),
				0,
				Input.GetActionStrength("forward") - Input.GetActionStrength("backward")
			);
			direction = direction.Rotated(Vector3.Up, hRot).Normalized();

			if (Input.IsActionPressed("sprint") && isWalking) {
				movementSpeed = RunSpeed;
				isRunning = true;
			} else {
				isWalking = true;
				movementSpeed = WalkSpeed;
			}
		} else {
			isWalking = false;
			isRunning = false;
			direction = Vector3.Zero;
		}

		if (isAttacking) {
			horizontalVelocity = horizontalVelocity.Lerp(direction.Normalized() * 0.1f, (float)delta * acceleration);

		} else {
			horizontalVelocity = horizontalVelocity.Lerp(direction.Normalized() * movementSpeed, (float)delta * acceleration);
		}

		UpdatePlayerRotation(delta);
	}

	private void UpdatePlayerRotation(double delta) {
		angularAcceleration = 10;

		// if (direction != Vector3.Zero) {
		float targetRotY;

		if (Input.IsActionPressed("aim")) {
			targetRotY = camrotH.Rotation.Y;
		} else {
			targetRotY = Mathf.Atan2(direction.X, direction.Z);
		}

		Vector3 targetRot = new(
			playerMesh.Rotation.X,
			targetRotY,
			playerMesh.Rotation.Z
		);

		playerMesh.Rotation = playerMesh.Rotation.Lerp(targetRot, (float)delta * angularAcceleration);
		// }
	}

	private void UpdateVelocity() {
		Velocity = new(
			horizontalVelocity.X + verticalVelocity.X,
			verticalVelocity.Y,
			horizontalVelocity.Z + verticalVelocity.Z
		);
	}

	private void SetAnimationParameters(bool isOnFloor) {
		animationTree.Set("parameters/conditions/IsOnFloor", isOnFloor);
		animationTree.Set("parameters/conditions/IsInAir", !isOnFloor);
		animationTree.Set("parameters/conditions/IsWalking", isWalking);
		animationTree.Set("parameters/conditions/IsNotWalking", !isWalking);
		animationTree.Set("parameters/conditions/IsRunning", isRunning);
		animationTree.Set("parameters/conditions/IsNotRunning", !isRunning);
		animationTree.Set("parameters/conditions/IsDying", isDying);
	}

	private void Attack01() {
		string currentNodeStatus = playBack.GetCurrentNode();

		if (currentNodeStatus.Contains(IdleNodeName) || currentNodeStatus.Contains(WalkNodeName) || currentNodeStatus.Contains(RunNodeName)) {
			if (Input.IsActionJustPressed("attack")) {
				if (!isAttacking) {
					playBack.Travel(Attack1NodeName);
				}
			}
		}
	}
}