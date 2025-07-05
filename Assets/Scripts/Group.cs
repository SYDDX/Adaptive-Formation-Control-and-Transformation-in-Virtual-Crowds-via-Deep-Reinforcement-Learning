using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using System;
using Random = UnityEngine.Random;
using System.Linq;
using UnityEngine.UIElements;
using System.Xml.Linq;
using UnityEditor;
using RRT;
using RVO;
using Vector2 = UnityEngine.Vector2;
using System.Linq;

public class Group : MonoBehaviour
{
    [HideInInspector]
    [Header("Agents")]

    public int m_GroupID = 0;
    public Color m_Color = Color.blue;
    public GameObject agentPrefab;
    public GameObject agentParent;
    public GameObject samplePrefab;
    private int m_NumOfAgents = 0;
    private int m_MaxNumofAgents = 7;
    private float m_CommRadius = 8f;
    private Vector3 m_GroupStartPos = Vector3.zero;
    private Vector3 m_GroupGoalPos = Vector3.zero;
    private Vector3 m_GroupCenter = Vector3.zero;
    private int Step = 0;
    private int MaxStep = 12000;
    private int m_NumReachGoal = 0;
    private int m_NumNearGoal = 0;
    private int m_LastNumReachGoal = 0;

    private int m_Stage = 0;
    private bool m_Init = true;
    private List<Vector3> m_GroupStartPosList = new List<Vector3>();
    private List<GameObject> m_Agents = new List<GameObject>();
    private Dictionary<int, Vector2> m_FormationPos = new Dictionary<int, Vector2>();
    private Dictionary<int, (List<int> ids, List<Vector3> offsets)> m_FormationOffset = new Dictionary<int, (List<int>,List<Vector3>)>();
    private int[,] m_G;
    private SimpleMultiAgentGroup m_AgentGroup;
    private List<Vector3> m_PathPoint = new List<Vector3>();
    private float m_PathIndex = 0;
    private List<List<List<Vector2>>> m_FormationRelativePosLists = new List<List<List<Vector2>>>();
    private float maxSpeed = 0.3f; // ����������ٶ�
    private EnvironmentParameters m_ResetParams;
    private int maxformationIndex = 2;
    private int currentFormationIndex = -1;
    private int minscaleCount = 0;
    private int midscaleCount = 0;
    private int transStep = -1;
    private bool scaleSignal = false;
    private float startTime = 0f;
    private float m_RandomScale = 1f;
    private float m_currentScale;
    private float m_Space;
    private int allPath = 0;
    // Start is called before the first frame update
    void Awake()
    {
        this.m_NumOfAgents = Random.Range(6, this.m_MaxNumofAgents);
        this.m_Init = true;
        this.m_FormationPos.Clear();
        this.m_G = new int[this.m_NumOfAgents, this.m_NumOfAgents];
        this.m_AgentGroup = new SimpleMultiAgentGroup();
        this.GetPathPoint();
        this.m_ResetParams = Academy.Instance.EnvironmentParameters;
        this.ResetGroup();
    }

    public void ResetGroup()
    {
        this.startTime = Time.time;
        this.scaleSignal = false;
        //Debug.Log("done!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
        this.m_PathIndex = this.m_ResetParams.GetWithDefault("pathIndex", 0);
        //6个观察，无矩形
        this.maxformationIndex = (int)this.m_ResetParams.GetWithDefault("maxformationIndex", 3f);
        this.currentFormationIndex = Random.Range(0, this.maxformationIndex);
        if(this.currentFormationIndex == 0)
        {
            switch(Random.Range(0, 2))
            {
                case 0:
                    this.m_RandomScale = 1f;
                    break;
                case 1:
                    this.m_RandomScale = 0.7f;
                    break;
            }
        }
        else
        {
            this.m_RandomScale = 1f;
        }
        // this.currentFormationIndex = 2;
        // this.m_RandomScale = 1f;

        // this.m_currentScale = this.m_RandomScale;
        this.m_Space = 3.0f;

        this.m_NumReachGoal = 0;
        this.m_NumNearGoal = 0;
        this.minscaleCount = 0;
        this.midscaleCount = 0;
        this.transStep = -1;

        // //测试用
        this.m_LastNumReachGoal = 0;
        this.Step = 0;
        this.m_Stage = 0;
        this.m_GroupStartPosList.Clear();
        ////�����ڳ����γ���Sqaure4
        //this.m_GroupGoalPos = new Vector3(23f, 0f, 0f);
        //�����ڳ����γ���Sqaure3
        //this.m_GroupGoalPos = new Vector3(22f, 0f, 6f);
        //Debug.Log("this.path:::"+this.m_PathPoint.Count);
        this.m_GroupGoalPos = this.m_PathPoint[(int)this.m_PathIndex];
        this.m_GroupCenter = Vector3.zero;
        if (this.m_Init)
        {
            //��ȡ�����Ӹ���ĽǶȼ���
            //this.GetFormationAngleLists();
            //this.GetPathPoint();
            for (int i = 0; i < this.m_NumOfAgents; i++)
            {
                GameObject newAgent = Instantiate(this.agentPrefab, new Vector3(0, 0, 0), Quaternion.identity);
                newAgent.transform.SetParent(this.agentParent.transform);
                this.m_Agents.Add(newAgent);
                this.m_Agents[i].GetComponent<SquareAgent>().AgentID = i;
                this.m_Agents[i].transform.SetParent(this.agentParent.transform);
                this.m_Agents[i].GetComponent<SquareAgent>().GroupID = this.m_GroupID;
                this.m_Agents[i].GetComponent<SquareAgent>().commRadius = this.m_CommRadius;
                //this.m_Agents[i].GetComponent<SquareAgent>().transform.Find("body").GetComponent<Renderer>().material.color = this.m_Color;
                //this.m_Agents[i].GetComponent<SquareAgent>().GetComponent<TrailRenderer>().startColor = this.m_Color;
                //this.m_Agents[i].GetComponent<SquareAgent>().GetComponent<TrailRenderer>().endColor = this.m_Color;
                this.m_Agents[i].GetComponent<SquareAgent>().Task_Num = this.m_NumOfAgents;
                this.m_Agents[i].GetComponent<SquareAgent>().Agent_Num = this.m_NumOfAgents;
                this.m_Agents[i].GetComponent<SquareAgent>().L_T = 1;
                this.m_Agents[i].GetComponent<SquareAgent>().groupGoalPos = this.m_GroupGoalPos;
                this.m_Agents[i].GetComponent<SquareAgent>().goalPos = this.m_GroupGoalPos;
                this.m_Agents[i].GetComponent<SquareAgent>().currentAllScale = this.m_RandomScale;
                this.m_Agents[i].GetComponent<SquareAgent>().desiredFormationIndex = this.currentFormationIndex;
                //this.m_Agents[i].GetComponent<SquareAgent>().agentCBBA = new CBBA();
                //this.m_Agents[i].GetComponent<SquareAgent>().formationAngleLists = this.m_FormationAngleLists;
                this.m_AgentGroup.RegisterAgent(this.m_Agents[i].GetComponent<SquareAgent>());
            }
            this.m_Init = false;
        }
        else
        {
            for (int i = 0; i < this.m_NumOfAgents; i++)
            {
                this.m_Agents[i].GetComponent<SquareAgent>().AgentID = i;
                this.m_Agents[i].transform.SetParent(this.agentParent.transform);
                this.m_Agents[i].GetComponent<SquareAgent>().GroupID = this.m_GroupID;
                this.m_Agents[i].GetComponent<SquareAgent>().commRadius = this.m_CommRadius;
                this.m_Agents[i].GetComponent<SquareAgent>().Task_Num = this.m_NumOfAgents;
                this.m_Agents[i].GetComponent<SquareAgent>().Agent_Num = this.m_NumOfAgents;
                this.m_Agents[i].GetComponent<SquareAgent>().L_T = 1;
                this.m_Agents[i].GetComponent<SquareAgent>().groupGoalPos = this.m_GroupGoalPos;
                this.m_Agents[i].GetComponent<SquareAgent>().goalPos = this.m_GroupGoalPos;
                this.m_Agents[i].GetComponent<SquareAgent>().currentAllScale = this.m_RandomScale;
                this.m_Agents[i].GetComponent<SquareAgent>().desiredFormationIndex = this.currentFormationIndex;
                //this.m_Agents[i].GetComponent<SquareAgent>().agentCBBA = new CBBA();
                //cccccccccccccccccccccccccc;
            }
        }

        this.m_GroupStartPosList = GetStartPos(4f, 8f, this.m_CommRadius, this.m_NumOfAgents, this.m_Space).ToList();
        for (int i = 0; i < this.m_NumOfAgents; i++)
        {
            this.m_Agents[i].transform.localPosition = this.m_GroupStartPosList[i];
            this.m_Agents[i].GetComponent<SquareAgent>().centerPos = this.m_Agents[i].transform.localPosition;
            this.m_Agents[i].GetComponent<SquareAgent>().ResetAgentWithAll();
            //this.m_Agents[i].GetComponent<SquareAgent>().SetCBBA();
        }
    }

