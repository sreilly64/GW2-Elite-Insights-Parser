﻿using System;
using System.Collections.Generic;
using System.Linq;
using GW2EIEvtcParser.EIData;
using GW2EIEvtcParser.Exceptions;
using GW2EIEvtcParser.Extensions;
using GW2EIEvtcParser.ParsedData;
using static GW2EIEvtcParser.ParserHelper;
using static GW2EIEvtcParser.SkillIDs;

namespace GW2EIEvtcParser.EncounterLogic
{
    internal class Deimos : BastionOfThePenitent
    {

        private long _deimos10PercentTime = 0;

        public Deimos(int triggerID) : base(triggerID)
        {
            MechanicList.AddRange(new List<Mechanic>
            {
            new HitOnPlayerMechanic(37716, "Rapid Decay", new MechanicPlotlySetting(Symbols.CircleOpen,Colors.Black), "Oil","Rapid Decay (Black expanding oil)", "Black Oil",0),
            new FirstHitOnPlayerMechanic(37716, "Rapid Decay", new MechanicPlotlySetting(Symbols.Circle,Colors.Black), "Oil T.","Rapid Decay Trigger (Black expanding oil)", "Black Oil Trigger",0, (ce, log) => {
                AbstractSingleActor actor = log.FindActor(ce.To);
                if (actor == null)
                {
                    return false;
                }
                (_, IReadOnlyList<(long start, long end)> downs , _) = actor.GetStatus(log);
                bool hitInDown = downs.Any(x => x.start < ce.Time && ce.Time < x.end);
                return !hitInDown;
            }),
            new EnemyCastStartMechanic(37846, "Off Balance", new MechanicPlotlySetting(Symbols.DiamondTall,Colors.DarkTeal), "TP CC","Off Balance (Saul TP Breakbar)", "Saul TP Start",0),
            new EnemyCastEndMechanic(37846, "Off Balance", new MechanicPlotlySetting(Symbols.DiamondTall,Colors.Red), "TP CC Fail","Failed Saul TP CC", "Failed CC (TP)",0, (ce,log) => ce.ActualDuration >= 2200),
            new EnemyCastEndMechanic(37846, "Off Balance", new MechanicPlotlySetting(Symbols.DiamondTall,Colors.DarkGreen), "TP CCed","Saul TP CCed", "CCed (TP)",0, (ce, log) => ce.ActualDuration < 2200),
            new EnemyCastStartMechanic(38272, "Boon Thief", new MechanicPlotlySetting(Symbols.DiamondWide,Colors.DarkTeal), "Thief CC","Boon Thief (Saul Breakbar)", "Boon Thief Start",0),
            new EnemyCastEndMechanic(38272, "Boon Thief", new MechanicPlotlySetting(Symbols.DiamondWide,Colors.Red), "Thief CC Fail","Failed Boon Thief CC", "Failed CC (Thief)",0,(ce,log) => ce.ActualDuration >= 4400),
            new EnemyCastEndMechanic(38272, "Boon Thief", new MechanicPlotlySetting(Symbols.DiamondWide,Colors.DarkGreen), "Thief CCed","Boon Thief CCed", "CCed (Thief)",0,(ce, log) => ce.ActualDuration < 4400),
            new HitOnPlayerMechanic(new long[] {38208, 37929 }, "Annihilate", new MechanicPlotlySetting(Symbols.Hexagon,Colors.Yellow), "Pizza","Annihilate (Cascading Pizza attack)", "Boss Smash",0),
            new HitOnPlayerMechanic(37980, "Demonic Shock Wave", new MechanicPlotlySetting(Symbols.TriangleRightOpen,Colors.Red), "10% RSmash","Knockback (right hand) in 10% Phase", "10% Right Smash",0),
            new HitOnPlayerMechanic(38046, "Demonic Shock Wave", new MechanicPlotlySetting(Symbols.TriangleLeftOpen,Colors.Red), "10% LSmash","Knockback (left hand) in 10% Phase", "10% Left Smash",0),
            new HitOnPlayerMechanic(37982, "Demonic Shock Wave", new MechanicPlotlySetting(Symbols.Bowtie,Colors.Red), "10% DSmash","Knockback (both hands) in 10% Phase", "10% Double Smash",0),
            new PlayerBuffApplyMechanic(37733, "Tear Instability", new MechanicPlotlySetting(Symbols.Diamond,Colors.DarkTeal), "Tear","Collected a Demonic Tear", "Tear",0),
            new HitOnPlayerMechanic(37613, "Mind Crush", new MechanicPlotlySetting(Symbols.Square,Colors.Blue), "Mind Crush","Hit by Mind Crush without Bubble Protection", "Mind Crush",0, (de,log) => de.HealthDamage > 0),
            new PlayerBuffApplyMechanic(38187, "Weak Minded", new MechanicPlotlySetting(Symbols.SquareOpen,Colors.LightPurple), "Weak Mind","Weak Minded (Debuff after Mind Crush)", "Weak Minded",0),
            new PlayerBuffApplyMechanic(37730, "Chosen by Eye of Janthir", new MechanicPlotlySetting(Symbols.Circle,Colors.Green), "Green","Chosen by the Eye of Janthir", "Chosen (Green)",0),
            new PlayerBuffApplyMechanic(38169, "Teleported", new MechanicPlotlySetting(Symbols.CircleOpen,Colors.Green), "TP","Teleport to/from Demonic Realm", "Teleport",0),
            new EnemyBuffApplyMechanic(38224, "Unnatural Signet", new MechanicPlotlySetting(Symbols.SquareOpen,Colors.Teal), "DMG Debuff","Double Damage Debuff on Deimos", "+100% Dmg Buff",0)
            });
            Extension = "dei";
            GenericFallBackMethod = FallBackMethod.None;
            Icon = "https://wiki.guildwars2.com/images/e/e0/Mini_Ragged_White_Mantle_Figurehead.png";
            EncounterCategoryInformation.InSubCategoryOrder = 3;
        }

