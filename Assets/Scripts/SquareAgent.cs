using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine;
using System.Collections.Generic;
using System;
using Random = UnityEngine.Random;
using static UnityEngine.GraphicsBuffer;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime;
using System.Linq;
using System.Collections;
using RVO;
using Vector2 = UnityEngine.Vector2;
using System.IO;
using UnityEngine.UIElements;
using System.Reflection;

public class SquareAgent : Unity.MLAgents.Agent
{

    public int GroupID = 0;
    public int AgentID = 0;
    public Material AgentMaterial;
    public Rigidbody AgentRb;
    public CBBA agentCBBA;
    public int IsReachGoal = 0;
    public int IsNearGoal = 0;
    public int IsCenterConsistent = 0;
    //认为初始阶段以比例1放缩
    public int IsScaleConsistent = 1;
    public int IsFormated = 0;
    public int IsAllAssigned = 0;
    public int IsSelfAssigned = 0;
    public int IsSelfNeedScale = 0;
    public int IsAllNeedScale = 0; 
    public int IsSelfGetSimFormationIndex = 0;
    public int IsAllGetSimFormationIndex = 0;
    private bool IsStop = false;
    private int Signal = 0;
    private bool updateCenter = false;
    public bool AllCanScale = false;
    public bool AllNeedScale = false;
    //认为初始阶段以比例1放缩
    public bool AllScaleConsistent = true;
    //public bool AllScaled = false;
    public bool AllAssigned = false;
    public bool AllFormated = false;
    public int NumNeighbors = 5;
    public List<int> ids;
    public List<Vector3> offsets;
    public Vector3 centerOffset;
    public Vector3 goalPos;
    public Vector3 formationPos;
    public Vector3 centerPos;
    public Vector3 groupGoalPos;
    private Vector3 uk_center;
    private float uk_scale;
    public float commRadius;
    private float turnSpeed = 90f;
    private float moveSpeed = 0.10f;
    private float standardSpeed = 0.75f;
    private Vector3 lastVel = Vector3.zero;

    public float goalError = 0f;
    public float angleError = 0f;
    private float lastgoalError = 0;
    private float[] velXErrorList;
    private float[] velZErrorList;
    private float[] lastvelXErrorList;
    private float[] lastvelZErrorList;
    private float[] formationXErrorList;
    private float[] formationZErrorList;
    private float[] lastformationXErrorList;
    private float[] lastformationZErrorList;

    public int Task_Num = 6;
    public int Agent_Num = 6;
    public int L_T = 0;

    private int negotiateCenterTimes = 0;
    public float disThreshold = 1f;
    public float angleThreshold = 0;
    public float velThreshold = 0f;
    private EnvironmentParameters resetParams;
    private float errorThreshold = 0f;

    //public List<Vector3> pathPoint = new List<Vector3>();
    private RayPerceptionSensorComponent3D ObstacleSensor;
    private RayPerceptionOutput ObstacleSensorOutput = new RayPerceptionOutput();
    private RayPerceptionSensorComponent3D AgentSensor;
    private RayPerceptionOutput AgentSensorOutput = new RayPerceptionOutput();

    public List<List<List<Vector2>>> relativeFormationPosLists = new List<List<List<Vector2>>>();
    public int desiredFormationIndex = 0;
    public float selfScale = 1f;
    public float currentAllScale = 1f;
    public float lastAllScale = 1f;
    private float space = 2.5f;
    private int onlygoal = 1;
    private float envshortAwarenessWeight = 0f;
    private float envlongAwarenessWeight = 0f;
    private int formatedTimes = 0;
    private float starttime;
    private int step = 0;
    public bool fixbug = false;


    public void Start()
    {
        this.AgentRb = this.GetComponent<Rigidbody>();
        this.resetParams = Academy.Instance.EnvironmentParameters;
        //this.agentHandler = Simulator.Instance.addAgent(transform.localPosition, agentHeight, neighborDist, maxNeighbors, timeHorizon, timeHorizonObst, radius, maxSpeed, Vector2.zero, isKinematic);
    }


    public void ResetAgentWithAll()
    {
        this.resetParams = Academy.Instance.EnvironmentParameters;
        this.errorThreshold = this.resetParams.GetWithDefault("errorThreshold", 0.15f); //0.15
        this.disThreshold = this.resetParams.GetWithDefault("disThreshold", 0.75f);
        this.angleThreshold = this.resetParams.GetWithDefault("angleThreshold", 30f);
        this.velThreshold = this.resetParams.GetWithDefault("velThreshold", 0.12f); //0.10
        // this.desiredFormationIndex = 0;
        this.selfScale = 1f;
        //this.currentAllScale = 1f;
        this.lastAllScale = 1f;
        this.onlygoal = (int)this.resetParams.GetWithDefault("onlygoal", 0f); //0.10
        this.envshortAwarenessWeight = 0f;
        this.envlongAwarenessWeight = 0f;
        this.centerOffset = Vector3.zero;
        this.formatedTimes = 0;

        this.AgentRb = this.GetComponent<Rigidbody>();
        this.AgentRb.velocity = Vector3.zero;
        this.AgentRb.angularVelocity = Vector3.zero;
        this.transform.LookAt(this.goalPos);
        //this.transform.GetComponent<TrailRenderer>().Clear();
        this.ObstacleSensor = this.AgentRb.transform.Find("obstaclesensor").GetComponent<RayPerceptionSensorComponent3D>();
        this.AgentSensor = this.AgentRb.transform.Find("agentsensor").GetComponent<RayPerceptionSensorComponent3D>();
        this.AgentRb.transform.Find("trail").GetComponent<TrailRenderer>().Clear();
        this.centerPos = this.transform.localPosition;

        this.IsStop = false;
        this.IsReachGoal = 0;
        this.IsNearGoal = 0;
        this.IsScaleConsistent = 1;
        this.IsCenterConsistent = 0;
        this.IsSelfNeedScale = 0;
        this.IsAllNeedScale = 0;
        this.IsAllAssigned = 0;
        this.IsSelfAssigned = 0;
        this.IsSelfGetSimFormationIndex = 0;
        this.IsAllGetSimFormationIndex = 0;
        this.Signal = 0;
        this.updateCenter = false;
        this.AllAssigned = false;
        //this.AllScaled = false;
        this.AllFormated = false;
        this.AllScaleConsistent = true;
        this.lastVel = Vector3.zero;
        this.velXErrorList = new float[this.Agent_Num-1];
        this.velZErrorList = new float[this.Agent_Num-1];
        this.lastvelXErrorList = new float[this.Agent_Num-1];
        this.lastvelZErrorList = new float[this.Agent_Num-1];
        this.formationXErrorList = new float[this.Agent_Num-1];
        this.formationZErrorList = new float[this.Agent_Num-1];
        this.lastformationXErrorList = new float[this.Agent_Num-1];
        this.lastformationZErrorList = new float[this.Agent_Num-1];
        this.negotiateCenterTimes = 0;
        this.ids.Clear();
        this.offsets.Clear();
        this.starttime = Time.time;
        this.step = 0;
        this.agentCBBA = this.AgentRb.transform.Find("cbba").GetComponent<CBBA>();
        this.fixbug = false;

    }




