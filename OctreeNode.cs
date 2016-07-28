using UnityEngine;
using System.Collections.Generic;
using SLua;

namespace BnH {
    [CustomLuaClass]
    public class OctreeNode {
        public enum Side {
            In,
            Out,
            Cross
        }

        public Octree Owner;
        public OctreeNode Parent;
        public bool isLeaf;

        public Bounds AABB;

        // 8个孩子
        public OctreeNode[] Childs;
        // 包括孩子节点
        public int TotalSceneObjs = 0;

        public List<SceneObj> SceneObjs;

        public OctreeNode(Octree tree, OctreeNode parent, Vector3 center, Vector3 size) {
            Owner = tree;
            Parent = parent;
            AABB = new Bounds(center, size);
        }

        public bool isDoubleSize(Bounds other) {
            return AABB.extents.x >= other.size.x && AABB.extents.y >= other.size.y && AABB.extents.z >= other.size.z;
        }

        public static Vector3 GetVectexP(ref Bounds bound, Vector3 normal) {
            return new Vector3(
                normal.x < 0 ? bound.min.x : bound.max.x,
                normal.y < 0 ? bound.min.y : bound.max.y,
                normal.z < 0 ? bound.min.z : bound.max.z
                );
        }

        public static Vector3 GetVectexN(ref Bounds bound, Vector3 normal) {
            return new Vector3(
                normal.x > 0 ? bound.min.x : bound.max.x,
                normal.y > 0 ? bound.min.y : bound.max.y,
                normal.z > 0 ? bound.min.z : bound.max.z
                );
        }

        public static Side IsInSide(ref Plane plane, ref Bounds bound) {
            //最远的点也在背面
            if (!plane.GetSide(GetVectexP(ref bound, plane.normal))) {
                return Side.Out;
            }
            if (plane.GetSide(GetVectexN(ref bound, plane.normal))) {
                return Side.In;
            }
            return Side.Cross;
        }

        public static Side IsInSide(ref Frustum frustum, ref Bounds bound) {
            var planes = frustum.Planes;
            bool hasCross = false;
            for (int i = 0; i < 4; ++i) {
                var side = IsInSide(ref planes[i], ref bound);
                if (side == Side.Out) {
                    return side;
                }else if (side == Side.Cross) {
                    hasCross = true;
                }
            }
            return hasCross ? Side.Cross : Side.In;
        }

        public OctreeNode GetFitNode(Bounds other) {
            Vector3 otherMin = other.min;
            Vector3 otherMax = other.max;

            Vector3 min = AABB.min;
            Vector3 max = AABB.max;
            Vector3 center = AABB.center;

            if (otherMin.x <= min.x || otherMin.y <= min.y || otherMin.z <= min.z ||
                otherMax.x >= max.x || otherMax.y >= max.y || otherMax.z >= max.z) {
                Debug.LogError("Enter Here, Why " + other + " , " + AABB);
                return this;
            }

            Plane[] splitPlanes = new Plane[3] {
                new Plane(Vector3.right, center),
                new Plane(Vector3.up, center),
                new Plane(Vector3.forward, center),
            };

            bool[] collideResult = new bool[3];
            int childIndex = 0;

            for (int i = 0; i < 3; ++i) {

                var side = IsInSide(ref splitPlanes[i], ref other);
                if (side == Side.Cross) {
                    return this;
                } else {
                    collideResult[i] = side == Side.In;
                    childIndex += ((collideResult[i] ? 1 : 0) << i);
                }
            }
            if (Childs == null) {
                Childs = new OctreeNode[8];
            }
            var child = Childs[childIndex];
            if (child == null) {
                Vector3 extents = AABB.extents;
                Vector3 newCenter = new Vector3(
                        (collideResult[0] ? 1 : -1) * extents.x/2 + center.x,
                        (collideResult[1] ? 1 : -1) * extents.y/2 + center.y,
                        (collideResult[2] ? 1 : -1) * extents.z/2 + center.z
                    );
                Childs[childIndex] = child = new OctreeNode(Owner, this, newCenter, extents);
            }
            return child;
        }

        public void AddSceneObj(SceneObj obj) {
            if (SceneObjs == null) SceneObjs = new List<SceneObj>();
            SceneObjs.Add(obj);
            obj.Parent = this;
            IncTotalSceneObjCount();
        }

        public void RemoveSceneObj(SceneObj obj) {
            SceneObjs.Remove(obj);
            obj.Parent = null;
            DecTotalSceneObjCount();
        }

        public void IncTotalSceneObjCount() {
            TotalSceneObjs++;
            if (Parent != null) Parent.IncTotalSceneObjCount();
        }

        public void DecTotalSceneObjCount() {
            TotalSceneObjs--;
            if (Parent != null) Parent.DecTotalSceneObjCount();
        }
    }
}