﻿using Mixer.Base.Util;
using MixItUp.Base;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Desktop.Services;
using MixItUp.WPF.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.MainControls
{
    /// <summary>
    /// Interaction logic for SongRequestControl.xaml
    /// </summary>
    public partial class SongRequestControl : MainControlBase
    {
        private static SemaphoreSlim songListLock = new SemaphoreSlim(1);

        private ObservableCollection<SongRequestItem> requests = new ObservableCollection<SongRequestItem>();

        private CancellationTokenSource backgroundThreadCancellationTokenSource = new CancellationTokenSource();

        public SongRequestControl()
        {
            InitializeComponent();
        }

        protected override async Task InitializeInternal()
        {
            GlobalEvents.OnSongRequestsChangedOccurred += GlobalEvents_OnSongRequestsChangedOccurred;

            this.SongRequestsQueueListView.ItemsSource = this.requests;

            this.SpotifyToggleButton.IsChecked = ChannelSession.Settings.SongRequestServiceTypes.Contains(SongRequestServiceTypeEnum.Spotify);
            this.YouTubeToggleButton.IsChecked = ChannelSession.Settings.SongRequestServiceTypes.Contains(SongRequestServiceTypeEnum.Youtube);

            this.SpotifyAllowExplicitSongToggleButton.IsChecked = ChannelSession.Settings.SpotifyAllowExplicit;

            await this.RefreshRequestsList();

            await base.InitializeInternal();
        }

        private async void EnableSongRequestsToggleButton_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                if (!this.SpotifyToggleButton.IsChecked.GetValueOrDefault() && !this.YouTubeToggleButton.IsChecked.GetValueOrDefault())
                {
                    await MessageBoxHelper.ShowMessageDialog("At least 1 song request service must be set");
                    this.EnableSongRequestsToggleButton.IsChecked = false;
                    return;
                }

                if (this.SpotifyToggleButton.IsChecked.GetValueOrDefault() && ChannelSession.Services.Spotify == null)
                {
                    await MessageBoxHelper.ShowMessageDialog("You must connect to your Spotify account in the Services area.");
                    this.EnableSongRequestsToggleButton.IsChecked = false;
                    return;
                }

                if (this.SpotifyToggleButton.IsChecked.GetValueOrDefault()) { ChannelSession.Settings.SongRequestServiceTypes.Add(SongRequestServiceTypeEnum.Spotify); }
                if (this.YouTubeToggleButton.IsChecked.GetValueOrDefault()) { ChannelSession.Settings.SongRequestServiceTypes.Add(SongRequestServiceTypeEnum.Youtube); }

                ChannelSession.Settings.SpotifyAllowExplicit = this.SpotifyAllowExplicitSongToggleButton.IsChecked.GetValueOrDefault();

                await ChannelSession.SaveSettings();

                if (await ChannelSession.Services.SongRequestService.Initialize())
                {
                    ChannelSession.SongRequestsEnabled = true;
                    this.SongRequestServicesGrid.IsEnabled = false;
                    this.CurrentlyPlayingAndSongQueueGrid.IsEnabled = true;

                    await this.RefreshRequestsList();
                }
                else
                {
                    await MessageBoxHelper.ShowMessageDialog("We were unable to initialize the Song Request service, please try again.");
                }
            });
        }

        private void EnableSongRequestsToggleButton_Unchecked(object sender, System.Windows.RoutedEventArgs e)
        {
            ChannelSession.Services.SongRequestService.Disable();

            ChannelSession.SongRequestsEnabled = false;

            this.SongRequestServicesGrid.IsEnabled = true;
            this.CurrentlyPlayingAndSongQueueGrid.IsEnabled = false;
        }

        private void SpotifyToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            this.SpotifyOptionsGrid.IsEnabled = this.SpotifyToggleButton.IsChecked.GetValueOrDefault();
        }

        private async void PlayPauseButton_Click(object sender, RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                if (ChannelSession.Services.SongRequestService != null)
                {
                    await ChannelSession.Services.SongRequestService.PlayPauseCurrentSong();
                }
            });
        }

        private async void NextSongButton_Click(object sender, RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                if (ChannelSession.Services.SongRequestService != null)
                {
                    await ChannelSession.Services.SongRequestService.SkipToNextSong();
                }
            });
        }

        private async void ClearQueueButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                if (await MessageBoxHelper.ShowConfirmationDialog("Are you sure you want to clear the Song Request queue?"))
                {
                    await ChannelSession.Services.SongRequestService.ClearAllRequests();
                    await this.RefreshRequestsList();
                }
            });
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                Button button = (Button)sender;
                SongRequestItem songRequest = (SongRequestItem)button.DataContext;
                await ChannelSession.Services.SongRequestService.RemoveSongRequest(songRequest);
                await this.RefreshRequestsList();
            });
        }

        private async void GlobalEvents_OnSongRequestsChangedOccurred(object sender, EventArgs e)
        {
            await this.Dispatcher.InvokeAsync(async () =>
            {
                await this.RefreshRequestsList();
            });
        }

        private async Task RefreshRequestsList()
        {
            await SongRequestControl.songListLock.WaitAsync();

            this.requests.Clear();
            foreach (SongRequestItem item in await ChannelSession.Services.SongRequestService.GetAllRequests())
            {
                this.requests.Add(item);
            }

            SongRequestControl.songListLock.Release();
        }
    }
}
