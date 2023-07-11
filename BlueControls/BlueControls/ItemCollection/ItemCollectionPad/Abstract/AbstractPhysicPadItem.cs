// Authors:
// Christian Peter
//
// Copyright (c) 2023 Christian Peter
// https://github.com/cromagan/BlueElements
//
// License: GNU Affero General Public License v3.0
// https://github.com/cromagan/BlueElements/blob/master/LICENSE
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

#nullable enable

using System;
using System.Collections.Generic;
using System.Drawing;

namespace BlueControls.ItemCollection;

public abstract class AbstractPhysicPadItem : BasicPadItem {

    #region Constructors

    protected AbstractPhysicPadItem(string internalname) : base(internalname) {
        MovablePoint.Add(new PointM(5, 0));
        MovablePoint.Add(new PointM(10, 10));
        MovablePoint.Add(new PointM(0, 10));
        PointsForSuccesfullyMove.AddRange(MovablePoint);
        Edges.Clear();
    }

    #endregion

    #region Properties

    public PointM Center {
        get {
            float totalX = 0;
            float totalY = 0;
            foreach (var thisPoint in MovablePoint) {
                totalX += thisPoint.X;
                totalY += thisPoint.Y;
            }
            return new PointM(totalX / MovablePoint.Count, totalY / MovablePoint.Count);
        }
    }

    public List<PointM> Edges { get; } = new();

    #endregion

    #region Methods

    // Return the cross product AB x BC.
    // The cross product is a vector perpendicular to AB
    // and BC having length |AB| * |BC| * Sin(theta) and
    // with direction given by the right-hand rule.
    // For two vectors in the X-Y plane, the result is a
    // vector with X and Y components 0 so the Z component
    // gives the vector's length and direction.
    public static float CrossProductLength(float ax, float ay, float bx, float by, float cx, float cy) {
        // Get the vectors' coordinates.
        var bAx = ax - bx;
        var bAy = ay - by;
        var bCx = cx - bx;
        var bCy = cy - by;
        // Calculate the Z coordinate of the cross product.
        return (bAx * bCy) - (bAy * bCx);
    }

    // Return the angle ABC.
    // Return a value between PI and -PI.
    // Note that the value is the opposite of what you might
    // expect because Y coordinates increase downward.
    public static float GetAngle(float ax, float ay, float bx, float by, float cx, float cy) {
        // Get the dot product.
        var dotProduct = DotProduct(ax, ay, bx, by, cx, cy);
        // Get the cross product.
        var crossProduct = CrossProductLength(ax, ay, bx, by, cx, cy);
        // Calculate the angle.
        return (float)Math.Atan2(crossProduct, dotProduct);
    }

    // Calculate the distance between [minA, maxA] and [minB, maxB]
    // The distance will be negative if the intervals overlap
    public static float IntervalDistance(float minA, float maxA, float minB, float maxB) => minA < minB ? minB - maxA : minA - maxB;

