﻿//Put this script on your agent.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;

public interface IPushAgent
{
    void IScoredAGoal(GameObject crate, GameObject goal);
    void IHitWrongGoal(GameObject crate, GameObject goal);
}

public class BeepoAgent: Agent, IPushAgent
{
    /// <summary>
    /// The ground. The bounds are used to spawn the elements.
    /// </summary>
	public GameObject ground;

    public GameObject area;

    /// <summary>
    /// The area bounds.
    /// </summary>
	[HideInInspector]
    public Bounds areaBounds;

    BeepoAcademy academy;

    /// <summary>
    /// The goals to push the blocks to.
    /// </summary>
    private CrateDestination[] goals;

    /// <summary>
    /// The blocks to be pushed to the goals.
    /// </summary>
    private Crate[] blocks;
    
    private Rigidbody agentRB;

    RayPerception rayPer;

    void Awake()
    {
        academy = FindObjectOfType<BeepoAcademy>(); //cache the academy

        goals = area.GetComponentsInChildren<CrateDestination>();
        blocks = area.GetComponentsInChildren<Crate>();

        foreach (var goal in goals)
        {
            goal.SetColor(academy.FindGoalDefinition(goal.type).color);
        }

        foreach (var block in blocks)
        {
            block.SetColor(academy.FindGoalDefinition(block.type).color);
        }
    }

    public override void InitializeAgent()
    {
        base.InitializeAgent();

        foreach (var block in blocks) {
            block.agent = this;
        }

        agentRB = GetComponent<Rigidbody>();

        rayPer = GetComponent<RayPerception>();

        // Get the ground's bounds
        areaBounds = ground.GetComponent<Collider>().bounds;
    }

    public override void CollectObservations()
    {
        // code for obs goes here
        var rayDistance = 12f;

        float[] rayAngles = { 0f, 45f, 90f, 135f, 180f, 110f, 70f };

        var detectableObjects = new[] { "crate", "goal", "wall" };

        AddVectorObs(rayPer.Perceive(rayDistance, rayAngles, detectableObjects, 0f, 0f));
        AddVectorObs(rayPer.Perceive(rayDistance, rayAngles, detectableObjects, 1.5f, 0f));
    }

    /// <summary>
    /// Called every step of the engine. Here the agent takes an action.
    /// </summary>
	public override void AgentAction(float[] vectorAction, string textAction)
    {
        // code for actions goes here
        MoveAgent(vectorAction);

        AddReward(-1f / agentParameters.maxStep);
    }

    /// <summary>
    /// Moves the agent according to the selected action.
    /// </summary>
	public void MoveAgent(float[] act)
    {
        Vector3 dirToGo = Vector3.zero;
        Vector3 rotateDir = Vector3.zero;

        int action = Mathf.FloorToInt(act[0]);

        switch (action)
        {
            case 1:
                dirToGo = transform.forward * 1f;
                break;
            case 2:
                dirToGo = transform.forward * -1f;
                break;
            case 3:
                rotateDir = transform.up * 1f;
                break;
            case 4:
                rotateDir = transform.up * -1f;
                break;
            case 5:
                dirToGo = transform.right * -0.75f;
                break;
            case 6:
                dirToGo = transform.right * 0.75f;
                break;
        }
        transform.Rotate(rotateDir, Time.fixedDeltaTime * 200f);

        agentRB.AddForce(dirToGo * academy.agentRunSpeed,
                         ForceMode.VelocityChange);
    }

    /// <summary>
    /// Called when the agent moves the block into the goal.
    /// </summary>
    public void IScoredAGoal(GameObject target, GameObject goal)
    {
        // This is where a reward would go
        AddReward(5f); // nice big reward

        Debug.Log("Agent delivered package!");

        var allGoalsComplete = true;
        foreach (var block in blocks) {
            if (block.IsActive == true) {
                allGoalsComplete = false;
            }
        }

        if (allGoalsComplete) {
            // By marking an agent as done AgentReset() will be called automatically.
        
            Done();
        }
   }

    public void IHitWrongGoal(GameObject target, GameObject goal)
    {
        // this is where we punish Beepo
        AddReward(-5f);
    }

    /// <summary>
    /// In the editor, if "Reset On Done" is checked then AgentReset() will
    /// be called automatically anytime we mark done = true in an agent
    /// script.
    /// </summary>
	public override void AgentReset()
    {
        int rotation = Random.Range(0, 4);
        float rotationAngle = rotation * 90f;
        area.transform.Rotate(new Vector3(0f, rotationAngle, 0f));

        ResetBlocks();
        transform.position = GetRandomSpawnPos();
        agentRB.velocity = Vector3.zero;
        agentRB.angularVelocity = Vector3.zero;
    }

    /// <summary>
    /// Swap ground material, wait time seconds, then swap back to the
    /// regular material.
    /// </summary>
    IEnumerator ShowGoalAchievedAnimation(GameObject target, GameObject goal)
    {
        // TODO: replace this with new 'goal scored' effect if you like
        yield break;
        
    }

    // ============== BELOW HERE SHOULD NOT NEED TO TOUCH =============

    /// <summary>
    /// Use the ground's bounds to pick a random spawn position.
    /// </summary>
    public Vector3 GetRandomSpawnPos()
    {
        Vector3 randomSpawnPos = Vector3.zero;

        while (true) {
            float randomPosX = Random.Range(-areaBounds.extents.x * academy.spawnAreaMarginMultiplier,
                                areaBounds.extents.x * academy.spawnAreaMarginMultiplier);

            float randomPosZ = Random.Range(-areaBounds.extents.z * academy.spawnAreaMarginMultiplier,
                                            areaBounds.extents.z * academy.spawnAreaMarginMultiplier);

            randomSpawnPos = ground.transform.position + new Vector3(randomPosX, 1f, randomPosZ);

            if (Physics.CheckBox(randomSpawnPos, new Vector3(2.5f, 0.01f, 2.5f)) == false)
            {
                break;
            }
        }

        return randomSpawnPos;
    }

    /// <summary>
    /// Resets the block position and velocities.
    /// </summary>
    void ResetBlocks() {
        foreach (var block in blocks)
        {
            // Get a random position for the block.
            block.transform.position = GetRandomSpawnPos();

            var blockRB = block.GetComponent<Rigidbody>();

            // Reset block velocity back to zero.
            blockRB.velocity = Vector3.zero;

            // Reset block angularVelocity back to zero.
            blockRB.angularVelocity = Vector3.zero;

            block.Reset();
        }
    }
}
