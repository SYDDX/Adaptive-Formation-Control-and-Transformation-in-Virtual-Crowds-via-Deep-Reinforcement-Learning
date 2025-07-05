using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;

namespace RRT
{
    /**
     * <summary>Defines k-D trees for agents and static obstacles in the
     * simulation.</summary>
     **/
    public struct Node
    {
        public int parent;
        public float cost;
        public float x;
        public float y;
    }

    public class RRT_Planner
    {
        //public LineRenderer lineRenderer;
        //public GameObject samplePrefab;
        public SampleSearcher samplesearcher = new SampleSearcher();
        public Node start = new Node();
        public Node goal = new Node();
        public List<Node> node_list = new List<Node>();
        public float expandDis = (float)4.0;
        public int goal_sample_rate = 50;
        public int max_iter = 7500;
        public float delta = (float)3f;
        public float w_w_ = 25/2f;
        public float w_l_ = 60/2f;


        public List<float[]> rrt_planning()
        {
            node_list.Add(start);
            List<float[]> path = new List<float[]>();
            List<float[]> smooth_path = new List<float[]>();
            List<float[]> new_smooth_path = new List<float[]>();
            for (int i = 0; i < max_iter; i++)
            {
                if (i == max_iter - 1)
                    Debug.Log("Last iter! ");
                Node rnd = sample();
                //Node rnd = OptimizedSample();
                //Debug.Log("rnd.x:", rnd.x);
                //Debug.Log("rnd.y:", rnd.y);
                int n_ind = get_nearest_list_index(node_list, rnd);
                Node nearestNode = node_list[n_ind];
                float theta = (float)Math.Atan2(rnd.y - nearestNode.y, rnd.x - nearestNode.x);
                //float theta2 = math.atan2(self.goal.y - nearestNode.y, self.goal.x - nearestNode.x)
                float[] nearestNode_ = new float[2]; nearestNode_[0] = nearestNode.x; nearestNode_[1] = nearestNode.y;
                float[] goal_ = new float[2]; goal_[0] = goal.x; goal_[1] = goal.y;
                Node newNode = get_new_node(theta, n_ind, nearestNode);
                float[] newNode_ = new float[2]; newNode_[0] = newNode.x; newNode_[1] = newNode.y;
                if ((samplesearcher.isCollision(newNode_, nearestNode_)) == false)
                {
                    //Debug.Log("No Collision!");

                    node_list.Add(newNode);
                    Debug.Log("node_list.Count: " + node_list.Count);
                    if (is_near_goal(newNode))
                    {
                        Debug.Log("is_near_goal! ");
                        if ((samplesearcher.isCollision(newNode_, goal_)) == false)
                        {
                            int lastIndex = node_list.Count - 1;
                            path = get_final_course(lastIndex);
                            smooth_path = path_smooth(path);

                            smooth_path.Reverse();
                            //for (int j = 0; j < smooth_path.Count - 2; j++)
                            //{
                            //        if (smooth_path[j][0] == smooth_path[j + 2][0])
                            //        {
                            //                for (int k = j; k < smooth_path.Count - 2; k++)
                            //                {
                            //                        smooth_path[k] = smooth_path[k + 2];
                            //                }
                            //                smooth_path.RemoveAt(smooth_path.Count - 1);
                            //                smooth_path.RemoveAt(smooth_path.Count - 1);
                            //        }
                            //}
                            //Debug.Log("find path!! ");
                            Debug.Log("smooth_path.Count: " + path.Count);

                            return smooth_path;
                        }
                    }
                }
            }
            return new List<float[]>();
        }

