using System.Collections.Generic;
using Physics.Collision;
using Physics.Collision.Model;

namespace Physics {
    public static class PhysicsCachePool {
        public static Stack<CollisionPair> collisionPairPool = new Stack<CollisionPair>();
        public static Stack<SimplexEdge> simplexEdgePool = new Stack<SimplexEdge>();
        public static Stack<Edge> edgePool = new Stack<Edge>();

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
                    edgePool.Push(edge);
                }

                edges.Clear();
            }
        }

        #endregion
    }
}