    public override void CollectObservations(VectorSensor sensor)
    {
        //if(Vector3.Distance(this.goalPos, this.transform.localPosition)<1f && this.pathPointIndex<this.pathPoint.Count-1)
        //{
        //    this.goalPos = this.pathPoint[pathPointIndex + 1];
        //    pathPointIndex++;
        //}
        sensor.AddObservation(this.AgentRb.velocity.normalized.x);// 1
        sensor.AddObservation(this.AgentRb.velocity.normalized.z);// 1
        sensor.AddObservation((this.goalPos - this.transform.localPosition).normalized.x);// 1
        sensor.AddObservation((this.goalPos - this.transform.localPosition).normalized.z);// 1
        sensor.AddObservation(this.currentAllScale);
        //Debug.Log(this.desiredScale);
        GameObject[] otherAgents = GameObject.FindGameObjectsWithTag("Agent");
        //Debug.Log("this.id:" + this.AgentID);
        foreach (GameObject agent in otherAgents)
        {
            if (this.ids.Contains(agent.GetComponent<SquareAgent>().AgentID))
            {
                int index = this.ids.IndexOf(agent.GetComponent<SquareAgent>().AgentID);
                if(Vector3.Distance(agent.transform.localPosition, this.transform.localPosition) <= this.commRadius)
                {
                    //Debug.Log("agent.GetComponent<SquareAgent>().AgentID:"+agent.GetComponent<SquareAgent>().AgentID);
                    //Debug.Log("this.offsets[index]:"+this.offsets[index]);
                    sensor.AddObservation(((agent.transform.localPosition - this.transform.localPosition - this.offsets[index]).normalized.x));
                    sensor.AddObservation(((agent.transform.localPosition - this.transform.localPosition - this.offsets[index]).normalized.z));
                    sensor.AddObservation(((agent.GetComponent<SquareAgent>().AgentRb.velocity - this.AgentRb.velocity).normalized.x));
                    sensor.AddObservation(((agent.GetComponent<SquareAgent>().AgentRb.velocity - this.AgentRb.velocity).normalized.z));
                }
                else
                {   
                    sensor.AddObservation(0f);
                    sensor.AddObservation(0f);
                    sensor.AddObservation(0f);
                    sensor.AddObservation(0f);
                }
            }
        }
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        if (this.IsStop)
        {
            //Debug.Log("Stop!");
            this.AgentRb.velocity = Vector3.zero;
            this.AgentRb.angularVelocity = Vector3.zero;
        }
        else
        {
            //RL学习编队速度
            this.MoveAgentContinuous(actionBuffers.ContinuousActions);
        }
        this.AgentUpdate();
        
        if(this.IsCenterConsistent==0)
        {
            CalCenterPos();
        }
        else if(this.IsCenterConsistent==1 && this.IsAllAssigned == 0)
        {
            if(this.Signal == 0)
            {
                this.Signal = 1;
                //Debug.Log("formation index:"+this.desiredFormationIndex);
                InitDistributedCBBA(this.desiredFormationIndex, this.currentAllScale, this.space);
                //Debug.Log("centerPos;"+this.centerPos);               
            }
            else if(this.Signal == 1)
            {
                this.Signal = 2;
                RunCBBAIteration1();
                //Debug.Log("id:"+this.AgentID);
            }
            else if(this.Signal == 2)
            {
                this.Signal = 3;
                RunCBBAIteration2();
                //Debug.Log("id:::::::::"+this.AgentID);
            }       
            else if(this.Signal == 3)
            {
                this.Signal = 1;
                RunCBBAIteration3();
                //Debug.Log("id::::::::::::::::::::"+this.AgentID);
            }
        }
        else if(this.IsCenterConsistent==1 && this.IsAllAssigned == 1 && this.fixbug == true)
        {
            if(this.Signal <= 3)
            {
                this.Signal = 4;
                this.AgentID = this.agentCBBA.path[0];
                //Debug.Log("path:" + this.agentCBBA.path[0]);
                this.formationPos.x = this.m_RelativeFormationPos[this.AgentID].x;
                this.formationPos.y = 0f;
                this.formationPos.z = this.m_RelativeFormationPos[this.AgentID].y;
                this.goalPos = this.groupGoalPos + this.formationPos;
            }
            else if(this.Signal == 4)
            {
                this.Signal = 5;
                this.ids.Clear();
                this.offsets.Clear();
                GameObject[] otherAgents = GameObject.FindGameObjectsWithTag("Agent");
                //Debug.Log("this.ids:" + this.ids.Count);
                foreach (GameObject agent in otherAgents)
                {
                    if (agent.GetComponent<SquareAgent>().AgentID != this.AgentID && Vector2.Distance(agent.GetComponent<SquareAgent>().formationPos, this.formationPos) < this.commRadius)
                    {
                        this.ids.Add(agent.GetComponent<SquareAgent>().AgentID);
                        this.offsets.Add(agent.GetComponent<SquareAgent>().formationPos - this.formationPos);
                    }
                }
                //this.agentCBBA = new CBBA();
            }
            // else
            // {
            //     this.step++;
            //     if(Time.time - this.starttime == 50)
            //     {
            //         if(this.AgentID == 0)
            //         {
            //             this.selfScale = 0.8f;
            //             Debug.Log("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!1");
            //         }
            //         //this.desiredFormationIndex = 1;
                    
            //         //this.agentCBBA = new CBBA();
                    
            //     }

            //     if(this.IsAllNeedScale == 1)
            //     {
            //         if(GetMinScaleAndCheckConverage())
            //         {
            //             if(this.Signal == 5)
            //             {
            //                 this.Signal = 6;
            //                 GetMostSimilarityFormation();
            //                 this.IsSelfGetSimFormationIndex = 1;
            //             }
            //             if(CheckAllGetSimFormation())
            //             {
            //                 if(GetFormationIndexAndCheckConverage())
            //                 {
            //                     this.Signal = 0;
            //                     this.centerPos = this.transform.localPosition;
            //                     this.selfScale = this.currentAllScale;
            //                     this.lastAllScale = this.currentAllScale;
            //                     this.IsCenterConsistent = 0;
            //                     this.IsAllAssigned = 0;
            //                     this.IsSelfAssigned = 0;
            //                     // this.selfScale = 0.8f;
            //                     // this.desiredFormationIndex = 0;
                                
            //                     this.IsSelfNeedScale = 0;
            //                     this.IsAllNeedScale = 0;
            //                     Debug.Log("this.AgentyId:"+this.AgentID);
            //                     //Debug.Log("this.currentAllScale:"+this.currentAllScale);
            //                     Debug.Log("deisreeeeeeeeeeeee formation index: "+this.desiredFormationIndex);
            //                 }
            //             }


            //         }
            //     }
            //     if(CheckNeedScale() && this.IsAllNeedScale == 0)
            //     {
            //         this.IsAllNeedScale = 1;
            //         this.agentCBBA = new CBBA();
            //         this.currentAllScale = this.selfScale;
            //         Debug.Log("ID"+this.AgentID+" is all need scale!");
            //     }
            //     if(this.selfScale != this.lastAllScale && this.IsSelfNeedScale == 0)
            //     {
            //         this.IsSelfNeedScale = 1;         
            //         Debug.Log("ID "+this.AgentID+" is self need scale!");
            //     }


            //     // //Debug.Log("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            //     // if(this.selfScale != this.lastAllScale)
            //     // {
            //     //     if(this.Signal==5)
            //     //     {
            //     //         this.Signal = 6;
            //     //         this.currentAllScale = this.selfScale;
            //     //         Debug.Log("selfscale:"+this.selfScale);
            //     //     }
            //     //     if(GetMinScaleAndCheckConverage())
            //     //     {
            //     //         this.Signal = 0;
            //     //         this.lastAllScale = this.currentAllScale;
            //     //         this.IsCenterConsistent = 0;
            //     //         this.IsAllAssigned = 0;
            //     //         this.IsSelfAssigned = 0;
            //     //         Debug.Log("this.currentAllScale:"+this.currentAllScale);
            //     //     }
            //     // }
            // }
        }




        // //（所有智能体中心全部收敛）编队约束和目标已经全部分配完毕，判断当前是否形成相应编队
        // if (this.AllAssigned && this.IsFormated==0)
        // {
        //     this.updateCenter = false;
        //     this.CalIsFormated();
        //     // if(this.IsFormated==1)
        //     // {
        //     //     Debug.Log("ID: "+this.AgentID);
        //     //     Debug.Log("Formated: "+this.IsFormated);
        //     // }

        // }
        // //所有智能体都符合误差范围内的编队约束，形成了（旧）编队形状
        // if (this.AllFormated&&this.IsNeedScale==0)
        // {
        //     //当前伸缩因子与之前协商的伸缩因子差距较大，编队需要放缩（执行一次）
        //     if (Math.Abs(this.currentScale - this.negotiateScale) >= 0.15f)
        //     {
        //         this.IsNeedScale = 1;
        //     }

        // }
        
        // //之前的编队约束和目标已经分配好，但是需要放缩编队，新中心、新伸缩因子开启计算（执行一次）
        // if(this.AllNeedScale == true)
        // {
        //     if(this.AllAssigned==false&&this.IsScaleConsistent == 0)
        //     {
        //         this.CalScale();
        //     }
        //     if(this.AllAssigned == true)
        //     {
        //         this.AllAssigned = false;
        //         //this.AllScaled = false;
        //         //this.IsFormated = 0;
        //         this.IsScaleConsistent = 0;
        //         this.IsCenterConsistent = 0;
        //         this.negotiateScale = this.currentScale;
        //         //Debug.Log(this.AgentID+"::::Scale:"+this.negotiateScale);
        //         // if(this.IsScaleConsistent==1)
        //         // {
        //         //     Debug.Log(this.AgentID+".Scale:"+this.negotiateScale);
        //         //     //Debug.Log("Scale: "+this.negotiateScale);
        //         // }
        //     }
        // }
        // //Debug.Log("IsCenterConsistent:" +this.IsCenterConsistent);
        // //Debug.Log("AllScaleConsistent: "+this.AllScaleConsistent);
        // //只有重新协商编队中心时才有作用
        // if (this.AllScaleConsistent == true)
        // {
        //     if(this.IsCenterConsistent == 0 && this.updateCenter==true)
        //     {
        //         this.CalCenterPos();
        //         //Debug.Log(this.AgentID+"CenterPos:"+this.centerPos);
        //     }
        //     if(this.updateCenter==false)
        //     {
        //         this.centerPos = this.transform.localPosition;
        //         //Debug.Log(this.AgentID+":::update:"+this.centerPos);
        //         this.updateCenter = true;
        //     }
        // }

        // !!!!!!!!!!!!!!!!!!!!!!
        // //（所有智能体中心全部收敛）编队约束和目标已经全部分配完毕，判断当前是否形成相应编队
        // if (this.AllAssigned && this.IsCanScale==0)
        // {            
        //     this.updateCenter = false;
        //     this.formatedTimes++;
        //     if(this.formatedTimes>150)
        //     {
        //         this.IsCanScale = 1;
        //         this.formatedTimes = 0;
        //     }
        //     //this.CalIsFormated();
        // }
        // //所有智能体都符合误差范围内的编队约束，形成了（旧）编队形状
        // if (this.AllCanScale&&this.IsNeedScale==0)
        // {
        //     if(this.IsNearGoal==0)
        //     {
        //         //当前伸缩因子与之前协商的伸缩因子差距较大，编队需要放缩（执行一次）
        //         if (this.currentScale != this.allScale)
        //         {
        //             this.IsNeedScale = 1;
        //         }
        //     }
        // }
        // // //所有智能体都符合误差范围内的编队约束，形成了（旧）编队形状
        // // if (this.AllFormated&&this.IsNeedScale==0)
        // // {
        // //     if(this.IsNearGoal==0)
        // //     {
        // //         //当前伸缩因子与之前协商的伸缩因子差距较大，编队需要放缩（执行一次）
        // //         if (Math.Abs(this.outputScale - this.negotiateScale) >= 0.15f)
        // //         {
        // //             this.IsNeedScale = 1;
        // //         }
        // //     }
        // // }
        
        // //之前的编队约束和目标已经分配好，但是需要放缩编队，新中心、新伸缩因子开启计算（执行一次）
        // if(this.AllNeedScale == true)
        // {
        //     if(this.AllAssigned==false&&this.IsScaleConsistent == 0)
        //     {
        //         this.CalScale();
        //     }
        //     if(this.AllAssigned == true)
        //     {
        //         this.AllAssigned = false;
        //         this.IsScaleConsistent = 0;
        //         this.IsCenterConsistent = 0;
        //         this.negotiateScale = this.currentScale;
        //     }
        // }

        // //只有重新协商编队中心时才有作用
        // if (this.AllScaleConsistent == true)
        // {
        //     if(this.IsCenterConsistent == 0 && this.updateCenter==true)
        //     {
        //         this.CalCenterPos();
        //         //Debug.Log(this.AgentID+"CenterPos:"+this.centerPos);
        //     }
        //     if(this.updateCenter==false)
        //     {
        //         this.centerPos = this.transform.localPosition;
        //         //Debug.Log(this.AgentID+":::update:"+this.centerPos);
        //         this.updateCenter = true;
        //     }
        // }
        // !!!!!!!!!!!!!!!!!!!!!!
    }

