using System;
using System.Collections.Generic;
using System.IO;
using Godot;

public partial class FatEnemy_base : Enemy_base
{
    public override async void WalkAlongNodes(List<PathNode> nodes){
		path_updated = false;
		for (int i = 0; i < nodes.Count; i++){
			if (path_updated){break;}
			Godot.Vector2 Velocity = Godot.Vector2.Zero;
			Godot.Vector2 cell_positon = new Godot.Vector2(nodes[i].x * cellsize, nodes[i].y * cellsize) + new Godot.Vector2(8,8);
            Godot.Vector2 cell_positon2 = new Godot.Vector2(nodes[i].x * cellsize, nodes[i].y * cellsize) + new Godot.Vector2(8,8);
            GD.Print(cell_positon);
            float distance = GlobalPosition.DistanceTo(cell_positon);
            float distance2 = GlobalPosition.DistanceTo(cell_positon2);
            
            if (distance > distance2){continue;}
			if (Mathf.Abs(GlobalPosition.X - cell_positon2.X) < cellsize && Mathf.Abs(GlobalPosition.Y - cell_positon2.Y) < cellsize){continue;}
			//
			if (nodes[i].is_obstruction){
				TileMapLayer script = tilemap as TileMapLayer;
				Godot.Vector2I TileMapPosition = nodes[i].tilemap_position;
                GlobalVelocity = Godot.Vector2.Zero;
                if (AreaDamage)
                {
                    while (true)
                    {
                        while (true)
                        {
                            bool Empty = true;
                            for (int x = -1; x < 2; x++){
                                for (int y = -1; y < 2; y++){
                                    if (tilemap.GetCellSourceId(new Godot.Vector2I(TileMapPosition.X + x, TileMapPosition.Y + y)) != -1){
                                        Empty = false;
                                    }
                                    script.Damage_tileI(TileMapPosition, 1);
                                }
                            }
                            if (Empty){break;}
                            await ToSignal(GetTree().CreateTimer(1/wall_damage), SceneTreeTimer.SignalName.Timeout);
                        }
					}
                }
                else
                {
                    for (int x = -1; x < 2; x++){
                        for (int y = -1; y < 2; y++){
                            Godot.Vector2I Pos = new Godot.Vector2I(TileMapPosition.X + x, TileMapPosition.Y + y);
                            if (tilemap.GetCellSourceId(Pos) == -1){
                                continue;
                            }
                            while (true){
                                if (script.Area_damageI(Pos, 1)){break;}
                                await ToSignal(GetTree().CreateTimer(1/wall_damage), SceneTreeTimer.SignalName.Timeout);
                            }
                        }
                    }
                    await ToSignal(GetTree().CreateTimer(1/wall_damage), SceneTreeTimer.SignalName.Timeout);
                }
                nodes[i].is_obstruction = false;
            }
            while ((Mathf.Abs(GlobalPosition.X - cell_positon.X) > 1 || Mathf.Abs(GlobalPosition.Y - cell_positon.Y) > 1) && !path_updated)
            {
				QueueRedraw();
                if (distance > distance2) { break; }
                //if (Mathf.Abs(GlobalPosition.X - cell_positon2.X) < cellsize && Mathf.Abs(GlobalPosition.Y - cell_positon2.Y) < cellsize){break;}
                Velocity = GlobalPosition.DirectionTo(cell_positon) * Speed * (float)GetPhysicsProcessDeltaTime();
				GlobalVelocity = Velocity;

				distance = GlobalPosition.DistanceTo(cell_positon);
				distance2 = GlobalPosition.DistanceTo(cell_positon2);

				await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
			}
		}
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
	}
}