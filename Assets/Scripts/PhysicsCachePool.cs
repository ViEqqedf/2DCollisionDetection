using System.Collections.Generic;
using Physics.Collision;
using Physics.Collision.Model;

namespace Physics {
    public static class PhysicsCachePool {
        public static Stack<CollisionPair> collisionPairPool = new Stack<CollisionPair>();
        public static Stack<SupportPoint> supPointPool = new Stack<SupportPoint>();
        public static Stack<Simplex> simplexPool = new Stack<Simplex>();
        public static Stack<SimplexEdge> simplexEdgePool = new Stack<SimplexEdge>();
        public static Stack<Edge> edgePool = new Stack<Edge>();

        #region SupPoint

        public static SupportPoint GetSupPointFromPool() {
            if (supPointPool.Count > 0) {
                return supPointPool.Pop();
            } else {
                return new SupportPoint();
            }
        }

        public static void RecycleSupPoint(SupportPoint point) {
            if (point != null) {
                supPointPool.Push(point);
            }
        }

        #endregion

        #region CollisionPair

        public static CollisionPair GetCollisionPairFromPool() {
            if (collisionPairPool.Count > 0) {
                return collisionPairPool.Pop();
            } else {
                return new CollisionPair();
            }
        }

        public static void RecycleCollisionPair(List<CollisionPair> pairs) {
            if (pairs != null) {
                for (int i = 0, count = pairs.Count; i < count; i++) {
                    collisionPairPool.Push(pairs[i]);
                }

                pairs.Clear();
            }
        }

        #endregion

        #region Simplex

        public static Simplex GetSimplexFromPool() {
            if (simplexPool.Count > 0) {
                return simplexPool.Pop();
            } else {
                return new Simplex();
            }
        }

        public static void RecycleSimplex(Simplex simplex) {
            if (simplex != null) {
                simplex.Clear();
                simplexPool.Push(simplex);
            }
        }

        #endregion

        #region SimplexEdge

        public static SimplexEdge GetSimplexEdgeFromPool() {
            if (simplexEdgePool.Count > 0) {
                return simplexEdgePool.Pop();
            } else {
                return new SimplexEdge();
            }
        }

        public static void RecycleSimplexEdge(SimplexEdge simplexEdge) {
            if (simplexEdge != null) {
                simplexEdge.Clear();
                simplexEdgePool.Push(simplexEdge);
            }
        }

        #endregion

        #region Edge

        public static Edge GetEdgeFromPool() {
            if (edgePool.Count > 0) {
                return edgePool.Pop();
            } else {
                return new Edge();
            }
        }

        public static void RecycleEdge(List<Edge> edges) {
            if (edges != null) {
                for (int i = 0, count = edges.Count; i < count; i++) {
                    Edge edge = edges[i];
                    RecycleSupPoint(edge.a);
                    RecycleSupPoint(edge.b);
                    edge.a = null;
                    edge.b = null;
                    edgePool.Push(edge);
                }

                edges.Clear();
            }
        }

        #endregion
    }
}