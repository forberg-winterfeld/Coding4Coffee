using Coding4Coffee.Basics.Configuration;

namespace Coding4Coffee.WinForms.Configuration.Controls
{
    internal sealed class ConfigurationValueBinding<T> : IDisposable
    {
        private readonly Control _control;
        private readonly Func<T?> _getValue;
        private readonly Action<T?> _setValue;
        private readonly int _ownerThreadId = Environment.CurrentManagedThreadId;
        private readonly Lock _updateLock = new();
        private readonly System.Windows.Forms.Timer? _timer;
        private Config<T>? _config;
        private string? _key;
        private bool _updating;
        private bool _handleReady;
        private bool _updatePending;
        private bool _disposed;

        public ConfigurationValueBinding(Control control, Func<T?> getValue, Action<T?> setValue, bool debounce)
        {
            _control = control;
            _getValue = getValue;
            _setValue = setValue;
            if (debounce)
            {
                _timer = new System.Windows.Forms.Timer { Interval = 300 };
                _timer.Tick += Timer_Tick;
            }
            control.HandleCreated += Control_HandleCreated;
            control.HandleDestroyed += Control_HandleDestroyed;
        }

        public int DebounceInterval
        {
            get => _timer!.Interval;
            set => _timer!.Interval = Math.Max(10, value);
        }

        public string? Key
        {
            get => _key;
            set
            {
                if (_key == value)
                    return;
                var config = string.IsNullOrWhiteSpace(value) ? null : ConfigFactory.GetConfig<T>(value);
                lock (_updateLock)
                {
                    if (_updatePending)
                    {
                        _updatePending = false;
                        _timer?.Stop();
                    }
                }
                Flush();
                _config?.ValueChanged -= Config_ValueChanged;
                _config = config;
                _key = value;
                _config?.ValueChanged += Config_ValueChanged;
                ApplyConfig();
            }
        }

        public void ValueChanged(T? value)
        {
            if (!_updating && _config != null)
                _config.Value = value;
        }

        public void ValueChanged()
        {
            if (_updating || _config == null)
                return;
            if (_timer == null)
                _config.Value = _getValue();
            else
            {
                _timer.Stop();
                _timer.Start();
            }
        }

        public void ChangeRange(Action change)
        {
            bool wasUpdating = _updating;
            _updating = true;
            try
            {
                change();
            }
            finally
            {
                _updating = wasUpdating;
            }
            if (!wasUpdating && _timer?.Enabled != true)
                ApplyConfig();
        }

        private void Config_ValueChanged(object? sender, EventArgs e)
        {
            if (Environment.CurrentManagedThreadId == _ownerThreadId)
            {
                ApplyConfig();
                return;
            }
            lock (_updateLock)
            {
                if (_disposed)
                    return;
                _updatePending = true;
                if (_handleReady)
                {
                    try
                    {
                        _control.BeginInvoke(ApplyPendingUpdate);
                    }
                    catch (InvalidOperationException) when (!_control.IsHandleCreated)
                    {
                        // The pending update will be applied when the handle is recreated.
                    }
                }
            }
        }

        private void Control_HandleCreated(object? sender, EventArgs e)
        {
            lock (_updateLock)
                _handleReady = true;
            ApplyPendingUpdate();
        }

        private void Control_HandleDestroyed(object? sender, EventArgs e)
        {
            lock (_updateLock)
                _handleReady = false;
        }

        private void ApplyPendingUpdate()
        {
            lock (_updateLock)
            {
                if (_disposed || !_updatePending)
                    return;
                _updatePending = false;
            }
            ApplyConfig();
        }

        public void ApplyConfig()
        {
            if (_config == null)
                return;
            _timer?.Stop();
            bool wasUpdating = _updating;
            _updating = true;
            try
            {
                _setValue(_config.Value);
            }
            finally
            {
                _updating = wasUpdating;
            }
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            ApplyPendingUpdate();
            Flush();
        }

        private void Flush()
        {
            if (_timer?.Enabled != true)
                return;
            _timer.Stop();
            _config?.Value = _getValue();
        }

        public void Dispose()
        {
            try
            {
                if (!_disposed)
                {
                    ApplyPendingUpdate();
                    Flush();
                }
            }
            finally
            {
                if (!_disposed)
                {
                    lock (_updateLock)
                        _disposed = true;
                    if (_timer != null)
                    {
                        _timer.Stop();
                        _timer.Tick -= Timer_Tick;
                        _timer.Dispose();
                    }
                    _control.HandleCreated -= Control_HandleCreated;
                    _control.HandleDestroyed -= Control_HandleDestroyed;
                    _config?.ValueChanged -= Config_ValueChanged;
                    _config = null;
                }
            }
        }
    }
}