    //// Update is called once per frame
    void FixedUpdate()
    {
        this.Step++;
        this.allPath = 0;
        if(this.Step == 50)
        {
            this.FixDuplicateIds(this.m_Agents);
            for(int i = 0; i<this.m_NumOfAgents; i++)
            {
                this.m_Agents[i].GetComponent<SquareAgent>().fixbug = true;
            }
            // for(int i = 0; i<this.m_NumOfAgents; i++)
            // {
            //     this.allPath += this.m_Agents[i].GetComponent<SquareAgent>().agentCBBA.path.Count;
            // }
            // if(this.allPath != this.m_NumOfAgents)
            // {
            //     this.m_AgentGroup.GroupEpisodeInterrupted();
            //     this.ResetGroup();
            // }
        }

        
        // if(this.Step%10==0)
        // {
        //     this.calFormationError();
        // }
        if(this.transStep != -1)
        {
            this.transStep += 1;
        }
        this.m_NumReachGoal = 0;
        this.m_NumNearGoal = 0;
        for (int i = 0; i < this.m_NumOfAgents; i++)
        {
            //Debug.DrawLine(this.m_Agents[i].transform.localPosition, this.m_Agents[i].GetComponent<SquareAgent>().centerPos, Color.red);
            Debug.DrawLine(this.m_Agents[i].transform.localPosition, this.m_Agents[i].GetComponent<SquareAgent>().goalPos, Color.red);
            this.m_NumReachGoal += this.m_Agents[i].GetComponent<SquareAgent>().IsReachGoal;
            this.m_NumNearGoal += this.m_Agents[i].GetComponent<SquareAgent>().IsNearGoal;
        }
        if (this.Step > this.MaxStep && this.m_NumReachGoal < this.m_NumOfAgents)
        {
            this.m_AgentGroup.AddGroupReward(-2000f);
            this.m_AgentGroup.GroupEpisodeInterrupted();
            this.ResetGroup();
        }
        else
        {
            if(this.m_NumReachGoal != 0)
            {
                //Debug.Log("NumReach: "+this.m_NumReachGoal);
                if (this.m_LastNumReachGoal != this.m_NumReachGoal)
                {
                    this.m_AgentGroup.AddGroupReward((this.m_NumReachGoal - this.m_LastNumReachGoal) * 200f);
                    this.m_LastNumReachGoal = this.m_NumReachGoal;
                }
                if (this.m_NumReachGoal == this.m_NumOfAgents)
                {
                    //Debug.Log("Reach Goal!");
                    this.m_AgentGroup.EndGroupEpisode();
                    this.ResetGroup();
                }
                else
                {
                    this.m_AgentGroup.AddGroupReward(-0.5f);
                }
            }

        }
        // else
        // {
        //     //���η����ʼ���Ŀ��ͱ��Լ�����ں��ڱ�������ͱ任ʱ���迼�Ǹı�
        //     if (this.m_Stage == 0)
        //     {
        //         if(this.m_NumCenterConsistent == this.m_NumOfAgents)
        //         {
        //             this.m_Stage = 1;
        //             this.transStep = 0;
        //             AssignFormationPos(this.currentFormationIndex, this.m_RandomScale);
        //             //AssignFormationPos(this.currentFormationIndex, this.m_Scale);
        //             //GetFormationOffset();
        //             for (int i = 0; i < this.m_Agents.Count; i++)
        //             {
        //                 //Debug.Log("this ID::::" + this.m_Agents[i].GetComponent<SquareAgent>().AgentID);
        //                 List<int> ids = new List<int>();
        //                 List<Vector3> offsets = new List<Vector3>();
        //                 for (int j = 0; j < this.m_Agents.Count; j++)
        //                 {
        //                     if (i != j && Vector2.Distance(this.m_Agents[j].GetComponent<SquareAgent>().formationPos, this.m_Agents[i].GetComponent<SquareAgent>().formationPos) < this.m_CommRadius)
        //                     {
        //                         ids.Add(this.m_Agents[j].GetComponent<SquareAgent>().AgentID);
        //                         offsets.Add(this.m_Agents[j].GetComponent<SquareAgent>().formationPos - this.m_Agents[i].GetComponent<SquareAgent>().formationPos);
        //                     }
        //                 }
        //                 // Debug.Log("idscount:"+ids.Count);
        //                 // Debug.Log("AgentID:"+this.m_Agents[i].GetComponent<SquareAgent>().AgentID);
        //                 // for(int k=0;k<ids.Count;k++)
        //                 // {
        //                 //     Debug.Log(ids[k]);
        //                 // }
        //                 this.m_FormationOffset[i] = (ids, offsets);
        //                 this.m_Agents[i].GetComponent<SquareAgent>().AllAssigned = true;
        //                 this.m_Agents[i].GetComponent<SquareAgent>().AllScaleConsistent = false;
        //                 this.m_Agents[i].GetComponent<SquareAgent>().AllNeedScale = false;
        //                 this.m_Agents[i].GetComponent<SquareAgent>().IsFormated = 0;
        //                 this.m_Agents[i].GetComponent<SquareAgent>().IsScaleConsistent = 0;
        //                 this.m_Agents[i].GetComponent<SquareAgent>().IsCenterConsistent = 0;
        //                 this.m_Agents[i].GetComponent<SquareAgent>().ids = this.m_FormationOffset[i].ids.ToList();
        //                 this.m_Agents[i].GetComponent<SquareAgent>().offsets = this.m_FormationOffset[i].offsets.ToList();
        //                 this.m_Agents[i].GetComponent<SquareAgent>().centerOffset = this.m_GroupCenter-this.m_Agents[i].GetComponent<SquareAgent>().formationPos;
        //                 this.m_Agents[i].GetComponent<SquareAgent>().goalPos = this.m_GroupGoalPos + this.m_Agents[i].GetComponent<SquareAgent>().formationPos - this.m_GroupCenter;
        //             }
        //         }
        //     }
        //     else
        //     {
        //         if (this.m_NumReachGoal == 0)
        //         {
        //             for(int i=0;i<this.m_NumOfAgents;i++)
        //             {
        //                 this.m_Agents[i].GetComponent<SquareAgent>().desiredScale = this.m_currentScale;
        //             }
        //             // if(this.Step%400==0)
        //             // {
        //             //     // for(int i=0;i<this.NumOtherAgents;i++)
        //             //     // {
        //             //     //     Debug.Log("ID:"+this.this.m_Agents[i].GetComponent<SquareAgent>().AgentID);
        //             //     //     Debug.Log("pos:"+this.m_Agents[i].GetComponent<SquareAgent>().goalPos);
        //             //     // }
        //             // }
        //             if(this.m_NumNearGoal == 0 && this.transStep>300)
        //             {
        //                 // //测试用
        //                 // //Debug.Log("ABCACABCBABCBA");
        //                 // if(this.m_NumCanScale == this.m_NumOfAgents)
        //                 // {
        //                 //     for (int i = 0; i < this.m_Agents.Count; i++)
        //                 //     {
        //                 //         this.m_Agents[i].GetComponent<SquareAgent>().AllCanScale = true;
        //                 //     }
        //                 //     //Debug.Log("this.m_NumNeedScale:"+this.m_NumNeedScale);
        //                 //     //�����������γɱ�ӣ��жϵ�ǰ����Ƿ���Ҫ����
        //                 //     if (this.Step > 1200 && this.Step < 1500 && this.scaleSignal == false)
        //                 //     {                               
        //                 //         //Debug.Log("!!!!!!!!!!!!!!!!!!!!!!!!");
        //                 //         for (int i = 0; i < this.m_Agents.Count; i++)
        //                 //         {
        //                 //             this.m_Agents[i].GetComponent<SquareAgent>().AllNeedScale = true;
        //                 //             //Debug.Log(i+": "+this.m_Agents[i].GetComponent<SquareAgent>().negotiateScale);
        //                 //         }
        //                 //         //Debug.Log("this.m_NumScaleConsistent: "+this.m_NumScaleConsistent);
        //                 //         //��Ҫ�������ж���������������������Ƿ�Э����ϣ�
        //                 //         if (this.m_NumScaleConsistent == this.m_NumOfAgents)
        //                 //         {
        //                 //             for (int i = 0; i < this.m_Agents.Count; i++)
        //                 //             {
        //                 //                 this.m_Agents[i].GetComponent<SquareAgent>().AllScaleConsistent = true;
        //                 //             }
        //                 //             //Debug.Log("this.m_NumCenterConsistent:"+this.m_NumCenterConsistent);
        //                 //             //��������Э�̣�Э����Ϻ����·�����Լ���ͱ��Ŀ��
        //                 //             if (this.m_NumCenterConsistent == this.m_NumOfAgents)
        //                 //             {
        //                 //                 this.scaleSignal = true;
        //                 //                 //Debug.Log("Assign!!!!");
        //                 //                 AssignFormationPos(1, 0.6f);
        //                 //                 this.m_currentSpace = 0.6f*this.m_idealSpace;
                                    
        //                 //                 for (int i = 0; i < this.m_Agents.Count; i++)
        //                 //                 {
        //                 //                     this.m_Agents[i].GetComponent<SquareAgent>().desiredSpace = this.m_currentSpace;
        //                 //                     List<int> ids = new List<int>();
        //                 //                     List<Vector3> offsets = new List<Vector3>();
        //                 //                     for (int j = 0; j < this.m_Agents.Count; j++)
        //                 //                     {
        //                 //                         if (i != j && Vector2.Distance(this.m_Agents[j].GetComponent<SquareAgent>().formationPos, this.m_Agents[i].GetComponent<SquareAgent>().formationPos) < this.m_CommRadius)
        //                 //                         {
        //                 //                             ids.Add(this.m_Agents[j].GetComponent<SquareAgent>().AgentID);
        //                 //                             offsets.Add(this.m_Agents[j].GetComponent<SquareAgent>().formationPos - this.m_Agents[i].GetComponent<SquareAgent>().formationPos);
        //                 //                         }
        //                 //                     }
        //                 //                     this.m_FormationOffset[i] = (ids, offsets);
        //                 //                     this.m_Agents[i].GetComponent<SquareAgent>().AllAssigned = true;
        //                 //                     this.m_Agents[i].GetComponent<SquareAgent>().AllScaleConsistent = false;
        //                 //                     this.m_Agents[i].GetComponent<SquareAgent>().AllNeedScale = false;
        //                 //                     this.m_Agents[i].GetComponent<SquareAgent>().AllCanScale = false;
        //                 //                     this.m_Agents[i].GetComponent<SquareAgent>().IsCanScale = 0;
        //                 //                     this.m_Agents[i].GetComponent<SquareAgent>().IsScaleConsistent = 0;
        //                 //                     this.m_Agents[i].GetComponent<SquareAgent>().IsCenterConsistent = 0;
        //                 //                     this.m_Agents[i].GetComponent<SquareAgent>().ids = this.m_FormationOffset[i].ids.ToList();
        //                 //                     this.m_Agents[i].GetComponent<SquareAgent>().offsets = this.m_FormationOffset[i].offsets.ToList();
        //                 //                     this.m_Agents[i].GetComponent<SquareAgent>().goalPos = this.m_GroupGoalPos + this.m_Agents[i].GetComponent<SquareAgent>().formationPos - this.m_GroupCenter;
        //                 //                 }
        //                 //             }
        //                 //         }
        //                 //     }
        //                 //     else if (this.Step >= 2500 && this.Step < 3000 && this.scaleSignal == true)
        //                 //     {                               
        //                 //         //Debug.Log("!!!!!!!!!!!!!!!!!!!!!!!!");
        //                 //         for (int i = 0; i < this.m_Agents.Count; i++)
        //                 //         {
        //                 //             this.m_Agents[i].GetComponent<SquareAgent>().AllNeedScale = true;
        //                 //             //Debug.Log(i+": "+this.m_Agents[i].GetComponent<SquareAgent>().negotiateScale);
        //                 //         }
        //                 //         //Debug.Log("this.m_NumScaleConsistent: "+this.m_NumScaleConsistent);
        //                 //         //��Ҫ�������ж���������������������Ƿ�Э����ϣ�
        //                 //         if (this.m_NumScaleConsistent == this.m_NumOfAgents)
        //                 //         {
        //                 //             for (int i = 0; i < this.m_Agents.Count; i++)
        //                 //             {
        //                 //                 this.m_Agents[i].GetComponent<SquareAgent>().AllScaleConsistent = true;
        //                 //             }
        //                 //             //Debug.Log("this.m_NumCenterConsistent:"+this.m_NumCenterConsistent);
        //                 //             //��������Э�̣�Э����Ϻ����·�����Լ���ͱ��Ŀ��
        //                 //             if (this.m_NumCenterConsistent == this.m_NumOfAgents)
        //                 //             {
        //                 //                 this.scaleSignal = true;
        //                 //                 //Debug.Log("Assign!!!!");
        //                 //                 AssignFormationPos(2, 0.8f);
        //                 //                 this.m_currentSpace = this.m_idealSpace;
                                    
        //                 //                 for (int i = 0; i < this.m_Agents.Count; i++)
        //                 //                 {
        //                 //                     this.m_Agents[i].GetComponent<SquareAgent>().desiredSpace = this.m_currentSpace;
        //                 //                     List<int> ids = new List<int>();
        //                 //                     List<Vector3> offsets = new List<Vector3>();
        //                 //                     for (int j = 0; j < this.m_Agents.Count; j++)
        //                 //                     {
        //                 //                         if (i != j && Vector2.Distance(this.m_Agents[j].GetComponent<SquareAgent>().formationPos, this.m_Agents[i].GetComponent<SquareAgent>().formationPos) < this.m_CommRadius)
        //                 //                         {
        //                 //                             ids.Add(this.m_Agents[j].GetComponent<SquareAgent>().AgentID);
        //                 //                             offsets.Add(this.m_Agents[j].GetComponent<SquareAgent>().formationPos - this.m_Agents[i].GetComponent<SquareAgent>().formationPos);
        //                 //                         }
        //                 //                     }
        //                 //                     this.m_FormationOffset[i] = (ids, offsets);
        //                 //                     this.m_Agents[i].GetComponent<SquareAgent>().AllAssigned = true;
        //                 //                     this.m_Agents[i].GetComponent<SquareAgent>().AllScaleConsistent = false;
        //                 //                     this.m_Agents[i].GetComponent<SquareAgent>().AllNeedScale = false;
        //                 //                     this.m_Agents[i].GetComponent<SquareAgent>().AllCanScale = false;
        //                 //                     this.m_Agents[i].GetComponent<SquareAgent>().IsCanScale = 0;
        //                 //                     this.m_Agents[i].GetComponent<SquareAgent>().IsScaleConsistent = 0;
        //                 //                     this.m_Agents[i].GetComponent<SquareAgent>().IsCenterConsistent = 0;
        //                 //                     this.m_Agents[i].GetComponent<SquareAgent>().ids = this.m_FormationOffset[i].ids.ToList();
        //                 //                     this.m_Agents[i].GetComponent<SquareAgent>().offsets = this.m_FormationOffset[i].offsets.ToList();
        //                 //                     this.m_Agents[i].GetComponent<SquareAgent>().goalPos = this.m_GroupGoalPos + this.m_Agents[i].GetComponent<SquareAgent>().formationPos - this.m_GroupCenter;
        //                 //                 }
        //                 //             }
        //                 //         }
        //                 //     }
        //                 // }

        //                 // //训练用
        //                 // //�ж������������Ƿ�ȫ���γɱ�ӣ�ֻҪ�γɹ�һ�Σ�����Ϊһֱ�γɣ���ǰ�����״�任�Ѿ���ɣ�
        //                 // if(this.m_NumCanScale == this.m_NumOfAgents)
        //                 // {
        //                 //     for (int i = 0; i < this.m_Agents.Count; i++)
        //                 //     {
        //                 //         this.m_Agents[i].GetComponent<SquareAgent>().AllCanScale = true;
        //                 //     }
        //                 //     //Debug.Log("this.m_NumNeedScale:"+this.m_NumNeedScale);
        //                 //     //�����������γɱ�ӣ��жϵ�ǰ����Ƿ���Ҫ����
        //                 //     if (this.m_NumNeedScale > 0)
        //                 //     {
        //                 //         for (int i = 0; i < this.m_Agents.Count; i++)
        //                 //         {
        //                 //             this.m_Agents[i].GetComponent<SquareAgent>().AllNeedScale = true;
        //                 //             //Debug.Log(i+": "+this.m_Agents[i].GetComponent<SquareAgent>().negotiateScale);
        //                 //         }
        //                 //         //Debug.Log("this.m_NumScaleConsistent: "+this.m_NumScaleConsistent);
        //                 //         //��Ҫ�������ж���������������������Ƿ�Э����ϣ�
        //                 //         this.minscaleCount = 0;
        //                 //         //this.midscaleCount = 0;
        //                 //         if (this.m_NumScaleConsistent == this.m_NumOfAgents)
        //                 //         {
        //                 //             float minScale = 1f;
        //                 //             for (int i = 0; i < this.m_Agents.Count; i++)
        //                 //             {
        //                 //                 if(minScale>this.m_Agents[i].GetComponent<SquareAgent>().currentScale)
        //                 //                 {
        //                 //                     minScale = this.m_Agents[i].GetComponent<SquareAgent>().currentScale;
        //                 //                 }
        //                 //                 this.m_Agents[i].GetComponent<SquareAgent>().AllScaleConsistent = true;
        //                 //                 //Debug.Log("i:"+this.m_Agents[i].GetComponent<SquareAgent>().AgentID);
        //                 //                 //Debug.Log("scale"+this.m_Agents[i].GetComponent<SquareAgent>().outputScale);
        //                 //                 // if(this.m_Agents[i].GetComponent<SquareAgent>().outputScale == 0.6f)
        //                 //                 // {                     
        //                 //                 //     Debug.Log("!!!!!!!!!!1");                       
        //                 //                 //     this.minscaleCount += 1;
        //                 //                 // }
        //                 //             }
        //                 //             if (this.m_NumCenterConsistent == this.m_NumOfAgents)
        //                 //             {
        //                 //                 this.transStep = 0;
        //                 //                 if(minScale==1f)
        //                 //                 {
        //                 //                     this.m_currentScale = 1.0f;
        //                 //                     AssignFormationPos(1, this.m_currentScale);
        //                 //                     for (int i = 0; i < this.m_Agents.Count; i++)
        //                 //                     {
        //                 //                         this.m_Agents[i].GetComponent<SquareAgent>().allScale = 1.0f;
        //                 //                         this.m_Agents[i].GetComponent<SquareAgent>().desiredScale = this.m_currentScale;
        //                 //                         List<int> ids = new List<int>();
        //                 //                         List<Vector3> offsets = new List<Vector3>();
        //                 //                         for (int j = 0; j < this.m_Agents.Count; j++)
        //                 //                         {
        //                 //                             if (i != j && Vector2.Distance(this.m_Agents[j].GetComponent<SquareAgent>().formationPos, this.m_Agents[i].GetComponent<SquareAgent>().formationPos) < this.m_CommRadius)
        //                 //                             {
        //                 //                                 ids.Add(this.m_Agents[j].GetComponent<SquareAgent>().AgentID);
        //                 //                                 offsets.Add(this.m_Agents[j].GetComponent<SquareAgent>().formationPos - this.m_Agents[i].GetComponent<SquareAgent>().formationPos);
        //                 //                             }
        //                 //                         }
        //                 //                         this.m_FormationOffset[i] = (ids, offsets);
        //                 //                         this.m_Agents[i].GetComponent<SquareAgent>().AllAssigned = true;
        //                 //                         this.m_Agents[i].GetComponent<SquareAgent>().AllScaleConsistent = false;
        //                 //                         this.m_Agents[i].GetComponent<SquareAgent>().AllNeedScale = false;
        //                 //                         this.m_Agents[i].GetComponent<SquareAgent>().AllCanScale = false;
        //                 //                         this.m_Agents[i].GetComponent<SquareAgent>().IsCanScale = 0;
        //                 //                         this.m_Agents[i].GetComponent<SquareAgent>().IsScaleConsistent = 0;
        //                 //                         this.m_Agents[i].GetComponent<SquareAgent>().IsCenterConsistent = 0;
        //                 //                         this.m_Agents[i].GetComponent<SquareAgent>().ids = this.m_FormationOffset[i].ids.ToList();
        //                 //                         this.m_Agents[i].GetComponent<SquareAgent>().offsets = this.m_FormationOffset[i].offsets.ToList();
        //                 //                         this.m_Agents[i].GetComponent<SquareAgent>().goalPos = this.m_GroupGoalPos + this.m_Agents[i].GetComponent<SquareAgent>().formationPos - this.m_GroupCenter;
        //                 //                     }                                      
        //                 //                 }
        //                 //                 else if(minScale==0.8f)                                           
        //                 //                 {                                      
        //                 //                     //Debug.Log("!!!!!!!1");
        //                 //                     this.m_currentScale = 0.7f;
        //                 //                     AssignFormationPos(1, this.m_currentScale);
        //                 //                     for (int i = 0; i < this.m_Agents.Count; i++)
        //                 //                     {
        //                 //                         this.m_Agents[i].GetComponent<SquareAgent>().allScale = 0.7f;
        //                 //                         this.m_Agents[i].GetComponent<SquareAgent>().desiredScale = this.m_currentScale;
        //                 //                         List<int> ids = new List<int>();
        //                 //                         List<Vector3> offsets = new List<Vector3>();
        //                 //                         for (int j = 0; j < this.m_Agents.Count; j++)
        //                 //                         {
        //                 //                             if (i != j && Vector2.Distance(this.m_Agents[j].GetComponent<SquareAgent>().formationPos, this.m_Agents[i].GetComponent<SquareAgent>().formationPos) < this.m_CommRadius)
        //                 //                             {
        //                 //                                 ids.Add(this.m_Agents[j].GetComponent<SquareAgent>().AgentID);
        //                 //                                 offsets.Add(this.m_Agents[j].GetComponent<SquareAgent>().formationPos - this.m_Agents[i].GetComponent<SquareAgent>().formationPos);
        //                 //                             }
        //                 //                         }
        //                 //                         this.m_FormationOffset[i] = (ids, offsets);
        //                 //                         this.m_Agents[i].GetComponent<SquareAgent>().AllAssigned = true;
        //                 //                         this.m_Agents[i].GetComponent<SquareAgent>().AllScaleConsistent = false;
        //                 //                         this.m_Agents[i].GetComponent<SquareAgent>().AllNeedScale = false;
        //                 //                         this.m_Agents[i].GetComponent<SquareAgent>().AllCanScale = false;
        //                 //                         this.m_Agents[i].GetComponent<SquareAgent>().IsCanScale = 0;
        //                 //                         this.m_Agents[i].GetComponent<SquareAgent>().IsScaleConsistent = 0;
        //                 //                         this.m_Agents[i].GetComponent<SquareAgent>().IsCenterConsistent = 0;
        //                 //                         this.m_Agents[i].GetComponent<SquareAgent>().ids = this.m_FormationOffset[i].ids.ToList();
        //                 //                         this.m_Agents[i].GetComponent<SquareAgent>().offsets = this.m_FormationOffset[i].offsets.ToList();
        //                 //                         this.m_Agents[i].GetComponent<SquareAgent>().goalPos = this.m_GroupGoalPos + this.m_Agents[i].GetComponent<SquareAgent>().formationPos - this.m_GroupCenter;
        //                 //                     }                                           
        //                 //                 }
        //                 //                 // else
        //                 //                 // {
        //                 //                 //     this.m_currentSpace = 0.7f*this.m_idealSpace;
        //                 //                 //     //AssignFormationPos(4, 1f);
        //                 //                 //     AssignFormationPos(2, 1f);
        //                 //                 //     for (int i = 0; i < this.m_Agents.Count; i++)
        //                 //                 //     {
        //                 //                 //         this.m_Agents[i].GetComponent<SquareAgent>().currentScale = 0.7f;
        //                 //                 //         this.m_Agents[i].GetComponent<SquareAgent>().desiredSpace = this.m_currentSpace;
        //                 //                 //         List<int> ids = new List<int>();
        //                 //                 //         List<Vector3> offsets = new List<Vector3>();
        //                 //                 //         for (int j = 0; j < this.m_Agents.Count; j++)
        //                 //                 //         {
        //                 //                 //             if (i != j && Vector2.Distance(this.m_Agents[j].GetComponent<SquareAgent>().formationPos, this.m_Agents[i].GetComponent<SquareAgent>().formationPos) < this.m_CommRadius)
        //                 //                 //             {
        //                 //                 //                 ids.Add(this.m_Agents[j].GetComponent<SquareAgent>().AgentID);
        //                 //                 //                 offsets.Add(this.m_Agents[j].GetComponent<SquareAgent>().formationPos - this.m_Agents[i].GetComponent<SquareAgent>().formationPos);
        //                 //                 //             }
        //                 //                 //         }
        //                 //                 //         this.m_FormationOffset[i] = (ids, offsets);
        //                 //                 //         this.m_Agents[i].GetComponent<SquareAgent>().AllAssigned = true;
        //                 //                 //         this.m_Agents[i].GetComponent<SquareAgent>().AllScaleConsistent = false;
        //                 //                 //         this.m_Agents[i].GetComponent<SquareAgent>().AllNeedScale = false;
        //                 //                 //         this.m_Agents[i].GetComponent<SquareAgent>().AllCanScale = false;
        //                 //                 //         this.m_Agents[i].GetComponent<SquareAgent>().IsCanScale = 0;
        //                 //                 //         this.m_Agents[i].GetComponent<SquareAgent>().IsScaleConsistent = 0;
        //                 //                 //         this.m_Agents[i].GetComponent<SquareAgent>().IsCenterConsistent = 0;
        //                 //                 //         this.m_Agents[i].GetComponent<SquareAgent>().ids = this.m_FormationOffset[i].ids.ToList();
        //                 //                 //         this.m_Agents[i].GetComponent<SquareAgent>().offsets = this.m_FormationOffset[i].offsets.ToList();
        //                 //                 //         this.m_Agents[i].GetComponent<SquareAgent>().goalPos = this.m_GroupGoalPos + this.m_Agents[i].GetComponent<SquareAgent>().formationPos - this.m_GroupCenter;
        //                 //                 //     }
        //                 //                 // }
        //                 //             }
        //                 //         }
        //                 //     }
        //                 // }
        //             }
        //         }
        //         else
        //         {
        //             //Debug.Log("NumReach: "+this.m_NumReachGoal);
        //             if (this.m_LastNumReachGoal != this.m_NumReachGoal)
        //             {
        //                 this.m_AgentGroup.AddGroupReward((this.m_NumReachGoal - this.m_LastNumReachGoal) * 100);
        //                 this.m_LastNumReachGoal = this.m_NumReachGoal;
        //             }
        //             if (this.m_NumReachGoal == this.m_NumOfAgents)
        //             {
        //                 //Debug.Log("Reach Goal!");
        //                 this.m_AgentGroup.EndGroupEpisode();
        //                 this.ResetGroup();
        //             }
        //             else
        //             {
        //                 this.m_AgentGroup.AddGroupReward(-0.5f);
        //             }
        //         }
        //     }
        // }
    }