        protected override CombatReplayMap GetCombatMapInternal(ParsedEvtcLog log)
        {
            return new CombatReplayMap("https://i.imgur.com/FFs9cFq.png",
                            (765, 1000),
                            (-9542, 1932, -7004, 5250)/*,
                            (-27648, -9216, 27648, 12288),
                            (11774, 4480, 14078, 5376)*/);
        }
        internal override List<InstantCastFinder> GetInstantCastFinders()
        {
            return new List<InstantCastFinder>()
            {
                new DamageCastFinder(37872, 37872, InstantCastFinder.DefaultICD), // Demonic Aura
            };
        }
        protected override HashSet<int> GetUniqueNPCIDs()
        {
            return new HashSet<int>
            {
                (int)ArcDPSEnums.TargetID.Deimos,
                (int)ArcDPSEnums.TrashID.Saul,
                (int)ArcDPSEnums.TrashID.Thief,
                (int)ArcDPSEnums.TrashID.Drunkard,
                (int)ArcDPSEnums.TrashID.Gambler,
            };
        }

        protected override List<int> GetFriendlyNPCIDs()
        {
            return new List<int>
            {
                (int)ArcDPSEnums.TrashID.Saul
            };
        }

        private static void MergeWithGadgets(AgentItem target, HashSet<ulong> gadgetAgents, List<CombatItem> combatData, IReadOnlyDictionary<uint, AbstractExtensionHandler> extensions)
        {
            foreach (CombatItem c in combatData)
            {
                if (c.HasTime(extensions) && c.Time < target.LastAware && c.IsStateChange != ArcDPSEnums.StateChange.AttackTarget && c.IsStateChange != ArcDPSEnums.StateChange.Targetable)
                {
                    continue;
                }
                if (gadgetAgents.Contains(c.SrcAgent) && c.SrcIsAgent(extensions))
                {
                    if (c.IsStateChange == ArcDPSEnums.StateChange.MaxHealthUpdate)
                    {
                        continue;
                    }
                    if (c.IsStateChange == ArcDPSEnums.StateChange.HealthUpdate && c.DstAgent > 1500)
                    {
                        continue;
                    }
                    c.OverrideSrcAgent(target.Agent);
                }
                if (gadgetAgents.Contains(c.DstAgent) && c.DstIsAgent(extensions))
                {
                    c.OverrideDstAgent(target.Agent);
                }
            }
        }

