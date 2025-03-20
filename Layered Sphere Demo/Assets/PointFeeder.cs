using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointFeeder
{
    bool circularSelection;
    Point2D center;
    int radius;

    List<Point2D> currentActive;
    List<Point2D> newlyActivePoints;
    List<Point2D> newlyInactivePoints;
    PointValidator validator;
    int maxY;

    public void UpdateCenter(Point2D in_newCenter, bool[,] in_already)
    {
        center = validator.ValidatePoint(in_newCenter);
        UpdateActivePoints(in_already);
    }

    public void UpdateRadius(int in_newRadius, bool[,] in_already)
    {
        radius = in_newRadius;
        UpdateActivePoints(in_already);
        Debug.Log("Feeder Radius = " + radius);
    }

    public void IncreaseRadius(bool[,] in_already)
    {
        UpdateRadius(radius + 1, in_already);
    }

    public void DecreaseRadius(bool[,] in_already)
    {
        if (radius > 1)
            UpdateRadius(radius - 1, in_already);
    }

    public PointFeeder()
    {
        maxY = 0;
        center = new Point2D(0, 0);
        radius = 0;
        currentActive = new List<Point2D>();
        newlyActivePoints = new List<Point2D>();
        newlyInactivePoints = new List<Point2D>();
        validator = new PointValidator();
        circularSelection = false;
    }

    public PointFeeder(Point2D in_center, int in_radius, PointValidator in_validator, bool[,] in_already)
    {
        maxY = in_validator.Get_maxY();
        center = in_center;
        radius = in_radius;
        currentActive = new List<Point2D>();
        newlyActivePoints = new List<Point2D>();
        newlyInactivePoints = new List<Point2D>();
        validator = in_validator;
        circularSelection = true;
        UpdateActivePoints(in_already);
    }

    private void UpdateActivePoints(bool[,] in_already)
    {
        // presumes center and radius have already been set by external agents
        List<Point2D> newActive = new List<Point2D>();
        List<Point2D> toActivate = new List<Point2D>();
        int x = center.x - radius;
        int xLim = center.x + radius;
        int y = center.y - radius;
        int yLim = center.y + radius;
        while (y <= yLim)
        {
            while (x <= xLim)
            {
                Point2D validated = validator.ValidatePoint(new Point2D(x, y));
                if ((validated.y != 0) && (validated.y != maxY - 1))
                {
                    if (!in_already[validated.x, validated.y])
                    {
                        if ((circularSelection) && (InsideRadius(new Vector2(x, y))))
                            toActivate.Add(validated);
                        else if (!circularSelection)
                            toActivate.Add(validated);
                    }
                    //if ((circularSelection) && (InsideRadius(new Vector2(validated.x, validated.y))))
                    if ((circularSelection) && (InsideRadius(new Vector2(x, y))))
                        newActive.Add(validated);
                    else if (!circularSelection)
                        newActive.Add(validated);
                }

                x++;
            }
            x = center.x - radius;
            y++;
        }
        //newlyInactivePoints = GetDeltaList(newActive);
        List<Point2D> cleaned = CleanDeltaList(GetDeltaList(newActive), newlyInactivePoints);
        newlyInactivePoints.AddRange(cleaned);
        //newlyInactivePoints.AddRange(CleanDeltaList(cleaned, currentActive));
        //activePoints = newActive;
        newlyActivePoints = CleanDeltaList(newlyActivePoints, newlyInactivePoints);
        newlyActivePoints.AddRange(CleanDeltaList(toActivate, newlyActivePoints));
        currentActive = newActive;

        //Debug.Log("New actives : " + newlyActivePoints.Count + " current actives : " + currentActive.Count + " and new deactivates : " +  newlyInactivePoints.Count);
    }

    private List<Point2D> GetDeltaList(List<Point2D> in_newActives)
    {
        // the goal here is to find the points from the active list that are not in the new active list
        List<Point2D> toRet = new List<Point2D>();
        int pos = 0;
        while (pos < currentActive.Count)
        {
            Point2D loc = currentActive[pos];
            int subPos = 0;
            bool found = false;
            while (subPos < in_newActives.Count)
            {
                // maybe could do with binary sorting operations
                if (loc.IsEqualTo(in_newActives[subPos]))
                {
                    found = true;
                    break;
                }
                else
                    subPos++;
            }

            if (!found)
                toRet.Add(loc);

            pos++;
        }
        return toRet;
    }

    private List<Point2D> CleanDeltaList(List<Point2D> in_deltas, List<Point2D> in_set)
    {
        List<Point2D> toRet = new List<Point2D>();
        int pos = 0;
        while (pos < in_deltas.Count)
        {
            Point2D toCheck = in_deltas[pos];
            int subPos = 0;
            bool detectedInSet = false;
            while (subPos < in_set.Count)
            {
                Point2D filter = in_set[subPos];
                if (filter.IsEqualTo(toCheck))
                {
                    detectedInSet = true;
                    break;
                }
                subPos++;
            }
            if (!detectedInSet)
                toRet.Add(toCheck);
            pos++;
        }
        return toRet;
    }

    public void RemoveFromInactive(Point2D ins)
    {
        int pos = 0;
        while (pos < newlyInactivePoints.Count)
        {
            if (newlyInactivePoints[pos].IsEqualTo(ins))
            {
                newlyInactivePoints.RemoveAt(pos);
                break;
            }
            pos++;
        }
    }

    public void RemoveFromActive(Point2D ins)
    {
        int pos = 0;
        while (pos < newlyActivePoints.Count)
        {
            if (newlyActivePoints[pos].IsEqualTo(ins))
            {
                newlyActivePoints.RemoveAt(pos);
                break;
            }
            pos++;
        }
    }

    public bool HasUpdates()
    {
        if ((newlyActivePoints.Count > 0) || (newlyInactivePoints.Count > 0))
            return true;
        else
            return false;
    }

    private bool InsideRadius(Vector2 in_loc)
    {
        float distance = Vector2.Distance(new Vector2(center.x, center.y), in_loc);
        float offsetRadius = (float)radius + 0.5f;
        if (distance <= offsetRadius)
            return true;
        else
            return false;
    }

    public List<Point2D> Get_currentActive() { return currentActive; }
    public List<Point2D> Get_newlyActivePoints() { return newlyActivePoints; }
    public List<Point2D> Get_newlyInactivePoints() { return newlyInactivePoints; }
}
