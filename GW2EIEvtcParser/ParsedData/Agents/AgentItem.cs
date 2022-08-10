﻿using System;
using System.Collections.Generic;
using System.Linq;
using GW2EIEvtcParser.EIData;

namespace GW2EIEvtcParser.ParsedData
{
    public class AgentItem
    {

        private static int AgentCount = 0;
        public enum AgentType { NPC, Gadget, Player, NonSquadPlayer}

        public bool IsPlayer => Type == AgentType.Player || Type == AgentType.NonSquadPlayer;
        public bool IsNPC => Type == AgentType.NPC || Type == AgentType.Gadget;

        // Fields
        public ulong Agent { get; }
        public int ID { get; protected set; }
        public int UniqueID { get; }
        public AgentItem Master { get; protected set; }
        public ushort InstID { get; protected set; }
        public AgentType Type { get; protected set; } = AgentType.NPC;
        public long FirstAware { get; protected set; }
        public long LastAware { get; protected set; } = long.MaxValue;
        public string Name { get; protected set; } = "UNKNOWN";
        public ParserHelper.Spec Spec { get; } = ParserHelper.Spec.Unknown;
        public ParserHelper.Spec BaseSpec { get; } = ParserHelper.Spec.Unknown;
        public ushort Toughness { get; protected set; }
        public ushort Healing { get; }
        public ushort Condition { get; }
        public ushort Concentration { get; }
        public uint HitboxWidth { get; }
        public uint HitboxHeight { get; }

        public bool IsFake { get; }
        public bool IsNotInSquadFriendlyPlayer { get; private set; }

        // Constructors
        internal AgentItem(ulong agent, string name, ParserHelper.Spec spec, int id, AgentType type, ushort toughness, ushort healing, ushort condition, ushort concentration, uint hbWidth, uint hbHeight)
        {
            UniqueID = AgentCount++;
            Agent = agent;
            Name = name;
            Spec = spec;
            BaseSpec = ParserHelper.SpecToBaseSpec(spec);
            ID = id;
            Type = type;
            Toughness = toughness;
            Healing = healing;
            Condition = condition;
            Concentration = concentration;
            HitboxWidth = hbWidth;
            HitboxHeight = hbHeight;
            //
            try
            {
                if (type == AgentType.Player)
                {
                    string[] splitStr = Name.Split('\0');
                    if (splitStr.Length < 2 || (splitStr[1].Length == 0 || splitStr[2].Length == 0 || splitStr[0].Contains("-")))
                    {
                        if (!splitStr[0].Any(char.IsDigit))
                        {
                            IsNotInSquadFriendlyPlayer = true;
                        } 
                        else
                        {
                            Name = Spec.ToString() + " " + Name;
                        }
                        Type = AgentType.NonSquadPlayer;
                    }
                }
            }
            catch (Exception)
            {

            }
        }

        internal AgentItem(ulong agent, string name, ParserHelper.Spec spec, int id, ushort instid, ushort toughness, ushort healing, ushort condition, ushort concentration, uint hbWidth, uint hbHeight, long firstAware, long lastAware, bool isFake) : this(agent, name, spec, id, AgentType.NPC, toughness, healing, condition, concentration, hbWidth, hbHeight)
        {
            InstID = instid;
            FirstAware = firstAware;
            LastAware = lastAware;
            IsFake = isFake;
        }

        internal AgentItem(AgentItem other)
        {
            UniqueID = AgentCount++;
            Agent = other.Agent;
            Name = other.Name;
            Spec = other.Spec;
            BaseSpec = other.BaseSpec;
            ID = other.ID;
            Type = other.Type;
            Toughness = other.Toughness;
            Healing = other.Healing;
            Condition = other.Condition;
            Concentration = other.Concentration;
            HitboxWidth = other.HitboxWidth;
            HitboxHeight = other.HitboxHeight;
            InstID = other.InstID;
            Master = other.Master;
            IsFake = other.IsFake;
        }

        internal AgentItem()
        {
            UniqueID = AgentCount++;
        }

        internal void OverrideIsNotInSquadFriendlyPlayer(bool status)
        {
            IsNotInSquadFriendlyPlayer = status;
        }

        internal void OverrideType(AgentType type)
        {
            Type = type;
        }

        internal void OverrideName(string name)
        {
            Name = name;
        }

        internal void SetInstid(ushort instid)
        {
            InstID = instid;
        }

        internal void OverrideID(int id)
        {
            ID = id;
        }

        internal void OverrideID(ArcDPSEnums.TrashID id)
        {
            ID = (int)id;
        }

        internal void OverrideID(ArcDPSEnums.TargetID id)
        {
            ID = (int)id;
        }

        internal void OverrideToughness(ushort toughness)
        {
            Toughness = toughness;
        }

