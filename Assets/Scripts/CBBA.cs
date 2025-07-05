using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CBBA : MonoBehaviour
{
    //CBBA
    public int TaskNum = 0;
    public int AgentNum = 0;
    public int agentID = 0;
    public bool IsConverged = false;
    public List<int> winningAgentList; //z
    public List<float> winningBidList; //y
    public List<int> bundle; //b
    public List<int> path; //p
    private float vel = 0;
    private int L_t = 0;
    private int timeStep = 0;
    public Dictionary<int, int> timeStampList;
    public Vector2 state;
    private List<float> scores; //c
    private float lambda = 0.95f;
    private List<float> scores_bar;
    public Dictionary<int, (List<float> winningBidList, List<int> winningAgentList, Dictionary<int, int> timeStampList)> Y;
    private float minnumber = 0.002f;

    // Start is called before the first frame update
    public void Init(int id, float vel, int task_num, int agent_num, int l_t, Vector2 state)
    {
        this.agentID = id;
        this.IsConverged = false;
        //this.Pos = new Vector2(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
        this.vel = vel;
        this.TaskNum = task_num;
        this.AgentNum = agent_num;
        this.L_t = l_t;
        this.winningAgentList = Enumerable.Repeat(this.agentID, this.TaskNum).ToList();
        this.winningBidList = Enumerable.Repeat(0f, this.TaskNum).ToList();
        this.bundle = new List<int>();
        this.path = new List<int>();
        this.timeStep = 0;
        this.timeStampList = new Dictionary<int, int>();
        for (int i = 0; i < this.AgentNum; i++) this.timeStampList[i] = this.timeStep;
        // Set initial random state and score arrays
        this.state = state;
        this.scores = Enumerable.Repeat(0f, this.TaskNum).ToList();
        this.scores_bar = Enumerable.Repeat(1.0f, this.TaskNum).ToList();
        this.Y = new Dictionary<int, (List<float>, List<int> , Dictionary<int, int>)>();
        //Debug.Log("id" + id);
        //Debug.Log(string.Join(", ", this.winningAgentList));
        //Debug.Log(string.Join(", ", this.winningBidList));
        //Debug.Log("state:" + this.state);
    }


    public (List<float>, List<int>, Dictionary<int, int>) SendMessage()
    {
        return (this.winningBidList.ToList(), this.winningAgentList.ToList(), new Dictionary<int, int>(this.timeStampList));
    }

    // public void ReceiveMessage(Dictionary<int, (List<float>, List<int>, Dictionary<int, int>)> Y_)
    // {
    //     this.Y = Y_;
    // }
    public void ReceiveMessage(Dictionary<int, (List<float>, List<int>, Dictionary<int, int>)> Y_)
    {
        // 显式定义元组元素名称以提高可读性和正确性
        foreach(var kvp in Y_)
        {
            // 解构元组为命名变量
            var (bidList, agentList, timestampDict) = kvp.Value;           
            // 检查所有必要字段
            if (bidList == null || agentList == null || timestampDict == null || 
                bidList.Count != TaskNum || 
                agentList.Count != TaskNum)
            {
                // 添加详细错误信息
                string bidCount = bidList != null ? bidList.Count.ToString() : "null";
                string agentCount = agentList != null ? agentList.Count.ToString() : "null";
                string timeStampState = timestampDict != null ? "not null" : "null";
                
                Debug.LogError($"Received invalid message from agent {kvp.Key}: " +
                            $"BidList.Count={bidCount} (expected {TaskNum}), " +
                            $"AgentList.Count={agentCount} (expected {TaskNum}), " +
                            $"TimestampDict={timeStampState}");                
                // 部分无效消息处理：跳过该邻居的消息
                continue;
            }
        }       
        // 添加额外一致性检查
        foreach (var neighborId in Y_.Keys)
        {
            var (bidList, agentList, timestampDict) = Y_[neighborId];            
            // 确保时间戳字典包含所有智能体
            foreach (int agentId in timeStampList.Keys)
            {
                if (!timestampDict.ContainsKey(agentId))
                {
                    Debug.LogWarning($"Timestamp from agent {neighborId} missing entry for agent {agentId}. " +
                                "Attempting to reconstruct.");                   
                    // 尝试添加默认值（使用当前时间戳作为保底）
                    timestampDict[agentId] = timeStampList.ContainsKey(agentId) ? 
                                            timeStampList[agentId] : 
                                            timeStep;
                }
            }
        }    
        // 仅接受通过验证的消息
        this.Y = Y_;
    }

    public void BuildBundle(Dictionary<int, Vector2> task)
    {
        List<int> J = new List<int>();
        for (int i = 0; i < this.TaskNum; i++)
        {
            J.Add(i);
        }

        while (this.bundle.Count < this.L_t)
        {
            float S_p = 0;
            float distance_j = 0;
            if (this.path.Count > 0)
            {
                // distance_j += Vector2.Distance(this.state, task[this.path[0]]);
                distance_j += CalculateDis(this.state, task[this.path[0]]);
                S_p += Mathf.Pow(this.lambda, distance_j / this.vel) * this.scores_bar[this.path[0]];
                for (int p_idx = 0; p_idx < this.path.Count - 1; p_idx++)
                {
                    // distance_j += Vector2.Distance(task[this.path[p_idx]], task[this.path[p_idx + 1]]);
                    distance_j += CalculateDis(task[this.path[p_idx]], task[this.path[p_idx + 1]]);
                    S_p += Mathf.Pow(this.lambda, distance_j / this.vel) * this.scores_bar[this.path[p_idx + 1]];
                }
            }

            Dictionary<int, int> best_pos = new Dictionary<int, int>();
            foreach (int j in J)
            {
                List<float> score_list = new List<float>();
                if (this.bundle.Contains(j))
                {
                    this.scores[j] = 0;
                }
                else
                {
                    for (int n = 0; n < this.path.Count + 1; n++)
                    {
                        List<int> path_temp = new List<int>(this.path);
                        path_temp.Insert(n, j);
                        float score_temp = 0;
                        distance_j = 0;
                        distance_j += CalculateDis(this.state, task[path_temp[0]]);
                        // distance_j += Vector2.Distance(this.state, task[path_temp[0]]);
                        score_temp += Mathf.Pow(this.lambda, distance_j / this.vel) * this.scores_bar[path_temp[0]];
                        if (path_temp.Count > 1)
                        {
                            //Debug.Log("path_temp.Count:" + path_temp.Count);
                            for (int p_loc = 0; p_loc < path_temp.Count - 1; p_loc++)
                            {
                                //Debug.Log("p_loc:" + p_loc);
                                // distance_j += Vector2.Distance(task[path_temp[p_loc]], task[path_temp[p_loc + 1]]);
                                distance_j += CalculateDis(task[path_temp[p_loc]], task[path_temp[p_loc + 1]]);
                                score_temp += Mathf.Pow(this.lambda, distance_j / this.vel) * this.scores_bar[path_temp[p_loc + 1]];
                            }
                        }
                        float score_jn = score_temp - S_p;
                        score_list.Add(score_jn);
                    }
                    int max_idx = score_list.IndexOf(score_list.Max());
                    float score_j = score_list[max_idx];
                    this.scores[j] = score_j;
                    best_pos[j] = max_idx;
                }
            }
            bool[] h = this.scores.Select((value, index) => value > this.winningBidList[index]).ToArray();
            if (h.Sum(condition => condition ? 1 : 0) == 0) // No valid task
            {
                break;
            }
            for (int i = 0; i < h.Length; i++)
            {
                if (!h[i])
                {
                    this.scores[i] = 0;
                }
            }
            int J_i = this.scores.IndexOf(this.scores.Max());
            int n_j = best_pos[J_i];
            //J.Remove(J_i);
            this.bundle.Add(J_i);
            this.path.Insert(n_j, J_i);
            //Debug.Log("this.bundle.Add(J_i):" + J_i);
            //Debug.Log(string.Join(", ", this.bundle));
            //Debug.Log("this.path.Insert(n_j, J_i):" + n_j + "," + J_i);
            //Debug.Log(string.Join(", ", this.path));
            this.winningBidList[J_i] = this.scores[J_i];
            this.winningAgentList[J_i] = this.agentID;
        }
    }

    private void _Update(int j, float y_kj, int z_kj)
    {
        this.winningBidList[j] = y_kj;
        this.winningAgentList[j] = z_kj;
    }

    private void _Reset(int j)
    {
        // ʵ�������߼�
        this.winningBidList[j] = 0;
        this.winningAgentList[j] = -1;
    }
    public bool UpdateTask()
    {
        var oldP = new List<int>(this.path);

        var idList = this.Y.Keys.ToList();
        idList.Insert(0, this.agentID);
        foreach (var id in this.timeStampList.Keys.ToList())
        {
            if (idList.Contains(id))
            {
                this.timeStampList[id] = this.timeStep;
            }
            else
            {
                var sList = new List<int>();
                foreach (var neighborId in idList.Skip(1))
                {
                    sList.Add(this.Y[neighborId].timeStampList[id]);
                }
                if (sList.Count > 0)
                {
                    this.timeStampList[id] = sList.Max();
                }
            }
        }


        // ���¹���
        for (int j = 0; j < this.TaskNum; j++)
        {
            foreach (var k in idList.Skip(1))
            {
                var yK = this.Y[k].winningBidList;
                var zK = this.Y[k].winningAgentList;
                var sK = this.Y[k].timeStampList;

                var zIj = this.winningAgentList[j];
                var zKj = zK[j];
                var yKj = yK[j];

                int i = this.agentID;
                var yIj = this.winningBidList[j];

                // ���ڹ���ĸ���
                // ���� 1~4
                if (zKj == k)
                {
                    // ���� 1
                    if (zIj == this.agentID)
                    {
                        if (yKj > yIj)
                        {
                            this._Update(j, yKj, zKj);
                        }
                        //else if (Mathf.Abs(yKj - yIj) < float.Epsilon) // ƽ�ִ���
                        else if (Mathf.Abs(yKj - yIj) < this.minnumber) // ƽ�ִ���
                        {
                            if (k < this.agentID)
                            {
                                this._Update(j, yKj, zKj);
                            }
                        }
                        else
                        {
                            //Do nothing
                        }
                    }
                    // ���� 2
                    else if (zIj == k)
                    {
                        this._Update(j, yKj, zKj);
                    }
                    // ���� 3
                    else if (zIj != -1)
                    {
                        int m = zIj;
                        if ((sK[m] > this.timeStampList[m]) || (yKj > yIj))
                        {
                            this._Update(j, yKj, zKj);
                        }
                        //else if (Math.Abs(yKj - yIj) < float.Epsilon) // ƽ�ִ���
                        else if (Math.Abs(yKj - yIj) < this.minnumber)
                        {
                            if (k < this.agentID)
                            {
                                this._Update(j, yKj, zKj);
                            }
                        }
                    }
                    // ���� 4
                    else if (zIj == -1)
                    {
                        this._Update(j, yKj, zKj);
                    }
                    else
                    {
                        throw new Exception("Error while updating");
                    }
                }
                // ���� 5~8
                else if (zKj == i)
                {
                    // ���� 5
                    if (zIj == i)
                    {
                        //Do nothing
                    }
                    // ���� 6
                    else if (zIj == k)
                    {
                        this._Reset(j);
                    }
                    // ���� 7
                    else if (zIj != -1)
                    {
                        int m = zIj;
                        if (sK[m] > this.timeStampList[m])
                        {
                            this._Reset(j);
                        }
                    }
                    // ���� 8
                    else if (zIj == -1)
                    {
                        //Do nothing
                    }
                    else
                    {
                        throw new Exception("Error while updating");
                    }
                }
                // ���� 9~13
                else if (zKj != -1)
                {
                    int m = zKj;
                    // ���� 9
                    if (zIj == i)
                    {
                        if ((sK[m] >= this.timeStampList[m]) && (yKj > yIj))
                        {
                            this._Update(j, yKj, zKj);
                        }
                        //else if ((sK[m] >= this.timeStampList[m]) && (Math.Abs(yKj - yIj) < float.Epsilon)) // ƽ�ִ���
                        else if ((sK[m] >= this.timeStampList[m]) && (Math.Abs(yKj - yIj) < this.minnumber)) // ƽ�ִ���
                        {
                            if (m < this.agentID)
                            {
                                this._Update(j, yKj, zKj);
                            }
                        }
                    }
                    // ���� 10
                    else if (zIj == k)
                    {
                        if (sK[m] > this.timeStampList[m])
                        {
                            this._Update(j, yKj, zKj);
                        }
                        else
                        {
                            this._Reset(j);
                        }
                    }
                    // ���� 11
                    else if (zIj == m)
                    {
                        if (sK[m] > this.timeStampList[m])
                        {
                            this._Update(j, yKj, zKj);
                        }
                    }
                    //���� 12
                    else if (zIj != -1)
                    {
                        int n = zIj;
                        if ((sK[m] > this.timeStampList[m]) && (sK[n] > this.timeStampList[n]))
                        {
                            this._Update(j, yKj, zKj);
                        }
                        else if ((sK[m] > this.timeStampList[m]) && (yKj > yIj))
                        {
                            this._Update(j, yKj, zKj);
                        }
                        //else if ((sK[m] > this.timeStampList[m]) && (Math.Abs(yKj - yIj) < float.Epsilon)) // ƽ�ִ���
                        else if ((sK[m] > this.timeStampList[m]) && (Math.Abs(yKj - yIj) < this.minnumber)) // ƽ�ִ���
                        {
                            if (m < n)
                            {
                                this._Update(j, yKj, zKj);
                            }
                        }
                        else if ((sK[n] > this.timeStampList[n]) && (this.timeStampList[m] > sK[m]))
                        {
                            this._Update(j, yKj, zKj);
                        }
                    }
                    // // ------ 重构规则12 ------
                    // else if (zKj != -1 && zKj != k && zKj != i && 
                    //         zIj != -1 && zIj != i && zIj != k && zIj != zKj)
                    // {
                    //     int n = zIj; // 当前智能体认为的赢家
                        
                    //     bool kInfoNewer = (sK[m] > this.timeStampList[m]);
                    //     bool nInfoNewer = (sK[n] > this.timeStampList[n]);
                        
                    //     // 情况1: 双方信息都新
                    //     if (kInfoNewer && nInfoNewer)
                    //     {
                    //         if (yKj > this.winningBidList[j])
                    //         {
                    //             this._Update(j, yKj, zKj);
                    //         }
                    //         else if (Mathf.Approximately(yKj, this.winningBidList[j]))
                    //         {
                    //             // 出价相等时选择ID更小的智能体
                    //             if (m < n) 
                    //             {
                    //                 this._Update(j, yKj, zKj);
                    //             }
                    //         }
                    //     }
                    //     // 情况2: 只有邻居信息新
                    //     else if (kInfoNewer)
                    //     {
                    //         this._Update(j, yKj, zKj);
                    //     }
                    //     // 情况3: 只有当前赢家信息新 → 保持原有分配
                    //     // 情况4: 信息都过时 → 本地决策
                    //     else if (!kInfoNewer && !nInfoNewer)
                    //     {
                    //         if (yKj > this.winningBidList[j] || 
                    //         (Mathf.Approximately(yKj, this.winningBidList[j]) && m < n))
                    //         {
                    //             this._Update(j, yKj, zKj);
                    //         }
                    //     }
                    // }
                    // ���� 13
                    else if (zIj == -1)
                    {
                        if (sK[m] > this.timeStampList[m])
                        {
                            this._Update(j, yKj, zKj);
                        }
                    }
                    else
                    {
                        throw new Exception("Error while updating");
                    }
                }
                // ���� 14~17
                else if (zKj == -1)
                {
                    // ���� 14
                    if (zIj == i)
                    {
                        //Do nothing
                    }
                    // ���� 15
                    else if (zIj == k)
                    {
                        this._Update(j, yKj, zKj);
                    }
                    // ���� 16
                    else if (zIj != -1)
                    {
                        int m = zIj;
                        if (sK[m] > this.timeStampList[m])
                        {
                            this._Update(j, yKj, zKj);
                        }
                    }
                    // ���� 17
                    else if (zIj == -1)
                    {
                        //Do nothing
                    }
                    else
                    {
                        throw new Exception("Error while updating");
                    }
                }
                else
                {
                    throw new Exception("Error while updating");
                }
            }
        }
  
        int nBar = this.bundle.Count; // 默认不需要清理       
        // 找出第一个当前智能体不再获胜的任务位置
        for (int position = 0; position < this.bundle.Count; position++)
        {
            int taskId = this.bundle[position];
            
            // 检查当前智能体是否仍然赢得此任务
            if (this.winningAgentList[taskId] != this.agentID)
            {
                nBar = position;
                break;
            }
        } 
        var bIdx1 = new List<int>(this.bundle.Skip(nBar + 1));
        if (bIdx1.Count > 0)
        {
            foreach (var idx in bIdx1)
            {
                this.winningBidList[idx] = 0;
                this.winningAgentList[idx] = -1;
            }
        }
      
        if (nBar < this.bundle.Count)
        {
            this.bundle.RemoveRange(nBar, this.bundle.Count - nBar);
        }

        this.path = new List<int>();
        foreach (var task in this.bundle)
        {
            this.path.Add(task);
        }

        this.timeStep++;

        this.IsConverged = false;
        if (oldP.SequenceEqual(this.path))
        {
            this.IsConverged = true;
        }
        // if (this.IsConverged)
        // {
        //     Debug.Log($"Agent {agentID} converged with {this.bundle.Count} tasks at step {timeStep}");
        // }
        // else
        // {
        //     Debug.Log($"Agent {agentID} updated: [Bundle] {string.Join(",", bundle)} [Path] {string.Join(",", path)}");
        // }       
        return this.IsConverged;
    }


    private float CalculateDis1(Vector2 state, Vector2 task)
    {
        float difference = 0;
        difference += 1f * Math.Abs(Vector2.Distance(state, Vector2.zero) - Vector2.Distance(task, Vector2.zero));
        //difference += Vector2.Angle(state, task) / 5;
        return difference;
    }
    private float CalculateDis(Vector2 state, Vector2 task)
    {
        float difference = 0;
        difference += 1f * Math.Abs(Vector2.Distance(state, Vector2.zero) - Vector2.Distance(task, Vector2.zero));
        difference += Vector2.Angle(state, task) / 5;
        //difference += 5f * Math.Abs(state.neighbor - task.neighbor);
        //difference += 1f * Math.Abs(state.distance - task.distance);
        //Debug.Log("distance:" + Math.Abs(state.distance - task.distance));
        //Debug.Log("angle:" + 0.1f * Math.Abs(state.angle - task.angle) / 30f);
        //difference += 0.2f * Math.Abs(state.angle - task.angle)/30f;
        return difference;
    }
}
