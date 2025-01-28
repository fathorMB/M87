using Xunit;
using M87.SimulatorCore.Engine;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace M87.Tests
{
    public class TimeProviderTests
    {
        [Fact]
        public async Task TimeProvider_ShouldEmitTickAtSpecifiedInterval()
        {
            // Arrange
            DateTime startTime = new DateTime(2025, 1, 1, 9, 30, 0);
            TimeSpan tickInterval = TimeSpan.FromMilliseconds(100); // 100 ms per test velocità
            TimeSpan deltaTime = TimeSpan.FromMinutes(1);

            var timeProvider = new SimulatorCore.Engine.TimeProvider(startTime, tickInterval, deltaTime);
            int tickCount = 0;
            DateTime lastTickTime = startTime;

            timeProvider.OnTick += (currentTime) =>
            {
                Interlocked.Increment(ref tickCount);
                lastTickTime = currentTime;
            };

            // Act
            timeProvider.Start();
            await Task.Delay(350); // Attendere per 350 ms
            timeProvider.Stop();

            // Assert
            Assert.InRange(tickCount, 3, 4); // Dovrebbero essere emessi 3 tick (a 100ms, 200ms, 300ms)
            Assert.Equal(startTime.Add(deltaTime * tickCount), lastTickTime);
        }

        [Fact]
        public void TimeProvider_ShouldStartAndStopCorrectly()
        {
            // Arrange
            DateTime startTime = new DateTime(2025, 1, 1, 9, 30, 0);
            TimeSpan tickInterval = TimeSpan.FromMilliseconds(100);
            TimeSpan deltaTime = TimeSpan.FromMinutes(1);

            var timeProvider = new SimulatorCore.Engine.TimeProvider(startTime, tickInterval, deltaTime);
            bool ticked = false;
            timeProvider.OnTick += (currentTime) =>
            {
                ticked = true;
            };

            // Act
            timeProvider.Start();
            Thread.Sleep(150); // Attendere 150 ms
            timeProvider.Stop();

            // Assert
            Assert.True(ticked, "Dovrebbe essere emesso almeno un tick.");
        }

        [Fact]
        public void TimeProvider_ShouldNotEmitTickAfterStop()
        {
            // Arrange
            DateTime startTime = new DateTime(2025, 1, 1, 9, 30, 0);
            TimeSpan tickInterval = TimeSpan.FromMilliseconds(100);
            TimeSpan deltaTime = TimeSpan.FromMinutes(1);

            var timeProvider = new SimulatorCore.Engine.TimeProvider(startTime, tickInterval, deltaTime);
            int tickCount = 0;

            timeProvider.OnTick += (currentTime) =>
            {
                Interlocked.Increment(ref tickCount);
            };

            // Act
            timeProvider.Start();
            Thread.Sleep(150); // Attendere 150 ms
            timeProvider.Stop();
            int ticksAfterStop = tickCount;
            Thread.Sleep(150); // Attendere ulteriori 150 ms

            // Assert
            Assert.InRange(tickCount, 1, 2); // Prima fase
            Assert.Equal(ticksAfterStop, tickCount); // Nessun tick dopo lo stop
        }
    }
}