    private void calFormationError()
    {
        float formerror = 0f;
        for (int i = 0; i < this.m_NumOfAgents; ++i)
        {
            float errorI = 0f;
            int numI = 0;
            Vector3 errVector = Vector3.zero;
            for (int j = 0; j < this.m_NumOfAgents; j++)
            {
                if (i != j && Vector3.Distance(this.m_Agents[j].GetComponent<SquareAgent>().formationPos, this.m_Agents[i].GetComponent<SquareAgent>().formationPos) < 8f)
                {
                    numI++;
                    errVector = (this.m_Agents[i].transform.localPosition - this.m_Agents[j].transform.localPosition) - (this.m_Agents[i].GetComponent<SquareAgent>().formationPos-this.m_Agents[j].GetComponent<SquareAgent>().formationPos);
                    errorI += Vector3.Distance(errVector, Vector3.zero)/Vector3.Distance(this.m_Agents[i].GetComponent<SquareAgent>().formationPos, this.m_Agents[j].GetComponent<SquareAgent>().formationPos);
                }
            }
            formerror += errorI / numI;
        }
        formerror /= this.m_NumOfAgents;
        GetComponent<DataLogger>().LogData(Time.time-this.startTime, formerror);
    }


    private void GetPathPoint()
    {
        //��Square4��
        //this.m_PathPoint.Add(new Vector3(23f, 0f, 0f)); 
        //��Square3��
        // this.m_PathPoint.Add(new Vector3(0f, 0f, -1.1f));
        // this.m_PathPoint.Add(new Vector3(7.7f, 0f, -5.61f));
        // this.m_PathPoint.Add(new Vector3(16.35f, 0f, -2.49f));
        //this.m_PathPoint.Add(new Vector3(22f, 0f, 6f));
        this.m_PathPoint.Add(new Vector3(41f, 0f, 0f));
    }





