using Godot;


public partial class CameraController : Node3D {
	private float camroot_h = 0;
	private float camroot_v = 0;
	[Export] private int cam_v_max = 75;
	[Export] private int cam_v_min = -55;
	private float h_sensitivity = 0.01f;
	private float v_sensitivity = 0.01f;
	private float h_acceleration = 10.0f;
	private float v_acceleration = 10.0f;

	public override void _Ready() {
		Input.MouseMode = Input.MouseModeEnum.Captured;
	}
	public override void _Input(InputEvent @event) {
		if (@event is InputEventMouseMotion mouseMotion) {
			camroot_h += -mouseMotion.Relative.X * h_sensitivity;
			camroot_v += mouseMotion.Relative.Y * v_sensitivity;
		}

		if (@event.IsActionPressed("ui_cancel")) {
			if (Input.MouseMode == Input.MouseModeEnum.Captured) {
				Input.MouseMode = Input.MouseModeEnum.Visible;

			} else {
				Input.MouseMode = Input.MouseModeEnum.Captured;
			}

		}
	}

	public override void _PhysicsProcess(double delta) {
		camroot_v = Mathf.Clamp(camroot_v, Mathf.DegToRad(cam_v_min), Mathf.DegToRad(cam_v_max));

		var hNode = GetNode<Node3D>("h");
		var vNode = GetNode<Node3D>("h/v");
		Vector3 hRotation = hNode.Rotation;
		Vector3 vRotation = vNode.Rotation;

		hRotation.Y = Mathf.Lerp(hNode.Rotation.Y, camroot_h, (float)delta * h_acceleration);
		hNode.Rotation = hRotation;

		vRotation.X = Mathf.Lerp(vNode.Rotation.X, camroot_v, (float)delta * v_acceleration);
		vNode.Rotation = vRotation;
	}
}