    // Check if polygon A is going to collide with polygon B for the given velocity
    public static StrPolygonCollisionResult PolygonCollision(AbstractPhysicPadItem polygonA, AbstractPhysicPadItem polygonB, PointM velocity) {
        StrPolygonCollisionResult result = new() {
            CheckedObjectA = polygonA,
            CheckedObjectB = polygonB,
            Intersect = true,
            WillIntersect = true
        };
        var edgeCountA = polygonA.Edges.Count;
        var edgeCountB = polygonB.Edges.Count;
        var minIntervalDistance = float.MaxValue;
        PointM translationAxis = new(0, 0);
        // Loop through all the edges of both polygons
        for (var edgeIndex = 0; edgeIndex < edgeCountA + edgeCountB; edgeIndex++) {
            var edge = edgeIndex < edgeCountA ? polygonA.Edges[edgeIndex] : polygonB.Edges[edgeIndex - edgeCountA];
            // ===== 1. Find if the polygons are currently intersecting =====
            // Find the axis perpendicular to the current edge
            PointM axis = new(-edge.Y, edge.X);
            axis.Normalize();
            // Find the projection of the polygon on the current axis
            ProjectPolygon(axis, polygonA, out var minA, out var maxA);
            ProjectPolygon(axis, polygonB, out var minB, out var maxB);
            // Check if the polygon projections are currentlty intersecting
            if (IntervalDistance(minA, maxA, minB, maxB) > 0) {
                result.Intersect = false;
            }
            // ===== 2. Now find if the polygons *will* intersect =====
            // Project the velocity on the current axis
            var velocityProjection = axis.DotProduct(velocity);
            // Get the projection of polygon A during the movement
            if (velocityProjection < 0) {
                minA += velocityProjection;
            } else {
                maxA += velocityProjection;
            }
            // Do the same test as above for the new projection
            var intervalDistance = IntervalDistance(minA, maxA, minB, maxB);
            if (intervalDistance > 0) { result.WillIntersect = false; }
            // If the polygons are not intersecting and won't intersect, exit the loop
            if (!result.Intersect && !result.WillIntersect) { break; }
            // Check if the current interval distance is the minimum one. If so store
            // the interval distance and the current distance.
            // This will be used to calculate the minimum translation vector
            intervalDistance = Math.Abs(intervalDistance);
            if (intervalDistance < minIntervalDistance) {
                minIntervalDistance = intervalDistance;
                translationAxis = axis;
                var d = polygonA.Center - polygonB.Center;
                if (d.DotProduct(translationAxis) < 0) {
                    translationAxis = -translationAxis;
                }
            }
        }
        // The minimum translation vector can be used to push the polygons appart.
        // First moves the polygons by their velocity
        // then move polygonA by MinimumTranslationVector.
        if (result.WillIntersect) {
            result.MinimumTranslationVector = translationAxis * minIntervalDistance;
        }

        #region Example

        //var playerTranslation = velocity;
        //foreach (var polygon in polygons)
        //{
        //    if (polygon == player) continue;
        //    var r = player.ColidesWith(polygon, velocity);
        //    if (r.WillIntersect)
        //    {
        //        playerTranslation = velocity + r.MinimumTranslationVector;
        //        break;
        //    }
        //}
        //player.Move(playerTranslation);

        #endregion Example

        return result;
    }

    // Calculate the projection of a polygon on an axis and returns it as a [min, max] interval
    public static void ProjectPolygon(PointM axis, AbstractPhysicPadItem polygon, out float min, out float max) {
        // To project a point on an axis use the dot product
        var d = axis.DotProduct(polygon.MovablePoint[0]);
        min = d;
        max = d;
        foreach (var t in polygon.MovablePoint) {
            d = t.DotProduct(axis);
            if (d < min) {
                min = d;
            } else {
                if (d > max) {
                    max = d;
                }
            }
        }
    }

    public void BuildEdges() {
        Edges.Clear();
        for (var i = 0; i < MovablePoint.Count; i++) {
            if (i + 1 >= MovablePoint.Count) {
                Edges.Add(MovablePoint[0] - MovablePoint[i]);
            } else {
                Edges.Add(MovablePoint[i + 1] - MovablePoint[i]);
            }
        }
    }

    // https://www.codeproject.com/Articles/15573/2D-Polygon-Collision-Detection
    public StrPolygonCollisionResult ColidesWith(AbstractPhysicPadItem polygonB, PointM velocity) => PolygonCollision(this, polygonB, velocity);