    // private void AssignFormationPos(int formationIndex, float scale)
    // {
    //     this.m_GroupCenter = Vector3.zero;
    //     for (int i = 0; i < this.m_NumOfAgents; i++)
    //     {
    //         this.m_GroupCenter += this.m_Agents[i].GetComponent<SquareAgent>().centerPos;
    //     }

    //     this.m_GroupCenter /= this.m_NumOfAgents;
    //     this.m_FormationPos.Clear();
    //     List<Vector2> temp = this.GetFormation(formationIndex, this.m_GroupCenter, this.m_GroupGoalPos, this.m_NumOfAgents, this.m_Space * scale);
    //     for(int i = 0;i<temp.Count;i++)
    //     {
    //         this.m_FormationPos[i] = temp[i];
    //     }
    //     // for (int i = 0; i < this.m_NumOfAgents; i++)
    //     // {
    //     //     this.m_Agents[i].GetComponent<SquareAgent>().SetCBBA();
    //     //     Debug.Log("path Count:" + this.m_Agents[i].GetComponent<SquareAgent>().agentCBBA.path.Count);
    //     // }
    //     this.SetCommNetwork();
    //     this.RunPosAllocation();
    //     for (int i = 0; i < this.m_NumOfAgents; i++)
    //     {
    //         this.m_Agents[i].GetComponent<SquareAgent>().AgentID = this.m_Agents[i].GetComponent<SquareAgent>().agentCBBA.path[0];
    //         Debug.Log("path:" + this.m_Agents[i].GetComponent<SquareAgent>().agentCBBA.path[0]);
    //     }
    //     //this.FixDuplicateIds(this.m_Agents);
    //     for (int i = 0; i < this.m_NumOfAgents; i++)
    //     {
    //         //Debug.Log("this.m_Agents[i].GetComponent<SquareAgent>().AgentID:"+this.m_Agents[i].GetComponent<SquareAgent>().AgentID);//Debug.Log("i: "+i);
    //         //Debug.Log("path:" + this.m_Agents[i].GetComponent<SquareAgent>().agentCBBA.path[0]);
    //         this.m_Agents[i].GetComponent<SquareAgent>().formationPos.x = this.m_FormationPos[this.m_Agents[i].GetComponent<SquareAgent>().AgentID].x;
    //         this.m_Agents[i].GetComponent<SquareAgent>().formationPos.y = 0f;
    //         this.m_Agents[i].GetComponent<SquareAgent>().formationPos.z = this.m_FormationPos[this.m_Agents[i].GetComponent<SquareAgent>().AgentID].y;
    //     }
    //     // for (int i = 0; i < this.m_NumOfAgents; i++)
    //     // {
    //     //     this.m_Agents[i].GetComponent<SquareAgent>().SetCBBA();
            
