using Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SensorProcessing
{
    internal abstract class ConditionDetectorBase
    {
        private readonly LightSensorIndexes targetSensor;

        public bool State { get; protected set; }

        protected ConditionDetectorBase(LightSensorIndexes targetSensor)
        {
            this.targetSensor = targetSensor;
        }

        internal bool Accept(LightSensorIndexes sensor, byte value)
        {
            if (sensor == targetSensor)
                DoAccept(sensor, value);

            return State;
        }

        protected abstract void DoAccept(LightSensorIndexes sensor, byte value);
    }

    internal class BalanceCondition : ConditionDetectorBase
    {
        private double a;
        private double b;

        private readonly double maxRatioWhenAWins;
        private readonly double maxRatioWhenBWins;

        internal BalanceCondition(double maxRatioWhenAWins, double maxRatioWhenBWins)
            :base(LightSensorIndexes.All)
        {
            this.maxRatioWhenAWins = maxRatioWhenAWins;
            this.maxRatioWhenBWins = maxRatioWhenBWins;
        }

        protected override void DoAccept(LightSensorIndexes sensor, byte value)
        {
            switch (sensor)
            {
                case LightSensorIndexes.Sensor_A:
                    a = value;
                    break;
                case LightSensorIndexes.Sensor_B:
                    b = value;
                    break;
            }

            if (a > b)
            {
                var ratio = a / b;
                State = ratio >= maxRatioWhenAWins;
            }
            else
            {
                var ratio = b / a;
                State = ratio >= maxRatioWhenBWins;
            }
        }
    }

    internal class LevelAttackCondition : ConditionDetectorBase
    {
        private readonly double maxAttack;
        private readonly double inertion;

        private DateTime lastTime;
        private double lastValue;

        /// <param name="sensor">Target sensor</param>
        /// <param name="maxAttack">Maximum delta in percent/second</param>
        /// <param name="inertion">1 - no inertion, 0 - max inertion</param>
        internal LevelAttackCondition(LightSensorIndexes sensor, double maxAttack, double inertion)
            :base(sensor)
        {
            this.maxAttack = maxAttack;
            this.inertion = inertion;
        }

        protected override void DoAccept(LightSensorIndexes sensor, byte value)
        {
            var now = DateTime.Now;
            var period = lastTime - now;

            var delta = Math.Abs((double)value - lastValue);

            var deltaPercent = (lastValue + delta) / lastValue;

            var deltaPercentPerSecond = deltaPercent / period.TotalSeconds;

            State = deltaPercentPerSecond >= maxAttack;

            lastValue = (lastValue + value) * inertion;
            lastTime = now;
        }
    }

}