        internal override List<AbstractBuffEvent> SpecialBuffEventProcess(CombatData combatData, SkillData skillData)
        {
            AbstractSingleActor target = Targets.FirstOrDefault(x => x.ID == (int)ArcDPSEnums.TargetID.Deimos);
            if (target == null)
            {
                throw new MissingKeyActorsException("Deimos not found");
            }
            var res = new List<AbstractBuffEvent>();
            IReadOnlyList<AbstractBuffEvent> signets = combatData.GetBuffData(SkillIDs.UnnaturalSignet);
            foreach (AbstractBuffEvent bfe in signets)
            {
                if (bfe is BuffApplyEvent ba)
                {
                    AbstractBuffEvent removal = signets.FirstOrDefault(x => x is BuffRemoveAllEvent && x.Time > bfe.Time && x.Time < bfe.Time + 30000);
                    if (removal == null)
                    {
                        res.Add(new BuffRemoveAllEvent(ParserHelper._unknownAgent, target.AgentItem, ba.Time + ba.AppliedDuration, 0, skillData.Get(38224), 1, 0));
                        res.Add(new BuffRemoveManualEvent(ParserHelper._unknownAgent, target.AgentItem, ba.Time + ba.AppliedDuration, 0, skillData.Get(38224)));
                    }
                }
                else if (bfe is BuffRemoveAllEvent)
                {
                    AbstractBuffEvent apply = signets.FirstOrDefault(x => x is BuffApplyEvent && x.Time < bfe.Time && x.Time > bfe.Time - 30000);
                    if (apply == null)
                    {
                        res.Add(new BuffApplyEvent(ParserHelper._unknownAgent, target.AgentItem, bfe.Time - 10000, 10000, skillData.Get(38224), uint.MaxValue, true));
                    }
                }
            }
            return res;
        }

        internal override void CheckSuccess(CombatData combatData, AgentData agentData, FightData fightData, IReadOnlyCollection<AgentItem> playerAgents)
        {
            base.CheckSuccess(combatData, agentData, fightData, playerAgents);
            if (!fightData.Success && _deimos10PercentTime > 0)
            {
                AbstractSingleActor deimos = Targets.FirstOrDefault(x => x.ID == (int)ArcDPSEnums.TargetID.Deimos);
                if (deimos == null)
                {
                    throw new MissingKeyActorsException("Deimos not found");
                }
                AgentItem saul = agentData.GetNPCsByID((int)ArcDPSEnums.TrashID.Saul).FirstOrDefault();
                if (saul == null)
                {
                    throw new MissingKeyActorsException("Saul not found");
                }
                if (combatData.GetDeadEvents(saul).Any())
                {
                    return;
                }
                IReadOnlyList<AttackTargetEvent> attackTargets = combatData.GetAttackTargetEvents(deimos.AgentItem);
                if (attackTargets.Count == 0)
                {
                    return;
                }
                AgentItem attackTarget = attackTargets.Last().AttackTarget;
                // sanity check
                TargetableEvent attackableEvent = combatData.GetTargetableEvents(attackTarget).LastOrDefault(x => x.Targetable && x.Time > _deimos10PercentTime - ParserHelper.ServerDelayConstant);
                if (attackableEvent == null)
                {
                    return;
                }
                TargetableEvent notAttackableEvent = combatData.GetTargetableEvents(attackTarget).LastOrDefault(x => !x.Targetable && x.Time > attackableEvent.Time);
                if (notAttackableEvent == null)
                {
                    return;
                }
                AbstractHealthDamageEvent lastDamageTaken = combatData.GetDamageTakenData(deimos.AgentItem).LastOrDefault(x => (x.HealthDamage > 0) && x.Time > _deimos10PercentTime && playerAgents.Contains(x.From.GetFinalMaster()));
                if (lastDamageTaken != null)
                {
                    if (!AtLeastOnePlayerAlive(combatData, fightData, notAttackableEvent.Time, playerAgents))
                    {
                        return;
                    }
                    fightData.SetSuccess(true, notAttackableEvent.Time);
                }
            }
        }