    //连续运动
    private void MoveAgentContinuous(ActionSegment<float> act)
    {
        //测试用
        //this.AgentRb.velocity += this.consistencyVel;
        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;
        var continuousActions = act;//连续运动控制

        continuousActions[0] = Mathf.Clamp(continuousActions[0], 0f, 1f);
        continuousActions[1] = Mathf.Clamp(continuousActions[1], -1f, 1f);
        dirToGo = transform.forward * continuousActions[0];
        rotateDir = transform.up * continuousActions[1];
        this.AgentRb.transform.Rotate(rotateDir, Math.Abs(continuousActions[1]) * Time.fixedDeltaTime * 150f);
        this.AgentRb.AddForce(dirToGo * this.moveSpeed, ForceMode.VelocityChange);        

    }



    //Assign rewards to the agent 
    private void AgentUpdate()
    {
        this.GetNearCollide();
        this.goalError = Vector3.Distance(this.goalPos, this.transform.localPosition);
        if(this.goalError < 10f)
        {
            this.IsNearGoal = 1;
        }

        if (this.IsAllAssigned == 0)    //部分智能体的预测中心未收敛，中心点不断迭代更新，目标点为中心点，
        {
            //Debug.Log("AAAAAAAAAA");
            this.IsReachGoal = 0;
            this.goalError = Vector3.Distance(this.centerPos, this.transform.localPosition);
            this.angleError = Vector3.Angle(this.transform.forward, this.centerPos - this.transform.localPosition);
            if (this.goalError < this.lastgoalError)
            {
                AddReward(75f * Math.Abs(this.lastgoalError - this.goalError));
            }
            if (this.angleError < this.angleThreshold)
            {
                AddReward(0.25f * Math.Abs(this.angleThreshold - this.angleError) / this.angleThreshold);
            }
            AddReward(-4f * Math.Abs(this.AgentRb.velocity.magnitude - 0.5f));
            AddReward(-20f * Vector3.Distance(this.AgentRb.velocity, this.lastVel));
            this.lastgoalError = this.goalError;
            this.lastVel = this.AgentRb.velocity;
        }
        else
        {
            if (this.IsReachGoal == 0)  //当前智能体的预测中心点收敛，，但未到达最终目标点
            {
                if(this.onlygoal==0)
                {
                    //Debug.Log("this.Agent ID:"+this.AgentID);
                    GameObject[] otherAgents = GameObject.FindGameObjectsWithTag("Agent");
                    foreach (GameObject agent in otherAgents)
                    {
                        if (this.ids.Contains(agent.GetComponent<SquareAgent>().AgentID)&&agent.GetComponent<SquareAgent>().AgentID!=this.AgentID)
                        {                            
                            if (agent.GetComponent<SquareAgent>().IsReachGoal==0 && Vector3.Distance(agent.transform.localPosition, this.transform.localPosition) <= this.commRadius)
                            {
                                int index = this.ids.IndexOf(agent.GetComponent<SquareAgent>().AgentID);
                                //Debug.Log("agent.GetComponent<SquareAgent>().AgentID"+agent.GetComponent<SquareAgent>().AgentID);
                                //Debug.Log("offset:"+this.offsets[index]);
                                this.formationXErrorList[index] = Math.Abs((agent.transform.localPosition - this.transform.localPosition - this.offsets[index]).x);
                                this.formationZErrorList[index] = Math.Abs((agent.transform.localPosition - this.transform.localPosition - this.offsets[index]).z);
                                this.velXErrorList[index] = Math.Abs((agent.GetComponent<SquareAgent>().AgentRb.velocity - this.AgentRb.velocity).x);
                                this.velZErrorList[index] = Math.Abs((agent.GetComponent<SquareAgent>().AgentRb.velocity - this.AgentRb.velocity).z);
                                if (this.velXErrorList[index] < this.velThreshold)
                                {
                                    AddReward(0.75f * (1f /(this.offsets[index].magnitude+2f))* Math.Abs(this.velThreshold - this.velXErrorList[index]) / this.velThreshold);
                                    // Debug.Log("GGGGGGGGGGGGGGGGGGGGG:" + (0.5f * (1f /(this.offsets[index].magnitude+2f))* (this.velThreshold - this.velXErrorList[index]) / this.velThreshold));
                                    
                                }
                                if (this.velZErrorList[index] < this.velThreshold)
                                {
                                    AddReward(0.75f * (1f / (this.offsets[index].magnitude+2f)) * Math.Abs(this.velThreshold - this.velZErrorList[index]) / this.velThreshold);
                                    // Debug.Log("H:" + (0.5f * (1f /(this.offsets[index].magnitude+2f))* (this.velThreshold - this.velZErrorList[index]) / this.velThreshold));
                                }
                                if (this.velXErrorList[index] < this.lastvelXErrorList[index])
                                {
                                    //AddReward(+0.05f);
                                    AddReward(30f * (1f / (this.offsets[index].magnitude+2f)) * Math.Abs(this.lastvelXErrorList[index] - this.velXErrorList[index]));
                                    //Debug.Log("this.velXErrorList[index]:"+this.velXErrorList[index]);
                                    //Debug.Log("A:" + (40f * (1f / (this.offsets[index].magnitude+1f)) * (this.lastvelXErrorList[index] - this.velXErrorList[index])));
                                        
                                }
                                if (this.velZErrorList[index] < this.lastvelZErrorList[index])
                                {
                                    //AddReward(+0.05f);
                                    AddReward(30f * (1f / (this.offsets[index].magnitude+2f)) * Math.Abs(this.lastvelZErrorList[index] - this.velZErrorList[index]));
                                    // if(25f * (1f / (this.offsets[index].magnitude+2f)) * (this.lastvelZErrorList[index] - this.velZErrorList[index])>1)
                                    // {

                                    // }
                                    // Debug.Log("j: " + agent.GetComponent<SquareAgent>().AgentID);
                                    // Debug.Log("id:"+this.AgentID);
                                    // Debug.Log("this.lastvelZErrorList[index]:"+this.lastvelZErrorList[index]);
                                    // Debug.Log("this.velZErrorList[index]:"+this.velZErrorList[index]);
                                    //Debug.Log("B:" + 40f * (1f / (this.offsets[index].magnitude+1f)) * (this.lastvelZErrorList[index] - this.velZErrorList[index]));
                                    // Debug.Log("agent.Vel:"+agent.GetComponent<SquareAgent>().AgentRb.velocity);
                                    // Debug.Log("vel:"+this.AgentRb.velocity);
                                    
                                }
                                if (this.formationXErrorList[index] < this.errorThreshold * Math.Abs(this.offsets[index].x))
                                {
                                    //AddReward(0.001f);
                                    //AddReward(0.5f * (1f / (this.offsets[index].magnitude+0.5f)) * Math.Abs(Math.Abs(this.offsets[index].x) - this.formationXErrorList[index]) / Math.Abs(this.offsets[index].x));
                                    AddReward(0.75f * (1f / (this.offsets[index].magnitude+2f)) * Math.Abs(Math.Abs(this.offsets[index].x) - this.formationXErrorList[index]));
                                    // if((0.75f * (1f / (this.offsets[index].magnitude+0.5f)) * (Math.Abs(this.offsets[index].x) - this.formationXErrorList[index]) / Math.Abs(this.offsets[index].x))>0.5f)
                                    // {
                                    //     Debug.Log("C:" +0.75f * (1f / (this.offsets[index].magnitude+0.5f)) * (Math.Abs(this.offsets[index].x) - this.formationXErrorList[index]) / Math.Abs(this.offsets[index].x));
                                    // }
                                    //Debug.Log("C:" +0.75f * (1f / (this.offsets[index].magnitude+1f)) * (Math.Abs(this.offsets[index].x) - this.formationXErrorList[index]) / Math.Abs(this.offsets[index].x));
                                                                        
                                }
                                else
                                {
                                    AddReward(-0.1f);
                                }

                                if (this.formationZErrorList[index] < this.errorThreshold * Math.Abs(this.offsets[index].z))
                                {
                                    //AddReward(0.001f);
                                    //AddReward(0.5f * (1f / (this.offsets[index].magnitude+0.5f)) * Math.Abs(Math.Abs(this.offsets[index].z) - this.formationZErrorList[index]) / Math.Abs(this.offsets[index].z));
                                    AddReward(0.75f * (1f / (this.offsets[index].magnitude+2f)) * Math.Abs(Math.Abs(this.offsets[index].z) - this.formationZErrorList[index]));
                                    //    if((0.5f * (1f / (this.offsets[index].magnitude+0.5f)) * (Math.Abs(this.offsets[index].z) - this.formationZErrorList[index]) / Math.Abs(this.offsets[index].z))>1)
                                    //    {
                                    // Debug.Log("D:" + 0.75f * (1f / (this.offsets[index].magnitude+1f)) * (Math.Abs(this.offsets[index].z) - this.formationZErrorList[index]) / Math.Abs(this.offsets[index].z));
                                    // //    }                                  
                                }
                                else
                                {
                                    AddReward(-0.1f);
                                }
                                if (this.formationXErrorList[index] < this.lastformationXErrorList[index])
                                {
                                    // AddReward(+0.001f);
                                    //AddReward(25f * (1f / (this.offsets[index].magnitude+0.5f)) * Math.Abs(this.lastformationXErrorList[index] - this.formationXErrorList[index]));
                                    AddReward(35f * (1f / (this.offsets[index].magnitude+2f)) * Math.Abs(this.lastformationXErrorList[index] - this.formationXErrorList[index]));
                                    //    if((25f * (1f / (this.offsets[index].magnitude+0.5f)) * (this.lastformationXErrorList[index] - this.formationXErrorList[index]))>1)
                                    //    {
                                    // Debug.Log("this.lastformationXErrorList[index]:"+this.lastformationXErrorList[index]);
                                    // Debug.Log("this.formationXErrorList[index]:"+this.formationXErrorList[index]);
                                    // Debug.Log("this.offsets[index]:"+this.offsets[index]);
                                    // Debug.Log("E:" + 40f * (1f / (this.offsets[index].magnitude+1f)) * (this.lastformationXErrorList[index] - this.formationXErrorList[index]));
                                    //    }                                   
                                }
                                if (this.formationZErrorList[index] < this.lastformationZErrorList[index])
                                {
                                    //AddReward(+0.001f);
                                    AddReward(35f * (1f / (this.offsets[index].magnitude+2f)) * Math.Abs(this.lastformationZErrorList[index] - this.formationZErrorList[index]));
                                    
                                }
                                this.lastvelXErrorList[index] = this.velXErrorList[index];
                                this.lastvelZErrorList[index] = this.velZErrorList[index];
                                this.lastformationXErrorList[index] = this.formationXErrorList[index];
                                this.lastformationZErrorList[index] = this.formationZErrorList[index];
                            }
                            else
                            {
                                //AddReward(-0.075f);                                
                                int index = this.ids.IndexOf(agent.GetComponent<SquareAgent>().AgentID);
                                this.lastvelXErrorList[index] = 0f;
                                this.lastvelZErrorList[index] = 0f;
                                this.lastformationXErrorList[index] = 0f;
                                this.lastformationZErrorList[index] = 0f;
                            }
                        }
                    }
                }

                this.IsStop = false;
                this.goalError = Vector3.Distance(this.goalPos, this.transform.localPosition);
                this.angleError = Vector3.Angle(this.transform.forward, this.goalPos - this.transform.localPosition);
                if (this.goalError < this.disThreshold)
                {
                    AddReward(+200f);
                    this.IsReachGoal = 1;
                }
                else
                {
                    if (this.goalError < this.lastgoalError)
                    {
                        AddReward(75f * Math.Abs(this.lastgoalError - this.goalError));
                        //Debug.Log("goal Error:" + 50f * (this.lastgoalError - this.goalError));
                    }
                    else
                    {
                        AddReward(-0.15f);
                    }
                    if (this.angleError < this.angleThreshold)
                    {
                        AddReward(0.25f * Math.Abs(this.angleThreshold - this.angleError) / this.angleThreshold);
                        //Debug.Log("angle Error:" + 0.15f * (this.angleThreshold - this.angleError) / this.angleThreshold);
                    }
                    else
                    {
                        if(this.angleError>90)
                        {
                            AddReward(-0.1f * Math.Abs(this.angleError-90) / this.angleThreshold);
                        }
                    }
                    // AddReward(-3.0f * Math.Abs(this.AgentRb.velocity.magnitude - 0.35f));
                    AddReward(-4f * Math.Abs(this.AgentRb.velocity.magnitude - 0.5f));
                    AddReward(-15f * Vector3.Distance(this.AgentRb.velocity, this.lastVel));
                    //Debug.Log("velMa:" + (-2.5f * Math.Abs(this.AgentRb.velocity.magnitude - 0.35f)));
                    //Debug.Log("velChange:" + (-20f * Vector3.Distance(this.AgentRb.velocity, this.lastVel)));
                    AddReward(-0.15f);
                    this.lastgoalError = this.goalError;
                    this.lastVel = this.AgentRb.velocity;
                }
            }
            else
            {
                AddReward(+0.35f);
                this.IsStop = true;
                this.IsReachGoal = 1;
                this.AgentRb.velocity = Vector3.zero;
                this.AgentRb.angularVelocity = Vector3.zero;
            }
        }
    }