        public List<float[]> rrt_planning_new()
        {
            node_list.Add(start);
            List<Node> path = new List<Node>();
            List<Node> smooth_path = new List<Node>();
            for (int i = 0; i < max_iter; i++)
            {
                if (i == max_iter - 1)
                    Debug.Log("Last iter! ");
                Node rnd = sample();
                //Debug.Log("rnd.x:", rnd.x);
                //Debug.Log("rnd.y:", rnd.y);
                int n_ind = get_nearest_list_index(node_list, rnd);
                Node nearestNode = node_list[n_ind];
                float theta = (float)Math.Atan2(rnd.y - nearestNode.y, rnd.x - nearestNode.x);
                Node newNode = get_new_node(theta, n_ind, nearestNode);
                float[] newNode_ = new float[2]; newNode_[0] = newNode.x; newNode_[1] = newNode.y;
                float[] nearestNode_ = new float[2]; nearestNode_[0] = nearestNode.x; nearestNode_[1] = nearestNode.y;
                float[] goal_ = new float[2]; goal_[0] = goal.x; goal_[1] = goal.y;
                if (!(samplesearcher.isCollision(newNode_, nearestNode_)))
                {
                    Debug.Log("No Collision!");

                    node_list.Add(newNode);
                    Debug.Log("node_list.Count: " + node_list.Count);

                    if (is_near_goal(newNode))
                    {
                        Debug.Log("is_near_goal! ");
                        if (!(samplesearcher.isCollision(newNode_, goal_)))
                        {
                            int lastIndex = node_list.Count - 1;
                            //                        path = get_final_course(lastIndex);
                            //                        smooth_path = path_smooth(path);
                        }
                    }
                }
            }
            //lineRenderer.positionCount = node_list.Count;
            //for (int i = 0; i < node_list.Count; i++)
            //{
            //        lineRenderer.SetPosition(i, new Vector3 (node_list[i].x, 0, node_list[i].y ));
            //}
            Debug.Log("smooth_path.Count: " + path.Count);
            List<float[]> new_smooth_path = new List<float[]>();
            for (int i = 0; i < smooth_path.Count; i++)
            {
                new_smooth_path.Add(new float[] { smooth_path[i].x, smooth_path[i].y });
                //new_smooth_path[i][0] = smooth_path[i].x;
                //new_smooth_path[i][1] = smooth_path[i].y;
            }
            new_smooth_path.Reverse();
            return new_smooth_path;
        }

        public List<float[]> rrt_star_planning_one()
        {
            node_list.Add(start);
            List<float[]> path = new List<float[]>();
            List<float[]> smooth_path = new List<float[]>();
            List<float[]> new_smooth_path = new List<float[]>();
            for (int i = 0; i < max_iter; i++)
            {
                if (i == max_iter - 1)
                    Debug.Log("Last iter! ");
                //Node rnd = OptimizedSample();
                Node rnd = sample();
                //Debug.Log("rnd.x:", rnd.x);
                //Debug.Log("rnd.y:", rnd.y);
                int n_ind = get_nearest_list_index(node_list, rnd);
                Node nearestNode = node_list[n_ind];
                float theta = (float)Math.Atan2(rnd.y - nearestNode.y, rnd.x - nearestNode.x);
                float theta2 = (float)Math.Atan2(goal.y - nearestNode.y, goal.x - nearestNode.x);

                Node newNode = get_new_node_Optimize(theta, theta2, n_ind, nearestNode);
                //Node newNode = get_new_node(theta, n_ind, nearestNode);
                float[] nearestNode_ = new float[2]; nearestNode_[0] = nearestNode.x; nearestNode_[1] = nearestNode.y;
                float[] goal_ = new float[2]; goal_[0] = goal.x; goal_[1] = goal.y;
                float[] newNode_ = new float[2]; newNode_[0] = newNode.x; newNode_[1] = newNode.y;
                if ((samplesearcher.isCollision(newNode_, nearestNode_)) == false)
                {
                    List<int> nearInds = find_near_nodes(newNode);
                    newNode = choose_parent(newNode, nearInds);
                    //Debug.Log("No Collision!");

                    node_list.Add(newNode);
                    rewire(newNode, nearInds);
                    //Debug.Log("node_list.Count: " + node_list.Count);

                    if (is_near_goal(newNode))
                    {
                        Debug.Log("is_near_goal! ");
                        newNode_[0] = newNode.x; newNode_[1] = newNode.y;
                        if ((samplesearcher.isCollision(newNode_, goal_)) == false)
                        {
                            int lastIndex = node_list.Count - 1;
                            path = get_final_course(lastIndex);
                            smooth_path = path_smooth(path);

                            smooth_path.Reverse();
                            for (int j = 0; j < smooth_path.Count - 2; j++)
                            {
                                if (smooth_path[j][0] == smooth_path[j + 2][0] && smooth_path[j][1] == smooth_path[j + 2][1])
                                {
                                    for (int k = j; k < smooth_path.Count - 2; k++)
                                    {
                                        smooth_path[k] = smooth_path[k + 2];
                                    }
                                    smooth_path.RemoveAt(smooth_path.Count - 1);
                                    smooth_path.RemoveAt(smooth_path.Count - 1);
                                }
                            }
                            //Debug.Log("find path!! ");
                            Debug.Log("smooth_path.Count: " + path.Count);

                            return smooth_path;
                        }
                    }
                }
            }
            List<float[]> node_list_ = new List<float[]>();
            for (int i = 0; i < node_list.Count; i++)
            {
                node_list_.Add(new float[] { node_list[i].x, node_list[i].y });
            }
            return node_list_;
        }