    //     // }
    //     // if(scale>=0.6f)
    //     // {
    //     //     this.m_GroupCenter = Vector3.zero;
    //     //     for (int i = 0; i < this.m_NumOfAgents; i++)
    //     //     {
    //     //         this.m_GroupCenter += this.m_Agents[i].GetComponent<SquareAgent>().centerPos;
    //     //     }

    //     //     this.m_GroupCenter /= this.m_NumOfAgents;
    //     //     // if(formationIndex == -1)
    //     //     // {
    //     //     //     formationIndex = Random.Range(1,this.maxformationIndex);
    //     //     //     this.currentFormationIndex = formationIndex;
    //     //     // }
    //     //     // else
    //     //     // {
    //     //     //     this.currentFormationIndex = formationIndex;
    //     //     // }
    //     //     this.GetFormation(formationIndex, this.m_GroupCenter, this.m_GroupGoalPos, this.m_NumOfAgents, this.m_idealSpace*scale);

    //     //     for (int i = 0; i < this.m_NumOfAgents; i++)
    //     //     {
    //     //         this.m_Agents[i].GetComponent<SquareAgent>().SetCBBA();
    //     //     }
    //     //     this.SetCommNetwork();
    //     //     this.RunPosAllocation();
    //     //     for (int i = 0; i < this.m_NumOfAgents; i++)
    //     //     {
    //     //         this.m_Agents[i].GetComponent<SquareAgent>().AgentID = this.m_Agents[i].GetComponent<SquareAgent>().agentCBBA.path[0];
    //     //     }
    //     //     this.FixDuplicateIds(this.m_Agents);
    //     //     for (int i = 0; i < this.m_NumOfAgents; i++)
    //     //     {
    //     //         //Debug.Log("this.m_Agents[i].GetComponent<SquareAgent>().AgentID:"+this.m_Agents[i].GetComponent<SquareAgent>().AgentID);//Debug.Log("i: "+i);
    //     //         //Debug.Log("path:" + this.m_Agents[i].GetComponent<SquareAgent>().agentCBBA.path[0]);
    //     //         this.m_Agents[i].GetComponent<SquareAgent>().formationPos.x = this.m_FormationPos[this.m_Agents[i].GetComponent<SquareAgent>().AgentID].x;
    //     //         this.m_Agents[i].GetComponent<SquareAgent>().formationPos.y = 0f;
    //     //         this.m_Agents[i].GetComponent<SquareAgent>().formationPos.z = this.m_FormationPos[this.m_Agents[i].GetComponent<SquareAgent>().AgentID].y;
    //     //     }
    //     // }
    //     // else
    //     // {
    //     //     this.m_GroupCenter = Vector3.zero;
    //     //     for (int i = 0; i < this.m_NumOfAgents; i++)
    //     //     {
    //     //         this.m_GroupCenter += this.m_Agents[i].GetComponent<SquareAgent>().centerPos;
    //     //     }