    private void CalIsFormated()
    {
        if(this.IsFormated==0)
        {
            this.IsFormated = 1;
            GameObject[] otherAgents = GameObject.FindGameObjectsWithTag("Agent");
            foreach (GameObject agent in otherAgents)
            {
                if (this.ids.Contains(agent.GetComponent<SquareAgent>().AgentID))
                {
                    if (Vector3.Distance(agent.transform.localPosition, this.transform.localPosition)>0.5f && Vector3.Distance(agent.transform.localPosition, this.transform.localPosition) <= this.commRadius)
                    {
                        int index = this.ids.IndexOf(agent.GetComponent<SquareAgent>().AgentID);
                        if(Math.Abs(this.offsets[index].x)>0.5f && Math.Abs(this.offsets[index].z)>0.5f)
                        {
                            if (this.formationXErrorList[index] > 0.2 * Math.Abs(this.offsets[index].x) && this.formationZErrorList[index] > 0.2 * Math.Abs(this.offsets[index].z))
                            {
                                this.IsFormated = 0;
                            }
                        }

                    }
                }
            }
        }
    }


    private void CalCenterPos()
    {
        //Debug.Log("CenterPos!");
        if(this.IsCenterConsistent==0)
        {
            this.uk_center = Vector3.zero;
            GameObject[] otheragents = GameObject.FindGameObjectsWithTag("Agent");
            int NumOtherAgents = 0;
            bool consistent = true;
            foreach (GameObject agent in otheragents)
            {
                if (Vector3.Distance(this.transform.localPosition, agent.transform.localPosition) > 0.5f && Vector3.Distance(this.transform.localPosition, agent.transform.localPosition) < this.commRadius && this.transform.localPosition != agent.transform.localPosition)
                {
                    NumOtherAgents++;
                    this.uk_center += (5f / Vector3.Distance(this.transform.localPosition, agent.transform.localPosition)) * (agent.GetComponent<SquareAgent>().centerPos - this.centerPos);
                    if (Vector3.Distance(agent.GetComponent<SquareAgent>().centerPos, this.centerPos)>0.2f)
                    {
                        consistent = false;
                        //this.uk_center += (5f / Vector3.Distance(this.transform.localPosition, agent.transform.localPosition)) * (agent.GetComponent<SquareAgent>().centerPos - this.centerPos + agent.GetComponent<SquareAgent>().AgentRb.velocity*0.02f);
                        
                        //Debug.Log("agent.GetComponent<SquareAgent>().AgentRb.velocity*0.02f:"+agent.GetComponent<SquareAgent>().AgentRb.velocity*Time.fixedDeltaTime);
                    }
                }
            }
            //Debug.Log(this.AgentID+"::uk_center"+this.uk_center+":consistent:"+consistent);
            if (this.uk_center.magnitude <= 0.1 && this.uk_center.magnitude>0 && consistent==true)
            {
                this.negotiateCenterTimes += 1;
            }
            else
            {
                this.negotiateCenterTimes = 0;
            }
            if (this.negotiateCenterTimes > 3)
            {
                this.IsCenterConsistent = 1;
                this.lastgoalError = 0;
            }
            this.centerPos += (1f / (NumOtherAgents * 1f + 1f)) * this.uk_center;
        } 
        else
        {
            this.negotiateCenterTimes = 0;           
        }
    }