        public List<float[]> rrt_star_planning_best()
        {
            node_list.Add(start);
            List<float[]> path = new List<float[]>();
            List<float[]> smooth_path = new List<float[]>();
            List<float[]> new_smooth_path = new List<float[]>();
            float pathLen = float.PositiveInfinity;
            for (int i = 0; i < max_iter; i++)
            {
                if (i == max_iter - 1)
                    Debug.Log("Last iter! ");
                //Node rnd = OptimizedSample();
                Node rnd = sample();
                //Debug.Log("rnd.x:", rnd.x);
                //Debug.Log("rnd.y:", rnd.y);
                int n_ind = get_nearest_list_index(node_list, rnd);
                Node nearestNode = node_list[n_ind];
                float theta = (float)Math.Atan2(rnd.y - nearestNode.y, rnd.x - nearestNode.x);
                float theta2 = (float)Math.Atan2(goal.y - nearestNode.y, goal.x - nearestNode.x);

                Node newNode = get_new_node_Optimize(theta, theta2, n_ind, nearestNode);
                //Node newNode = get_new_node(theta, n_ind, nearestNode);
                float[] nearestNode_ = new float[2]; nearestNode_[0] = nearestNode.x; nearestNode_[1] = nearestNode.y;
                float[] goal_ = new float[2]; goal_[0] = goal.x; goal_[1] = goal.y;
                float[] newNode_ = new float[2]; newNode_[0] = newNode.x; newNode_[1] = newNode.y;
                if ((samplesearcher.isCollision(newNode_, nearestNode_)) == false)
                {
                    List<int> nearInds = find_near_nodes(newNode);
                    newNode = choose_parent(newNode, nearInds);
                    //Debug.Log("No Collision!");

                    node_list.Add(newNode);
                    rewire(newNode, nearInds);
                    //Debug.Log("node_list.Count: " + node_list.Count);

                    if (is_near_goal(newNode))
                    {
                        //Debug.Log("is_near_goal! ");
                        newNode_[0] = newNode.x; newNode_[1] = newNode.y;
                        if ((samplesearcher.isCollision(newNode_, goal_)) == false)
                        {
                            int lastIndex = node_list.Count - 1;
                            List<float[]> temp_path = get_final_course(lastIndex);

                            //for (int j = 0; j < temp_path.Count - 2; j++)
                            //{
                            //    if (temp_path[j][0] == temp_path[j + 2][0] && temp_path[j][1] == temp_path[j + 2][1])
                            //    {
                            //        for (int k = j; k < temp_path.Count - 2; k++)
                            //        {
                            //            temp_path[k] = temp_path[k + 2];
                            //        }
                            //        temp_path.RemoveAt(temp_path.Count - 1);
                            //        temp_path.RemoveAt(temp_path.Count - 1);
                            //    }
                            //}
                            // 光滑
                            float temp_path_len = get_path_len(temp_path);
                            if (temp_path_len < pathLen)
                            {
                                path = temp_path;
                                smooth_path = path_smooth(path);
                                pathLen = temp_path_len;
                            }
                        }
                    }
                }
            }
            //for (int j = 0; j < smooth_path.Count - 2; j++)
            //{
            //    if (smooth_path[j][0] == smooth_path[j + 2][0] && smooth_path[j][1] == smooth_path[j + 2][1])
            //    {
            //        for (int k = j; k < smooth_path.Count - 2; k++)
            //        {
            //            smooth_path[k] = smooth_path[k + 2];
            //        }
            //        smooth_path.RemoveAt(smooth_path.Count - 1);
            //        smooth_path.RemoveAt(smooth_path.Count - 1);
            //    }
            //}
            //Debug.Log("find path!! ");
            Debug.Log("smooth_path.Count: " + path.Count);
            smooth_path.Reverse();
            return smooth_path;
            //List<float[]> node_list_ = new List<float[]>();
            //for (int i = 0; i < node_list.Count; i++)
            //{
            //        node_list_.Add(new float[] { node_list[i].x, node_list[i].y });
            //}
            //return node_list_;
        }