    //     //     this.m_GroupCenter /= this.m_NumOfAgents;
    //     //     this.GetFormation(1, this.m_GroupCenter, this.m_GroupGoalPos, this.m_NumOfAgents, this.m_idealSpace * scale);
    //     //     for (int i = 0; i < this.m_NumOfAgents; i++)
    //     //     {
    //     //         this.m_Agents[i].GetComponent<SquareAgent>().SetCBBA();
    //     //     }
    //     //     this.SetCommNetwork();
    //     //     this.RunPosAllocation();
    //     //     for (int i = 0; i < this.m_NumOfAgents; i++)
    //     //     {
    //     //         this.m_Agents[i].GetComponent<SquareAgent>().AgentID = this.m_Agents[i].GetComponent<SquareAgent>().agentCBBA.path[0];
    //     //         //Debug.Log("path:" + this.m_Agents[i].GetComponent<SquareAgent>().agentCBBA.path.Count);
    //     //         this.m_Agents[i].GetComponent<SquareAgent>().formationPos.x = this.m_FormationPos[this.m_Agents[i].GetComponent<SquareAgent>().agentCBBA.path[0]].x;
    //     //         this.m_Agents[i].GetComponent<SquareAgent>().formationPos.y = 0f;
    //     //         this.m_Agents[i].GetComponent<SquareAgent>().formationPos.z = this.m_FormationPos[this.m_Agents[i].GetComponent<SquareAgent>().agentCBBA.path[0]].y;
    //     //     }
    //     // }

    // }



    // private void SetCommNetwork()
    // {
    //     ClearArray(this.m_G);
    //     this.m_G = new int[this.m_NumOfAgents, this.m_NumOfAgents];
    //     for (int i = 0; i < this.m_NumOfAgents; i++)
    //     {
    //         for (int j = 0; j < this.m_NumOfAgents; j++)
    //         {
    //             //this.m_G[i, j] = 1;
    //             if (Vector3.Distance(this.m_Agents[i].transform.localPosition, this.m_Agents[j].transform.localPosition) < this.m_CommRadius)
    //             {
    //                 this.m_G[i, j] = 1; //Connected
    //             }
    //             else
    //                 this.m_G[i, j] = 0;
    //         }
    //     }
    // }

    // private void RunPosAllocation()
    // {
    //     Dictionary<int, Vector2> relativeFormationPos = new Dictionary<int, Vector2>();
    //     for(int i=0;i<this.m_NumOfAgents;i++)
    //     {
    //         relativeFormationPos[i] = this.m_FormationPos[i]-new Vector2(this.m_GroupCenter.x, this.m_GroupCenter.z);
    //         //Debug.Log("i:"+i+"  pos :"+relativeFormationPos[i]);
    //     }
    //     while (true)
    //     {
    //         List<bool> convergedList = new List<bool>();

    //         // Phase 1: Auction Process
    //         //Debug.Log("Auction Process");
    //         foreach (var robot in this.m_Agents)
    //         {
    //             // Select task by local information
    //             //robot.GetComponent<SquareAgentBC>().agentCBBA.BuildBundle(this.m_FormationPos);
    //             robot.GetComponent<SquareAgent>().agentCBBA.BuildBundle(relativeFormationPos);
    //         }

    //         List<(List<float>, List<int>, Dictionary<int, int>)> messagePool = new List<(List<float>, List<int>, Dictionary<int, int>)>();
    //         foreach (var robot in this.m_Agents)
    //         {
    //             messagePool.Add(robot.GetComponent<SquareAgent>().agentCBBA.SendMessage());
    //         }

    //         Dictionary<int, (List<float>, List<int>, Dictionary<int, int>)> Y = new Dictionary<int, (List<float>, List<int>, Dictionary<int, int>)>();
    //         for (int robotId = 0; robotId < this.m_Agents.Count; robotId++)
    //         {
    //             var robot = this.m_Agents[robotId];
    //             // Receive winning bid list from neighbors
    //             List<int> connected = new List<int>();
    //             for (int j = 0; j < this.m_NumOfAgents; j++)
    //             {
    //                 if (this.m_G[robotId, j] == 1 && robotId != j)
    //                 {
    //                     connected.Add(j);
    //                 }
    //             }
    //             if (connected.Count > 0)
    //             {
    //                 foreach (var neighborId in connected)
    //                 {
    //                     Y[neighborId] = messagePool[neighborId];
    //                 }
    //             }
    //             robot.GetComponent<SquareAgent>().agentCBBA.ReceiveMessage(Y);
    //         }
    //         // Phase 2: Consensus Process
    //         //Debug.Log("Consensus Process");
    //         foreach (var robot in this.m_Agents)
    //         {
    //             // Update local information and decision
    //             if (Y != null)
    //             {
    //                 bool converged = robot.GetComponent<SquareAgent>().agentCBBA.UpdateTask();
    //                 convergedList.Add(converged);
    //             }
    //         }
    //         if (convergedList.Count == this.m_NumOfAgents && convergedList.TrueForAll(x => x))
    //         {
    //             break;
    //         }
    //     }
    // }


