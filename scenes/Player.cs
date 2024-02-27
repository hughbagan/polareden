using Godot;
using System;
using System.Numerics;

public partial class Player : CharacterBody2D
{
	[Export] public int Speed = 100;
	public Camera2D Camera;
	private AnimatedSprite2D Anim;
	private int StepCount = 0;
	private AudioStreamPlayer Step1;
	private AudioStreamPlayer Step2;
	private bool StepFlag = false;

	public override void _Ready()
	{
		Camera = GetNode<Camera2D>("Camera2D");
		Anim = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		Step1 = GetNode<AudioStreamPlayer>("Step1");
		Step2 = GetNode<AudioStreamPlayer>("Step2");
	}

	public override void _PhysicsProcess(double delta)
	{
		Velocity = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down") * Speed;
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