      // 公有状态变量（供其他智能体读取）
    [System.Serializable]
    public struct PublicState
    {
        public List<float> winningBidList;
        public List<int> winningAgentList;
        public Dictionary<int, int> timeStampList;
    }
    public PublicState publicState;
    // 私有状态
    private Dictionary<int, Vector2> m_RelativeFormationPos;
    private int m_ConsecutiveUnchanged = 0;
    private float m_NextIterationTime = 0f;

    // 初始化分布式数据
    public void InitDistributedCBBA(int formationIndex, float scale, float space)
    {
        List<Vector2> temp = this.GetFormation(formationIndex, this.centerPos, this.centerPos+10f*new Vector3(1f,0f, 0f), this.Agent_Num, space * scale);
        this.m_RelativeFormationPos = new Dictionary<int, Vector2>();
        for(int i=0;i<this.Agent_Num;i++)
        {
            //Debug.Log("temp:"+temp[i]);
            
            this.m_RelativeFormationPos[i] = temp[i]-new Vector2(this.centerPos.x, this.centerPos.z);
            //Debug.Log("formationPos;"+this.m_RelativeFormationPos[i]);
        }  
        // 初始化公有状态
        this.publicState = new PublicState();
        //this.AgentRb.transform.Find("cbba").GetComponent<CBBA>() = new CBBA();
        //this.agentCBBA = this.AgentRb.transform.Find("cbba").GetComponent<CBBA>();
        this.agentCBBA = new CBBA();
        this.agentCBBA.Init(this.AgentID, this.standardSpeed, this.Task_Num, this.Agent_Num, this.L_T, new Vector2(this.transform.localPosition.x-this.centerPos.x, this.transform.localPosition.z-this.centerPos.z));
        this.IsSelfNeedScale = 0;
        this.IsAllNeedScale = 0;
        // Debug.Log("AgentID:"+this.AgentID);
        // Debug.Log("CBBA Init!");
        
    }
    
    
    private void RunCBBAIteration1()
    {
        // 阶段1: 构建本地任务分配
        this.agentCBBA.BuildBundle(this.m_RelativeFormationPos);
        
        // 更新公有状态
        this.publicState.winningBidList = new List<float>(this.agentCBBA.winningBidList);
        this.publicState.winningAgentList = new List<int>(this.agentCBBA.winningAgentList);
        this.publicState.timeStampList = new Dictionary<int, int>(this.agentCBBA.timeStampList);
    }

