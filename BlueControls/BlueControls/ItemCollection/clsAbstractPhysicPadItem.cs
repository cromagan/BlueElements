#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2021 Christian Peter
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
#endregion
using BlueControls.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace BlueControls.ItemCollection {
    public abstract class clsAbstractPhysicPadItem : BasicPadItem {
        public readonly List<PointM> Edges = new();
        protected clsAbstractPhysicPadItem(ItemCollectionPad parent, string internalname) : base(parent, internalname) {
            MovablePoint.Add(new PointM(5, 0));
            MovablePoint.Add(new PointM(10, 10));
            MovablePoint.Add(new PointM(0, 10));
            PointsForSuccesfullyMove.AddRange(MovablePoint);
            Edges.Clear();
        }
        protected override RectangleM CalculateUsedArea() {
            var minx = decimal.MaxValue;
            var miny = decimal.MaxValue;
            var maxx = decimal.MinValue;
            var maxy = decimal.MinValue;
            foreach (var thisP in MovablePoint) {
                minx = Math.Min(minx, thisP.X);
                maxx = Math.Max(maxx, thisP.X);
                miny = Math.Min(miny, thisP.Y);
                maxy = Math.Max(maxy, thisP.Y);
            }
            return new RectangleM(minx, miny, maxx - minx, maxy - miny);
        }
        protected override void DrawExplicit(Graphics GR, RectangleF DCoordinates, decimal cZoom, decimal shiftX, decimal shiftY, enStates vState, Size SizeOfParentControl, bool ForPrinting) {
            if (MovablePoint.Count < 1) { return; }
            var lastP = MovablePoint[MovablePoint.Count - 1];
            foreach (var thisP in MovablePoint) {
                GR.DrawLine(Pens.Black, lastP.ZoomAndMove(cZoom, shiftX, shiftY), thisP.ZoomAndMove(cZoom, shiftX, shiftY));
                lastP = thisP;
            }
        }
        protected override void ParseFinished() { }
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
        public PointM Center {
            get {
                decimal totalX = 0;
                decimal totalY = 0;
                for (var i = 0; i < MovablePoint.Count; i++) {
                    totalX += MovablePoint[i].X;
                    totalY += MovablePoint[i].Y;
                }
                return new PointM(totalX / MovablePoint.Count, totalY / MovablePoint.Count);
            }
        }
        public void Move(PointM v) => Move(v.X, v.Y);

        #region Polygon Union
        //   http://csharphelper.com/blog/2016/01/find-a-polygon-union-in-c/
        // Return the union of the two polygons.
        public List<PointF> FindPolygonUnion(List<PointF>[] polygons) {
            // Find the lower-leftmost point in either polygon.
            var cur_pgon = 0;
            var cur_index = 0;
            var cur_point = polygons[cur_pgon][cur_index];
            for (var pgon = 0; pgon < 2; pgon++) {
                for (var index = 0; index < polygons[pgon].Count; index++) {
                    var test_point = polygons[pgon][index];
                    if ((test_point.X < cur_point.X) ||
                        ((test_point.X == cur_point.X) &&
                         (test_point.Y > cur_point.Y))) {
                        cur_pgon = pgon;
                        cur_index = index;
                        cur_point = polygons[cur_pgon][cur_index];
                    }
                }
            }
            // Create the result polygon.
            List<PointF> union = new();
            // Start here.
            var start_point = cur_point;
            union.Add(start_point);
            // Start traversing the polygons.
            // Repeat until we return to the starting point.
            for (; ; )
            {
                // Find the next point.
                var next_index = (cur_index + 1) % polygons[cur_pgon].Count;
                var next_point = polygons[cur_pgon][next_index];
                // Each time through the loop:
                //      cur_pgon is the index of the polygon we're following
                //      cur_point is the last point added to the union
                //      next_point is the next point in the current polygon
                //      next_index is the index of next_point
                // See if this segment intersects
                // any of the other polygon's segments.
                var other_pgon = (cur_pgon + 1) % 2;
                // Keep track of the closest intersection.
                PointF best_intersection = new(0, 0);
                var best_index1 = -1;
                var best_t = 2f;
                for (var index1 = 0; index1 < polygons[other_pgon].Count; index1++) {
                    // Get the index of the next point in the polygon.
                    var index2 = (index1 + 1) % polygons[other_pgon].Count;
                    // See if the segment between points index1
                    // and index2 intersect the current segment.
                    var point1 = polygons[other_pgon][index1];
                    var point2 = polygons[other_pgon][index2];
                    FindIntersection(cur_point, next_point, point1, point2,
                        out _, out var segments_intersect,
                        out var intersection, out _, out _, out var t1, out _);
                    if (segments_intersect && // The segments intersect
                        (t1 > 0.001) &&         // Not at the previous intersection
                        (t1 < best_t))          // Better than the last intersection found
                    {
                        // See if this is an improvement.
                        if (t1 < best_t) {
                            // Save this intersection.
                            best_t = t1;
                            best_index1 = index1;
                            best_intersection = intersection;
                        }
                    }
                }
                // See if we found any intersections.
                if (best_t < 2f) {
                    // We found an intersection. Use it.
                    union.Add(best_intersection);
                    // Prepare to search for the next point from here.
                    // Start following the other polygon.
                    cur_pgon = (cur_pgon + 1) % 2;
                    cur_point = best_intersection;
                    cur_index = best_index1;
                } else {
                    // We didn't find an intersection.
                    // Move to the next point in this polygon.
                    cur_point = next_point;
                    cur_index = next_index;
                    // If we've returned to the starting point, we're done.
                    if (cur_point == start_point) { break; }
                    // Add the current point to the union.
                    union.Add(cur_point);
                }
            }
            return union;
        }
        // Find the point of intersection between
        // the lines p1 --> p2 and p3 --> p4.
        private void FindIntersection(PointF p1, PointF p2, PointF p3, PointF p4,
            out bool lines_intersect, out bool segments_intersect,
            out PointF intersection, out PointF close_p1, out PointF close_p2,
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
                lines_intersect = false;
                segments_intersect = false;
                intersection = new PointF(float.NaN, float.NaN);
                close_p1 = new PointF(float.NaN, float.NaN);
                close_p2 = new PointF(float.NaN, float.NaN);
                t2 = float.PositiveInfinity;
                return;
            }
            lines_intersect = true;
            t2 = (((p3.X - p1.X) * dy12) + ((p1.Y - p3.Y) * dx12)) / -denominator;
            // Find the point of intersection.
            intersection = new PointF(p1.X + (dx12 * t1), p1.Y + (dy12 * t1));
            // The segments intersect if t1 and t2 are between 0 and 1.
            segments_intersect = (t1 >= 0) && (t1 <= 1) && (t2 >= 0) && (t2 <= 1);
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
            close_p1 = new PointF(p1.X + (dx12 * t1), p1.Y + (dy12 * t1));
            close_p2 = new PointF(p3.X + (dx34 * t2), p3.Y + (dy34 * t2));
        }
        // Return true if the polygon is oriented clockwise.
        public bool PolygonIsOrientedClockwise(List<PointF> points) => SignedPolygonArea(points) < 0;
        private float SignedPolygonArea(List<PointF> points) {
            // Add the first point to the end.
            var num_points = points.Count;
            var pts = new PointF[num_points + 1];
            points.CopyTo(pts, 0);
            pts[num_points] = points[0];
            // Get the areas.
            float area = 0;
            for (var i = 0; i < num_points; i++) {
                area +=
                    (pts[i + 1].X - pts[i].X) *
                    (pts[i + 1].Y + pts[i].Y) / 2;
            }
            // Return the result.
            return area;
        }
        #endregion


        #region Collission Calculation
        // https://www.codeproject.com/Articles/15573/2D-Polygon-Collision-Detection
        public strPolygonCollisionResult ColidesWith(clsAbstractPhysicPadItem polygonB, PointM velocity) => PolygonCollision(this, polygonB, velocity);
        // Check if polygon A is going to collide with polygon B for the given velocity
        public static strPolygonCollisionResult PolygonCollision(clsAbstractPhysicPadItem polygonA, clsAbstractPhysicPadItem polygonB, PointM velocity) {
            strPolygonCollisionResult result = new() {
                CheckedObjectA = polygonA,
                CheckedObjectB = polygonB,
                Intersect = true,
                WillIntersect = true
            };
            var edgeCountA = polygonA.Edges.Count;
            var edgeCountB = polygonB.Edges.Count;
            var minIntervalDistance = decimal.MaxValue;
            PointM translationAxis = new(0, 0);
            PointM edge;
            // Loop through all the edges of both polygons
            for (var edgeIndex = 0; edgeIndex < edgeCountA + edgeCountB; edgeIndex++) {
                edge = edgeIndex < edgeCountA ? polygonA.Edges[edgeIndex] : polygonB.Edges[edgeIndex - edgeCountA];
                // ===== 1. Find if the polygons are currently intersecting =====
                // Find the axis perpendicular to the current edge
                PointM axis = new(-edge.Y, edge.X);
                axis.Normalize();
                // Find the projection of the polygon on the current axis
                var minA = 0m;
                var minB = 0m;
                var maxA = 0m;
                var maxB = 0m;
                ProjectPolygon(axis, polygonA, ref minA, ref maxA);
                ProjectPolygon(axis, polygonB, ref minB, ref maxB);
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
            #endregion

            return result;
        }
        // Calculate the distance between [minA, maxA] and [minB, maxB]
        // The distance will be negative if the intervals overlap
        public static decimal IntervalDistance(decimal minA, decimal maxA, decimal minB, decimal maxB) => minA < minB ? minB - maxA : minA - maxB;
        // Calculate the projection of a polygon on an axis and returns it as a [min, max] interval
        public static void ProjectPolygon(PointM axis, clsAbstractPhysicPadItem polygon, ref decimal min, ref decimal max) {
            // To project a point on an axis use the dot product
            var d = axis.DotProduct(polygon.MovablePoint[0]);
            min = d;
            max = d;
            for (var i = 0; i < polygon.MovablePoint.Count; i++) {
                d = polygon.MovablePoint[i].DotProduct(axis);
                if (d < min) {
                    min = d;
                } else {
                    if (d > max) {
                        max = d;
                    }
                }
            }
        }
        #endregion


        #region Schwerpunkt
        //   http://csharphelper.com/blog/2014/07/find-the-centroid-of-a-polygon-in-c/
        // Find the polygon's centroid.
        public PointM FindCentroid() {
            // Add the first point at the end of the array.
            var num_points = MovablePoint.Count - 1;
            var pts = new PointM[num_points + 1];
            MovablePoint.CopyTo(pts, 0);
            pts[num_points] = MovablePoint[0];
            // Find the centroid.
            decimal X = 0;
            decimal Y = 0;
            decimal second_factor;
            for (var i = 0; i < num_points; i++) {
                second_factor =
                    (pts[i].X * pts[i + 1].Y) -
                    (pts[i + 1].X * pts[i].Y);
                X += (pts[i].X + pts[i + 1].X) * second_factor;
                Y += (pts[i].Y + pts[i + 1].Y) * second_factor;
            }
            // Divide by 6 times the polygon's area.
            var polygon_area = PolygonArea();
            X /= 6 * polygon_area;
            Y /= 6 * polygon_area;
            // If the values are negative, the polygon is
            // oriented counterclockwise so reverse the signs.
            if (X < 0) {
                X = -X;
                Y = -Y;
            }
            return new PointM(X, Y);
        }
        #endregion


        #region Flächenberechnung
        //http://csharphelper.com/blog/2014/07/calculate-the-area-of-a-polygon-in-c/
        // Return the polygon's area in "square units."
        // The value will be negative if the polygon is
        // oriented clockwise.
        private decimal SignedPolygonArea() {
            // Add the first point to the end.
            var num_points = MovablePoint.Count - 1;
            var pts = new PointM[num_points + 1];
            MovablePoint.CopyTo(pts, 0);
            pts[num_points] = MovablePoint[0];
            // Get the areas.
            decimal area = 0;
            for (var i = 0; i < num_points; i++) {
                area += (pts[i + 1].X - pts[i].X) * (pts[i + 1].Y + pts[i].Y) / 2;
            }
            // Return the result.
            return area;
        }
        // Return the polygon's area in "square units."
        public decimal PolygonArea() =>
            // Return the absolute value of the signed area.
            // The signed area is negative if the polyogn is
            // oriented clockwise.
            Math.Abs(SignedPolygonArea());
        #endregion


        #region Point in Polygon
        //http://csharphelper.com/blog/2014/07/determine-whether-a-point-is-inside-a-polygon-in-c/
        // Alternative:  https://stackoverflow.com/questions/4243042/c-sharp-point-in-polygon
        // Return True if the point is in the polygon.
        public bool PointInPolygon(decimal X, decimal Y) {
            // Get the angle between the point and the
            // first and last vertices.
            var max_point = MovablePoint.Count - 2;
            var total_angle = GetAngle(MovablePoint[max_point].X, MovablePoint[max_point].Y, X, Y, MovablePoint[0].X, MovablePoint[0].Y);
            // Add the angles from the point
            // to each other pair of vertices.
            for (var i = 0; i < max_point; i++) {
                total_angle += GetAngle(
                    MovablePoint[i].X, MovablePoint[i].Y,
                    X, Y,
                    MovablePoint[i + 1].X, MovablePoint[i + 1].Y);
            }
            // The total angle should be 2 * PI or -2 * PI if
            // the point is in the polygon and close to zero
            // if the point is outside the polygon.
            // The following statement was changed. See the comments.
            //return (Math.Abs(total_angle) > 0.000001);
            return Math.Abs(total_angle) > 1;
        }
        // Return the angle ABC.
        // Return a value between PI and -PI.
        // Note that the value is the opposite of what you might
        // expect because Y coordinates increase downward.
        public static double GetAngle(decimal Ax, decimal Ay, decimal Bx, decimal By, decimal Cx, decimal Cy) {
            // Get the dot product.
            var dot_product = DotProduct(Ax, Ay, Bx, By, Cx, Cy);
            // Get the cross product.
            var cross_product = CrossProductLength(Ax, Ay, Bx, By, Cx, Cy);
            // Calculate the angle.
            return Math.Atan2((double)cross_product, (double)dot_product);
        }
        // Return the dot product AB · BC.
        // Note that AB · BC = |AB| * |BC| * Cos(theta).
        private static decimal DotProduct(decimal Ax, decimal Ay, decimal Bx, decimal By, decimal Cx, decimal Cy) {
            // Get the vectors' coordinates.
            var BAx = Ax - Bx;
            var BAy = Ay - By;
            var BCx = Cx - Bx;
            var BCy = Cy - By;
            // Calculate the dot product.
            return (BAx * BCx) + (BAy * BCy);
        }
        #endregion


        #region IsConvex
        //http://csharphelper.com/blog/2014/07/determine-whether-a-polygon-is-convex-in-c/
        // Return True if the polygon is convex.
        public bool PolygonIsConvex() {
            // For each set of three adjacent points A, B, C,
            // find the cross product AB · BC. If the sign of
            // all the cross products is the same, the angles
            // are all positive or negative (depending on the
            // order in which we visit them) so the polygon
            // is convex.
            var got_negative = false;
            var got_positive = false;
            var num_points = MovablePoint.Count - 1;
            int B, C;
            for (var A = 0; A < num_points; A++) {
                B = (A + 1) % num_points;
                C = (B + 1) % num_points;
                var cross_product =
                    CrossProductLength(
                        MovablePoint[A].X, MovablePoint[A].Y,
                        MovablePoint[B].X, MovablePoint[B].Y,
                        MovablePoint[C].X, MovablePoint[C].Y);
                if (cross_product < 0) {
                    got_negative = true;
                } else if (cross_product > 0) {
                    got_positive = true;
                }
                if (got_negative && got_positive) {
                    return false;
                }
            }
            // If we got this far, the polygon is convex.
            return true;
        }
        // Return the cross product AB x BC.
        // The cross product is a vector perpendicular to AB
        // and BC having length |AB| * |BC| * Sin(theta) and
        // with direction given by the right-hand rule.
        // For two vectors in the X-Y plane, the result is a
        // vector with X and Y components 0 so the Z component
        // gives the vector's length and direction.
        public static decimal CrossProductLength(decimal Ax, decimal Ay, decimal Bx, decimal By, decimal Cx, decimal Cy) {
            // Get the vectors' coordinates.
            var BAx = Ax - Bx;
            var BAy = Ay - By;
            var BCx = Cx - Bx;
            var BCy = Cy - By;
            // Calculate the Z coordinate of the cross product.
            return (BAx * BCy) - (BAy * BCx);
        }
        #endregion

    }
}
