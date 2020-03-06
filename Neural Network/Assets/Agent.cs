﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent : MonoBehaviour, IComparable<Agent>
{
    public CarController carController;

    public NeuralNetwork net;
    public float fitness;

    public Transform nextCheckpoint;
    public Transform prevCheckpoint;
    public float nextCheckpointDist;
    public float distanceTraveled;
    public float negativeDistance;

    public MeshRenderer meshRenderer;

    public Material defaultMat;
    public Material mutatedMat;
    public Material firstMat;

    public Transform tr;
    public Rigidbody rb;

    public float[] inputs;

    Vector3 pos;
    RaycastHit hit;

    public LayerMask layerMask;
    public LayerMask layerMask2;

    public float rayRange = 1;


/*
    public Transform frontRay;
    public Transform backRay;*/

    [Space]
    Vector3 velocity;
    public float outVelocity;

    void Awake()
    {
        prevCheckpoint = GameObject.Find("CheckPoints Group").transform;
    }

    void FixedUpdate()
    {
        InputUpdate();
        OutputUpdate();
        UpdateFitness();
        //CheckIfFacing();
    }


    public void CheckPointReached(Transform newNextCheckpoint)
    {
        distanceTraveled += nextCheckpointDist;
        prevCheckpoint = nextCheckpoint;
        nextCheckpoint = newNextCheckpoint;
        nextCheckpointDist = (tr.position - nextCheckpoint.position).magnitude;
        negativeDistance = nextCheckpointDist * 2;
    }

    void InputUpdate()
    {
        pos = tr.position;

        inputs[0] = RaySensor(pos + Vector3.up * 0.2f, tr.forward, 4);

        inputs[1] = RaySensor(pos + Vector3.up * 0.2f, tr.forward + tr.right, 2);
        inputs[2] = RaySensor(pos + Vector3.up * 0.2f, tr.forward - tr.right, 2);

        inputs[3] = RaySensor(pos + Vector3.up * 0.2f, tr.right, 1.5f);
        inputs[4] = RaySensor(pos + Vector3.up * 0.2f, -tr.right, 1.5f);

        ///\

        inputs[5] = 1 - (float)Math.Tanh(rb.velocity.magnitude / 20);
        inputs[6] = (float)Math.Tanh(rb.angularVelocity.y * 0.01f);

        velocity = rb.velocity;
        outVelocity = Mathf.Abs(rb.velocity.x) + Mathf.Abs(rb.velocity.z);
    }
    IEnumerator Distance()
    {
        //Debug.Log(nextCheckpointDist = (tr.position - prevCheckpoint.position).magnitude);
        if (nextCheckpointDist > negativeDistance) Debug.LogWarning("Destroy this bruh");

        //float previousFrameDistance = (tr.position - nextCheckpoint.position).magnitude;
        yield return new WaitForSeconds(0.1f);
        float newDistance = (tr.position - nextCheckpoint.position).magnitude;
        inputs[7] = Mathf.Abs(CustomScaler.FloatScale(newDistance, 0, nextCheckpointDist * 2, 0, 1));

        /*if (previousFrameDistance > newDistance) inputs[7] = 1f;
        else if (previousFrameDistance <= newDistance) inputs[7] = -1f;*/
        //else if (previousFrameDistance == newDistance) inputs[7] = 0;

        StartCoroutine(Distance());
    }
    /*
    void CheckIfFacing()
    {
        if (Physics.Linecast(frontRay.position, nextCheckpoint.position, out hit, layerMask2))
        {
            Debug.DrawLine(frontRay.position, nextCheckpoint.position, Color.cyan);
            if (Physics.Linecast(backRay.position, nextCheckpoint.position, out hit, layerMask2))
            {
                Debug.DrawLine(backRay.position, nextCheckpoint.position, Color.magenta);
            }


            if (hit.collider.name.Contains("CheckPoint"))
            {
                if ((nextCheckpoint.position - frontRay.position).magnitude < (nextCheckpoint.position - backRay.position).magnitude)
                {
                    Debug.Log("yeet1");
                    inputs[8] = 1;
                }

                else if ((nextCheckpoint.position - backRay.position).magnitude < (nextCheckpoint.position - frontRay.position).magnitude)
                {
                    Debug.Log("yeet2");
                    inputs[8] = -1;
                }
            }
            else return;
        }
    }*/

    void OutputUpdate()
    {
        net.FeedForward(inputs);

        carController.horizontalInput = net.neurons[net.layers.Length - 1][0]; // 1er Neurone du Layer Output (Neurone 0, Layer 4)
        carController.verticalInput = net.neurons[net.layers.Length - 1][1]; // 2ème Neurone du Layer Output (Neurone 1, Layer 4)
    }

    float RaySensor(Vector3 pos, Vector3 direction, float lenght)
    {
        if (Physics.Raycast(pos, direction, out hit, lenght * rayRange, layerMask))
        {
            Debug.DrawRay(pos, direction * lenght * rayRange, Color.green);
            return ((rayRange * lenght) - hit.distance) / (rayRange * lenght);
        }
        else
        {
            Debug.DrawRay(pos, direction * lenght * rayRange, Color.red);
            return 0;
        }
    }

    void UpdateFitness()
    {
        SetFitness(distanceTraveled + (nextCheckpointDist - (tr.position - nextCheckpoint.position).magnitude));
    }

    void SetFitness(float _fitness)
    {
        if (fitness < _fitness)
        {
            fitness = _fitness;
        }
    }

    public void SetDefaultColor()
    {
        meshRenderer.material = defaultMat;
    }

    public void SetMutatedColor()
    {
        meshRenderer.material = mutatedMat;
    }

    public void SetFirstColor()
    {
        meshRenderer.material = firstMat;
    }

    public int CompareTo(Agent other)
    {
        if (fitness < other.fitness)
        {
            return 1;
        }
        if (fitness > other.fitness)
        {
            return -1;
        }

        return 0;
    }

    public void ResetAgent(Quaternion _rotation, Vector3 spawnPos)
    {
        fitness = 0;
        //tr.position = Vector3.zero;
        tr.position = spawnPos;
        //tr.rotation = Quaternion.identity;
        tr.rotation = _rotation;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        distanceTraveled = 0;

        nextCheckpoint = CheckPointManager.instance.firstCheckPoint;
        nextCheckpointDist = (tr.position - nextCheckpoint.position).magnitude;

        inputs = new float[net.layers[0]]; // Layer 0 => Inputs
    }
}