    private void RunCBBAIteration2()
    {
        // 阶段2: 从其他智能体收集消息（通过读取公有状态）
        Dictionary<int, (List<float> bidList, List<int> agentList, Dictionary<int, int> timestamps)> messages = 
            CollectNeighborMessages();   
        // 将收集到的消息传递给CBBA
        this.agentCBBA.Y = messages;
    }

    // 执行一次CBBA迭代
    private void RunCBBAIteration3()
    {
        // 阶段3: 状态更新
        bool changed = this.agentCBBA.UpdateTask();
        // 更新公有状态（可能在UpdateTask中已改变）
        this.publicState.winningBidList = new List<float>(this.agentCBBA.winningBidList);
        this.publicState.winningAgentList = new List<int>(this.agentCBBA.winningAgentList);
        this.publicState.timeStampList = new Dictionary<int, int>(this.agentCBBA.timeStampList);        
        // 本地收敛检测
        if (changed)
        {
            this.m_ConsecutiveUnchanged++;
            if(this.m_ConsecutiveUnchanged >= 3)
            {
                this.IsSelfAssigned = 1;
                //Debug.Log("Assigned!");
            }
        }
        else
        {
            this.m_ConsecutiveUnchanged = 0;
        }
        
        // 检查全局收敛（仅当本地收敛时）
        if (this.IsSelfAssigned==1)
        {       
            // 如果全局收敛，停止执行
            if (CheckGlobalAssignConvergence())
            {
                //Debug.Log($"Agent {this.AgentID} 任务分配完成并收敛");
                this.IsAllAssigned = 1;
            }
        }
    }

    
    // 收集邻居的消息（只读取公共变量）
    private Dictionary<int, (List<float> bidList, List<int> agentList, Dictionary<int, int> timestamps)> CollectNeighborMessages()
    {
        Dictionary<int, (List<float>, List<int>, Dictionary<int, int>)> messages = 
            new Dictionary<int, (List<float>, List<int>, Dictionary<int, int>)>();
        
        // 找到场景中的所有智能体
        GameObject[] allAgents = GameObject.FindGameObjectsWithTag("Agent");
        Vector3 myPosition = this.transform.localPosition;
        
        // 只收集距离内的邻居消息
        foreach (var agent in allAgents)
        {
            if (agent.GetComponent<SquareAgent>().AgentID == this.AgentID) continue;
            
            // 检查距离是否在通信范围内
            if (Vector3.Distance(myPosition, agent.transform.position) <= this.commRadius)
            {
                // 收集邻居的公共状态
                var agentScript = agent.GetComponent<SquareAgent>();
                if (agentScript.publicState.winningBidList != null &&
                    agentScript.publicState.winningAgentList != null &&
                    agentScript.publicState.timeStampList != null)
                {
                    messages[agentScript.AgentID] = (
                        new List<float>(agentScript.publicState.winningBidList),
                        new List<int>(agentScript.publicState.winningAgentList),
                        new Dictionary<int, int>(agentScript.publicState.timeStampList)
                    );
                }
            }
        }        
        return messages;
    }
    
    // 分布式全局收敛检测
    private bool CheckGlobalAssignConvergence()
    {
        // 找到场景中的所有智能体
        GameObject[] allAgents = GameObject.FindGameObjectsWithTag("Agent");
        Vector3 myPosition = transform.localPosition;     
        // 只检查通信范围内的邻居
        int connectedNeighbors = 0;
        int convergedNeighbors = 0;     
        foreach (var agent in allAgents)
        {
            if (agent.GetComponent<SquareAgent>().AgentID == this.AgentID) continue;
            
            // 检查距离是否在通信范围内
            if (Vector3.Distance(myPosition, agent.GetComponent<SquareAgent>().transform.localPosition) <= this.commRadius)
            {
                connectedNeighbors++;
                if (agent.GetComponent<SquareAgent>().IsSelfAssigned==1) convergedNeighbors++;
            }
        }      
        // 如果所有邻居都收敛，则本智能体也收敛
        return (connectedNeighbors > 0) && (connectedNeighbors == convergedNeighbors);
    }

