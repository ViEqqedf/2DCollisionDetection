using System.Collections.Generic;
using CustomPhysics.Collision.Model;

namespace CustomPhysics.Tool {
    public static class PhysicsCachePool {
        public static Stack<SimplexEdge> simplexEdgePool = new Stack<SimplexEdge>();
        public static Stack<Edge> edgePool = new Stack<Edge>();

        private static int simplexEdgeCount = 0;
        private static int edgeCount = 0;

        #region SimplexEdge

        public static SimplexEdge GetSimplexEdgeFromPool() {
            if (simplexEdgeCount > 0) {
                simplexEdgeCount--;
                return simplexEdgePool.Pop();
            } else {
                return new SimplexEdge();
            }
        }

        public static void RecycleSimplexEdge(SimplexEdge simplexEdge) {
            if (simplexEdge != null) {
                simplexEdge.Clear();
                simplexEdgePool.Push(simplexEdge);
                simplexEdgeCount++;
            }
        }

        #endregion

        #region Edge

        public static Edge GetEdgeFromPool() {
            if (edgeCount > 0) {
                edgeCount--;
                return edgePool.Pop();
            } else {
                return new Edge();
            }
        }

        public static void RecycleEdge(Edge edge) {
            if (edge != null) {
                edgePool.Push(edge);
                edgeCount++;
            }
        }

        public static void RecycleEdge(List<Edge> edges) {
            if (edges != null) {
                for (int i = 0, count = edges.Count; i < count; i++) {
                    Edge edge = edges[i];
                    edgePool.Push(edge);
                    edgeCount++;
                }

                edges.Clear();
            }
        }

        #endregion
    }
}