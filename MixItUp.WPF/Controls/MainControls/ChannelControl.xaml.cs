﻿using Mixer.Base.Model.Channel;
using Mixer.Base.Model.Game;
using Mixer.Base.Model.Teams;
using Mixer.Base.Model.User;
using MixItUp.Base;
using MixItUp.Base.Model.Favorites;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Favorites;
using MixItUp.Base.ViewModel.User;
using MixItUp.WPF.Util;
using MixItUp.WPF.Windows.Favorites;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MixItUp.WPF.Controls.MainControls
{
    public enum RaidSearchCriteriaEnum
    {
        [Name("Same Game")]
        SameGame,
        [Name("Same Team")]
        SameTeam,
        [Name("Same Age Rating")]
        AgeRating,
        [Name("Small Streamer (< 10)")]
        SmallStreamer,
        [Name("Medium Streamer (10-25)")]
        MediumStreamer,
        [Name("Large Streamer (> 25)")]
        LargeStreamer,
        [Name("Partnered Streamer")]
        PartneredStreamer,
        [Name("Random Streamer")]
        Random,
    }

    /// <summary>
    /// Interaction logic for ChannelControl.xaml
    /// </summary>
    public partial class ChannelControl : MainControlBase
    {
        private ObservableCollection<FavoriteGroupViewModel> favoritedGroups = new ObservableCollection<FavoriteGroupViewModel>();

        private ChannelModel channelToRaid;
        private bool shouldShowIntellisense = false;

        public ChannelControl()
        {
            InitializeComponent();
        }

        protected override Task InitializeInternal()
        {
            this.AgeRatingComboBox.ItemsSource = EnumHelper.GetEnumNames<AgeRatingEnum>();

            this.StreamTitleComboBox.Text = ChannelSession.MixerChannel.name;
            List<string> streamTitles = new List<string>(ChannelSession.Settings.RecentStreamTitles);
            this.StreamTitleComboBox.ItemsSource = streamTitles;

            this.shouldShowIntellisense = false;
            if (ChannelSession.MixerChannel?.type?.name != null)
            {
                this.GameNameTextBox.Text = ChannelSession.MixerChannel.type.name;
            }
            else
            {
                this.GameNameTextBox.Text = "Web Show";
            }
            this.shouldShowIntellisense = true;

            List<string> ageRatingList = EnumHelper.GetEnumNames<AgeRatingEnum>().Select(s => s.ToLower()).ToList();
            this.AgeRatingComboBox.SelectedIndex = ageRatingList.IndexOf(ChannelSession.MixerChannel.audience);

            this.ChannelToRaidSearchCriteriaComboBox.ItemsSource = EnumHelper.GetEnumNames<RaidSearchCriteriaEnum>();

            this.AddFavoriteTypeComboBox.ItemsSource = new List<string>() { "Team", "User" };
            this.AddFavoriteUserGroupNameTextBox.Text = "Favorite Users";

            this.FavoriteGroupsDataGrid.ItemsSource = this.favoritedGroups;

            return base.InitializeInternal();
        }

        protected override async Task OnVisibilityChanged()
        {
            await this.InitializeInternal();

            await this.RefreshFavoriteGroups();
        }

        private async void GameNameTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            HideIntellisense();
            if (shouldShowIntellisense)
            {
                if (!string.IsNullOrEmpty(this.GameNameTextBox.Text))
                {
                    var games = (await ChannelSession.MixerStreamerConnection.GetGameTypes(this.GameNameTextBox.Text, 5))
                        .Take(5)
                        .ToList();
                    if (games.Count > 0)
                    {
                        GameNameIntellisenseListBox.ItemsSource = games;

                        // Select the first game
                        GameNameIntellisenseListBox.SelectedIndex = 0;

                        Rect positionOfCarat = this.GameNameTextBox.GetRectFromCharacterIndex(this.GameNameTextBox.CaretIndex, true);
                        if (!positionOfCarat.IsEmpty)
                        {
                            Point topLeftOffset = this.GameNameTextBox.TransformToAncestor(MainGrid).Transform(new Point(positionOfCarat.Left, positionOfCarat.Top));
                            ShowIntellisense(topLeftOffset.X, topLeftOffset.Y);
                        }
                    }
                }
            }
        }

        private void ShowIntellisense(double x, double y)
        {
            this.GameNameIntellisenseContent.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Canvas.SetLeft(this.GameNameIntellisense, x + 10);
            Canvas.SetTop(this.GameNameIntellisense, y - 10);
            this.GameNameIntellisense.UpdateLayout();

            if (!this.GameNameIntellisense.IsPopupOpen)
            {
                this.GameNameIntellisense.IsPopupOpen = true;
            }
        }

        private void GameNameTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (this.GameNameIntellisense.IsPopupOpen)
            {
                switch (e.Key)
                {
                    case Key.Escape:
                        HideIntellisense();
                        e.Handled = true;
                        break;
                    case Key.Tab:
                    case Key.Enter:
                        SelectIntellisenseGame();
                        e.Handled = true;
                        break;
                    case Key.Up:
                        GameNameIntellisenseListBox.SelectedIndex = MathHelper.Clamp(GameNameIntellisenseListBox.SelectedIndex - 1, 0, GameNameIntellisenseListBox.Items.Count - 1);
                        e.Handled = true;
                        break;
                    case Key.Down:
                        GameNameIntellisenseListBox.SelectedIndex = MathHelper.Clamp(GameNameIntellisenseListBox.SelectedIndex + 1, 0, GameNameIntellisenseListBox.Items.Count - 1);
                        e.Handled = true;
                        break;
                }
            }
        }

        private void GameNameIntellisenseListBox_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            SelectIntellisenseGame();
        }

        private async Task<GameTypeModel> GetValidGameType(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                return (await ChannelSession.MixerStreamerConnection.GetGameTypes(name, 15)).FirstOrDefault(g => g.name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            }

            return null;
        }

        private void SelectIntellisenseGame()
        {
            this.shouldShowIntellisense = false;
            GameTypeModel gameType = GameNameIntellisenseListBox.SelectedItem as GameTypeModel;
            if (gameType != null)
            {
                this.GameNameTextBox.Text = gameType.name;
                this.GameNameTextBox.CaretIndex = this.GameNameTextBox.Text.Length;
            }
            this.shouldShowIntellisense = true;

            HideIntellisense();
        }

        private void HideIntellisense()
        {
            if (this.GameNameIntellisense.IsPopupOpen)
            {
                this.GameNameIntellisense.IsPopupOpen = false;
            }
        }

        private async void UpdateChannelDataButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(this.StreamTitleComboBox.Text))
            {
                await DialogHelper.ShowMessage("A stream title must be specified");
                return;
            }

            GameTypeModel gameType = await GetValidGameType(this.GameNameTextBox.Text);
            if (gameType == null)
            {
                await DialogHelper.ShowMessage("A valid & existing game name must be selected");
                return;
            }

            if (this.AgeRatingComboBox.SelectedIndex < 0)
            {
                await DialogHelper.ShowMessage("A valid age rating must be selected");
                return;
            }

            await this.Window.RunAsyncOperation(async () =>
            {
                await ChannelSession.MixerStreamerConnection.UpdateChannel(ChannelSession.MixerChannel.id, this.StreamTitleComboBox.Text, gameType.id, ((string)this.AgeRatingComboBox.SelectedItem).ToLower());

                await ChannelSession.RefreshChannel();

                List<string> streamTitles = new List<string>(ChannelSession.Settings.RecentStreamTitles);
                this.StreamTitleComboBox.ItemsSource = streamTitles;
            });
        }

        private async void FindChannelToRaidButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                if (this.ChannelToRaidSearchCriteriaComboBox.SelectedIndex < 0)
                {
                    await DialogHelper.ShowMessage("Must select a search criteria for finding a channel to raid");
                    return;
                }

                await ChannelSession.RefreshChannel();

                IEnumerable<ChannelModel> channels = null;
                RaidSearchCriteriaEnum searchCriteria = EnumHelper.GetEnumValueFromString<RaidSearchCriteriaEnum>((string)this.ChannelToRaidSearchCriteriaComboBox.SelectedItem);
                if (searchCriteria == RaidSearchCriteriaEnum.SameGame)
                {
                    channels = await ChannelSession.MixerStreamerConnection.GetChannelsByGameTypes(ChannelSession.MixerChannel.type, 1);
                }
                else if (searchCriteria == RaidSearchCriteriaEnum.SameTeam)
                {
                    Dictionary<uint, UserWithChannelModel> teamChannels = new Dictionary<uint, UserWithChannelModel>();
                    foreach (TeamMembershipExpandedModel extendedTeam in await ChannelSession.MixerStreamerConnection.GetUserTeams(ChannelSession.MixerStreamerUser))
                    {
                        TeamModel team = await ChannelSession.MixerStreamerConnection.GetTeam(extendedTeam.id);
                        IEnumerable<UserWithChannelModel> teamUsers = await ChannelSession.MixerStreamerConnection.GetTeamUsers(team);
                        foreach (UserWithChannelModel userChannel in teamUsers.Where(u => u.channel.online))
                        {
                            teamChannels[userChannel.id] = userChannel;
                        }
                    }
                    channels = teamChannels.Values.Select(c => c.channel);
                }
                else
                {
                    string filters = string.Empty;
                    if (searchCriteria == RaidSearchCriteriaEnum.AgeRating)
                    {
                        filters = "audience:eq:" + ChannelSession.MixerChannel.audience;
                    }
                    else if (searchCriteria == RaidSearchCriteriaEnum.LargeStreamer)
                    {
                        filters = "viewersCurrent:gte:25";
                    }
                    else if (searchCriteria == RaidSearchCriteriaEnum.MediumStreamer)
                    {
                        filters = "viewersCurrent:gte:10,viewersCurrent:lt:25";
                    }
                    else if (searchCriteria == RaidSearchCriteriaEnum.SmallStreamer)
                    {
                        filters = "viewersCurrent:gt:0,viewersCurrent:lt:10";
                    }
                    else if (searchCriteria == RaidSearchCriteriaEnum.PartneredStreamer)
                    {
                        filters = "partnered:eq:true";
                    }
                    channels = await ChannelSession.MixerStreamerConnection.Connection.Channels.GetChannels(filters, 50);
                }

                this.ChannelRaidNameTextBox.Clear();
                if (channels != null && channels.Count() > 0)
                {
                    this.channelToRaid = channels.ElementAt(RandomHelper.GenerateRandomNumber(channels.Count()));

                    UserModel user = await ChannelSession.MixerStreamerConnection.GetUser(this.channelToRaid.userId);
                    GameTypeModel game = await ChannelSession.MixerStreamerConnection.GetGameType(this.channelToRaid.typeId.GetValueOrDefault());

                    this.ChannelRaidNameTextBox.Text = user.username;
                    this.ChannelRaidViewersTextBox.Text = this.channelToRaid.viewersCurrent.ToString();
                    this.ChannelRaidAudienceTextBox.Text = EnumHelper.GetEnumName(EnumHelper.GetEnumValueFromString<AgeRatingEnum>(this.channelToRaid.audience));
                    this.ChannelRaidGameTextBox.Text = (game != null) ? game.name : "Unknown";
                }
                else
                {
                    await DialogHelper.ShowMessage("Unable to find a channel that met your search critera, please try selecting a different option");
                }
            });
        }

        private void AddFavoriteTypeComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            this.AddFavoriteTeamGrid.Visibility = System.Windows.Visibility.Collapsed;
            this.AddFavoriteUserGrid.Visibility = System.Windows.Visibility.Collapsed;
            if (this.AddFavoriteTypeComboBox.SelectedIndex >= 0)
            {
                string selection = (string)this.AddFavoriteTypeComboBox.SelectedItem;
                if (selection.Equals("Team"))
                {
                    this.AddFavoriteTeamGrid.Visibility = System.Windows.Visibility.Visible;
                }
                else if (selection.Equals("User"))
                {
                    this.AddFavoriteUserGrid.Visibility = System.Windows.Visibility.Visible;
                }
            }
        }

        private async void AddFavoriteButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                if (this.AddFavoriteTypeComboBox.SelectedIndex >= 0)
                {
                    string selection = (string)this.AddFavoriteTypeComboBox.SelectedItem;
                    if (selection.Equals("Team"))
                    {
                        if (!string.IsNullOrEmpty(this.AddFavoriteTeamTextBox.Text))
                        {
                            TeamModel team = await ChannelSession.MixerStreamerConnection.GetTeam(this.AddFavoriteTeamTextBox.Text);
                            if (team != null)
                            {
                                if (ChannelSession.Settings.FavoriteGroups.Any(t => t.Team != null && t.Team.id.Equals(team.id)))
                                {
                                    await DialogHelper.ShowMessage("You have already favorited this team.");
                                    return;
                                }

                                ChannelSession.Settings.FavoriteGroups.Add(new FavoriteGroupModel(team));
                                await ChannelSession.SaveSettings();

                                await this.RefreshFavoriteGroups();
                            }
                            else
                            {
                                await DialogHelper.ShowMessage("Could not find a team by that name.");
                            }
                        }
                    }
                    else if (selection.Equals("User"))
                    {
                        if (!string.IsNullOrEmpty(this.AddFavoriteUserTextBox.Text) && !string.IsNullOrEmpty(this.AddFavoriteUserGroupNameTextBox.Text))
                        {
                            UserModel user = await ChannelSession.MixerStreamerConnection.GetUser(this.AddFavoriteUserTextBox.Text);
                            if (user != null)
                            {
                                FavoriteGroupModel group = ChannelSession.Settings.FavoriteGroups.FirstOrDefault(t => t.GroupName != null && t.GroupName.Equals(this.AddFavoriteUserGroupNameTextBox.Text));
                                if (group != null)
                                {
                                    if (group.GroupUserIDs.Any(id => id.Equals(user.id)))
                                    {
                                        await DialogHelper.ShowMessage("This user already exists in this group.");
                                        return;
                                    }
                                }

                                if (group == null)
                                {
                                    group = new FavoriteGroupModel(this.AddFavoriteUserGroupNameTextBox.Text);
                                    ChannelSession.Settings.FavoriteGroups.Add(group);
                                }
                                group.GroupUserIDs.Add(user.id);
                                await ChannelSession.SaveSettings();

                                await this.RefreshFavoriteGroups();
                            }
                            else
                            {
                                await DialogHelper.ShowMessage("Could not find a user by that name.");
                            }
                        }
                    }
                }
            });
        }

        private void ViewFavoriteGroupButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Button button = (Button)sender;
            FavoriteGroupViewModel group = (FavoriteGroupViewModel)button.DataContext;
            FavoriteGroupWindow window = new FavoriteGroupWindow(group);
            window.Closed += Window_Closed;
            window.Show();
        }

        private async void DeleteFavoriteGroupButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Button button = (Button)sender;
            FavoriteGroupViewModel group = (FavoriteGroupViewModel)button.DataContext;
            await this.Window.RunAsyncOperation(async () =>
            {
                if (await DialogHelper.ShowConfirmation("Are you sure you wish to remove this group?"))
                {
                    ChannelSession.Settings.FavoriteGroups.Remove(group.Group);
                    await ChannelSession.SaveSettings();

                    await this.RefreshFavoriteGroups();
                }
            });
        }

        private async void Window_Closed(object sender, EventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                await this.RefreshFavoriteGroups();
            });
        }

        private async Task RefreshFavoriteGroups()
        {
            this.AddFavoriteTeamTextBox.Clear();
            this.AddFavoriteUserTextBox.Clear();

            this.favoritedGroups.Clear();
            foreach (FavoriteGroupModel group in ChannelSession.Settings.FavoriteGroups)
            {
                FavoriteGroupViewModel groupViewModel = new FavoriteGroupViewModel(group);
                await groupViewModel.RefreshGroup();
                this.favoritedGroups.Add(groupViewModel);
            }
        }

        private async void HostChannelButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.channelToRaid != null)
            {
                await this.Window.RunAsyncOperation(async () =>
                {
                    await ChannelSession.MixerStreamerConnection.SetHostChannel(ChannelSession.MixerChannel, this.channelToRaid);
                });
            }
        }
    }
}
