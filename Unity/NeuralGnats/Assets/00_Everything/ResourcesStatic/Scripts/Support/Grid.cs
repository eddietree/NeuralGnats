﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct GridPos
{
    public int x;
    public int z;

    public GridPos(int gridPosX, int gridPosZ)
    {
        x = gridPosX;
        z = gridPosZ;
    }

    public static GridPos operator +(GridPos a, GridPos b)
    {
        return new GridPos(a.x+b.x, a.z+b.z);
    }

    public static GridPos operator -(GridPos a, GridPos b)
    {
        return new GridPos(a.x - b.x, a.z - b.z);
    }

    public static bool operator ==(GridPos lhs, GridPos rhs)
    {
        return lhs.x == rhs.x && lhs.z == rhs.z;
    }

    public static bool operator !=(GridPos lhs, GridPos rhs)
    {
        return !(lhs == rhs);
    }
}