    //   http://csharphelper.com/blog/2014/07/find-the-centroid-of-a-polygon-in-c/
    // Find the polygon's centroid.
    public PointM FindCentroid() {
        // GenerateAndAdd the first point at the end of the array.
        var numPoints = MovablePoint.Count - 1;
        var pts = new PointM[numPoints + 1];
        MovablePoint.CopyTo(pts, 0);
        pts[numPoints] = MovablePoint[0];
        // Find the centroid.
        float x = 0;
        float y = 0;
        for (var i = 0; i < numPoints; i++) {
            var secondFactor = (pts[i].X * pts[i + 1].Y) -
                               (pts[i + 1].X * pts[i].Y);
            x += (pts[i].X + pts[i + 1].X) * secondFactor;
            y += (pts[i].Y + pts[i + 1].Y) * secondFactor;
        }
        // Divide by 6 times the polygon's area.
        var polygonArea = PolygonArea();
        x /= 6 * polygonArea;
        y /= 6 * polygonArea;
        // If the values are negative, the polygon is
        // oriented counterclockwise so reverse the signs.
        if (x < 0) {
            x = -x;
            y = -y;
        }
        return new PointM(x, y);
    }

    //   http://csharphelper.com/blog/2016/01/find-a-polygon-union-in-c/
    // Return the union of the two polygons.
    public List<PointF> FindPolygonUnion(List<PointF>[] polygons) {
        // Find the lower-leftmost point in either polygon.
        var curPgon = 0;
        var curIndex = 0;
        var curPoint = polygons[curPgon][curIndex];
        for (var pgon = 0; pgon < 2; pgon++) {
            for (var index = 0; index < polygons[pgon].Count; index++) {
                var testPoint = polygons[pgon][index];
                if (testPoint.X < curPoint.X ||
                    (testPoint.X == curPoint.X &&
                     testPoint.Y > curPoint.Y)) {
                    curPgon = pgon;
                    curIndex = index;
                    curPoint = polygons[curPgon][curIndex];
                }
            }
        }
        // Create the result polygon.
        List<PointF> union = new();
        // Start here.
        var startPoint = curPoint;
        union.Add(startPoint);
        // Start traversing the polygons.
        // Repeat until we return to the starting point.
        for (; ; )
        {
            // Find the next point.
            var nextIndex = (curIndex + 1) % polygons[curPgon].Count;
            var nextPoint = polygons[curPgon][nextIndex];
            // Each time through the loop:
            //      cur_pgon is the index of the polygon we're following
            //      cur_point is the last point added to the union
            //      next_point is the next point in the current polygon
            //      next_index is the index of next_point
            // See if this segment intersects
            // any of the other polygon's segments.
            var otherPgon = (curPgon + 1) % 2;
            // Keep track of the closest intersection.
            PointF bestIntersection = new(0, 0);
            var bestIndex1 = -1;
            var bestT = 2f;
            for (var index1 = 0; index1 < polygons[otherPgon].Count; index1++) {
                // Get the index of the next point in the polygon.
                var index2 = (index1 + 1) % polygons[otherPgon].Count;
                // See if the segment between points index1
                // and index2 intersect the current segment.
                var point1 = polygons[otherPgon][index1];
                var point2 = polygons[otherPgon][index2];
                FindIntersection(curPoint, nextPoint, point1, point2,
                    out _, out var segmentsIntersect,
                    out var intersection, out _, out _, out var t1, out _);
                if (segmentsIntersect && // The segments intersect
                    t1 > 0.001 &&         // Not at the previous intersection
                    t1 < bestT)          // Better than the last intersection found
                {
                    // See if this is an improvement.
                    if (t1 < bestT) {
                        // Save this intersection.
                        bestT = t1;
                        bestIndex1 = index1;
                        bestIntersection = intersection;
                    }
                }
            }
            // See if we found any intersections.
            if (bestT < 2f) {
                // We found an intersection. Use it.
                union.Add(bestIntersection);
                // Prepare to search for the next point from here.
                // Start following the other polygon.
                curPgon = (curPgon + 1) % 2;
                curPoint = bestIntersection;
                curIndex = bestIndex1;
            } else {
                // We didn't find an intersection.
                // Move to the next point in this polygon.
                curPoint = nextPoint;
                curIndex = nextIndex;
                // If we've returned to the starting point, we're done.
                if (curPoint == startPoint) { break; }
                // GenerateAndAdd the current point to the union.
                union.Add(curPoint);
            }
        }
        return union;
    }

    public void Move(PointM v) => Move(v.X, v.Y);

