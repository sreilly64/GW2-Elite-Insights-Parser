﻿using System;
using System.Collections.Generic;
using System.Linq;
using GW2EIEvtcParser.EIData;
using GW2EIEvtcParser.Exceptions;
using GW2EIEvtcParser.ParsedData;
using static GW2EIEvtcParser.SkillIDs;

namespace GW2EIEvtcParser.EncounterLogic
{
    internal class Cairn : BastionOfThePenitent
    {
        public Cairn(int triggerID) : base(triggerID)
        {
            MechanicList.AddRange(new List<Mechanic>
            {
            // (ID, ingame name, Type, BossID, plotly marker, Table header name, ICD, Special condition) // long table hover name, graph legend name
            new HitOnPlayerMechanic(38113, "Displacement", new MechanicPlotlySetting(Symbols.Circle,Colors.LightOrange), "Port","Orange Teleport Field", "Orange TP",0),
            new HitOnPlayerMechanic(new long[] {37611, 37629, 37642, 37673, 38074, 38302 }, "Spatial Manipulation", new MechanicPlotlySetting(Symbols.Circle,Colors.Green), "Green","Green Spatial Manipulation Field (lift)", "Green (lift)",0, (de, log) => !de.To.HasBuff(log, Stability, de.Time - ParserHelper.ServerDelayConstant)),
            new HitOnPlayerMechanic(new long[] {37611, 37629, 37642, 37673, 38074, 38302 }, "Spatial Manipulation", new MechanicPlotlySetting(Symbols.CircleOpen,Colors.Green), "Stab.Green","Green Spatial Manipulation Field while affected by stability", "Stabilized Green",0, (de, log) => de.To.HasBuff(log, Stability, de.Time - ParserHelper.ServerDelayConstant)),
            new HitOnPlayerMechanic(31875, "Spectral Impact", new MechanicPlotlySetting(Symbols.Hexagram,Colors.Red), "Slam","Spectral Impact (KB Slam)", "Slam",4000, (de, log) => !de.To.HasBuff(log, Stability, de.Time - ParserHelper.ServerDelayConstant)),
            new HitOnPlayerMechanic(38313, "Meteor Swarm", new MechanicPlotlySetting(Symbols.DiamondTall,Colors.Red), "KB","Knockback Crystals (tornado like)", "KB Crystal",1000),
            new PlayerBuffApplyMechanic(SharedAgony, "Shared Agony", new MechanicPlotlySetting(Symbols.Circle,Colors.Red), "Agony","Shared Agony Debuff Application", "Shared Agony",0),//could flip
            new PlayerBuffApplyMechanic(38170, "Shared Agony", new MechanicPlotlySetting(Symbols.StarTriangleUpOpen,Colors.Pink), "Agony 25","Shared Agony Damage (25% Player's HP)", "SA dmg 25%",0), // Seems to be a (invisible) debuff application for 1 second from the Agony carrier to the closest(?) person in the circle.
            new PlayerBuffApplyMechanic(37768, "Shared Agony", new MechanicPlotlySetting(Symbols.StarDiamondOpen,Colors.Orange), "Agony 50","Shared Agony Damage (50% Player's HP)", "SA dmg 50%",0), //Chaining from the first person hit by 38170, applying a 1 second debuff to the next person.
            new PlayerBuffApplyMechanic(38209, "Shared Agony", new MechanicPlotlySetting(Symbols.StarOpen,Colors.Red), "Agony 75","Shared Agony Damage (75% Player's HP)", "SA dmg 75%",0), //Chaining from the first person hit by 37768, applying a 1 second debuff to the next person.
            // new Mechanic(37775, "Shared Agony", Mechanic.MechType.SkillOnPlayer, ParseEnum.BossIDS.Cairn, new MechanicPlotlySetting(Symbols.CircleOpen,Color.Red), "Agony Damage",0), from old raidheroes logs? Small damage packets. Is also named "Shared Agony" in the evtc. Doesn't seem to occur anymore.
            // new Mechanic(38210, "Shared Agony", Mechanic.MechType.SkillOnPlayer, ParseEnum.BossIDS.Cairn, new MechanicPlotlySetting(Symbols.CircleOpen,Color.Red), "SA.dmg","Shared Agony Damage dealt", "Shared Agony dmg",0), //could flip. HP% attack, thus only shows on down/absorb hits.
            new HitOnPlayerMechanic(38060, "Energy Surge", new MechanicPlotlySetting(Symbols.TriangleLeft,Colors.DarkGreen), "Leap","Jump between green fields", "Leap",100),
            new HitOnPlayerMechanic(37631, "Orbital Sweep", new MechanicPlotlySetting(Symbols.DiamondWide,Colors.Magenta), "Sweep","Sword Spin (Knockback)", "Sweep",100),//short cooldown because of multihits. Would still like to register second hit at the end of spin though, thus only 0.1s
            new HitOnPlayerMechanic(37910, "Gravity Wave", new MechanicPlotlySetting(Symbols.Octagon,Colors.Magenta), "Donut","Expanding Crystal Donut Wave (Knockback)", "Crystal Donut",0)
            // Spatial Manipulation IDs correspond to the following: 1st green when starting the fight: 37629;
            // Greens after Energy Surge/Orbital Sweep: 38302
            //100% - 75%: 37611
            // 75% - 50%: 38074
            // 50% - 25%: 37673
            // 25% -  0%: 37642
            });
            Extension = "cairn";
            Icon = "https://wiki.guildwars2.com/images/b/b8/Mini_Cairn_the_Indomitable.png";
            EncounterCategoryInformation.InSubCategoryOrder = 0;
        }

