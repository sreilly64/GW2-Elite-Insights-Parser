﻿using System.Collections.Generic;
using System.Linq;
using GW2EIEvtcParser.EIData;
using GW2EIEvtcParser.Exceptions;
using GW2EIEvtcParser.ParsedData;

namespace GW2EIEvtcParser.EncounterLogic
{
    internal class FraenirOfJormag : Bjora
    {
        public FraenirOfJormag(int triggerID) : base(triggerID)
        {
            MechanicList.AddRange(new List<Mechanic>
            {
                new HitOnPlayerMechanic(58811, "Icequake", new MechanicPlotlySetting(Symbols.Hexagram,Colors.Red), "Icequake","Icequake", "Icequake",4000, (de, log) => !de.To.HasBuff(log, SkillIDs.Stability, de.Time - ParserHelper.ServerDelayConstant)),
                new HitOnPlayerMechanic(58740, "Ice Shock Wave", new MechanicPlotlySetting(Symbols.Square,Colors.Red), "Ice Shock Wave","Ice Shock Wave", "Ice Shock Wave",4000, (de, log) => !de.To.HasBuff(log, SkillIDs.Stability, de.Time - ParserHelper.ServerDelayConstant)),
                new PlayerBuffApplyMechanic(58376, "Frozen", new MechanicPlotlySetting(Symbols.Circle,Colors.Blue), "Frozen","Frozen", "Frozen",500),
                new PlayerBuffRemoveMechanic(58376, "Unfrozen", new MechanicPlotlySetting(Symbols.CircleOpen,Colors.Blue), "Unfrozen","Unfrozen", "Unfrozen",500),
                new PlayerBuffApplyMechanic(58276, "Snowblind", new MechanicPlotlySetting(Symbols.Square,Colors.Blue), "Snowblind","Snowblind", "Snowblind",500),
            }
            );
            Extension = "fraenir";
            Icon = "https://wiki.guildwars2.com/images/thumb/6/67/Fraenir_of_Jormag.jpg/208px-Fraenir_of_Jormag.jpg";
            EncounterCategoryInformation.InSubCategoryOrder = 0;
        }

        protected override CombatReplayMap GetCombatMapInternal(ParsedEvtcLog log)
        {
            return new CombatReplayMap("https://i.imgur.com/kLjZ7eU.png",
                            (905, 789),
                            (-833, -1780, 2401, 1606)/*,
                            (-0, -0, 0, 0),
                            (0, 0, 0, 0)*/);
        }

        internal override List<InstantCastFinder> GetInstantCastFinders()
        {
            return new List<InstantCastFinder>()
            {
                new DamageCastFinder(58219, 58219, InstantCastFinder.DefaultICD), // Frostbite Aura
            };
        }

        internal override List<PhaseData> GetPhases(ParsedEvtcLog log, bool requirePhases)
        {
            List<PhaseData> phases = GetInitialPhase(log);
            AbstractSingleActor fraenir = Targets.FirstOrDefault(x => x.ID == (int)ArcDPSEnums.TargetID.FraenirOfJormag);
            if (fraenir == null)
            {
                throw new MissingKeyActorsException("Fraenir of Jormag not found");
            }
            phases[0].AddTarget(fraenir);
            AbstractSingleActor icebrood = Targets.FirstOrDefault(x => x.ID == (int)ArcDPSEnums.TargetID.IcebroodConstructFraenir);
            if (icebrood != null)
            {
                phases[0].AddTarget(icebrood);
            }
            if (!requirePhases)
            {
                return phases;
            }
            AbstractBuffEvent invulApplyFraenir = log.CombatData.GetBuffData(SkillIDs.Determined762).Where(x => x.To == fraenir.AgentItem && x is BuffApplyEvent).FirstOrDefault();
            if (invulApplyFraenir != null)
            {
                // split happened
                phases.Add(new PhaseData(0, invulApplyFraenir.Time, "Fraenir 1"));
                if (icebrood != null)
                {
                    // icebrood enters combat
                    EnterCombatEvent enterCombatIce = log.CombatData.GetEnterCombatEvents(icebrood.AgentItem).LastOrDefault();
                    if (enterCombatIce != null)
                    {
                        // icebrood phasing
                        AbstractBuffEvent invulApplyIce = log.CombatData.GetBuffData(SkillIDs.Invulnerability757).Where(x => x.To == icebrood.AgentItem && x is BuffApplyEvent).FirstOrDefault();
                        AbstractBuffEvent invulRemoveIce = log.CombatData.GetBuffData(SkillIDs.Invulnerability757).Where(x => x.To == icebrood.AgentItem && x is BuffRemoveAllEvent).FirstOrDefault();
                        long icebroodStart = enterCombatIce.Time;
                        long icebroodEnd = log.FightData.FightEnd;
                        if (invulApplyIce != null && invulRemoveIce != null)
                        {
                            long icebrood2Start = invulRemoveIce.Time;
                            phases.Add(new PhaseData(icebroodStart + 1, invulApplyIce.Time, "Icebrood 1"));
                            AbstractBuffEvent invulRemoveFraenir = log.CombatData.GetBuffData(SkillIDs.Determined762).Where(x => x.To == fraenir.AgentItem && x is BuffRemoveAllEvent).FirstOrDefault();
                            if (invulRemoveFraenir != null)
                            {
                                // fraenir came back
                                DeadEvent deadIce = log.CombatData.GetDeadEvents(icebrood.AgentItem).LastOrDefault();
                                if (deadIce != null)
                                {
                                    icebroodEnd = deadIce.Time;
                                }
                                else
                                {
                                    icebroodEnd = invulRemoveFraenir.Time - 1;
                                }
                                phases.Add(new PhaseData(invulRemoveFraenir.Time, log.FightData.FightEnd, "Fraenir 2"));
                            }
                            phases.Add(new PhaseData(icebrood2Start, icebroodEnd, "Icebrood 2"));
                        }
                        phases.Add(new PhaseData(icebroodStart, icebroodEnd, "Icebrood"));
                    }
                }
            }
            phases.Sort((x, y) => x.Start.CompareTo(y.Start));
            for (int i = 1; i < phases.Count; i++)
            {
                PhaseData phase = phases[i];
                if (i == 1 || i == 5)
                {
                    phase.AddTarget(fraenir);
                }
                else
                {
                    phase.AddTarget(icebrood);
                }
            }
            return phases;
        }

        protected override List<int> GetTargetsIDs()
        {
            return new List<int>
            {
                (int)ArcDPSEnums.TargetID.FraenirOfJormag,
                (int)ArcDPSEnums.TargetID.IcebroodConstructFraenir,
            };
        }

        protected override List<ArcDPSEnums.TrashID> GetTrashMobsIDs()
        {
            return new List<ArcDPSEnums.TrashID>
            {
                ArcDPSEnums.TrashID.IcebroodElemental
            };
        }
    }
}
