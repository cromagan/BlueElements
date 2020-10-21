using BlueBasics;
using BlueControls;
using BlueControls.Enums;
using BlueControls.ItemCollection;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueControls.ItemCollection
{
    public abstract class clsAbstractPhysicPadItem : BasicPadItem
    {
        //    https://stackoverflow.com/questions/4243042/c-sharp-point-in-polygon


        public readonly List<PointM> Edges = new List<PointM>();

        protected clsAbstractPhysicPadItem(ItemCollectionPad parent, string internalname) : base(parent, internalname)
        {
            Points.Add(new PointM(5, 0));
            Points.Add(new PointM(10, 10));
            Points.Add(new PointM(0, 10));

            Edges.Clear();
        }

        protected override RectangleM CalculateUsedArea()
        {
            var minx = decimal.MaxValue;
            var miny = decimal.MaxValue;
            var maxx = decimal.MinValue;
            var maxy = decimal.MinValue;


            foreach (var thisP in Points)
            {
                minx = Math.Min(minx, thisP.X);
                maxx = Math.Max(maxx, thisP.X);
                miny = Math.Min(miny, thisP.Y);
                maxy = Math.Max(maxy, thisP.Y);
            }

            return new RectangleM(minx, miny, maxx - minx, maxy - miny);

        }






        protected override void DrawExplicit(Graphics GR, RectangleF DCoordinates, decimal cZoom, decimal MoveX, decimal MoveY, enStates vState, Size SizeOfParentControl, bool ForPrinting)
        {

            if (Points.Count < 1) { return; }

            var lastP = Points[Points.Count - 1];

            foreach (var thisP in Points)
            {
                GR.DrawLine(Pens.Black, lastP.ZoomAndMove(cZoom, MoveX, MoveY), thisP.ZoomAndMove(cZoom, MoveX, MoveY));
                lastP = thisP;
            }



        }

        protected override void GenerateInternalRelationExplicit() { }

        protected override void ParseFinished() { }


        public void BuildEdges()
        {

            Edges.Clear();
            for (var i = 0; i < Points.Count; i++)
            {
                if (i + 1 >= Points.Count)
                {
                    Edges.Add(Points[0] - Points[i]);
                }
                else
                {
                    Edges.Add(Points[i + 1] - Points[i]);
                }
            }
        }

        public PointM Center
        {
            get
            {
                decimal totalX = 0;
                decimal totalY = 0;
                for (var i = 0; i < Points.Count; i++)
                {
                    totalX += Points[i].X;
                    totalY += Points[i].Y;
                }

                return new PointM(totalX / (decimal)Points.Count, totalY / (decimal)Points.Count);
            }
        }

        public void Move(PointM v)
        {
            Move(v.X, v.Y);
        }

        public override void Move(decimal x, decimal y)
        {
            for (var i = 0; i < Points.Count; i++)
            {
                Points[i].X += x;
                Points[i].Y += y;
            }

            base.Move(x, y);

            // Edges nicht neu berechnen, da diese sich nicht verändern
        }

        #region Collission Calculation
        // https://www.codeproject.com/Articles/15573/2D-Polygon-Collision-Detection






        public strPolygonCollisionResult ColidesWith(clsAbstractPhysicPadItem polygonB, PointM velocity)
        {
            return clsPhysicPadItem.PolygonCollision(this, polygonB, velocity);
        }

        // Check if polygon A is going to collide with polygon B for the given velocity
        public static strPolygonCollisionResult PolygonCollision(clsAbstractPhysicPadItem polygonA, clsAbstractPhysicPadItem polygonB, PointM velocity)
        {
            var result = new strPolygonCollisionResult();
            result.CheckedObjectA = polygonA;
            result.CheckedObjectB = polygonB;
            result.Intersect = true;
            result.WillIntersect = true;

            var edgeCountA = polygonA.Edges.Count;
            var edgeCountB = polygonB.Edges.Count;
            var minIntervalDistance = decimal.MaxValue;
            var translationAxis = new PointM(0, 0);
            PointM edge;

            // Loop through all the edges of both polygons
            for (var edgeIndex = 0; edgeIndex < edgeCountA + edgeCountB; edgeIndex++)
            {
                if (edgeIndex < edgeCountA)
                {
                    edge = polygonA.Edges[edgeIndex];
                }
                else
                {
                    edge = polygonB.Edges[edgeIndex - edgeCountA];
                }

                // ===== 1. Find if the polygons are currently intersecting =====

                // Find the axis perpendicular to the current edge
                var axis = new PointM(-edge.Y, edge.X);
                axis.Normalize();

                // Find the projection of the polygon on the current axis
                var minA = 0m; var minB = 0m; var maxA = 0m; var maxB = 0m;
                ProjectPolygon(axis, polygonA, ref minA, ref maxA);
                ProjectPolygon(axis, polygonB, ref minB, ref maxB);

                // Check if the polygon projections are currentlty intersecting
                if (IntervalDistance(minA, maxA, minB, maxB) > 0) result.Intersect = false;

                // ===== 2. Now find if the polygons *will* intersect =====

                // Project the velocity on the current axis
                var velocityProjection = axis.DotProduct(velocity);

                // Get the projection of polygon A during the movement
                if (velocityProjection < 0)
                {
                    minA += velocityProjection;
                }
                else
                {
                    maxA += velocityProjection;
                }

                // Do the same test as above for the new projection
                var intervalDistance = IntervalDistance(minA, maxA, minB, maxB);
                if (intervalDistance > 0) result.WillIntersect = false;

                // If the polygons are not intersecting and won't intersect, exit the loop
                if (!result.Intersect && !result.WillIntersect) break;

                // Check if the current interval distance is the minimum one. If so store
                // the interval distance and the current distance.
                // This will be used to calculate the minimum translation vector
                intervalDistance = Math.Abs(intervalDistance);
                if (intervalDistance < minIntervalDistance)
                {
                    minIntervalDistance = intervalDistance;
                    translationAxis = axis;

                    var d = polygonA.Center - polygonB.Center;
                    if (d.DotProduct(translationAxis) < 0) translationAxis = -translationAxis;
                }
            }

            // The minimum translation vector can be used to push the polygons appart.
            // First moves the polygons by their velocity
            // then move polygonA by MinimumTranslationVector.
            if (result.WillIntersect) result.MinimumTranslationVector = translationAxis * minIntervalDistance;

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
        public static decimal IntervalDistance(decimal minA, decimal maxA, decimal minB, decimal maxB)
        {
            if (minA < minB)
            {
                return minB - maxA;
            }
            else
            {
                return minA - maxB;
            }
        }


        // Calculate the projection of a polygon on an axis and returns it as a [min, max] interval
        public static void ProjectPolygon(PointM axis, clsAbstractPhysicPadItem polygon, ref decimal min, ref decimal max)
        {
            // To project a point on an axis use the dot product
            var d = axis.DotProduct(polygon.Points[0]);
            min = d;
            max = d;
            for (var i = 0; i < polygon.Points.Count; i++)
            {
                d = polygon.Points[i].DotProduct(axis);
                if (d < min)
                {
                    min = d;
                }
                else
                {
                    if (d > max)
                    {
                        max = d;
                    }
                }
            }
        }

        #endregion

    }
}
