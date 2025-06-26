using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class MoveToGoal : Agent
{
    [SerializeField] private Transform targetTransform;
    private float previousDistanceToTarget;
    private float episodeTimeLimit = 15f; // 15-second time limit
    private float timer; // To track the elapsed time

    public override void OnEpisodeBegin()
    {
        // Reset the agent's and target's positions
        transform.localPosition = new Vector3(0, 1, -0.4f);
        targetTransform.localPosition = new Vector3(Random.Range(-0.4f, 0.4f), 1, Random.Range(0f, 0.4f));
        
        // Initialize previous distance and reset timer
        previousDistanceToTarget = Vector3.Distance(transform.localPosition, targetTransform.localPosition);
        timer = 0f; // Reset timer at the start of each episode
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(targetTransform.localPosition);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];
        
        float moveSpeed = 0.8f;
        transform.localPosition += new Vector3(moveX, 0, moveZ) * Time.deltaTime * moveSpeed;

        // Calculate the new distance to the target
        float currentDistanceToTarget = Vector3.Distance(transform.localPosition, targetTransform.localPosition);

        // Give rewards based on distance improvement
        if (currentDistanceToTarget < previousDistanceToTarget)
        {
            SetReward(0.05f);  // Reward for getting closer
        }
        else
        {
            SetReward(-0.1f);  // Penalty for getting farther
        }

        // Update the previous distance to the current distance
        previousDistanceToTarget = currentDistanceToTarget;

        // Update timer and check if the time limit is reached
        timer += Time.deltaTime;
        if (timer >= episodeTimeLimit)
        {
            SetReward(-0.5f);  // Optionally penalize for not reaching the target in time
            EndEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxisRaw("Horizontal");
        continuousActions[1] = Input.GetAxisRaw("Vertical");
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Wall"))
        {
            SetReward(-1f);
            EndEpisode();
        }

        if (other.CompareTag("Target"))
        {
            SetReward(1f);
            EndEpisode();
        }
    }
}
