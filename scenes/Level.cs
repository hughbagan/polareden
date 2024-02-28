using Godot;
using System;

public partial class Level : Node2D
{
	private PackedScene PlayerScene;
	private WFCGenerator Generator;
	private Control UI;
	private NavigationRegion2D TargetNavRegion;
	private TileMap TargetTileMap;
	private Camera2D Camera;
	private CanvasModulate CanvasBlack;
	private Player PlayerInstance;
	private AudioStreamPlayer RainforestAudio;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GD.Randomize();
		PlayerScene = GD.Load<PackedScene>("res://scenes/Player.tscn");
		Generator = GetNode<WFCGenerator>("WFC Generator");
		TargetNavRegion = GetNode<NavigationRegion2D>("WFC Generator/NavigationRegion2D2");
		TargetTileMap = TargetNavRegion.GetNode<TileMap>("Target");
		Camera = GetNode<Camera2D>("Camera2D");
		CanvasBlack = GetNode<CanvasModulate>("CanvasModulate");
		UI = GetNode<Control>("UI");
		RainforestAudio = GetNode<AudioStreamPlayer>("RainforestAudio");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public async void OnWFCGeneratorDone()
	{
		GD.Print("Done");
		GD.Print(TargetTileMap.GetUsedCells(0));
		GD.Print(TargetTileMap.GetUsedRect());
		GetNode<NavigationRegion2D>("WFC Generator/NavigationRegion2D").Enabled = false;
		UI.Hide();
		CanvasBlack.Show();

		RainforestAudio.Play(GD.Randi() % 300);

		// Array[Vector2I] cells = TargetTileMap.GetUsedCells(0);

		TargetNavRegion.BakeNavigationPolygon(false); // bake on same thread

		PlayerInstance = (Player) PlayerScene.Instantiate();
		AddChild(PlayerInstance);

		Tween CamPosTween = GetTree().CreateTween();
		Tween CamZoomTween = GetTree().CreateTween();
		CamPosTween.TweenProperty(Camera, "position", PlayerInstance.GlobalPosition, 1.0).SetEase(Tween.EaseType.InOut);
		CamZoomTween.TweenProperty(Camera, "zoom", PlayerInstance.Camera.Zoom, 1.0).SetEase(Tween.EaseType.InOut);
		Tween CanvasModulateTween = GetTree().CreateTween();
		CanvasModulateTween.TweenProperty(CanvasBlack, "color:a", 1.0, 1.0);
		await ToSignal(CamPosTween, "finished");

		PlayerInstance.Camera.MakeCurrent();
	}
}
