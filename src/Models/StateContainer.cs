namespace AzureNamingTool.Models
{
    /// <summary>
    /// Represents the state of the application
    /// </summary>
    public class StateContainer
    {
        private bool? _verified;
        private bool? _admin;
        private bool? _password;
        private string _apptheme = String.Empty;
        private bool? _latestnewsenabled;
        /// <summary>
        /// Gets or sets the reloadnav setting.
        /// </summary>
        public bool _reloadnav;
        /// <summary>
        /// Gets or sets the _configurationdatasynced setting.
        /// </summary>
        public bool? _configurationdatasynced;

        /// <summary>
        /// Gets or sets the Verified state.
        /// </summary>
        public bool Verified
        {
            get => _verified ?? false;
            set
            {
                _verified = value;
                NotifyStateChanged();
            }
        }

        /// <summary>
        /// Sets the Verified state.
        /// </summary>
        /// <param name="verified">The Verified state.</param>
        public void SetVerified(bool verified)
        {
            _verified = verified;
            NotifyStateChanged();
        }

        /// <summary>
        /// Gets or sets the Admin state.
        /// </summary>
        public bool Admin
        {
            get => _admin ?? false;
            set
            {
                _admin = value;
                NotifyStateChanged();
            }
        }

        /// <summary>
        /// Sets the Admin state.
        /// </summary>
        /// <param name="admin">The Admin state.</param>
        public void SetAdmin(bool admin)
        {
            _admin = admin;
            NotifyStateChanged();
        }

        /// <summary>
        /// Gets or sets the Password state.
        /// </summary>
        public bool Password
        {
            get => _password ?? false;
            set
            {
                _password = value;
                NotifyStateChanged();
            }
        }

        /// <summary>
        /// Sets the Password state.
        /// </summary>
        /// <param name="password">The Password state.</param>
        public void SetPassword(bool password)
        {
            _password = password;
            NotifyStateChanged();
        }

        /// <summary>
        /// Gets or sets the AppTheme value.
        /// </summary>
        public string AppTheme
        {
            get => _apptheme ?? "bg-default text-dark";
            set
            {
                _apptheme = value;
                NotifyStateChanged();
            }
        }

        /// <summary>
        /// Sets the AppTheme value.
        /// </summary>
        /// <param name="value">The AppTheme value.</param>
        public void SetAppTheme(string value)
        {
            _apptheme = value;
            NotifyStateChanged();
        }

        /// <summary>
        /// Gets or sets the LatestNewsEnabled state.
        /// </summary>
        public bool LatestNewsEnabled
        {
            get => _latestnewsenabled ?? true;
            set
            {
                _latestnewsenabled = value;
                NotifyStateChanged();
            }
        }

        /// <summary>
        /// Sets the LatestNewsEnabled state.
        /// </summary>
        /// <param name="latestnewsenabled">The LatestNewsEnabled state.</param>
        public void SetLatestNewsEnabled(bool latestnewsenabled)
        {
            _latestnewsenabled = latestnewsenabled;
            NotifyStateChanged();
        }

        /// <summary>
        /// Sets the NavReload state.
        /// </summary>
        /// <param name="reloadnav">The NavReload state.</param>
        public void SetNavReload(bool reloadnav)
        {
            _reloadnav = reloadnav;
            NotifyStateChanged();
        }

        /// <summary>
        /// Gets or sets the ConfigurationDataSynced state.
        /// </summary>
        public bool ConfigurationDataSynced
        {
            get => _configurationdatasynced ?? false;
            set
            {
                _configurationdatasynced = value;
                NotifyStateChanged();
            }
        }

        /// <summary>
        /// Sets the ConfigurationDataSynced state.
        /// </summary>
        /// <param name="configurationdatasynced">The ConfigurationDataSynced state.</param>
        public void SetConfigurationDataSynced(bool configurationdatasynced)
        {
            _configurationdatasynced = configurationdatasynced;
            NotifyStateChanged();
        }

        /// <summary>
        /// Event that is triggered when the state changes.
        /// </summary>
        public event Action? OnChange;

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}