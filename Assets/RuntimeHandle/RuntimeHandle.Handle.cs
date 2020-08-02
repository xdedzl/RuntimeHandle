using UnityEngine;
using System;

namespace xdedzl.RuntimeHandle
{
    public partial class RuntimeHandle
    {
        /// <summary>
        /// 手柄基类
        /// </summary>
        public class BaseHandle
        {
            protected Camera camera { get { return handle.camera; } }
            protected Transform target { get { return handle.m_Target; } }
            protected float screenScale { get { return handle.m_ScreenScale; } }
            protected Matrix4x4 localToWorld { get { return handle.m_LocalToWorld; } }

            protected float colliderPixel = 10;  // 鼠标距离轴多少时算有碰撞（单位：像素）

            protected RuntimeHandle handle;

            public BaseHandle(RuntimeHandle handle)
            {
                this.handle = handle;
            }

            public virtual float GetTransformAxis(Vector2 inputDir, Vector3 axis) { return 0; }
            public virtual void Transform(Vector3 value) { }

            /// <summary>
            /// 最基本的碰撞选择
            /// </summary>
            public virtual RuntimeHandleAxis SelectedAxis()
            {
                float distanceX, distanceY, distanceZ;
                bool hit = HitAxis(Vector3.right, out distanceX);
                hit |= HitAxis(Vector3.up, out distanceY);
                hit |= HitAxis(Vector3.forward, out distanceZ);

                if (hit)
                {
                    if (distanceX < distanceY && distanceX < distanceZ)
                    {
                        return RuntimeHandleAxis.X;
                    }
                    else if (distanceY < distanceZ)
                    {
                        return RuntimeHandleAxis.Y;
                    }
                    else
                    {
                        return RuntimeHandleAxis.Z;
                    }
                }

                return RuntimeHandleAxis.None;
            }

            /// <summary>
            /// 是否和手柄有碰撞
            /// </summary>
            /// <param name="axis"></param>
            /// <param name="localToWorlad">手柄坐标系转换矩阵</param>
            /// <param name="distanceAxis"></param>
            /// <returns></returns>
            public bool HitAxis(Vector3 axis, out float distanceToAxis)
            {
                // 把坐标轴本地坐标转为世界坐标
                axis = localToWorld.MultiplyPoint(axis);

                // 坐标轴转屏幕坐标(有问题)
                Vector2 screenVectorBegin = camera.WorldToScreenPoint(target.position);
                Vector2 screenVectorEnd = camera.WorldToScreenPoint(axis);
                Vector2 screenVector = screenVectorEnd - screenVectorBegin;
                float screenVectorMag = screenVector.magnitude;
                screenVector.Normalize();

                if (screenVector != Vector2.zero)
                {
                    Vector2 perp = PerpendicularClockwise(screenVector).normalized;
                    Vector2 mousePosition = Input.mousePosition;
                    Vector2 relMousePositon = mousePosition - screenVectorBegin;    // 鼠标相对轴远点位置
                    distanceToAxis = Mathf.Abs(Vector2.Dot(perp, relMousePositon)); // 在屏幕坐标系中，鼠标到轴的距离

                    Vector2 hitPoint = (relMousePositon - perp * distanceToAxis);
                    float vectorSpaceCoord = Vector2.Dot(screenVector, hitPoint);

                    bool result = vectorSpaceCoord >= 0 && hitPoint.magnitude <= screenVectorMag && distanceToAxis < colliderPixel;
                    return result;
                }
                else  // 坐标轴正对屏幕
                {
                    Vector2 mousePosition = Input.mousePosition;

                    distanceToAxis = (screenVectorBegin - mousePosition).magnitude;
                    bool result = distanceToAxis <= colliderPixel;
                    if (!result)
                    {
                        distanceToAxis = float.PositiveInfinity;
                    }
                    else
                    {
                        distanceToAxis = 0.0f;
                    }
                    return result;
                }
            }

            /// <summary>
            /// 获取顺时针的垂直向量
            /// </summary>
            protected Vector2 PerpendicularClockwise(Vector2 vector2)
            {
                return new Vector2(-vector2.y, vector2.x);
            }

        }

