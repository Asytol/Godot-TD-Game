using Godot;
using System;

public class CrackTextureSignifier
{
	public Vector2 Positon;
	public Texture2D Texture;

	public CrackTextureSignifier(Vector2 Position, Texture2D Texture)
	{
		this.Positon = Position;
		this.Texture = Texture;
	}
}