    public void FixDuplicateIds(List<GameObject> agents)
    {
        int N = agents.Count;
        HashSet<int> usedIds = new HashSet<int>();
        List<GameObject> duplicateAgents = new List<GameObject>();

        // 1. 识别重复Agent并记录已用id
        foreach (GameObject agent in agents)
        {
            if (agent.GetComponent<SquareAgent>().agentCBBA.path.Count>0 && !usedIds.Contains(agent.GetComponent<SquareAgent>().agentCBBA.path[0]))
            {
                usedIds.Add(agent.GetComponent<SquareAgent>().agentCBBA.path[0]);
                //Debug.Log("use:"+agent.GetComponent<SquareAgent>().agentCBBA.path[0]);
            }
            else
            {
                duplicateAgents.Add(agent);
            }
        }
        // 2. 生成所有空缺的id（0~N-1中未使用的）
        List<int> availableIds = new List<int>();
        for (int i = 0; i < N; i++)
        {
            if (!usedIds.Contains(i))
            {
                availableIds.Add(i);
                //Debug.Log("availableIds:"+i);
            }
        }

        // 3. 将空缺id分配给重复Agent
        for (int i = 0; i < duplicateAgents.Count; i++)
        {
            if(duplicateAgents[i].GetComponent<SquareAgent>().agentCBBA.path.Count>0)
            {
                duplicateAgents[i].GetComponent<SquareAgent>().agentCBBA.path[0] = (availableIds[i]);
            }
            else
            {
                duplicateAgents[i].GetComponent<SquareAgent>().agentCBBA.path.Add(availableIds[i]);
            }
            
        }
        //Debug.Log("!!!!!!!!!1");
    }

    private List<Vector3> GetStartPos(float innerRadius, float outerRadius, float perceptionRadius, int agentCount, float minDistance)
    {
        //List<Vector3> points = new List<Vector3>();
        //// ������ɵ�һ���㣨�������ھ�������
        //Vector3 firstPoint = RandomPos(innerRadius, outerRadius);
        //points.Add(firstPoint);
        //for (int i = 1; i < agentCount; i++)
        //{
        //    Vector3 newPoint;
        //    bool hasNeighbor;
        //    bool satisfiesMinDistance;
        //    do
        //    {
        //        newPoint = RandomPos(innerRadius, outerRadius);
        //        hasNeighbor = false;
        //        satisfiesMinDistance = true;
        //        foreach (Vector3 point in points)
        //        {
        //            float distance = Vector3.Distance(newPoint, point);
        //            if (distance <= perceptionRadius)
        //            {
        //                hasNeighbor = true;
        //            }
        //            if (distance < minDistance)
        //            {
        //                satisfiesMinDistance = false;
        //                break;
        //            }
        //        }
        //    } while (!hasNeighbor || !satisfiesMinDistance); // ������������㣬����������
        //    points.Add(newPoint);
        //}
        //return points;
        List<Vector3> points= new List<Vector3>();
        //points.Add(new Vector3(-6f, 0f, -17f));
        //points.Add(new Vector3(-5f, 0f, -20f));
        //points.Add(new Vector3(-2f, 0f, -19.5f));
        //points.Add(new Vector3(2f, 0f, -25f));
        //points.Add(new Vector3(5f, 0f, -17.5f));
        //points.Add(new Vector3(5.5f, 0f, -22f));

        // //�����γ���
        // points.Add(new Vector3(-16f, 0f, -12.5f));
        // points.Add(new Vector3(-21.4f, 0f, -16.5f));
        // points.Add(new Vector3(-12.8f, 0f, -13.7f));
        // points.Add(new Vector3(-10.9f, 0f, -18.9f));
        // points.Add(new Vector3(-20f, 0f, -20f));
        // points.Add(new Vector3(-15f, 0f, -20f));

        ////�����γ���Square4��
        points.Add(new Vector3(-29.8f, 0f, 0.6f));
        //points.Add(new Vector3(-36f, 0f, 3.5f));
        points.Add(new Vector3(-36.1f, 0f, 2.3f));
        points.Add(new Vector3(-27.0f, 0f, 2.0f));
        //points.Add(new Vector3(-32.5f, 0f, -4.9f));
        points.Add(new Vector3(-32.5f, 0f, -1.9f));
        //points.Add(new Vector3(-35f, 0f, -2.5f));
        points.Add(new Vector3(-36.52f, 0f, -3.62f));
        points.Add(new Vector3(-27f, 0f, -3.9f));
        //�����γ���Square3��
        // points.Add(new Vector3(-27.0f, 0f, 1.0f));
        // points.Add(new Vector3(-27.4f, 0f, -3f));
        // points.Add(new Vector3(-20.0f, 0f, -5.5f));
        // points.Add(new Vector3(-23.5f, 0f, -8.9f));
        // points.Add(new Vector3(-27.5f, 0f, -7f));
        // points.Add(new Vector3(-19.5f, 0f, -8f));

        return points;
    }

    private void GetFormationRelativePosLists()
    {
        List<Vector2> TriangleFormation = new List<Vector2>();
        List<Vector2> DoubleTriangleFormation = new List<Vector2>();
        List<Vector2> CircleFormation = new List<Vector2>();
        List<Vector2> RectangleFormation = new List<Vector2>();
        List<Vector2> LineFormation = new List<Vector2>();
        TriangleFormation = GetFormation(0, new Vector3(0, 0, 0), new Vector3(10, 0, 0), this.m_NumOfAgents, 2f).ToList();
        DoubleTriangleFormation = GetFormation(1, new Vector3(0, 0, 0), new Vector3(10, 0, 0), this.m_NumOfAgents, 2f).ToList();
        CircleFormation = GetFormation(2, new Vector3(0, 0, 0), new Vector3(10, 0, 0), this.m_NumOfAgents, 2f).ToList();
        RectangleFormation = GetFormation(3, new Vector3(0, 0, 0), new Vector3(10, 0, 0), this.m_NumOfAgents, 2f).ToList();
        LineFormation = GetFormation(4, new Vector3(0, 0, 0), new Vector3(10, 0, 0), this.m_NumOfAgents, 2f).ToList();
        this.m_FormationRelativePosLists.Add(CalculateRelativePos(TriangleFormation, this.m_CommRadius));
        this.m_FormationRelativePosLists.Add(CalculateRelativePos(DoubleTriangleFormation, this.m_CommRadius));
        this.m_FormationRelativePosLists.Add(CalculateRelativePos(CircleFormation, this.m_CommRadius));
        this.m_FormationRelativePosLists.Add(CalculateRelativePos(RectangleFormation, this.m_CommRadius));
        this.m_FormationRelativePosLists.Add(CalculateRelativePos(LineFormation, this.m_CommRadius));
        for(int i = 0; i < this.m_FormationRelativePosLists.Count; i++)
        {
            for(int j = 0; j < this.m_FormationRelativePosLists[i].Count; j++)
            {
                for (int k = 0; k < this.m_FormationRelativePosLists[i][j].Count; k++)
                {
                    Debug.Log(i+", "+j+", "+k+","+ this.m_FormationRelativePosLists[i][j][k]);
                }
            }
        }
    }

    public static List<List<Vector2>> CalculateRelativePos(List<Vector2> points, float range)
    {
        List<List<Vector2>> allRelativePos= new List<List<Vector2>>();
        for (int i = 0; i < points.Count; i++)
        {
            List<Vector2> relativePos = new List<Vector2>();
            for (int j = 0; j < points.Count; j++)
            {
                if (i == j) continue; // ��������
                if (Vector2.Distance(points[i], points[j]) <= range)
                {
                    relativePos.Add(points[i]-points[j]);
                }
            }
            if(relativePos.Count>0)
            {
                allRelativePos.Add(relativePos);
            }
        }
        return allRelativePos;
    }