    //http://csharphelper.com/blog/2014/07/determine-whether-a-point-is-inside-a-polygon-in-c/
    // Alternative:  https://stackoverflow.com/questions/4243042/c-sharp-point-in-polygon
    // Return True if the point is in the polygon.
    public bool PointInPolygon(float x, float y) {
        // Get the angle between the point and the
        // first and last vertices.
        var maxPoint = MovablePoint.Count - 2;
        var totalAngle = GetAngle(MovablePoint[maxPoint].X, MovablePoint[maxPoint].Y, x, y, MovablePoint[0].X, MovablePoint[0].Y);
        // GenerateAndAdd the angles from the point
        // to each other pair of vertices.
        for (var i = 0; i < maxPoint; i++) {
            totalAngle += GetAngle(
                MovablePoint[i].X, MovablePoint[i].Y,
                x, y,
                MovablePoint[i + 1].X, MovablePoint[i + 1].Y);
        }
        // The total angle should be 2 * PI or -2 * PI if
        // the point is in the polygon and close to zero
        // if the point is outside the polygon.
        // The following statement was changed. See the comments.
        //return (Math.Abs(total_angle) > 0.000001);
        return Math.Abs(totalAngle) > 1;
    }

    // Return the polygon's area in "square units."
    public float PolygonArea() =>
        // Return the absolute value of the signed area.
        // The signed area is negative if the polyogn is
        // oriented clockwise.
        Math.Abs(SignedPolygonArea());

    //http://csharphelper.com/blog/2014/07/determine-whether-a-polygon-is-convex-in-c/
    // Return True if the polygon is convex.
    public bool PolygonIsConvex() {
        // For each set of three adjacent points A, B, C,
        // find the cross product AB · BC. If the sign of
        // all the cross products is the same, the angles
        // are all positive or negative (depending on the
        // order in which we visit them) so the polygon
        // is convex.
        var gotNegative = false;
        var gotPositive = false;
        var numPoints = MovablePoint.Count - 1;
        for (var a = 0; a < numPoints; a++) {
            var b = (a + 1) % numPoints;
            var c = (b + 1) % numPoints;
            var crossProduct =
                CrossProductLength(
                    MovablePoint[a].X, MovablePoint[a].Y,
                    MovablePoint[b].X, MovablePoint[b].Y,
                    MovablePoint[c].X, MovablePoint[c].Y);
            if (crossProduct < 0) {
                gotNegative = true;
            } else if (crossProduct > 0) {
                gotPositive = true;
            }
            if (gotNegative && gotPositive) {
                return false;
            }
        }
        // If we got this far, the polygon is convex.
        return true;
    }

    // Return true if the polygon is oriented clockwise.
    public bool PolygonIsOrientedClockwise(List<PointF> points) => SignedPolygonArea(points) < 0;

    protected override RectangleF CalculateUsedArea() {
        var minx = float.MaxValue;
        var miny = float.MaxValue;
        var maxx = float.MinValue;
        var maxy = float.MinValue;
        foreach (var thisP in MovablePoint) {
            minx = Math.Min(minx, thisP.X);
            maxx = Math.Max(maxx, thisP.X);
            miny = Math.Min(miny, thisP.Y);
            maxy = Math.Max(maxy, thisP.Y);
        }
        return new RectangleF(minx, miny, maxx - minx, maxy - miny);
    }

    protected override void DrawExplicit(Graphics gr, RectangleF positionModified, float zoom, float shiftX, float shiftY, bool forPrinting) {
        if (MovablePoint.Count > 0) {
            var lastP = MovablePoint[MovablePoint.Count - 1];
            foreach (var thisP in MovablePoint) {
                gr.DrawLine(Pens.Black, lastP.ZoomAndMove(zoom, shiftX, shiftY), thisP.ZoomAndMove(zoom, shiftX, shiftY));
                lastP = thisP;
            }
        }
        base.DrawExplicit(gr, positionModified, zoom, shiftX, shiftY, forPrinting);
    }