        private static long AttackTargetSpecialParse(CombatItem targetable, AgentData agentData, List<CombatItem> combatData, HashSet<ulong> gadgetAgents)
        {
            if (targetable == null)
            {
                return 0;
            }
            long firstAware = targetable.Time;
            AgentItem targetAgent = agentData.GetAgent(targetable.SrcAgent, targetable.Time);
            if (targetAgent == ParserHelper._unknownAgent)
            {
                return 0;
            }
            CombatItem attackTargetEvent = combatData.FirstOrDefault(x => x.IsStateChange == ArcDPSEnums.StateChange.AttackTarget && x.SrcMatchesAgent(targetAgent));
            if (attackTargetEvent == null)
            {
                return 0;
            }
            AgentItem deimosStructBody = agentData.GetAgent(attackTargetEvent.DstAgent, attackTargetEvent.Time);
            if (deimosStructBody == ParserHelper._unknownAgent)
            {
                return 0;
            }
            gadgetAgents.Add(deimosStructBody.Agent);
            CombatItem armDeimosDamageEvent = combatData.FirstOrDefault(x => x.Time >= firstAware && (x.SkillID == 37980 || x.SkillID == 37982 || x.SkillID == 38046) && x.SrcAgent != 0 && x.SrcInstid != 0 && !x.IsExtension);
            if (armDeimosDamageEvent != null)
            {
                gadgetAgents.Add(armDeimosDamageEvent.SrcAgent);
            }
            return firstAware;
        }

        internal override long GetFightOffset(FightData fightData, AgentData agentData, List<CombatItem> combatData)
        {
            IReadOnlyList<AgentItem> deimosAgents = agentData.GetNPCsByID((int)ArcDPSEnums.TargetID.Deimos);
            long start = fightData.LogStart;
            foreach (AgentItem deimos in deimosAgents)
            {
                // enter combat
                CombatItem spawnProtectionRemove = combatData.FirstOrDefault(x => x.DstMatchesAgent(deimos) && x.IsBuffRemove == ArcDPSEnums.BuffRemove.All && x.SkillID == 34113);
                if (spawnProtectionRemove != null)
                {
                    start = Math.Max(start, spawnProtectionRemove.Time);

                }
            }
            return start;
        }

        internal override List<ErrorEvent> GetCustomWarningMessages(FightData fightData, int arcdpsVersion)
        {
            List<ErrorEvent> res = base.GetCustomWarningMessages(fightData, arcdpsVersion);
            if (!fightData.IsCM)
            {
                res.Add(new ErrorEvent("Missing outgoing Saul damage due to % based damage"));
            }
            return res;
        }

        internal override void EIEvtcParse(ulong gw2Build, FightData fightData, AgentData agentData, List<CombatItem> combatData, IReadOnlyDictionary<uint, AbstractExtensionHandler> extensions)
        {
            ComputeFightTargets(agentData, combatData, extensions);
            // Find target
            AbstractSingleActor deimos = Targets.FirstOrDefault(x => x.ID == (int)ArcDPSEnums.TargetID.Deimos);
            if (deimos == null)
            {
                throw new MissingKeyActorsException("Deimos not found");
            }
            // invul correction
            CombatItem invulApp = combatData.FirstOrDefault(x => x.DstMatchesAgent(deimos.AgentItem) && x.IsBuffApply() && x.SkillID == 762);
            if (invulApp != null)
            {
                invulApp.OverrideValue((int)(deimos.LastAware - invulApp.Time));
            }
            // Deimos gadgets
            CombatItem targetable = combatData.LastOrDefault(x => x.IsStateChange == ArcDPSEnums.StateChange.Targetable && x.DstAgent > 0);
            var gadgetAgents = new HashSet<ulong>();
            long firstAware = AttackTargetSpecialParse(targetable, agentData, combatData, gadgetAgents);
            // legacy method
            if (firstAware == 0)
            {
                CombatItem armDeimosDamageEvent = combatData.FirstOrDefault(x => x.Time >= deimos.LastAware && (x.SkillID == 37980 || x.SkillID == 37982 || x.SkillID == 38046) && x.SrcAgent != 0 && x.SrcInstid != 0 && !x.IsExtension);
                if (armDeimosDamageEvent != null)
                {
                    var deimosGadgets = agentData.GetAgentByType(AgentItem.AgentType.Gadget).Where(x => x.Name.Contains("Deimos") && x.LastAware > armDeimosDamageEvent.Time).ToList();
                    if (deimosGadgets.Count > 0)
                    {
                        firstAware = deimosGadgets.Max(x => x.FirstAware);
                        gadgetAgents = new HashSet<ulong>(deimosGadgets.Select(x => x.Agent));
                    }
                }
            }
            if (gadgetAgents.Count > 0)
            {
                _deimos10PercentTime = (firstAware >= deimos.LastAware ? firstAware : deimos.LastAware);
                MergeWithGadgets(deimos.AgentItem, gadgetAgents, combatData, extensions);
                // Add custom spawn event
                combatData.Add(new CombatItem(_deimos10PercentTime + 1, deimos.AgentItem.Agent, 0, 0, 0, 0, 0, deimos.AgentItem.InstID, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 6, 0, 0, 0, 0));
            }
            deimos.AgentItem.OverrideAwareTimes(deimos.FirstAware, fightData.FightEnd);
            deimos.OverrideName("Deimos");
            foreach (AbstractSingleActor target in Targets)
            {
                if (target.ID == (int)ArcDPSEnums.TrashID.Thief || target.ID == (int)ArcDPSEnums.TrashID.Drunkard || target.ID == (int)ArcDPSEnums.TrashID.Gambler)
                {

                    string name = (target.ID == (int)ArcDPSEnums.TrashID.Thief ? "Thief" : (target.ID == (int)ArcDPSEnums.TrashID.Drunkard ? "Drunkard" : (target.ID == (int)ArcDPSEnums.TrashID.Gambler ? "Gambler" : "")));
                    target.OverrideName(name);
                }
            }
        }

