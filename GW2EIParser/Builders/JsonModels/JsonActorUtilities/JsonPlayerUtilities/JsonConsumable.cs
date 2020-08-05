﻿using GW2EIEvtcParser.EIData;

namespace GW2EIBuilders.JsonModels
{
    /// <summary>
    /// Class representing consumables
    /// </summary>
    public class JsonConsumable
    {
        /// <summary>
        /// Number of stacks
        /// </summary>
        public int Stack { get; }
        /// <summary>
        /// Duration of the consumable
        /// </summary>
        public int Duration { get; }
        /// <summary>
        /// Time of application of the consumable
        /// </summary>
        public long Time { get; }
        /// <summary>
        /// ID of the consumable
        /// </summary>
        /// <seealso cref="JsonLog.BuffMap"/>
        public long Id { get; }

        internal JsonConsumable(Consumable food)
        {
            Stack = food.Stack;
            Duration = food.Duration;
            Time = food.Time;
            Id = food.Buff.ID;
        }
    }
}
