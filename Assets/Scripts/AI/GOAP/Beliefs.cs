using System;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.InputSystem;

public class BeliefFactory
{
    readonly GoapAgent agent;
    readonly Dictionary<string, AgentBelief> beliefs;

    public BeliefFactory(GoapAgent agent, Dictionary<string, AgentBelief> beliefs)
    {
        this.agent = agent;
        this.beliefs = beliefs;
    }

    public void AddBelief(string key, Func<bool> condition)
    {
        beliefs.Add(key, new AgentBelief.Builder(key)
            .WithCondition(condition)
            .Build());
    }

    public void AddSensorBelief(string key, Sensor sensor)
    {
        foreach (Sensor.unitData target in sensor.seenTargets)
        {
            bool found = false;
            foreach (var belief in beliefs.Values)
            {
                if (belief.unitDataPack.id == target.id)
                {
                    found = true;
                    belief.unitDataPack = target;
                    break;
                }
            }
            if (!found) AddUnitBelief(key + target.id, target);
        }
    }

    public void AddLocationBelief(string key, float distance, Transform locationCondition)
    {
        AddLocationBelief(key, distance, locationCondition.position);
    }

    public void AddLocationBelief(string key, float distance, Vector3 locationCondition)
    {
        beliefs.Add(key, new AgentBelief.Builder(key)
            .WithCondition(() => InRangeOf(locationCondition, distance))
            .WithLocation(() => locationCondition)
            .Build());
    }

    public void AddUnitBelief(string key, Sensor.unitData unitData) {
        beliefs.Add(key, new AgentBelief.Builder(key)
            .WithUnit(unitData)
            .WithLocation(() => unitData.position)
            .Build());
    }

    bool InRangeOf(Vector3 pos, float range) => Vector3.Distance(agent.transform.position, pos) < range;
}

public class AgentBelief
{
    public string Name { get; }

    Func<bool> condition = () => false;
    Func<Vector3> observedLocation = () => Vector3.zero;

    public Vector3 Location => observedLocation();

    AgentBelief(string name)
    {
        Name = name;
    }

    public bool Evaluate() => condition();

    public Sensor.unitData unitDataPack;

    public class Builder
    {
        readonly AgentBelief belief;

        public Builder(string name)
        {
            belief = new AgentBelief(name);
        }

        public Builder WithCondition(Func<bool> condition)
        {
            belief.condition = condition;
            return this;
        }

        public Builder WithLocation(Func<Vector3> observedLocation)
        {
            belief.observedLocation = observedLocation;
            return this;
        }

        public Builder WithUnit(Sensor.unitData uD)
        {
            belief.unitDataPack = uD;
            return this;
        }

        public AgentBelief Build()
        {
            return belief;
        }
    }
}