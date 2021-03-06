﻿using MixItUp.Base.ViewModels;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Window.Dashboard
{
    public enum DashboardLayoutTypeEnum
    {
        One,
        Two,
        [Name("Three Right")]
        ThreeRight,
        [Name("Three Left")]
        ThreeLeft,
        Four
    }

    public enum DashboardItemTypeEnum
    {
        None,
        Chat,
        Alerts,
        Statistics,
        [Name("Game Queue")]
        GameQueue,
        [Name("Song Requests")]
        SongRequests,
        [Name("Quick Commands")]
        QuickCommands
    }

    public class LayoutItemsViewModel : UIViewModelBase
    {
        public object ItemOne
        {
            get { return this.itemOne; }
            set
            {
                this.itemOne = value;
                this.NotifyPropertyChanged();
            }
        }
        private object itemOne;

        public object ItemTwo
        {
            get { return this.itemTwo; }
            set
            {
                this.itemTwo = value;
                this.NotifyPropertyChanged();
            }
        }
        private object itemTwo;

        public object ItemThree
        {
            get { return this.itemThree; }
            set
            {
                this.itemThree = value;
                this.NotifyPropertyChanged();
            }
        }
        private object itemThree;

        public object ItemFour
        {
            get { return this.itemFour; }
            set
            {
                this.itemFour = value;
                this.NotifyPropertyChanged();
            }
        }
        private object itemFour;

        public void ClearItems()
        {
            this.ItemOne = null;
            this.ItemTwo = null;
            this.ItemThree = null;
            this.ItemFour = null;
        }

        public void NotifyItemPropertiesChanged()
        {
            this.NotifyPropertyChanged("ItemOne");
            this.NotifyPropertyChanged("ItemTwo");
            this.NotifyPropertyChanged("ItemThree");
            this.NotifyPropertyChanged("ItemFour");
        }
    }

    public class DashboardWindowViewModel : WindowViewModelBase
    {
        public DashboardWindowViewModel() { }

        public object ChatControl { get; set; }
        public object AlertsControl { get; set; }
        public object StatisticsControl { get; set; }
        public object GameQueueControl { get; set; }
        public object SongRequestsControl { get; set; }
        public object QuickCommandsControl { get; set; }

        public IEnumerable<string> LayoutTypes { get { return EnumHelper.GetEnumNames<DashboardLayoutTypeEnum>(); } }

        public string LayoutTypeString
        {
            get { return EnumHelper.GetEnumName(this.LayoutType); }
            set
            {
                this.LayoutType = EnumHelper.GetEnumValueFromString<DashboardLayoutTypeEnum>(value);

                this.NotifyPropertyChanged();
            }
        }
        public DashboardLayoutTypeEnum LayoutType
        {
            get { return ChannelSession.Settings.DashboardLayout; }
            set
            {
                ChannelSession.Settings.DashboardLayout = value;

                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("IsLayoutOne");
                this.NotifyPropertyChanged("IsLayoutTwo");
                this.NotifyPropertyChanged("IsLayoutThreeRight");
                this.NotifyPropertyChanged("IsLayoutThreeLeft");
                this.NotifyPropertyChanged("IsLayoutFour");

                this.NotifyItemPropertiesChanged();
            }
        }

        public bool IsLayoutOne { get { return this.LayoutType == DashboardLayoutTypeEnum.One; } }
        public bool IsLayoutTwo { get { return this.LayoutType == DashboardLayoutTypeEnum.Two; } }
        public bool IsLayoutThreeRight { get { return this.LayoutType == DashboardLayoutTypeEnum.ThreeRight; } }
        public bool IsLayoutThreeLeft { get { return this.LayoutType == DashboardLayoutTypeEnum.ThreeLeft; } }
        public bool IsLayoutFour { get { return this.LayoutType == DashboardLayoutTypeEnum.Four; } }

        public string ItemTypeOneString
        {
            get { return EnumHelper.GetEnumName(this.ItemTypeOne); }
            set
            {
                this.ItemTypeOne = EnumHelper.GetEnumValueFromString<DashboardItemTypeEnum>(value);
                this.NotifyPropertyChanged();
            }
        }
        public DashboardItemTypeEnum ItemTypeOne
        {
            get { return ChannelSession.Settings.DashboardItems[0]; }
            set
            {
                this.AssignType(value, 0);
                this.NotifyPropertyChanged();
            }
        }

        public string ItemTypeTwoString
        {
            get { return EnumHelper.GetEnumName(this.ItemTypeTwo); }
            set
            {
                this.ItemTypeTwo = EnumHelper.GetEnumValueFromString<DashboardItemTypeEnum>(value);
                this.NotifyPropertyChanged();
            }
        }
        public DashboardItemTypeEnum ItemTypeTwo
        {
            get { return ChannelSession.Settings.DashboardItems[1]; }
            set
            {
                this.AssignType(value, 1);
                this.NotifyPropertyChanged();
            }
        }

        public string ItemTypeThreeString
        {
            get { return EnumHelper.GetEnumName(this.ItemTypeThree); }
            set
            {
                this.ItemTypeThree = EnumHelper.GetEnumValueFromString<DashboardItemTypeEnum>(value);
                this.NotifyPropertyChanged();
            }
        }
        public DashboardItemTypeEnum ItemTypeThree
        {
            get { return ChannelSession.Settings.DashboardItems[2]; }
            set
            {
                this.AssignType(value, 2);
                this.NotifyPropertyChanged();
            }
        }

        public string ItemTypeFourString
        {
            get { return EnumHelper.GetEnumName(this.ItemTypeFour); }
            set
            {
                this.ItemTypeFour = EnumHelper.GetEnumValueFromString<DashboardItemTypeEnum>(value);
                this.NotifyPropertyChanged();
            }
        }
        public DashboardItemTypeEnum ItemTypeFour
        {
            get { return ChannelSession.Settings.DashboardItems[3]; }
            set
            {
                this.AssignType(value, 3);
                this.NotifyPropertyChanged();
            }
        }

        public LayoutItemsViewModel LayoutOne { get; set; } = new LayoutItemsViewModel();
        public LayoutItemsViewModel LayoutTwo { get; set; } = new LayoutItemsViewModel();
        public LayoutItemsViewModel LayoutThreeRight { get; set; } = new LayoutItemsViewModel();
        public LayoutItemsViewModel LayoutThreeLeft { get; set; } = new LayoutItemsViewModel();
        public LayoutItemsViewModel LayoutFour { get; set; } = new LayoutItemsViewModel();

        public IEnumerable<string> ItemTypes { get { return EnumHelper.GetEnumNames<DashboardItemTypeEnum>(); } }

        protected override async Task OnLoadedInternal()
        {
            this.NotifyItemPropertiesChanged();

            await base.OnLoadedInternal();
        }

        protected override async Task OnVisibleInternal()
        {
            this.NotifyItemPropertiesChanged();

            await base.OnVisibleInternal();
        }

        private void NotifyItemPropertiesChanged()
        {
            this.NotifyPropertyChanged("ItemTypeOne");
            this.NotifyPropertyChanged("ItemTypeOneString");
            this.NotifyPropertyChanged("ItemTypeTwo");
            this.NotifyPropertyChanged("ItemTypeTwoString");
            this.NotifyPropertyChanged("ItemTypeThree");
            this.NotifyPropertyChanged("ItemTypeThreeString");
            this.NotifyPropertyChanged("ItemTypeFour");
            this.NotifyPropertyChanged("ItemTypeFourString");

            this.LayoutOne.ClearItems();
            this.LayoutTwo.ClearItems();
            this.LayoutThreeLeft.ClearItems();
            this.LayoutThreeRight.ClearItems();
            this.LayoutFour.ClearItems();

            if (this.LayoutType == DashboardLayoutTypeEnum.One)
            {
                this.AssignItemsToLayout(this.LayoutOne);
            }
            else if (this.LayoutType == DashboardLayoutTypeEnum.Two)
            {
                this.AssignItemsToLayout(this.LayoutTwo);
            }
            else if (this.LayoutType == DashboardLayoutTypeEnum.ThreeLeft)
            {
                this.AssignItemsToLayout(this.LayoutThreeLeft);
            }
            else if (this.LayoutType == DashboardLayoutTypeEnum.ThreeRight)
            {
                this.AssignItemsToLayout(this.LayoutThreeRight);
            }
            else if (this.LayoutType == DashboardLayoutTypeEnum.Four)
            {
                this.AssignItemsToLayout(this.LayoutFour);
            }

            this.LayoutOne.NotifyItemPropertiesChanged();
            this.LayoutTwo.NotifyItemPropertiesChanged();
            this.LayoutThreeLeft.NotifyItemPropertiesChanged();
            this.LayoutThreeRight.NotifyItemPropertiesChanged();
            this.LayoutFour.NotifyItemPropertiesChanged();
        }

        private void AssignType(DashboardItemTypeEnum type, int index)
        {
            for (int i = 0; i < ChannelSession.Settings.DashboardItems.Count; i++)
            {
                if (ChannelSession.Settings.DashboardItems[i] == type)
                {
                    ChannelSession.Settings.DashboardItems[i] = DashboardItemTypeEnum.None;
                }
            }
            ChannelSession.Settings.DashboardItems[index] = type;

            this.NotifyItemPropertiesChanged();
        }

        private void AssignItemsToLayout(LayoutItemsViewModel layout)
        {
            layout.ItemOne = this.GetItemForType(this.ItemTypeOne);
            layout.ItemTwo = this.GetItemForType(this.ItemTypeTwo);
            layout.ItemThree = this.GetItemForType(this.ItemTypeThree);
            layout.ItemFour = this.GetItemForType(this.ItemTypeFour);
        }

        private object GetItemForType(DashboardItemTypeEnum type)
        {
            switch (type)
            {
                case DashboardItemTypeEnum.Chat: return this.ChatControl;
                case DashboardItemTypeEnum.Alerts: return this.AlertsControl;
                case DashboardItemTypeEnum.Statistics: return this.StatisticsControl;
                case DashboardItemTypeEnum.GameQueue: return this.GameQueueControl;
                case DashboardItemTypeEnum.SongRequests: return this.SongRequestsControl;
                case DashboardItemTypeEnum.QuickCommands: return this.QuickCommandsControl;
            }
            return null;
        }
    }
}
