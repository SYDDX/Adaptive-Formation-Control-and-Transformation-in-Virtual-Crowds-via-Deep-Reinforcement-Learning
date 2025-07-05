using System.Collections.Generic;
using System;

namespace RRT
{
    /**
     * <summary>Defines k-D trees for agents and static obstacles in the
     * simulation.</summary>
     */

    public class SampleSearcher
    {
        public List<float[]> obs_rect_pos_;
        public List<float[]> obs_circ_pos_;
        public List<float[]> boundary_;
        private float delta = (float)1.5f;

        // pos1、pos2:节点
        // obs_rect_pos_、obs_circ_pos_:障碍物位置列表
        //boundary_:边界列表
        public bool isCollision(float[] pos1, float[] pos2)
        {
            if (isInsideObs(pos1) || isInsideObs(pos2))
            {
                return true;
            }
            if (obs_rect_pos_.Count != 0)
            {
                foreach (float[] rect_pos in obs_rect_pos_)
                {
                    if (isInterRect(pos1, pos2, rect_pos))
                    {
                        return true;
                    }
                }
            }
            if (obs_circ_pos_.Count != 0)
            {
                foreach (float[] circ_pos in obs_circ_pos_)
                {
                    if (isInterCirc(pos1, pos2, circ_pos))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        internal bool isInsideObs(float[] pos)
        {
            float x = pos[0];
            float y = pos[1];
            if (obs_rect_pos_.Count != 0)
            {
                foreach (float[] rect_pos in obs_rect_pos_)
                {
                    if ((0 <= x - rect_pos[0] + delta) && (x - rect_pos[0] + delta <= rect_pos[2] +  delta)
                            && (0 <= y - rect_pos[1] + delta) && (y - rect_pos[1] + delta <= rect_pos[3] +  delta))
                    { return true; }
                }
            }
            if (obs_circ_pos_.Count != 0)
            {
                foreach (float[] circ_pos in obs_circ_pos_)
                {
                    if (Math.Sqrt((x - circ_pos[0]) * (x - circ_pos[0]) + (y - circ_pos[1]) * (y - circ_pos[1])) <= circ_pos[2] + delta)
                    { return true; }
                }
            }
            if (boundary_.Count != 0)
            {
                foreach (float[] boun in boundary_)
                {
                    if ((0 <= x - (boun[0] - delta)) && (x - (boun[0] - delta) <= boun[2] + 2 * delta) && (0 <= y - (boun[1] - delta)) && (y - (boun[1] - delta) <= boun[3] + 2 * delta))
                    { return true; }
                }
            }
            return false;
        }

        internal bool isInterRect(float[] pos1, float[] pos2, float[] rect_pos)
        {
            float ox = rect_pos[0];
            float oy = rect_pos[1];
            float w = rect_pos[2];
            float h = rect_pos[3];
            List<float[]> vertex = new List<float[]>()
                        {
                            new float[] { ox - w/2-delta, oy -h/2- delta },
                            new float[] { ox + w/2 + delta, oy -h/2- delta },
                            new float[] { ox + w/2 + delta, oy + h/2 + delta },
                            new float[] { ox -w/2- delta, oy + h / 2 + delta }
                        };

            float x1 = pos1[0];
            float y1 = pos1[1];
            float x2 = pos2[0];
            float y2 = pos2[1];

            for (int i = 0; i < vertex.Count; i++)
            {
                for (int j = i + 1; j < vertex.Count; j++)
                {
                    float[] v1 = vertex[i];
                    float[] v2 = vertex[j];
                    if (Math.Max(x1, x2) >= Math.Min(v1[0], v2[0]) &&
                        Math.Min(x1, x2) <= Math.Max(v1[0], v2[0]) &&
                        Math.Max(y1, y2) >= Math.Min(v1[1], v2[1]) &&
                        Math.Min(y1, y2) <= Math.Max(v1[1], v2[1]))
                    {
                        float cross1 = CrossProduct(v1, v2, pos1);
                        float cross2 = CrossProduct(v1, v2, pos2);
                        float cross3 = CrossProduct(pos1, pos2, v1);
                        float cross4 = CrossProduct(pos1, pos2, v2);
                        if (cross1 * cross2 <= 0 && cross3 * cross4 <= 0)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }


        internal bool isInterCirc(float[] pos1, float[] pos2, float[] circ_pos)
        {
            float ox = circ_pos[0];
            float oy = circ_pos[1];
            float r = circ_pos[2];

            float x = pos1[0];
            float y = pos1[1];

            float dx = pos2[0] - pos1[0];
            float dy = pos2[1] - pos1[1];
            float d2 = dx * dx + dy * dy;

            if (d2 == 0)
            {
                return false;
            }

            float t = ((ox - x) * dx + (oy - y) * dy) / d2;
            if (t >= 0 && t <= 1)
            {
                float sx = x + t * dx;
                float sy = y + t * dy;
                float distance_squared = (ox - sx) * (ox - sx) + (oy - sy) * (oy - sy);
                if (distance_squared <= (r + delta) * (r + delta))
                {
                    return true;
                }
            }

            return false;

        }

        internal float distance_to_rect(float[] pos, float[] rect_pos)
        {
            float x = pos[0]; float y = pos[1];
            float ox = rect_pos[0]; float oy = rect_pos[1]; float w = rect_pos[2]; float h = rect_pos[3];
            if (x >= ox && x <= ox + w && y >= oy && y <= oy + h)
            {
                return 0;
            }

            float distance_x = Math.Max(ox - x, Math.Max(0, x - (ox + w)));
            float distance_y = Math.Max(oy - y, Math.Max(0, y - (oy + h)));
            return (float)Math.Sqrt(distance_x * distance_x + distance_y * distance_y);
        }

        internal float distance_to_circ(float[] pos, float[] circ_pos)
        {
            float dx = pos[0] - circ_pos[0];
            float dy = pos[1] - circ_pos[1];
            float distance = (float)Math.Sqrt(dx * dx + dy * dy);
            // 判断点是否在圆内，如果在则返回0，否则返回距离
            if (distance <= circ_pos[2])
                return 0;
            else
                return distance - circ_pos[2];
        }

        public float no_collision_area(float[] pos)
        {
            float min_dist = float.PositiveInfinity;
            if (obs_circ_pos_.Count != 0)
            {
                foreach (float[] circ_pos in obs_circ_pos_)
                {
                    float dist = distance_to_circ(pos, circ_pos);
                    if (dist < min_dist)
                    {
                        min_dist = dist;
                    }
                }
            }

            if (obs_rect_pos_.Count != 0)
            {
                foreach (float[] rect_pos in obs_rect_pos_)
                {
                    float dist = distance_to_rect(pos, rect_pos);
                    if (dist < min_dist)
                    {
                        min_dist = dist;
                    }
                }
            }

            return min_dist;
        }

        //static bool IsEmptyArray(List<List<float>> array)
        //{
        //        return array.Count == 0;
        //}

        internal float CrossProduct(float[] p1, float[] p2, float[] p3)
        {
            float x1 = p2[0] - p1[0];
            float y1 = p2[1] - p1[1];
            float x2 = p3[0] - p1[0];
            float y2 = p3[1] - p1[1];
            return x1 * y2 - x2 * y1;
        }
    }
}
