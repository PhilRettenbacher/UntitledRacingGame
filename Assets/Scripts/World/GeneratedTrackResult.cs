using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneratedTrackResult
{
    public readonly List<TrackSegment> segments;
    public readonly Path borderLeft;
    public readonly Path borderRight;
    public readonly Path centerPath;

    public GeneratedTrackResult(List<TrackSegment> segments, Path borderLeft, Path borderRight, Path centerPath)
    {
        this.segments = segments;
        this.borderLeft = borderLeft;
        this.borderRight = borderRight;
        this.centerPath = centerPath;
    }
}