        protected override CombatReplayMap GetCombatMapInternal(ParsedEvtcLog log)
        {
            return new CombatReplayMap("https://i.imgur.com/NlpsLZa.png",
                            (607, 607),
                            (12981, 642, 15725, 3386)/*,
                            (-27648, -9216, 27648, 12288),
                            (11774, 4480, 14078, 5376)*/);
        }

        internal override List<InstantCastFinder> GetInstantCastFinders()
        {
            return new List<InstantCastFinder>()
            {
                new DamageCastFinder(37989, 37989, InstantCastFinder.DefaultICD), // Cosmic Aura
            };
        }

        internal override List<PhaseData> GetPhases(ParsedEvtcLog log, bool requirePhases)
        {
            List<PhaseData> phases = GetInitialPhase(log);
            AbstractSingleActor cairn = Targets.FirstOrDefault(x => x.ID == (int)ArcDPSEnums.TargetID.Cairn);
            if (cairn == null)
            {
                throw new MissingKeyActorsException("Cairn not found");
            }
            phases[0].AddTarget(cairn);
            if (!requirePhases)
            {
                return phases;
            }
            BuffApplyEvent enrageApply = log.CombatData.GetBuffData(EnragedCairn).OfType<BuffApplyEvent>().FirstOrDefault(x => x.To == cairn.AgentItem);
            if (enrageApply != null)
            {
                var normalPhase = new PhaseData(0, enrageApply.Time)
                {
                    Name = "Calm"
                };
                normalPhase.AddTarget(cairn);

                var enragePhase = new PhaseData(enrageApply.Time + 1, log.FightData.FightEnd)
                {
                    Name = "Angry"
                };
                enragePhase.AddTarget(cairn);

                phases.Add(normalPhase);
                phases.Add(enragePhase);
            }
            return phases;
        }