        /// <summary>
        /// 位置修改手柄
        /// </summary>
        public class PositionHandle : BaseHandle
        {
            public PositionHandle(RuntimeHandle handle) : base(handle) { }

            /// <summary>
            /// 返回鼠标和手柄的碰撞信息
            /// </summary>
            public override RuntimeHandleAxis SelectedAxis()
            {
                float scale = screenScale/* * 0.2f*/;
                // TODO 方块的位置是会变化的
                if (HitQuad(target.position, target.right, target.up, camera))
                {
                    return RuntimeHandleAxis.XY;
                }
                else if (HitQuad(target.position, target.right, target.forward, camera))
                {
                    return RuntimeHandleAxis.XZ;
                }
                else if (HitQuad(target.position, target.up, target.forward, camera))
                {
                    return RuntimeHandleAxis.YZ;
                }

                return base.SelectedAxis();
            }

            public override float GetTransformAxis(Vector2 inputDir, Vector3 axis)
            {
                Vector2 screenStart = camera.WorldToScreenPoint(target.position);
                Vector2 screenEnd = camera.WorldToScreenPoint(target.position + axis);
                Vector2 screenDir = (screenEnd - screenStart).normalized;

                return Vector2.Dot(screenDir, inputDir);
            }

            public override void Transform(Vector3 value)
            {
                target.Translate(value * Time.deltaTime * 20, Space.Self);
            }

            /// <summary>
            /// 是否和小方块有碰撞
            /// </summary>
            /// <param name="origin">方块左下角</param>
            /// <param name="offset"></param>
            /// <param name="camera"></param>
            /// <returns></returns>
            private bool HitQuad(Vector3 origin, Vector3 dir0, Vector3 dir1, Camera camera)
            {
                Vector3 pos1 = origin + dir0;
                Vector3 pos2 = origin + dir0 + dir1;
                Vector3 pos3 = origin + dir1;

                Vector2 mousePos = Input.mousePosition;
                Vector2 screenOrigin = camera.WorldToScreenPoint(origin);
                Vector2 screenPos1 = camera.WorldToScreenPoint(pos1);
                Vector2 screenPos2 = camera.WorldToScreenPoint(pos2);
                Vector2 screenPos3 = camera.WorldToScreenPoint(pos3);


                //PhysicsMath.IsPointInsidePolygon

                //if (mousePos.x > Mathf.Max(screenOrigin.x, screenOffset.x) ||
                //    mousePos.x < Mathf.Min(screenOrigin.x, screenOffset.x) ||
                //    mousePos.y > Mathf.Max(screenOrigin.y, screenOffset.y) ||
                //    mousePos.y < Mathf.Min(screenOrigin.y, screenOffset.y))
                //    return false;
                //else
                //    return true;

                return IsPointInsidePolygon(mousePos, new Vector2[] { screenOrigin, screenPos1, screenPos2, screenPos3 });
            }
        }

        /// <summary>
        /// 角度修改手柄
        /// </summary>
        public class RotationHandle : BaseHandle
        {
            public RotationHandle(RuntimeHandle handle) : base(handle) { }
            public override RuntimeHandleAxis SelectedAxis()
            {
                float distanceX, distanceY, distanceZ;
                Vector2 mousePos = Input.mousePosition;
                bool hit = HitCircle(handle.circlePosX, mousePos, out distanceX);
                hit |= HitCircle(handle.circlePosY, mousePos, out distanceY);
                hit |= HitCircle(handle.circlePosZ, mousePos, out distanceZ);

                if (hit)
                {
                    if (distanceX < distanceY && distanceX < distanceZ)
                    {
                        return RuntimeHandleAxis.X;
                    }
                    else if (distanceY < distanceZ)
                    {
                        return RuntimeHandleAxis.Y;
                    }
                    else
                    {
                        return RuntimeHandleAxis.Z;
                    }
                }

                return RuntimeHandleAxis.None;
            }

