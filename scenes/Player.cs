using Godot;
using System;
using System.Numerics;

public partial class Player : CharacterBody2D
{
	[Export] public int Speed = 100;
	public Camera2D Camera;
	public Rect2 LevelRect;
	private AnimatedSprite2D Anim;
	private int StepCount = 0;
	private AudioStreamPlayer Step1;
	private AudioStreamPlayer Step2;
	private bool StepFlag = false;
	private Godot.Vector2 Size;

	public override void _Ready()
	{
		Camera = GetNode<Camera2D>("Camera2D");
		Anim = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		Step1 = GetNode<AudioStreamPlayer>("Step1");
		Step2 = GetNode<AudioStreamPlayer>("Step2");
		Size = Anim.SpriteFrames.GetFrameTexture("default", 0).GetSize();
		LevelRect = new Rect2(
			LevelRect.Position.X,
			LevelRect.Position.Y,
			LevelRect.Size.X-Size.X/2,
			LevelRect.Size.Y-Size.Y/2);
	}

	public override void _PhysicsProcess(double delta)
	{
		Velocity = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down") * Speed;
		Godot.Vector2 projected = GlobalPosition+Velocity.Normalized();
		if (projected.X > LevelRect.Position.X && projected.X < LevelRect.End.X && projected.Y > LevelRect.Position.Y && projected.Y < LevelRect.End.Y)
			MoveAndSlide();

		if (Velocity == Godot.Vector2.Zero)
		{
			Anim.Stop();
			Anim.Frame = 0;
		}
		else if (!Anim.IsPlaying())
		{
			Anim.Play();
		}
		if (Velocity.X < 0.0f)
			Anim.FlipH = true;
		else if (Velocity.X > 0.0f)
			Anim.FlipH = false;
		if (Anim.Frame == 3)
			StepFlag = true;
		if (Anim.Frame == 4 && StepFlag)
		{
			if (StepCount % 2 == 0)
				Step1.Play();
			else
				Step2.Play();
			++StepCount;
			StepFlag = false;
		}
	}
}
