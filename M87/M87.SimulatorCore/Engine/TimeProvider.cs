using System;
using System.Timers;

namespace M87.SimulatorCore.Engine
{
    public class TimeProvider
    {
        public event Action<DateTime> OnTick;
        private System.Timers.Timer _timer;
        private DateTime _currentTime;
        private TimeSpan _tickInterval;
        private TimeSpan _deltaTime;

        public TimeSpan DeltaTime => _deltaTime;
        public DateTime CurrentTime => _currentTime; // Aggiunta della proprietà CurrentTime

        public TimeProvider(DateTime startTime, TimeSpan tickInterval, TimeSpan deltaTime)
        {
            _currentTime = startTime;
            _tickInterval = tickInterval;
            _deltaTime = deltaTime;
            _timer = new System.Timers.Timer(_tickInterval.TotalMilliseconds);
            _timer.Elapsed += TimerElapsed;
        }

        public void Start()
        {
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            _currentTime = _currentTime.Add(_deltaTime);
            OnTick?.Invoke(_currentTime);
        }
    }
}