        internal override void ComputeNPCCombatReplayActors(NPC target, ParsedEvtcLog log, CombatReplay replay)
        {
            IReadOnlyList<AbstractCastEvent> cls = target.GetCastEvents(log, 0, log.FightData.FightEnd);
            switch (target.ID)
            {
                case (int)ArcDPSEnums.TargetID.Cairn:
                    var swordSweep = cls.Where(x => x.SkillId == 37631).ToList();
                    foreach (AbstractCastEvent c in swordSweep)
                    {
                        int start = (int)c.Time;
                        int preCastTime = 1400;
                        int initialHitDuration = 850;
                        int sweepDuration = 1100;
                        int width = 1400; int height = 80;
                        Point3D facing = replay.Rotations.FirstOrDefault(x => x.Time >= start);
                        if (facing != null)
                        {
                            int initialDirection = (int)(Math.Atan2(facing.Y, facing.X) * 180 / Math.PI);
                            replay.Decorations.Add(new RotatedRectangleDecoration(true, 0, width, height, initialDirection, width / 2, (start, start + preCastTime), "rgba(200, 0, 255, 0.1)", new AgentConnector(target)));
                            replay.Decorations.Add(new RotatedRectangleDecoration(true, 0, width, height, initialDirection, width / 2, (start + preCastTime, start + preCastTime + initialHitDuration), "rgba(150, 0, 180, 0.5)", new AgentConnector(target)));
                            replay.Decorations.Add(new RotatedRectangleDecoration(true, 0, width, height, initialDirection, width / 2, 360, (start + preCastTime + initialHitDuration, start + preCastTime + initialHitDuration + sweepDuration), "rgba(150, 0, 180, 0.5)", new AgentConnector(target)));
                        }
                    }
                    var wave = cls.Where(x => x.SkillId == 37910).ToList();
                    foreach (AbstractCastEvent c in wave)
                    {
                        int start = (int)c.Time;
                        int preCastTime = 1200;
                        int duration = 600;
                        int firstRadius = 400;
                        int secondRadius = 700;
                        int thirdRadius = 1000;
                        int fourthRadius = 1300;
                        replay.Decorations.Add(new DoughnutDecoration(true, 0, firstRadius, secondRadius, (start + preCastTime, start + preCastTime + duration), "rgba(100,0,155,0.3)", new AgentConnector(target)));
                        replay.Decorations.Add(new DoughnutDecoration(true, 0, secondRadius, thirdRadius, (start + preCastTime + 2 * duration, start + preCastTime + 3 * duration), "rgba(100,0,155,0.3)", new AgentConnector(target)));
                        replay.Decorations.Add(new DoughnutDecoration(true, 0, thirdRadius, fourthRadius, (start + preCastTime + 5 * duration, start + preCastTime + 6 * duration), "rgba(100,0,155,0.3)", new AgentConnector(target)));
                    }
                    break;
                default:
                    break;
            }
        }

        internal override long GetFightOffset(FightData fightData, AgentData agentData, List<CombatItem> combatData)
        {
            AgentItem target = agentData.GetNPCsByID((int)ArcDPSEnums.TargetID.Cairn).FirstOrDefault();
            if (target == null)
            {
                throw new MissingKeyActorsException("Cairn not found");
            }
            // spawn protection loss -- most reliable
            CombatItem spawnProtectionLoss = combatData.Find(x => x.IsBuffRemove == ArcDPSEnums.BuffRemove.All && x.SrcMatchesAgent(target) && x.SkillID == 34113);
            if (spawnProtectionLoss != null)
            {
                return spawnProtectionLoss.Time - 1;
            }
            else
            {
                // get first end casting
                CombatItem firstCastEnd = combatData.FirstOrDefault(x => x.EndCasting() && (x.Time - fightData.LogStart) < 2000 && x.SrcMatchesAgent(target));
                // It has to Impact(38102), otherwise anomaly, player may have joined mid fight, do nothing
                if (firstCastEnd != null && firstCastEnd.SkillID == 38102)
                {
                    // Action 4 from skill dump for 38102
                    long actionHappened = 1025;
                    // Adds around 10 to 15 ms diff compared to buff loss
                    if (firstCastEnd.BuffDmg > 0)
                    {
                        double nonScaledToScaledRatio = (double)firstCastEnd.Value / firstCastEnd.BuffDmg;
                        return firstCastEnd.Time - firstCastEnd.Value + (long)Math.Round(nonScaledToScaledRatio * actionHappened) - 1;
                    }
                    // Adds around 15 to 20 ms diff compared to buff loss
                    else
                    {
                        return firstCastEnd.Time - firstCastEnd.Value + actionHappened - 1;
                    }
                }
            }
            return fightData.LogStart;
        }

        internal override void ComputePlayerCombatReplayActors(AbstractPlayer p, ParsedEvtcLog log, CombatReplay replay)
        {
            // shared agony
            var agony = log.CombatData.GetBuffData(SharedAgony).Where(x => (x.To == p.AgentItem && x is BuffApplyEvent)).ToList();
            foreach (AbstractBuffEvent c in agony)
            {
                int agonyStart = (int)c.Time;
                int agonyEnd = agonyStart + 62000;
                replay.Decorations.Add(new CircleDecoration(false, 0, 220, (agonyStart, agonyEnd), "rgba(255, 0, 0, 0.5)", new AgentConnector(p)));
            }
        }

        internal override FightData.CMStatus IsCM(CombatData combatData, AgentData agentData, FightData fightData)
        {
            return combatData.GetSkills().Contains(38098) ? FightData.CMStatus.CM : FightData.CMStatus.NoCM;
        }

        internal override string GetLogicName(CombatData combatData, AgentData agentData)
        {
            return "Cairn";
        }
    }
}