        internal override List<PhaseData> GetPhases(ParsedEvtcLog log, bool requirePhases)
        {
            List<PhaseData> phases = GetInitialPhase(log);
            AbstractSingleActor mainTarget = Targets.FirstOrDefault(x => x.ID == (int)ArcDPSEnums.TargetID.Deimos);
            if (mainTarget == null)
            {
                throw new MissingKeyActorsException("Deimos not found");
            }
            phases[0].AddTarget(mainTarget);

            if (requirePhases)
            {
                phases = AddBossPhases(phases, log, mainTarget);
                phases = AddAddPhases(phases, log, mainTarget);
                phases = AddBurstPhases(phases, log, mainTarget);
            }

            return phases;
        }

        private List<PhaseData> AddBossPhases(List<PhaseData> phases, ParsedEvtcLog log, AbstractSingleActor mainTarget)
        {
            // Determined + additional data on inst change
            AbstractBuffEvent invulDei = log.CombatData.GetBuffData(SkillIDs.Determined762).FirstOrDefault(x => x is BuffApplyEvent && x.To == mainTarget.AgentItem);

            if (invulDei != null)
            {
                var phase100to10 = new PhaseData(0, invulDei.Time, "100% - 10%");
                phase100to10.AddTarget(mainTarget);
                phases.Add(phase100to10);

                if (_deimos10PercentTime > 0 && log.FightData.FightEnd - _deimos10PercentTime > ParserHelper.PhaseTimeLimit)
                {
                    var phase10to0 = new PhaseData(_deimos10PercentTime, log.FightData.FightEnd, "10% - 0%");
                    phase10to0.AddTarget(mainTarget);
                    phases.Add(phase10to0);
                }
                //mainTarget.AddCustomCastLog(end, -6, (int)(start - end), ParseEnum.Activation.None, (int)(start - end), ParseEnum.Activation.None, log);
            }

            return phases;
        }

        private List<PhaseData> AddAddPhases(List<PhaseData> phases, ParsedEvtcLog log, AbstractSingleActor mainTarget)
        {
            foreach (AbstractSingleActor target in Targets)
            {
                if (target.ID == (int)ArcDPSEnums.TrashID.Thief || target.ID == (int)ArcDPSEnums.TrashID.Drunkard || target.ID == (int)ArcDPSEnums.TrashID.Gambler)
                {
                    var addPhase = new PhaseData(target.FirstAware - 1000, Math.Min(target.LastAware + 1000, log.FightData.FightEnd), target.Character);
                    addPhase.AddTarget(target);
                    addPhase.OverrideTimes(log);
                    // override first then add Deimos so that it does not disturb the override process
                    addPhase.AddTarget(mainTarget);
                    phases.Add(addPhase);
                }
            }

            return phases;
        }     

