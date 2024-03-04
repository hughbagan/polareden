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
	private Player PlayerInstance = null;
	private AudioStreamPlayer RainforestAudio;
	private Rect2 LevelRect; // target tilemap rect in global position
	private Button StartButton;

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
		StartButton = GetNode<Button>("UI/StartButton");

		Camera.Position = new Vector2(
			Generator.H*Generator.target.TileSet.TileSize.X*0.5f,
			Generator.V*Generator.target.TileSet.TileSize.Y*0.5f);
		UI.Position = Camera.Position;
	}

	public override void _Process(double delta)
	{
	}

	public void OnWFCGeneratorDone()
	{
		// Interpret what the generator made and prepare for gameplay

		if (PlayerInstance != null)
			return; // generator may finish on multiple threads if regenerated

		Rect2I targetRect = TargetTileMap.GetUsedRect();
		Vector2 targetPos = ToGlobal(TargetTileMap.MapToLocal(targetRect.Position));
		Vector2 targetSize = ToGlobal(TargetTileMap.MapToLocal(targetRect.Size - new Vector2I(1,1)));
		LevelRect = new Rect2(targetPos.X, targetPos.Y, targetSize.X, targetSize.Y);
		TargetNavRegion.BakeNavigationPolygon(false); // bake on same thread

		PlayerInstance = (Player) PlayerScene.Instantiate();
		PlayerInstance.LevelRect = LevelRect;
		// Try to place the player
		Rid map = GetWorld2D().NavigationMap;
		Vector2 pos = LevelRect.Position + LevelRect.Size/2;
		float tileSize = TargetTileMap.TileSet.TileSize.X;
		while (true)
		{
			Vector2 closest = NavigationServer2D.MapGetClosestPoint(map, pos);
			if ((closest - pos).IsZeroApprox())
				break;
			else // try a new pos
			{
				if (GD.Randf() > 0.5f)
				{
					if (GD.Randf() > 0.5f)
						pos = new Vector2(pos.X+tileSize, pos.Y);
					else
						pos = new Vector2(pos.X-tileSize, pos.Y);
				}
				if (GD.Randf() > 0.5f)
				{
					if (GD.Randf() > 0.5f)
						pos = new Vector2(pos.X, pos.Y+tileSize);
					else
						pos = new Vector2(pos.X, pos.Y-tileSize);
				}
				GD.Print("invalid player start pos, try "+pos.ToString());
			}
		}
		PlayerInstance.GlobalPosition = pos;

		StartButton.Show();
	}

	public async void OnStartButtonPressed()
	{
		UI.Hide();
		CanvasBlack.Show();
		AddChild(PlayerInstance);
		RainforestAudio.Play(GD.Randi() % 300);
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
