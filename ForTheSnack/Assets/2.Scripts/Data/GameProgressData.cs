using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class GameProgressData
{
    public int version = 1;
    public string timestampIso; //저장 시각

    public float playerPosX, playerPosY;
    public float playerVelX, playerVelY;

    public double elapsed;

    public List<SpecificPointSave> specificPoints = new();

    public List<StickSave> sticks = new();


}

[Serializable]
public class SpecificPointSave
{
    public string id;
    public bool triggered;
}

[Serializable]
public class StickSave
{
    public string id;
    public bool isOn;
}