﻿using MixItUp.Base;
using MixItUp.Base.ViewModel.Controls.MainControls;
using MixItUp.Base.ViewModel.Window;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace MixItUp.WPF.Controls.MainControls
{
    /// <summary>
    /// Interaction logic for ChatControl.xaml
    /// </summary>
    public partial class ChatControl : MainControlBase
    {
        public static BitmapImage SubscriberBadgeBitmap { get; private set; }

        private ScrollViewer chatListScrollViewer;

        private ChatMainControlViewModel viewModel;

        public ChatControl()
        {
            InitializeComponent();
        }

        protected override async Task InitializeInternal()
        {
            this.viewModel = new ChatMainControlViewModel((MainWindowViewModel)this.Window.ViewModel);
            this.viewModel.ScrollingLockChanged += ViewModel_ScrollingLockChanged;
            await this.viewModel.OnLoaded();
            this.DataContext = this.viewModel;
        }

        private void ViewModel_ScrollingLockChanged(object sender, System.EventArgs e)
        {
            this.ChatLockButtonIcon.Kind = (this.viewModel.IsScrollingLocked) ? MaterialDesignThemes.Wpf.PackIconKind.LockOutline : MaterialDesignThemes.Wpf.PackIconKind.LockOpenOutline;
            if (this.chatListScrollViewer != null)
            {
                this.chatListScrollViewer.VerticalScrollBarVisibility = (this.viewModel.IsScrollingLocked) ? ScrollBarVisibility.Hidden : ScrollBarVisibility.Visible;
                if (this.viewModel.IsScrollingLocked)
                {
                    if (ChannelSession.Settings.LatestChatAtTop)
                    {
                        this.chatListScrollViewer.ScrollToTop();
                    }
                    else
                    {
                        this.chatListScrollViewer.ScrollToBottom();
                    }
                }
            }
        }

        private void SendChatMessageButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private void ChatList_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (this.chatListScrollViewer == null)
            {
                this.chatListScrollViewer = (ScrollViewer)e.OriginalSource;
            }

            if (this.viewModel.IsScrollingLocked && this.chatListScrollViewer != null)
            {
                if (ChannelSession.Settings.LatestChatAtTop)
                {
                    this.chatListScrollViewer.ScrollToTop();
                }
                else
                {
                    this.chatListScrollViewer.ScrollToBottom();
                }
            }
        }

        private void ChatLockButton_MouseEnter(object sender, MouseEventArgs e) { this.ChatLockButton.Opacity = 1; }

        private void ChatLockButton_MouseLeave(object sender, MouseEventArgs e) { this.ChatLockButton.Opacity = 0.3; }

        private void ChatLockButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private void ChatMessageTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
        }

        private void ChatMessageTextBox_KeyDown(object sender, KeyEventArgs e)
        {
        }

        private void ChatMessageTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
        }

        private void UsernameIntellisenseListBox_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
        }

        private void EmoticonIntellisenseListBox_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
        }

        private void UserList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
        }

        private void MessageCopyMenuItem_Click(object sender, RoutedEventArgs e)
        {
        }

        private void MessageDeleteMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
        }

        private void MessageWhisperUserMenuItem_Click(object sender, RoutedEventArgs e)
        {
        }

        private void UserInformationMenuItem_Click(object sender, RoutedEventArgs e)
        {
        }
    }
}
