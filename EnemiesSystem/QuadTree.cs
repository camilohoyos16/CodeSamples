using GoogleMobileAds.Api;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.XR;

[Serializable]
public class QuadTree
{
    public Boundary boundaries;
    public int capacity;
    public List<EnemyController> enemies = new List<EnemyController>();

    public bool divided = false;
    private QuadTree topLeftQ;
    private QuadTree topRightQ;
    private QuadTree downleftQ;
    private QuadTree downRightQ;

    public QuadTree(Boundary boundaries, int capacity) {
        this.boundaries = boundaries;
        this.capacity = capacity;
        this.divided = false;
        //EventManager.Instance.AddListener<ClearQuadPointsEvent>(ClearPoints);
    }

    public bool InsertPoint(EnemyController newEnemy) {
        if (!this.boundaries.SquareContainsPoint(newEnemy.transform.position))
            return false;

        if (this.enemies.Count < this.capacity) {
            this.enemies.Add(newEnemy);
            return true;
        } else {
            if (!this.divided) {
                this.SubDivide();
            }

            if (this.topLeftQ.InsertPoint(newEnemy)) {
                return true;
            }
            if (this.topRightQ.InsertPoint(newEnemy)) {
                return true;
            }
            if (this.downleftQ.InsertPoint(newEnemy)) {
                return true;
            }
            if (this.downRightQ.InsertPoint(newEnemy)) { 
                return true;
            }

            return false;
        }
    }

    public void Query(Boundary range, List<EnemyController> pointsInside) {
        if(pointsInside == null) {
            pointsInside = new List<EnemyController>();
        }

        if (!this.boundaries.IntersectsCircle(range)) {
            return;
        } else {
            for (int i = 0; i < this.enemies.Count; i++) {
                if (range.CircleContainsPoint(this.enemies[i].transform.position)){
                    pointsInside.Add(this.enemies[i]);
                }
            }

            if (this.divided) {
                this.topLeftQ.Query(range, pointsInside);
                this.topRightQ.Query(range, pointsInside);
                this.downleftQ.Query(range, pointsInside);
                this.downRightQ.Query(range, pointsInside);
            }
        }
    }


    public void Draw() {
        Vector3 topLeft = new Vector3(this.boundaries.x - this.boundaries.w / 2, 0.2f, this.boundaries.z + this.boundaries.h / 2);
        Vector3 topRight = new Vector3(this.boundaries.x + this.boundaries.w / 2, 0.2f, this.boundaries.z + this.boundaries.h / 2);
        Vector3 downleft = new Vector3(this.boundaries.x - this.boundaries.w / 2, 0.2f, this.boundaries.z - this.boundaries.h / 2);
        Vector3 downRight = new Vector3(this.boundaries.x + this.boundaries.w / 2, 0.2f, this.boundaries.z - this.boundaries.h / 2);
        Debug.DrawLine(topLeft, topRight, Color.green);
        Debug.DrawLine(topRight, downRight, Color.green);
        Debug.DrawLine(downRight, downleft, Color.green);
        Debug.DrawLine(downleft, topLeft, Color.green);
    }

    public void SubDivide() {
        float x = this.boundaries.x;
        float y = this.boundaries.z;
        float w = this.boundaries.w;
        float h = this.boundaries.h;

        Boundary TL = new Boundary(x - w/4, y + h/4, w/2, h/2);
        Boundary TR = new Boundary(x + w/4, y + h/4, w/2, h/2);
        Boundary DL = new Boundary(x - w/4, y - h/4, w/2, h/2);
        Boundary DR = new Boundary(x + w/4, y - h/4, w/2, h/2);

        this.topLeftQ = new QuadTree(TL, this.capacity);
        this.topRightQ = new QuadTree(TR, this.capacity);
        this.downleftQ = new QuadTree(DL, this.capacity);
        this.downRightQ = new QuadTree(DR, this.capacity);

        this.divided = true;

        //QuadTreeController.instance.AddQuads(topLeftQ, topRightQ, downleftQ, downRightQ);
    }
}

[Serializable]
public struct Boundary
{
    public Boundary(float x, float z, float w, float h) {
        this.x = x;
        this.z = z;
        this.w = w;
        this.h = h;
    }

    public float x;
    public float z;
    public float h;
    public float w;


    public bool SquareContainsPoint(Vector3 p) {
        return (p.x >= x - w / 2 &&
            p.x <= x + w / 2 &&
            p.z >= z - h / 2 &&
            p.z <= z + h / 2);
    }

    public bool CircleContainsPoint(Vector3 p) {
        Vector2 circleCenter = new Vector2(x, z);
        Vector2 pointCenter = new Vector2(p.x, p.z);

        float distance = Vector2.Distance(pointCenter, circleCenter);

        return distance < this.w / 2;
    }

    public bool IntersectsSquare(Boundary range) {
        float overlapX = Math.Max(0, Math.Min(range.x + range.w / 2, x + w / 2) - Math.Max(range.x - range.w / 2, x - w / 2));
        float overlapY = Math.Max(0, Math.Min(range.z + range.h / 2, z + h / 2) - Math.Max(range.z - range.h / 2, z - h / 2));
        float totalOverlap = overlapX * overlapY;

        return totalOverlap > 0;
    }

    public bool IntersectsCircle(Boundary circle) {
        float closestX = 0;
        float closestY = 0;

        if (circle.x < x - w / 2)
            closestX = x - w / 2;
        else if (circle.x > x + w / 2)
            closestX = x + w / 2;
        else
            closestX = circle.x;

        if (circle.z < z - h / 2)
            closestY = z - h / 2;
        else if (circle.z > z + h / 2)
            closestY = z + h / 2;
        else
            closestY = circle.z;

        double distance;

        float a = Math.Abs(circle.x - closestX);
        float b = Math.Abs(circle.z - closestY);

        distance = Math.Sqrt((a * a) + (b * b));

        return distance < circle.w / 2;
    }
}
