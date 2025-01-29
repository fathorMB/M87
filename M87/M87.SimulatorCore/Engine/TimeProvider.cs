using System;
using System.Timers;

namespace M87.SimulatorCore.Engine
{
    public class TimeProvider
    {
        public event Action<DateTime> OnTick;
        private System.Timers.Timer _timer;
        private DateTime _currentTime;
        private readonly TimeSpan _tickInterval = TimeSpan.FromSeconds(1); // Fixed 1s tick
        private readonly TimeSpan _deltaTime = TimeSpan.FromSeconds(1); // Fixed 1s deltaTime

        public TimeSpan DeltaTime => _deltaTime;
        public DateTime CurrentTime => _currentTime;

        public TimeProvider(DateTime startTime)
        {
            _currentTime = startTime;
            _timer = new System.Timers.Timer(_tickInterval.TotalMilliseconds);
            _timer.Elapsed += TimerElapsed;
        }

        public void Start() => _timer.Start();
        public void Stop() => _timer.Stop();

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            _currentTime = _currentTime.Add(_deltaTime);
            OnTick?.Invoke(_currentTime);
        }
    }
}