    protected override void ParseFinished() => base.ParseFinished();

    // Return the dot product AB · BC.
    // Note that AB · BC = |AB| * |BC| * Cos(theta).
    private static float DotProduct(float ax, float ay, float bx, float by, float cx, float cy) {
        // Get the vectors' coordinates.
        var bAx = ax - bx;
        var bAy = ay - by;
        var bCx = cx - bx;
        var bCy = cy - by;
        // Calculate the dot product.
        return (bAx * bCx) + (bAy * bCy);
    }

    // Find the point of intersection between
    // the lines p1 --> p2 and p3 --> p4.
    private static void FindIntersection(PointF p1, PointF p2, PointF p3, PointF p4,
        out bool linesIntersect, out bool segmentsIntersect,
        out PointF intersection, out PointF closeP1, out PointF closeP2,
        out float t1, out float t2) {
        // Get the segments' parameters.
        var dx12 = p2.X - p1.X;
        var dy12 = p2.Y - p1.Y;
        var dx34 = p4.X - p3.X;
        var dy34 = p4.Y - p3.Y;
        // Solve for t1 and t2
        var denominator = (dy12 * dx34) - (dx12 * dy34);
        t1 = (((p1.X - p3.X) * dy34) + ((p3.Y - p1.Y) * dx34)) / denominator;
        if (float.IsInfinity(t1)) {
            // The lines are parallel (or close enough to it).
            linesIntersect = false;
            segmentsIntersect = false;
            intersection = new PointF(float.NaN, float.NaN);
            closeP1 = new PointF(float.NaN, float.NaN);
            closeP2 = new PointF(float.NaN, float.NaN);
            t2 = float.PositiveInfinity;
            return;
        }
        linesIntersect = true;
        t2 = (((p3.X - p1.X) * dy12) + ((p1.Y - p3.Y) * dx12)) / -denominator;
        // Find the point of intersection.
        intersection = new PointF(p1.X + (dx12 * t1), p1.Y + (dy12 * t1));
        // The segments intersect if t1 and t2 are between 0 and 1.
        segmentsIntersect = t1 >= 0 && t1 <= 1 && t2 >= 0 && t2 <= 1;
        // Find the closest points on the segments.
        if (t1 < 0) {
            t1 = 0;
        } else if (t1 > 1) {
            t1 = 1;
        }
        if (t2 < 0) {
            t2 = 0;
        } else if (t2 > 1) {
            t2 = 1;
        }
        closeP1 = new PointF(p1.X + (dx12 * t1), p1.Y + (dy12 * t1));
        closeP2 = new PointF(p3.X + (dx34 * t2), p3.Y + (dy34 * t2));
    }

    private static float SignedPolygonArea(IList<PointF> points) {
        // GenerateAndAdd the first point to the end.
        var numPoints = points.Count;
        var pts = new PointF[numPoints + 1];
        points.CopyTo(pts, 0);
        pts[numPoints] = points[0];
        // Get the areas.
        float area = 0;
        for (var i = 0; i < numPoints; i++) {
            area +=
                (pts[i + 1].X - pts[i].X) *
                (pts[i + 1].Y + pts[i].Y) / 2;
        }
        // Return the result.
        return area;
    }

    //http://csharphelper.com/blog/2014/07/calculate-the-area-of-a-polygon-in-c/
    // Return the polygon's area in "square units."
    // The value will be negative if the polygon is
    // oriented clockwise.
    private float SignedPolygonArea() {
        // GenerateAndAdd the first point to the end.
        var numPoints = MovablePoint.Count - 1;
        var pts = new PointM[numPoints + 1];
        MovablePoint.CopyTo(pts, 0);
        pts[numPoints] = MovablePoint[0];
        // Get the areas.
        float area = 0;
        for (var i = 0; i < numPoints; i++) {
            area += (pts[i + 1].X - pts[i].X) * (pts[i + 1].Y + pts[i].Y) / 2;
        }
        // Return the result.
        return area;
    }

    #endregion
}