    //检测奖励：射线越短，被遮挡的射线数越多，则惩罚越多；射线长度阈值0.3
    private void GetNearCollide()
    {
        //Debug.Log("Agent ID::::" + this.AgentID);
        //output.RayOutputs = new RayPerceptionOutput.RayOutput[input.Angles.Count];
        this.envshortAwarenessWeight = 0f;
        this.ObstacleSensorOutput = RayPerceptionSensor.Perceive(this.ObstacleSensor.GetRayPerceptionInput(), false);
        this.AgentSensorOutput = RayPerceptionSensor.Perceive(this.AgentSensor.GetRayPerceptionInput(), false);
        List<float> leftshortRays = new List<float>();
        List<float> rightshortRays = new List<float>();
        List<float> leftlongRays = new List<float>();
        List<float> rightlongRays = new List<float>();
        List<int> shortRays = new List<int>();
        for (var rayIndex = 0; rayIndex < this.ObstacleSensor.GetRayPerceptionInput().Angles.Count; rayIndex++)
        {
            if(this.ObstacleSensorOutput.RayOutputs[rayIndex].HitFraction > 0 && this.ObstacleSensorOutput.RayOutputs[rayIndex].HitFraction<0.4f 
            && this.ObstacleSensorOutput.RayOutputs[rayIndex].HitGameObject.transform.tag == "Obstacle")
            {
                AddReward(1f-1f * (float)Math.Exp(2f*Math.Abs(0.3f - this.ObstacleSensorOutput.RayOutputs[rayIndex].HitFraction)));
            }
            if (rayIndex!=0&&rayIndex%2==0&&rayIndex!=this.ObstacleSensor.GetRayPerceptionInput().Angles.Count&&this.ObstacleSensorOutput.RayOutputs[rayIndex].HitFraction >= 0)
            {
                if(this.ObstacleSensorOutput.RayOutputs[rayIndex].HitFraction < 0.15f)
                {
                    leftshortRays.Add(this.ObstacleSensorOutput.RayOutputs[rayIndex].HitFraction);
                }
                else if(this.ObstacleSensorOutput.RayOutputs[rayIndex].HitFraction > 0.6f)
                {
                    leftlongRays.Add(this.ObstacleSensorOutput.RayOutputs[rayIndex].HitFraction);
                }               
                //Debug.Log("leftL"+this.ObstacleSensorOutput.RayOutputs[rayIndex].HitFraction);
            }
            if (rayIndex%2==1&&rayIndex!=this.ObstacleSensor.GetRayPerceptionInput().Angles.Count-1&&this.ObstacleSensorOutput.RayOutputs[rayIndex].HitFraction > 0f)
            {
                if(this.ObstacleSensorOutput.RayOutputs[rayIndex].HitFraction < 0.15f)
                {
                    rightshortRays.Add(this.ObstacleSensorOutput.RayOutputs[rayIndex].HitFraction);
                }
                else if(this.ObstacleSensorOutput.RayOutputs[rayIndex].HitFraction > 0.6f)
                {
                    rightlongRays.Add(this.ObstacleSensorOutput.RayOutputs[rayIndex].HitFraction);
                }
                
                //Debug.Log("rightL"+this.ObstacleSensorOutput.RayOutputs[rayIndex].HitFraction);
            }
            if (this.ObstacleSensorOutput.RayOutputs[rayIndex].HitFraction > 0 && this.ObstacleSensorOutput.RayOutputs[rayIndex].HitFraction < 0.15f)
            {
                shortRays.Add(rayIndex);
            }
        }
        float leftshortAwarness = 0f;
        float rightshortAwarness = 0f;
        if(leftshortRays.Count>1&&leftshortRays.Count>=leftlongRays.Count)
        {
            for(int i=0;i<leftshortRays.Count;i++)
            {
                leftshortAwarness += leftshortRays[i];
            }
            leftshortAwarness /= leftshortRays.Count;
        }
        if(rightshortRays.Count>1&&rightshortRays.Count>=rightlongRays.Count)
        {
            for(int i=0;i<rightshortRays.Count;i++)
            {
                rightshortAwarness += rightshortRays[i];
            }
            rightshortAwarness /= rightshortRays.Count;
        }

        if(leftshortRays.Count!=0&&rightshortRays.Count!=0)
        {
            if(leftshortAwarness<rightshortAwarness)
            {
                this.envshortAwarenessWeight = leftshortAwarness;
            }
            else
            {
                this.envshortAwarenessWeight = rightshortAwarness;
            }
        }
        else
        {
            if(leftshortAwarness!=0&&rightshortAwarness==0)
            {
                this.envshortAwarenessWeight = leftshortAwarness;
            }
            else if(leftshortAwarness==0&&rightshortAwarness!=0)
            {
                this.envshortAwarenessWeight = rightshortAwarness;
            }
            else
            {
                this.envshortAwarenessWeight = 0f;
            }
        }
        if(this.envshortAwarenessWeight > 0f)
        {
            if(this.lastAllScale==1f)
            {
                this.selfScale = 0.8f;
            }
            else if(this.lastAllScale==0.8f)
            {
                this.selfScale = 0.6f;
            }
            else
            {
                this.selfScale = 0.6f;
            }
        }
        int leftlongAwarnessCount = 0;
        int rightlongAwarnessCount = 0;
        if(leftlongRays.Count>4)
        {
            leftlongAwarnessCount = leftlongRays.Count;
        }
        if(rightlongRays.Count>4)
        {
            rightlongAwarnessCount = rightlongRays.Count;
        }
        if(leftlongAwarnessCount!=0&&rightlongAwarnessCount!=0)
        {
            // Debug.Log("leftlong:"+leftlongAwarnessCount);
            // Debug.Log("rightlong:"+rightlongAwarnessCount);
            if(this.lastAllScale==0.6f)
            {
                this.selfScale=0.8f;
            }
            else if(this.lastAllScale==0.8f)
            {
                this.selfScale=1.0f;
            }
            else
            {
                this.selfScale=1.0f;
            }
        }
        for (var rayIndex = 0; rayIndex < this.AgentSensor.GetRayPerceptionInput().Angles.Count; rayIndex++)
        {
            if (this.ObstacleSensorOutput.RayOutputs[rayIndex].HitFraction > 0 && this.AgentSensorOutput.RayOutputs[rayIndex].HitFraction < 0.2f)
            {
                AddReward(-0.5f * Math.Abs(0.2f - this.AgentSensorOutput.RayOutputs[rayIndex].HitFraction));
            }
        }
    }


    private bool CheckNeedScale()
    {
        GameObject[] allAgents = GameObject.FindGameObjectsWithTag("Agent");
        Vector3 myPosition = transform.localPosition;     
        // 只检查通信范围内的邻居
        int connectedNeighbors = 0;
        int convergedNeighbors = 0; 
        foreach (var agent in allAgents)
        {
            if (Vector3.Distance(myPosition, agent.GetComponent<SquareAgent>().transform.localPosition) <= this.commRadius)
            {
                connectedNeighbors++;
                if(agent.GetComponent<SquareAgent>().IsSelfNeedScale==1) 
                {
                    //Debug.Log("!!!!!!!!!!agentID:"+agent.GetComponent<SquareAgent>().AgentID);
                    this.IsSelfNeedScale = 1;
                    return true;
                }
            }
        }  
        // 如果所有邻居都收敛，则本智能体也收敛
        return false;
    }

    private bool GetMinScaleAndCheckConverage()
    {
        GameObject[] allAgents = GameObject.FindGameObjectsWithTag("Agent");
        Vector3 myPosition = transform.localPosition;     
        // 只检查通信范围内的邻居
        int connectedNeighbors = 0;
        int convergedNeighbors = 0; 
        foreach (var agent in allAgents)
        {
            // if (agent.GetComponent<SquareAgent>().AgentID == this.AgentID) continue;
            
            // // 检查距离是否在通信范围内
            if (Vector3.Distance(myPosition, agent.GetComponent<SquareAgent>().transform.localPosition) <= this.commRadius)
            {
                connectedNeighbors++;
                if(agent.GetComponent<SquareAgent>().currentAllScale<this.currentAllScale) this.currentAllScale=agent.GetComponent<SquareAgent>().currentAllScale;
                if (agent.GetComponent<SquareAgent>().currentAllScale==this.currentAllScale) convergedNeighbors++;
            }
        }  
        // 如果所有邻居都收敛，则本智能体也收敛
        return (connectedNeighbors > 0) && (connectedNeighbors == convergedNeighbors);
    }