        public Node sample()
        {
            Node rnd = new Node();
            if (UnityEngine.Random.Range(0, 100) > goal_sample_rate)
            {
                rnd.x = UnityEngine.Random.Range(-w_w_ + delta, w_w_ - delta);
                rnd.y = UnityEngine.Random.Range(-w_l_ + delta, w_l_ - delta);
            }
            else
            {
                rnd.x = goal.x;
                rnd.y = goal.y;
            }
            return rnd;
        }

        public Node OptimizedSample()
        {
            float[] goal_ = new float[2]; goal_[0] = goal.x; goal_[1] = goal.y;
            float min_dist = samplesearcher.no_collision_area(goal_);
            Node rnd = new Node();
            if (UnityEngine.Random.Range(0, 100) > goal_sample_rate)
            {
                rnd.x = UnityEngine.Random.Range(-w_w_ + delta, w_w_ - delta);
                rnd.y = UnityEngine.Random.Range(-w_l_ + delta, w_l_ - delta);
                float dis = (float)Math.Sqrt(((rnd.x - goal.x) * (rnd.x - goal.x) + (rnd.y - goal.y) * (rnd.y - goal.y)));
                if (dis < min_dist)
                {
                    rnd.x = goal.x;
                    rnd.y = goal.y;
                }
            }
            else
            {
                rnd.x = goal.x;
                rnd.y = goal.y;
            }
            return rnd;
        }

        public int get_nearest_list_index(List<Node> nodes, Node rnd)
        {
            float dList = float.PositiveInfinity;
            int minIndex = 0;
            for (int i = 0; i < nodes.Count; i++)
            {
                float dis = (nodes[i].x - rnd.x) * (nodes[i].x - rnd.x) + (nodes[i].y - rnd.y) * (nodes[i].y - rnd.y);
                if (dList > dis)
                {
                    dList = dis;
                    minIndex = i;
                }
            }
            return minIndex;
        }

        // //KD树，实现太麻烦了
        //public int get_nearest_list_index_KD(float[] rnd)
        //{
        //        float dList = float.PositiveInfinity;
        //        int minIndex = 0;
        //        for (int i = 0; i < node_list.Count; i++)
        //        {
        //                float dis = (node_list[i][0] - rnd[0]) * (node_list[i][0] - rnd[0]) + (node_list[i][1] - rnd[1]) * (node_list[i][1] - rnd[1]);
        //                if (dList > dis)
        //                {
        //                        dList = dis;
        //                        minIndex = i;
        //                }
        //        }
        //        return minIndex;
        //}

        public float get_path_len(List<float[]> path)
        {
            float pathLen = 0.0f;
            for (int i = 1; i < path.Count; i++)
            {
                pathLen += (float)Math.Sqrt((path[i][0] - path[i - 1][0]) * (path[i][0] - path[i - 1][0]) + (path[i][1] - path[i - 1][1]) * (path[i][1] - path[i - 1][1]));
            }
            return pathLen;
        }