        private static List<PhaseData> AddBurstPhases(List<PhaseData> phases, ParsedEvtcLog log, AbstractSingleActor mainTarget)
        {
            List<AbstractBuffEvent> signets = GetFilteredList(log.CombatData, UnnaturalSignet, mainTarget, true, true);
            long sigStart = 0;
            int burstID = 1;
            for (int i = 0; i < signets.Count; i++)
            {
                AbstractBuffEvent signet = signets[i];
                if (signet is BuffApplyEvent)
                {
                    sigStart = Math.Max(signet.Time + 1, 0);
                }
                else
                {
                    long sigEnd = Math.Min(signet.Time - 1, log.FightData.FightEnd);
                    var burstPhase = new PhaseData(sigStart, sigEnd, "Burst " + burstID++);
                    burstPhase.AddTarget(mainTarget);
                    phases.Add(burstPhase);
                }
            }
            return phases;
        }

        protected override List<int> GetTargetsIDs()
        {
            return new List<int>
            {
                (int)ArcDPSEnums.TargetID.Deimos,
                (int)ArcDPSEnums.TrashID.Thief,
                (int)ArcDPSEnums.TrashID.Drunkard,
                (int)ArcDPSEnums.TrashID.Gambler
            };
        }

        protected override List<ArcDPSEnums.TrashID> GetTrashMobsIDs()
        {
            return new List<ArcDPSEnums.TrashID>
            {
                ArcDPSEnums.TrashID.GamblerClones,
                ArcDPSEnums.TrashID.GamblerReal,
                ArcDPSEnums.TrashID.Greed,
                ArcDPSEnums.TrashID.Pride,
                ArcDPSEnums.TrashID.Oil,
                ArcDPSEnums.TrashID.Tear,
                ArcDPSEnums.TrashID.Hands
            };
        }

        internal override void ComputeNPCCombatReplayActors(NPC target, ParsedEvtcLog log, CombatReplay replay)
        {
            int start = (int)replay.TimeOffsets.start;
            int end = (int)replay.TimeOffsets.end;
            IReadOnlyList<AbstractCastEvent> cls = target.GetCastEvents(log, 0, log.FightData.FightEnd);
            switch (target.ID)
            {
                case (int)ArcDPSEnums.TargetID.Deimos:
                    var mindCrush = cls.Where(x => x.SkillId == 37613).ToList();
                    foreach (AbstractCastEvent c in mindCrush)
                    {
                        start = (int)c.Time;
                        end = start + 5000;
                        replay.Decorations.Add(new CircleDecoration(true, end, 180, (start, end), "rgba(255, 0, 0, 0.5)", new AgentConnector(target)));
                        replay.Decorations.Add(new CircleDecoration(false, 0, 180, (start, end), "rgba(255, 0, 0, 0.5)", new AgentConnector(target)));
                        if (!log.FightData.IsCM)
                        {
                            replay.Decorations.Add(new CircleDecoration(true, 0, 180, (start, end), "rgba(0, 0, 255, 0.3)", new PositionConnector(new Point3D(-8421.818f, 3091.72949f, -9.818082e8f))));
                        }
                    }
                    var annihilate = cls.Where(x => (x.SkillId == 38208) || (x.SkillId == 37929)).ToList();
                    foreach (AbstractCastEvent c in annihilate)
                    {
                        start = (int)c.Time;
                        int delay = 1000;
                        end = start + 2400;
                        int duration = 120;
                        Point3D facing = replay.Rotations.FirstOrDefault(x => x.Time >= start);
                        if (facing == null)
                        {
                            continue;
                        }
                        for (int i = 0; i < 6; i++)
                        {
                            replay.Decorations.Add(new PieDecoration(true, 0, 900, (int)Math.Round(Math.Atan2(facing.Y, facing.X) * 180 / Math.PI + i * 360 / 10), 360 / 10, (start + delay + i * duration, end + i * duration), "rgba(255, 200, 0, 0.5)", new AgentConnector(target)));
                            replay.Decorations.Add(new PieDecoration(false, 0, 900, (int)Math.Round(Math.Atan2(facing.Y, facing.X) * 180 / Math.PI + i * 360 / 10), 360 / 10, (start + delay + i * duration, end + i * 120), "rgba(255, 150, 0, 0.5)", new AgentConnector(target)));
                            if (i % 5 != 0)
                            {
                                replay.Decorations.Add(new PieDecoration(true, 0, 900, (int)Math.Round(Math.Atan2(facing.Y, facing.X) * 180 / Math.PI - i * 360 / 10), 360 / 10, (start + delay + i * duration, end + i * 120), "rgba(255, 200, 0, 0.5)", new AgentConnector(target)));
                                replay.Decorations.Add(new PieDecoration(false, 0, 900, (int)Math.Round(Math.Atan2(facing.Y, facing.X) * 180 / Math.PI - i * 360 / 10), 360 / 10, (start + delay + i * duration, end + i * 120), "rgba(255, 150, 0, 0.5)", new AgentConnector(target)));
                            }
                        }
                    }
                    List<AbstractBuffEvent> signets = GetFilteredList(log.CombatData, UnnaturalSignet, target, true, true);
                    int sigStart = 0;
                    int sigEnd = 0;
                    foreach (AbstractBuffEvent signet in signets)
                    {
                        if (signet is BuffApplyEvent)
                        {
                            sigStart = (int)signet.Time;
                        }
                        else
                        {
                            sigEnd = (int)signet.Time;
                            replay.Decorations.Add(new CircleDecoration(true, 0, 120, (sigStart, sigEnd), "rgba(0, 200, 200, 0.5)", new AgentConnector(target)));
                        }
                    }
                    break;
                case (int)ArcDPSEnums.TrashID.Gambler:
                case (int)ArcDPSEnums.TrashID.Thief:
                case (int)ArcDPSEnums.TrashID.Drunkard:
                    break;
                case (int)ArcDPSEnums.TrashID.GamblerClones:
                case (int)ArcDPSEnums.TrashID.GamblerReal:
                case (int)ArcDPSEnums.TrashID.Greed:
                case (int)ArcDPSEnums.TrashID.Pride:
                case (int)ArcDPSEnums.TrashID.Tear:
                    break;
                case (int)ArcDPSEnums.TrashID.Hands:
                    replay.Decorations.Add(new CircleDecoration(true, 0, 90, (start, end), "rgba(255, 0, 0, 0.2)", new AgentConnector(target)));
                    break;
                case (int)ArcDPSEnums.TrashID.Oil:
                    int delayOil = 3000;
                    replay.Decorations.Add(new CircleDecoration(true, start + delayOil, 200, (start, start + delayOil), "rgba(255,100, 0, 0.5)", new AgentConnector(target)));
                    replay.Decorations.Add(new CircleDecoration(true, 0, 200, (start + delayOil, end), "rgba(0, 0, 0, 0.5)", new AgentConnector(target)));
                    break;
                default:
                    break;
            }

        }

