using Godot;
using System;
using System.Collections.Generic;

public static class LevelDictionary
{
    public static readonly WavePreset[] Main_test_scene =
    {
        //Wave1
        new WavePreset(new List<SpawnPreset>{
            new SpawnPreset(4,2,4,"res://Scenes/Enemies/Lizard_enemy.tscn"),
            new SpawnPreset(5,2,5,"res://Scenes/Enemies/Lizard_enemy.tscn")},
            WaveMoney: 400
            ),
        //Wave2
        new WavePreset(new List<SpawnPreset>{
            new SpawnPreset(6,2,4,"res://Scenes/Enemies/Lizard_enemy.tscn"),
            new SpawnPreset(9,2,4,"res://Scenes/Enemies/Lizard_enemy.tscn")},
            WaveMoney:300
            ),
    };

    public static readonly WavePreset[][] Levels =
    [
        /*Level 1*/ [
            //Wave1
            new WavePreset(new List<SpawnPreset>{
                new SpawnPreset(4,2,4,"res://Scenes/Enemies/Lizard_enemy.tscn"),
                new SpawnPreset(5,2,5,"res://Scenes/Enemies/Lizard_enemy.tscn")
            }),
            //Wave2
        ],
        /*Level 2*/ [
            //Wave1
            new WavePreset(new List<SpawnPreset>{
                new SpawnPreset(4,2,4,"res://Scenes/Enemies/Lizard_enemy.tscn"),
                new SpawnPreset(5,2,5,"res://Scenes/Enemies/Lizard_enemy.tscn")
            }),
            //Wave2
        ]
    ];
}