        public List<int> find_near_nodes(Node newNode)
        {
            int n_node = node_list.Count;
            //float r = (float)(10.0 * Math.Sqrt((Math.Log(n_node) / n_node)));
            float r = 1f;
            List<int> near_inds = new List<int>();

            for (int i = 0; i < node_list.Count; i++)
            {
                float dis = (node_list[i].x - newNode.x) * (node_list[i].x - newNode.x) + (node_list[i].y - newNode.y) * (node_list[i].y - newNode.y);
                if (dis <= r * r)
                {
                    near_inds.Add(i);
                }
            }
            return near_inds;
        }
        public bool check_collision(Node nearNode, float theta, float d)
        {
            Node tmpNode = nearNode;
            Node end = new Node();
            end.x = tmpNode.x + (float)Math.Cos(theta) * d;
            end.y = tmpNode.y + (float)Math.Sin(theta) * d;
            float[] tmpNode_ = new float[] { tmpNode.x, tmpNode.y };
            float[] end_ = new float[] { end.x, end.y };
            return (!samplesearcher.isCollision(tmpNode_, end_));
        }

        public void rewire(Node newNode, List<int> nearInds)
        {
            int n_node = node_list.Count;
            for (int i = 0; i < nearInds.Count; i++)
            {
                Node nearNode = node_list[i];
                float d = (float)Math.Sqrt((nearNode.x - newNode.x) * (nearNode.x - newNode.x) + (nearNode.y - newNode.y) * (nearNode.y - newNode.y));
                float s_cost = newNode.cost + d;
                if (nearNode.cost > s_cost)
                {
                    float theta = (float)Math.Atan2(newNode.y - nearNode.y, newNode.x - nearNode.x);
                    if (check_collision(nearNode, theta, d))
                    {
                        nearNode.parent = n_node - 1;
                        nearNode.cost = s_cost;
                    }
                }
            }
        }



        public Node choose_parent(Node newNode, List<int> nearInds)
        {
            if (nearInds.Count == 0)
                return newNode;
            List<float> dList = new List<float>();
            for (int i = 0; i < nearInds.Count; i++)
            {
                float dx = newNode.x - node_list[i].x;
                float dy = newNode.y - node_list[i].y;
                float d = (float)Math.Sqrt(dx * dx + dy * dy);
                float theta = (float)Math.Atan2(dy, dx);
                if (check_collision(node_list[i], theta, d))
                {
                    dList.Add(node_list[i].cost + d);
                }
                else
                    dList.Add(float.PositiveInfinity);
            }
            float minCost = dList.Min();
            int minInd = nearInds[dList.IndexOf(minCost)];
            if (minCost == float.PositiveInfinity)
                return newNode;
            newNode.cost = minCost;
            newNode.parent = minInd;
            return newNode;
        }

        public List<float[]> path_smooth(List<float[]> path)
        {
            for (int i = 2; i < path.Count - 1; i++)
            {
                float[] node1 = path[i - 1];
                float[] node2 = path[i];
                float[] node3 = path[i + 1];
                int insideAngle = get_angle(node1, node2, node3);
                if (insideAngle <= 150)
                {
                    List<float[]> point_list = new List<float[]>();
                    point_list.Add(node1); point_list.Add(node2); point_list.Add(node3);
                    List<float[]> point_list1 = getbezier(point_list);
                    if ((!(samplesearcher.isCollision(point_list1[0], point_list1[1]))) && (!(samplesearcher.isCollision(point_list1[1], point_list1[2]))))
                    {
                        node1[0] = point_list1[0][0]; node1[1] = point_list1[0][1];
                        node1[0] = point_list1[1][0]; node1[1] = point_list1[1][1];
                        node1[0] = point_list1[2][0]; node1[1] = point_list1[2][1];
                        path[i - 1] = node1;
                        path[i] = node2;
                        path[i + 1] = node3;
                    }
                }
            }
            return path;
        }