    private List<Vector2> GetFormation(int formationId, Vector3 center, Vector3 end, int agentCount, float spacing)
    {
        List<Vector2> formation_positions = new List<Vector2>();
        float formationFactor = 1.4f;
        switch (formationId)
        {
            case 0:
                formation_positions = GetTriangleFormation(center, end, agentCount, formationFactor*spacing).ToList(); break;
            case 1:
                formation_positions = GetDoubleTriangleFormation(center, end, agentCount, formationFactor*spacing).ToList(); break;
            case 2:
                formation_positions = GetCircleFormation(center, end, agentCount, spacing).ToList(); break;
            case 3:
                formation_positions = GetRectangleFormation(center, end, agentCount, formationFactor*spacing).ToList(); break;
            case 4:
                formation_positions = GetLineFormation(center, end, agentCount, formationFactor*spacing).ToList(); break;

        }
        return formation_positions;
    }

    public List<Vector2> GetDoubleTriangleFormation(Vector3 start, Vector3 end, int agentCount, float spacing)
    {
        List<Vector2> positions = new List<Vector2>();
        
        // 1. 计算从 start 到 end 的方向
        Vector2 direction = new Vector2(end.x - start.x, end.z - start.z);
        if (direction.magnitude < 0.001f)
            direction = new Vector2(1f, 0f); // 默认方向（防止零向量）
        else
            direction.Normalize();
        
        // 2. 计算垂直于方向的左侧方向（顺时针90度）
        Vector2 perpendicularDirection = new Vector2(-direction.y, direction.x);
        
        // 3. **第一个小三角形更靠近终点**
        Vector2 firstTriangleStart = new Vector2(start.x, start.z) + direction * spacing; // 向终点方向偏移
        GenerateTriangle(firstTriangleStart, direction, perpendicularDirection, spacing, positions);
        
        // 4. **第二个小三角形在第一个后方（远离终点）**
        Vector2 secondTriangleStart = firstTriangleStart - direction * spacing * 2; // 反向移动
        GenerateTriangle(secondTriangleStart, direction, perpendicularDirection, spacing, positions);
        Vector2 formationCenter = Vector2.zero;
        for(int i=0;i<agentCount;i++)
        {
            formationCenter += positions[i];
        }
        formationCenter /= agentCount;
        for(int i=0;i<agentCount;i++)
        {
            positions[i] += new Vector2(start.x, start.z) - formationCenter;
        }
        return positions;
    }

    // 辅助函数：生成单个小三角形（3个智能体）
    private void GenerateTriangle(Vector2 start, Vector2 direction, Vector2 perpendicularDirection, float spacing, List<Vector2> positions)
    {
        // 添加三角形的顶点（尖端指向终点）
        positions.Add(start);
        
        // 在顶点后方（远离终点方向）生成两个对称点
        Vector2 layerCenter = start - direction * spacing; // 反向移动
        for (int i = 0; i < 2; i++)
        {
            float offset = (i - 0.5f) * spacing; 
            Vector2 agentPos = layerCenter + perpendicularDirection * offset;
            positions.Add(agentPos);
        }
    }

    public List<Vector2> GetTriangleFormation(Vector3 start, Vector3 end, int agentCount, float spacing)
    {
        List<Vector2> positions = new List<Vector2>();
        Vector2 direction = new Vector2(end.x - start.x, end.z - start.z).normalized;
        // ���㴹ֱ�ڷ���������ƫ�Ʒ��������Ĵ�ֱ����
        Vector2 perpendicularDirection = new Vector2(-direction.y, direction.x); // ��ʱ��90��
        positions.Add(new Vector2(start.x+3f, start.z));
        int currentLayer = 1;          // ��ǰ�������ӵ�1�㿪ʼ
        int agentsInCurrentLayer = 2;  // �ڶ��㿪ʼ��2��������
        int totalAgentsPlaced = 1;     // �ѷ��õ���������������ʼֵΪ1����㣩
        while (totalAgentsPlaced < agentCount)
        {
            // ���㵱ǰ�������λ�ã�������ط��������ƶ�
            Vector2 layerCenterPosition = new Vector2(start.x, start.z) - direction * spacing * currentLayer;
            for (int i = 0; i < agentsInCurrentLayer; i++)
            {
                if (totalAgentsPlaced >= agentCount)
                    break;
                float offset = (i - (agentsInCurrentLayer - 1) / 2f) * spacing;
                Vector2 agentPosition = layerCenterPosition - perpendicularDirection * offset;
                positions.Add(new Vector2(agentPosition.x+3f, agentPosition.y));
                totalAgentsPlaced++;
            }
            currentLayer++;
            agentsInCurrentLayer++;
        }
        return positions;
    }


    // ���α��
    List<Vector2> GetRectangleFormation(Vector3 start, Vector3 end, int agentCount, float spacing)
    {
        List<Vector2> positions = new List<Vector2>();
        Vector2 direction = new Vector2(1f, 0f);
        Vector2 perpendicular = Vector2.Perpendicular(direction);
        int rows = Mathf.CeilToInt(Mathf.Sqrt(agentCount)); // ����
        int columns = Mathf.CeilToInt((float)agentCount / rows); // ����
        Vector2 formationCenter = Vector2.zero;
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                if (positions.Count >= agentCount)
                    break;
                Vector2 pos = new Vector2(start.x+3f, start.z) - direction * (row * 1f*spacing) + perpendicular * (col - (columns - 1) / 2f) * 0.9f*spacing;
                positions.Add(pos);
                formationCenter += pos;
            }
        }
        formationCenter /= agentCount;
        for(int i=0;i<agentCount;i++)
        {
            positions[i] += new Vector2(start.x, start.z) - formationCenter;
        }
        return positions;
    }

    //Բ�α��
    List<Vector2> GetCircleFormation(Vector3 start, Vector3 end, int agentCount, float spacing)
    {
        List<Vector2> positions = new List<Vector2>();
        Vector2 direction = new Vector2(0f, 1f);
        float radius = 1.5f * spacing;
        for (int i = 0; i < agentCount; i++)
        {
            float angle = 2 * Mathf.PI * i / agentCount; // ����Բ
            Vector2 circleOffset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            // ����������ת��Ϊ��ά����ϵ����ת
            Vector2 rotatedOffset = new Vector2(
                direction.x * circleOffset.x - direction.y * circleOffset.y,
                direction.x * circleOffset.y + direction.y * circleOffset.x
            );
            Vector2 position = new Vector2(start.x, start.z) - rotatedOffset;
            positions.Add(position);
        }
        return positions;
    }

    // һ���ͱ��
    List<Vector2> GetLineFormation(Vector3 start, Vector3 end, int agentCount, float spacing)
    {
        List<Vector2> positions = new List<Vector2>();
        Vector2 direction = (new Vector2(end.x, end.z) - new Vector2(start.x, start.z)).normalized;
        // Vector2 perpendicular = Vector2.Perpendicular(direction);
        Vector2 perpendicular = direction;
        for (int i = 0; i < agentCount; i++)
        {
            float offset = (i - (agentCount - 1) / 2f) * spacing;
            Vector2 pos = new Vector2(start.x-2f, start.z) - perpendicular * offset;
            positions.Add(pos);
        }
        return positions;
    }

    private Vector2 GetCenterOffset(List<Vector2> positions)
    {
        float minY = float.MaxValue;
        float maxY = float.MinValue;
        float minX = float.MaxValue;
        float maxX = float.MinValue;
        for (int i = 0; i < positions.Count; i++)
        {
            if (positions[i].y < minY)
            {
                minY = positions[i].y;
            }
            if (positions[i].y > maxY)
            {
                maxY = positions[i].y;
            }
            if (positions[i].x < minX)
            {
                minX = positions[i].x;
            }
            if (positions[i].x > maxX)
            {
                maxX = positions[i].x;
            }
        }
        return new Vector2(0.5f*(maxX-minX), 0.5f*(maxY-minY));
    }
    static void ClearArray(int[,] array)
    {
        int rows = array.GetLength(0);
        int cols = array.GetLength(1);
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                array[i, j] = 0; // ��ÿ��Ԫ������Ϊ 0
            }
        }
    }
}