    private bool CheckAllGetSimFormation()
    {
        GameObject[] allAgents = GameObject.FindGameObjectsWithTag("Agent");
        Vector3 myPosition = transform.localPosition;     
        int connectedNeighbors = 0;
        int convergedNeighbors = 0; 
        foreach (var agent in allAgents)
        {
            // if (agent.GetComponent<SquareAgent>().AgentID == this.AgentID) continue;
            if (Vector3.Distance(myPosition, agent.GetComponent<SquareAgent>().transform.localPosition) <= this.commRadius)
            {
                connectedNeighbors++;
                if (agent.GetComponent<SquareAgent>().IsSelfGetSimFormationIndex==1) convergedNeighbors++;
            }
        }  
        if((connectedNeighbors > 0) && (connectedNeighbors == convergedNeighbors))
        {
            //Debug.Log("All get sim formation!");
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool GetFormationIndexAndCheckConverage()
    {
        GameObject[] allAgents = GameObject.FindGameObjectsWithTag("Agent");
        Vector3 myPosition = transform.localPosition;     
        List<int> formationIndexCount = new List<int> {0, 0, 0, 0};
        int connectedNeighbors = 0;
        int convergedNeighbors = 0; 
        foreach (var agent in allAgents)
        {
            if (agent.GetComponent<SquareAgent>().AgentID == this.AgentID) continue;
            if (Vector3.Distance(myPosition, agent.GetComponent<SquareAgent>().transform.localPosition) <= this.commRadius)
            {
                connectedNeighbors++;
                formationIndexCount[agent.GetComponent<SquareAgent>().desiredFormationIndex] += 1;
                if (agent.GetComponent<SquareAgent>().desiredFormationIndex==this.desiredFormationIndex) convergedNeighbors++;
            }
        }  
        if((connectedNeighbors > 0) && (connectedNeighbors == convergedNeighbors))
        {
            return true;
        }
        else
        {
            this.desiredFormationIndex = formationIndexCount.IndexOf(formationIndexCount.Max());
            return false;
        }
    }


    private void GetMostSimilarityFormation()
    {
        List<Vector2> TriangleFormation = new List<Vector2>();
        List<Vector2> DoubleTriangleFormation = new List<Vector2>();
        List<Vector2> CircleFormation = new List<Vector2>();
        List<Vector2> RectangleFormation = new List<Vector2>();
        List<Vector2> LineFormation = new List<Vector2>();
        TriangleFormation = GetFormation(0, new Vector3(0, 0, 0), new Vector3(10, 0, 0), this.Agent_Num, 2f).ToList();
        DoubleTriangleFormation = GetFormation(1, new Vector3(0, 0, 0), new Vector3(10, 0, 0), this.Agent_Num, 2f).ToList();
        CircleFormation = GetFormation(2, new Vector3(0, 0, 0), new Vector3(10, 0, 0), this.Agent_Num, 2f).ToList();
        RectangleFormation = GetFormation(3, new Vector3(0, 0, 0), new Vector3(10, 0, 0), this.Agent_Num, 2f).ToList();
        //LineFormation = GetFormation(4, new Vector3(0, 0, 0), new Vector3(10, 0, 0), this.Agent_Num, 2f).ToList();
        this.relativeFormationPosLists.Add(CalculateRelativePos(TriangleFormation, this.commRadius));
        this.relativeFormationPosLists.Add(CalculateRelativePos(DoubleTriangleFormation, this.commRadius));
        this.relativeFormationPosLists.Add(CalculateRelativePos(CircleFormation, this.commRadius));
        this.relativeFormationPosLists.Add(CalculateRelativePos(RectangleFormation, this.commRadius));
        //this.relativeFormationPosLists.Add(CalculateRelativePos(LineFormation, this.commRadius));
        // for(int i = 0; i < this.m_FormationRelativePosLists.Count; i++)
        // {
        //     for(int j = 0; j < this.m_FormationRelativePosLists[i].Count; j++)
        //     {
        //         for (int k = 0; k < this.m_FormationRelativePosLists[i][j].Count; k++)
        //         {
        //             Debug.Log(i+", "+j+", "+k+","+ this.m_FormationRelativePosLists[i][j][k]);
        //         }
        //     }
        // }
        GameObject[] otheragents = GameObject.FindGameObjectsWithTag("Agent");
        List<Vector2> relativePosList = new List<Vector2>();
        foreach (GameObject agent in otheragents)
        {
            if (Vector3.Distance(this.transform.localPosition, agent.transform.localPosition) < this.commRadius && this.transform.localPosition != agent.transform.localPosition)
            {
                relativePosList.Add(new Vector2(this.transform.localPosition.x-agent.transform.localPosition.x, this.transform.localPosition.z-agent.transform.localPosition.z));
            }
        }
        float minMse = float.PositiveInfinity;
        float Mse = 0;
        for(int i=0;i<this.relativeFormationPosLists.Count;i++)
        {
            for(int j = 0; j < this.relativeFormationPosLists[i].Count;j++)
            {
                Mse = CalculateAverageMaxDistance(this.relativeFormationPosLists[i][j], relativePosList);
                if(Mse<minMse)
                {
                    minMse = Mse;
                    this.desiredFormationIndex = i;
                }
            }
        }
        //Debug.Log("desired formation index:"+this.desiredFormationIndex);
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

    private float CalculateAverageMaxDistance(List<Vector2> sourceSet, List<Vector2> targetSet)
    {
        float totalMaxDifference = 0f;
        foreach (Vector2 sourcePoint in sourceSet)
        {
            float maxDiffForCurrentPoint = float.MinValue;           
            foreach (Vector2 targetPoint in targetSet)
            {
                // 使用自定义的差异计算方法替代欧氏距离
                float diff = CalculateDis(sourcePoint, targetPoint);
                if (diff > maxDiffForCurrentPoint)
                {
                    maxDiffForCurrentPoint = diff;
                }
            }          
            totalMaxDifference += maxDiffForCurrentPoint;
        }
        return totalMaxDifference / sourceSet.Count;
    }


    /// <summary>
    /// 自定义的两点差异计算方法
    /// 结合径向距离差异和角度差异
    /// </summary>
    private float CalculateDis(Vector2 state, Vector2 task)
    {
        float difference = 0f;
        
        // 计算到原点的径向距离差异
        float distState = Vector2.Distance(state, Vector2.zero);
        float distTask = Vector2.Distance(task, Vector2.zero);
        difference += Mathf.Abs(distState - distTask);
        
        // 计算角度差异（归一化到0-1范围）
        float angleDiff = Vector2.Angle(state, task);
        difference += angleDiff / 5f;  // 除以5减小角度影响权重
        
        return difference;
    }


    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            AddReward(-5.0f);
            //Debug.Log("Collision!!!!!!!!!!");
            
        }
        else if (collision.gameObject.CompareTag("Agent"))
        {
            AddReward(-3.0f);
        }
    }

    void OnCollisionStay(Collision collision)
    {
        //Debug.Log("Colliding!!!!!!");
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            AddReward(-5f);
            //Debug.Log("Collisinginggggggg!!!!!!!!!!");
        }
        else if (collision.gameObject.CompareTag("Agent"))
        {
            AddReward(-5f);
        }
    }

    private List<Vector2> GetFormation(int formationId, Vector3 center, Vector3 end, int agentCount, float spacing)
    {
        List<Vector2> formation_positions = new List<Vector2>();
        float formationFactor = 1.4f;
        switch (formationId)
        {
            case 0:
                formation_positions = GetTriangleFormation(center, end, agentCount, 1.8f*spacing).ToList(); break;
            case 1:
                formation_positions = GetDoubleTriangleFormation(center, end, agentCount, 1.4f*spacing).ToList(); break;
            case 2:
                formation_positions = GetRectangleFormation(center, end, agentCount, formationFactor*spacing).ToList(); break;
            case 3:
                formation_positions = GetCircleFormation(center, end, agentCount, spacing).ToList(); break;
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
        float radius = 1.5f*spacing;
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


    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        if (Input.GetKeyDown(KeyCode.D))
        {
            //Debug.Log("!!!!!!!!!!");
            discreteActionsOut[0] = 1;
        }
        else if (Input.GetKey(KeyCode.W))
        {
            discreteActionsOut[0] = 2;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            discreteActionsOut[0] = 7;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            discreteActionsOut[0] = 8;
        }
    }

}