        public List<float[]> getbezier(List<float[]> point_list)
        {
            List<float[]> point_list1 = new List<float[]>();
            int n = 2;
            List<float[]> init_t = new List<float[]>();
            init_t.Add(new float[] { 1.0f, 1.0f, 1.0f });
            init_t.Add(new float[] { 0.0f, 0.5f, 1.0f });
            init_t.Add(new float[] { 0.0f, 0.25f, 1.0f });
            List<float[]> init_t1 = new List<float[]>();
            init_t1.Add(new float[] { 1.0f, 1.0f, 1.0f });
            init_t1.Add(new float[] { 1.0f, 0.5f, 0.0f });
            init_t1.Add(new float[] { 1.0f, 0.25f, 0.0f });
            float[] t = new float[3];
            float[,] P = new float[3, 2];
            float[,] getB = new float[3, 2];
            for (int i = 0; i < 3; i++)
            {
                int m = 1;
                int z = 2;
                if (i == 2) { m = 2; z = 1; }

                for (int j = 0; j < 3; j++)
                {
                    t[j] = (float)(2 * init_t[i][j] * init_t1[n - i][j] / (m * z));
                }
                getB[0, 0] = t[0]; getB[0, 1] = t[0];
                getB[1, 0] = t[1]; getB[1, 1] = t[1];
                getB[2, 0] = t[2]; getB[2, 1] = t[2];
                for (int j = 0; j < 3; j++)
                    for (int k = 0; k < 2; k++)
                    {
                        P[j, k] = getB[j, k] * point_list[j][k];
                    }
            }
            point_list1.Add(new float[] { P[0, 0], P[0, 1] });
            point_list1.Add(new float[] { P[1, 0], P[1, 1] });
            point_list1.Add(new float[] { P[2, 0], P[2, 1] });
            return point_list1;
        }

        public int get_angle(float[] node1, float[] node2, float[] node3)
        {
            double theta1 = Math.Atan2(node2[1] - node1[1], node1[0] - node2[0]);
            double theta2 = Math.Atan2(node2[1] - node3[1], node1[0] - node3[0]);
            int angle1 = (int)(theta1 * 180 / Math.PI);
            int angle2 = (int)(theta2 * 180 / Math.PI);
            int insideAngle = 0;
            if (angle1 * angle2 >= 0)
            {
                insideAngle = (int)Math.Abs(theta1 - angle2);
            }
            else
            {
                insideAngle = (int)(Math.Abs(angle1) + Math.Abs(theta2));
                if (insideAngle > 180)
                    insideAngle = 360 - insideAngle;
            }
            insideAngle = insideAngle % 180;
            return insideAngle;
        }

        public Node get_new_node(float theta, int n_ind, Node nearestNode)
        {
            Node newNode = new Node(); newNode.x = nearestNode.x; newNode.y = nearestNode.y;
            newNode.parent = nearestNode.parent; newNode.cost = nearestNode.cost;
            newNode.x += (float)(expandDis * Math.Cos(theta));// 根据步长和角度计算得到新节点
            newNode.y += (float)(expandDis * Math.Sin(theta));//
            newNode.cost += expandDis;
            newNode.parent = n_ind;
            return newNode;
        }

        public Node get_new_node_Optimize(float theta1, float theta2, int n_ind, Node nearestNode)
        {
            Node newNode = new Node(); newNode.x = nearestNode.x; newNode.y = nearestNode.y;
            newNode.parent = nearestNode.parent; newNode.cost = nearestNode.cost;
            float theta = (theta1 + theta2) / 2;
            newNode.x += (float)(expandDis * Math.Cos(theta));  //根据步长和角度计算得到新节点
            newNode.y += (float)(expandDis * Math.Sin(theta));
            newNode.cost += expandDis;
            newNode.parent = n_ind;
            return newNode;
        }

        public List<float[]> get_final_course(int lastIndex)
        {
            List<float[]> path = new List<float[]>();
            path.Add(new float[] { goal.x, goal.y });
            while (node_list[lastIndex].parent != 0)
            {
                Node node = node_list[lastIndex];
                path.Add(new float[] { node.x, node.y });
                lastIndex = node.parent;
            }
            path.Add(new float[] { start.x, start.y });
            //Debug.Log("len:::" + path.Count);
            return path;
        }

        public bool is_near_goal(Node node)
        {
            float d = line_cost(node, goal);
            if (d < expandDis)
            {
                return true;
            }
            else
                return false;
        }

        public float line_cost(Node node1, Node node2)
        {
            return (float)Math.Sqrt((node1.x - node2.x) * (node1.x - node2.x) + (node1.y - node2.y) * (node1.y - node2.y));
        }
    }
}
