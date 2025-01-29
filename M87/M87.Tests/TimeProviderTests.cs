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
        public async Task TimeProvider_ShouldEmitTickEverySecond()
        {
            // Arrange
            DateTime startTime = new DateTime(2025, 1, 1, 9, 30, 0);
            var timeProvider = new SimulatorCore.Engine.TimeProvider(startTime);
            int tickCount = 0;
            DateTime lastTickTime = startTime;

            timeProvider.OnTick += (currentTime) =>
            {
                Interlocked.Increment(ref tickCount);
                lastTickTime = currentTime;
            };

            // Act
            timeProvider.Start();
            await Task.Delay(3100); // Wait ~3 ticks
            timeProvider.Stop();

            // Assert: Expecting at least 3 ticks
            Assert.InRange(tickCount, 3, 4);
            Assert.Equal(startTime.AddSeconds(tickCount), lastTickTime);
        }

        [Fact]
        public void TimeProvider_ShouldStartAndStopCorrectly()
        {
            // Arrange
            DateTime startTime = new DateTime(2025, 1, 1, 9, 30, 0);
            var timeProvider = new SimulatorCore.Engine.TimeProvider(startTime);
            bool ticked = false;

            timeProvider.OnTick += (currentTime) => { ticked = true; };

            // Act
            timeProvider.Start();
            Thread.Sleep(1500); // Wait 1.5 seconds
            timeProvider.Stop();

            // Assert: Should have at least one tick
            Assert.True(ticked, "At least one tick should have been emitted.");
        }

        [Fact]
        public void TimeProvider_ShouldNotEmitTickAfterStop()
        {
            // Arrange
            DateTime startTime = new DateTime(2025, 1, 1, 9, 30, 0);
            var timeProvider = new SimulatorCore.Engine.TimeProvider(startTime);
            int tickCount = 0;

            timeProvider.OnTick += (currentTime) => { Interlocked.Increment(ref tickCount); };

            // Act
            timeProvider.Start();
            Thread.Sleep(2100); // Wait for ~2 ticks
            timeProvider.Stop();
            int ticksAfterStop = tickCount;
            Thread.Sleep(2000); // Wait another 2 seconds

            // Assert
            Assert.InRange(tickCount, 2, 3); // Before stopping, 2-3 ticks should occur
            Assert.Equal(ticksAfterStop, tickCount); // No extra ticks after stopping
        }
    }
}