        internal void OverrideAwareTimes(long firstAware, long lastAware)
        {
            FirstAware = firstAware;
            LastAware = lastAware;
        }

        internal void SetMaster(AgentItem master)
        {
            if (IsPlayer)
            {
                return;
            }
            Master = master;
        }

        public AgentItem GetMainAgentWhenAttackTarget(ParsedEvtcLog log, long time)
        {
            IReadOnlyList<AttackTargetEvent> atEvents = log.CombatData.GetAttackTargetEventsByAttackTarget(this);
            if (atEvents.Any()) // agent is attack target
            {
                return atEvents.LastOrDefault(y => time >= y.Time)?.Src;
            }
            else
            {
                return this;
            }
        }

        private static void AddValueToStatusList(List<(long start, long end)> dead, List<(long start, long end)> down, List<(long start, long end)> dc, AbstractStatusEvent cur, AbstractStatusEvent next, long endTime, int index)
        {
            long cTime = cur.Time;
            long nTime = next != null ? next.Time : endTime;
            if (cur is DownEvent)
            {
                down.Add((cTime, nTime));
            }
            else if (cur is DeadEvent)
            {
                dead.Add((cTime, nTime));
            }
            else if (cur is DespawnEvent)
            {
                dc.Add((cTime, nTime));
            }
            else if (index == 0)
            {
                if (cur is SpawnEvent)
                {
                    dc.Add((0, cTime));
                }
                else if (cur is AliveEvent)
                {
                    dead.Add((0, cTime));
                }
            }
        }

        internal void GetAgentStatus(List<(long start, long end)> dead, List<(long start, long end)> down, List<(long start, long end)> dc, CombatData combatData, FightData fightData)
        {
            var status = new List<AbstractStatusEvent>();
            status.AddRange(combatData.GetDownEvents(this));
            status.AddRange(combatData.GetAliveEvents(this));
            status.AddRange(combatData.GetDeadEvents(this));
            status.AddRange(combatData.GetSpawnEvents(this));
            status.AddRange(combatData.GetDespawnEvents(this));
            status = status.OrderBy(x => x.Time).ToList();
            for (int i = 0; i < status.Count - 1; i++)
            {
                AbstractStatusEvent cur = status[i];
                AbstractStatusEvent next = status[i + 1];
                AddValueToStatusList(dead, down, dc, cur, next, fightData.FightEnd, i);
            }
            // check last value
            if (status.Count > 0)
            {
                AbstractStatusEvent cur = status.Last();
                AddValueToStatusList(dead, down, dc, cur, null, fightData.FightEnd, status.Count - 1);
            }
        }

        public AgentItem GetFinalMaster()
        {
            AgentItem cur = this;
            while (cur.Master != null)
            {
                cur = cur.Master;
            }
            return cur;
        }

        public bool InAwareTimes(long time)
        {
            return FirstAware <= time && LastAware >= time;
        }

        /// <summary>
        /// Checks if a buff is present on the actor that corresponds to. Given buff id must be in the buff simulator, throws <see cref="InvalidOperationException"/> otherwise
        /// </summary>
        /// <param name="log"></param>
        /// <param name="buffId"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public bool HasBuff(ParsedEvtcLog log, long buffId, long time)
        {
            AbstractSingleActor actor = log.FindActor(this);
            return actor.HasBuff(log, buffId, time);
        }

        /// <summary>
        /// Checks if agent is downed at given time
        /// </summary>
        /// <param name="log"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public bool IsDowned(ParsedEvtcLog log, long time)
        {
            AbstractSingleActor actor = log.FindActor(this);
            return actor.IsDowned(log, time);
        }

        /// <summary>
        /// Checks if agent is dead at given time
        /// </summary>
        /// <param name="log"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public bool IsDead(ParsedEvtcLog log, long time)
        {
            AbstractSingleActor actor = log.FindActor(this);
            return actor.IsDead(log, time);
        }

        /// <summary>
        /// Checks if agent is dc/not spawned at given time
        /// </summary>
        /// <param name="log"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public bool IsDC(ParsedEvtcLog log, long time)
        {
            AbstractSingleActor actor = log.FindActor(this);
            return actor.IsDC(log, time);
        }

        public double GetCurrentHealthPercent(ParsedEvtcLog log, long time)
        {
            AbstractSingleActor actor = log.FindActor(this);
            return actor.GetCurrentHealthPercent(log, time);
        }

        public double GetCurrentBarrierPercent(ParsedEvtcLog log, long time)
        {
            AbstractSingleActor actor = log.FindActor(this);
            return actor.GetCurrentBarrierPercent(log, time);
        }

        public Point3D GetCurrentPosition(ParsedEvtcLog log, long time)
        {
            AbstractSingleActor actor = log.FindActor(this);
            return actor.GetCurrentPosition(log, time);
        }
    }
}
