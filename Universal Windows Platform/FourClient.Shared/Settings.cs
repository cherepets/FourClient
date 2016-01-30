﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.ApplicationModel;
using FourToolkit.Extensions.Runtime;
using FourToolkit.Settings.SettingsProviders;
using System;
using FourToolkit.UI;
using System.Linq;
using System.Collections.ObjectModel;

namespace FourClient
{
    public class Settings : FourToolkit.Settings.Settings, INotifyPropertyChanged
    {
        public string DisplayVersion => Package.Current.Id.Version.ToDisplayVersion();
        public long CurrentVersion => Package.Current.Id.Version.ToNumber();
        public long LastVersion
        {
            get
            {
                try
                {
                    return GetProperty<long>();
                }
                catch (InvalidCastException)
                {
                    return default(long);
                }
            }
            set
            {
                SetProperty(value);
                OnPropertyChanged();
            }
        }
        public bool AppTheme
        {
            get { return GetProperty<bool>(); }
            set
            {
                SetProperty(value);
                OnPropertyChanged();
            }
        }
        public bool ArticleTheme
        {
            get { return GetProperty<bool>(); }
            set
            {
                SetProperty(value);
                OnPropertyChanged();
            }
        }
        public bool LiveTile
        {
            get { return GetProperty<bool>(); }
            set
            {
                SetProperty(value);
                OnPropertyChanged();
            }
        }
        public int FontSize
        {
            get { return GetProperty<int>(); }
            set
            {
                SetProperty(value);
                OnPropertyChanged();
            }
        }
        public string FontFace
        {
            get { return GetProperty<string>(); }
            set
            {
                SetProperty(value);
                OnPropertyChanged();
            }
        }
        public string Align
        {
            get { return GetProperty<string>(); }
            set
            {
                SetProperty(value);
                OnPropertyChanged();
            }
        }
        public string YouTube
        {
            get { return GetProperty<string>(); }
            set
            {
                SetProperty(value);
                OnPropertyChanged();
            }
        }
        public ObservableCollection<string> HiddenSources
        {
            get
            {
                if (_hiddenSources == null)
                {
                    _hiddenSources = new ObservableCollection<string>(GetProperty<string>().Split(','));
                    _hiddenSources.CollectionChanged += (s, a) => HiddenSources = s as ObservableCollection<string>;
                }
                return _hiddenSources;
            }
            set
            {
                SetProperty(string.Join(",", value.ToArray()));
                _hiddenSources = value;
                OnPropertyChanged();
            }
        }
        private ObservableCollection<string> _hiddenSources;

        Settings()
        {
            var defaults = new Dictionary<string, object>
            {
                {nameof(LastVersion), new PackageVersion()},
                {nameof(AppTheme), false},
                {nameof(ArticleTheme), false},
                {nameof(LiveTile), true},
                {nameof(FontSize), Platform.IsMobile ? 2 : 3},
                {nameof(FontFace), "Segoe UI"},
                {nameof(Align), "left"},
                {nameof(YouTube), "vnd.youtube:"},
                {nameof(HiddenSources), string.Empty},
            };
            DefaultsProvider = new DictionarySettingsProvider(defaults);
            SettingsProvider = new ApplicationDataSettingsProvider(ApplicationDataContainerType.Local);
        }

        public static Settings Current { get; } = new Settings();
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
