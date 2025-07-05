using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using static UnityEngine.GraphicsBuffer;
using Unity.VisualScripting;
using Random = UnityEngine.Random;

public class SquareEnv : MonoBehaviour
{
    [HideInInspector]
    private float timeScale = 1f;
    [Header("Environment")]
    [Tooltip("Groups")]
    public int m_NumGroups = 1;
    [Tooltip("Materials:Num >= m_NumGroups!")]
    public Color[] m_Colors = new Color[10];
    //public List<int> m_NumAgentsOfGroup = new List<int>();
    public Group[] m_Groups = new Group[1];

    public void Awake()
    {
        for (int i = 0; i < m_NumGroups; i++)
        {
            this.m_Groups[i].m_GroupID = i;
            this.m_Groups[i].m_Color = this.m_Colors[i];
        }
        Debug.Log("Env start!");
    }

        // Start is called before the first frame update
     void Start()
    {

        //InstantiateAgents();
        //StartCoroutine(instantiateAgents());
    }

    //private void InstantiateAgents()
    //{
    //    for (int i = 1; i <= this.numOfAgents; i++)
    //    {
    //        GameObject tempAgent = Instantiate(this.agentPrefab, new Vector3(0, 0, 0), Quaternion.identity);
    //        tempAgent.GetComponent<SquareAgent>().agentID = Random.Range(0, (int)(this.numOfAgents/scalefactor));
    //        if  (agentsDict.ContainsKey(tempAgent.GetComponent<SquareAgent>().agentID) != true)
    //        {
    //            m_AgentGroups[tempAgent.GetComponent<SquareAgent>().agentID] = new SimpleMultiAgentGroup();
    //            m_AgentGroups[tempAgent.GetComponent<SquareAgent>().agentID].RegisterAgent(tempAgent.GetComponent<SquareAgent>());
    //            tempAgent.transform.localPosition = RandomStartPos();
    //            tempAgent.GetComponent<SquareAgent>().goalPos = RandomGoalPos();
    //            agentsDict[tempAgent.GetComponent<SquareAgent>().agentID].Add(tempAgent.transform.localPosition);
    //            agentsDict[tempAgent.GetComponent<SquareAgent>().agentID].Add(tempAgent.GetComponent<SquareAgent>().goalPos);
    //        }
    //        else
    //        {
    //            m_AgentGroups[tempAgent.GetComponent<SquareAgent>().agentID].RegisterAgent(tempAgent.GetComponent<SquareAgent>());
    //            tempAgent.transform.localPosition = agentsDict[tempAgent.GetComponent<SquareAgent>().agentID][0] + new Vector3(Random.Range(-8f, 8f), 0f, Random.Range(-8f, 8f));
    //            tempAgent.GetComponent<SquareAgent>().goalPos = agentsDict[tempAgent.GetComponent<SquareAgent>().agentID][1] + new Vector3(Random.Range(-8f, 8f), 0f, Random.Range(-8f, 8f));
    //        }
    //        tempAgent.transform.SetParent(this.agentParent.transform);
    //        //Debug.Log("Agent " + i + " instantiated!");
    //    }
    //}

    //private IEnumerator instantiateAgents()
    //{
    //    WaitForSeconds wait = new WaitForSeconds(UnityEngine.Random.Range(3, 6));
    //    for (int i = 1; i <= this.numOfAgents; i++)
    //    {
    //        GameObject tempAgent = Instantiate(this.agentPrefab, new Vector3(0, 0, 0), Quaternion.identity);
    //        tempAgent.transform.SetParent(this.agentParent.transform);
    //        //tempAgent.transform.localPosition = RandomPosition();
    //        tempAgent.GetComponent<RollerAgent>().agentID = i;
    //        //if (this.unlimitedEpisodeSteps)
    //        //    tempAgent.GetComponent<RollerAgent>().MaxStep = 0;
    //        if (this.spawnGradually)
    //            if (i % this.spawnGraduallyNumber == 0)
    //                yield return wait;
    //        //Debug.Log("tempAgent.transform.localPosition:" + tempAgent.transform.localPosition);

    //        //tempAgent.transform.SetParent(this.agentParent.transform);
    //        //tempAgent.GetComponent<>().agentID = i;
    //        Debug.Log("Agent " + i + " instantiated!");
    //    }
    //}

// Update is called once per frame
    void Update()
    {
        	
    }

}
