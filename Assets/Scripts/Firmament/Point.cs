using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Point{

    public override bool Equals(object obj){
        Point p = (Point) obj;
        return p.x == this.y && p.y == this.y;
    }
    public int x;
    public int y;
    public int g;
    public int cost;
    public Point parent;

    public Point(int x, int y){
        this.x = x; this.y = y;
    }
    public Point(int x, int y, int g){
        this.x = x;
        this.y = y;
        this.g = g;
    }
}