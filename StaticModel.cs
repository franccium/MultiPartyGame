using Godot;
using System;

public partial class StaticModel : StaticBody3D
{
    private MeshInstance3D _mesh;
    public string MeshPath { get; set; } = "res://Models/Models/Hats/Dumb.glb";
    //todo collision from the blender model, so at this point its just a packedscene with everything and this is redundant lmfao

    public override void _Ready()
    {
        //_mesh = new MeshInstance3D();
        //_mesh.Mesh
        _mesh = ResourceLoader.Load<MeshInstance3D>(MeshPath);
    }

    public override void _Process(double delta)
    {
    }
}