            public override float GetTransformAxis(Vector2 inputDir, Vector3 axis)
            {
                Vector2 screenStart = camera.WorldToScreenPoint(target.position);
                Vector2 screenEnd = camera.WorldToScreenPoint(target.position + axis);
                Vector2 screenDir = PerpendicularClockwise((screenEnd - screenStart)).normalized;

                return Vector2.Dot(screenDir, inputDir);
            }

            public override void Transform(Vector3 value)
            {
                target.Rotate(value, Space.Self);
            }

            private bool HitCircle(Vector3[] circlePos, Vector2 mousePos, out float distance)
            {
                distance = 1000;
                if (circlePos == null)
                    return false;
                for (int i = 0, length = circlePos.Length; i < length; i++)
                {
                    Vector3 worldPos = localToWorld.MultiplyPoint(circlePos[i]);
                    Vector2 screenPos = camera.WorldToScreenPoint(worldPos);
                    float tempDis = Vector2.Distance(mousePos, screenPos);
                    if (tempDis < distance)
                        distance = tempDis;
                }
                if (distance < 10)
                    return true;
                else
                    return false;
            }
        }

        /// <summary>
        /// 比例修改手柄
        /// </summary>
        public class ScaleHandle : BaseHandle
        {
            public ScaleHandle(RuntimeHandle handle) : base(handle) { }
            public override RuntimeHandleAxis SelectedAxis()
            {
                if (HitCube(target.position, camera))
                    return RuntimeHandleAxis.XYZ;

                return base.SelectedAxis();
            }

            public override float GetTransformAxis(Vector2 inputDir, Vector3 axis)
            {
                Vector2 screenStart = camera.WorldToScreenPoint(target.position);
                Vector2 screenEnd = camera.WorldToScreenPoint(target.position + axis);
                Vector2 screenDir = (screenEnd - screenStart).normalized;

                return Vector2.Dot(screenDir, inputDir);
            }

            public override void Transform(Vector3 value)
            {
                target.localScale += value * Time.deltaTime;
            }

            /// <summary>
            /// 与中心方块的碰撞
            /// 不需要很精确，只要鼠标位置和方块位置在一定的像素差之内就算碰撞到了
            /// </summary>
            private bool HitCube(Vector3 position, Camera camera)
            {
                Vector2 mousePos = Input.mousePosition;
                Vector2 screenPos = camera.WorldToScreenPoint(position);

                if (Mathf.Abs((mousePos.x - screenPos.x) * (mousePos.y - screenPos.y)) < 10)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// 判断点是否在多边形区域内
        /// </summary>
        /// <param name="p">待判断的点，格式：{ x: X坐标, y: Y坐标 }</param>
        /// <param name="poly">多边形顶点，数组成员的格式同</param>
        /// <returns>true:在多边形内，凹点   false：在多边形外，凸点</returns>
        public static bool IsPointInsidePolygon(Vector2 p, Vector2[] poly, bool containEdge = true)
        {
            float px = p.x;
            float py = p.y;
            double sum = 0;

            for (int i = 0, j = poly.Length - 1; i < poly.Length; j = i, i++)
            {
                float sx = poly[i].x;
                float sy = poly[i].y;
                float tx = poly[j].x;
                float ty = poly[j].y;

                // 点与多边形顶点重合或在多边形的边上(这个判断有些情况不需要)
                if ((sx - px) * (px - tx) >= 0 && (sy - py) * (py - ty) >= 0 && (px - sx) * (ty - sy) == (py - sy) * (tx - sx))
                {
                    return containEdge;
                }

                // 点与相邻顶点连线的夹角
                var angle = Mathf.Atan2(sy - py, sx - px) - Math.Atan2(ty - py, tx - px);

                // 确保夹角不超出取值范围（-π 到 π）
                if (angle >= Mathf.PI)
                {
                    angle = angle - Mathf.PI * 2;
                }
                else if (angle <= -Mathf.PI)
                {
                    angle = angle + Mathf.PI * 2;
                }

                sum += angle;
            }

            // 计算回转数并判断点和多边形的几何关系
            return Mathf.RoundToInt((float)(sum / Math.PI)) != 0;
        }
    }
}