        internal override void ComputePlayerCombatReplayActors(AbstractPlayer p, ParsedEvtcLog log, CombatReplay replay)
        {
            // teleport zone
            List<AbstractBuffEvent> tpDeimos = GetFilteredList(log.CombatData, DeimosSelectedByGreen, p, true, true);
            int tpStart = 0;
            foreach (AbstractBuffEvent c in tpDeimos)
            {
                if (c is BuffApplyEvent)
                {
                    tpStart = (int)c.Time;
                }
                else
                {
                    int tpEnd = (int)c.Time;
                    replay.Decorations.Add(new CircleDecoration(true, 0, 180, (tpStart, tpEnd), "rgba(0, 150, 0, 0.3)", new AgentConnector(p)));
                    replay.Decorations.Add(new CircleDecoration(true, tpEnd, 180, (tpStart, tpEnd), "rgba(0, 150, 0, 0.3)", new AgentConnector(p)));
                }
            }
        }

        internal override FightData.CMStatus IsCM(CombatData combatData, AgentData agentData, FightData fightData)
        {
            AbstractSingleActor target = Targets.FirstOrDefault(x => x.ID == (int)ArcDPSEnums.TargetID.Deimos);
            if (target == null)
            {
                throw new MissingKeyActorsException("Deimos not found");
            }
            FightData.CMStatus cmStatus = (target.GetHealth(combatData) > 40e6) ? FightData.CMStatus.CM : FightData.CMStatus.NoCM;
            
            if (_deimos10PercentTime > 0)
            {
                // Deimos gains additional health during the last 10% so the max-health needs to be corrected
                // done here because this method will get called during the creation of the ParsedEvtcLog and the ParsedEvtcLog should contain complete and correct values after creation
                if (cmStatus == FightData.CMStatus.CM)
                {
                    target.SetManualHealth(42804900);
                }
                else
                {
                    target.SetManualHealth(37388210);
                }
            }

            return cmStatus;
        }
    }
}
