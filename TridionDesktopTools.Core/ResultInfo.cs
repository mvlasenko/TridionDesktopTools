using System;
using System.ComponentModel;
using Tridion.ContentManager.CoreService.Client;
using TridionDesktopTools.Core.Annotations;

namespace TridionDesktopTools.Core
{
    public class ResultInfo : INotifyPropertyChanged
    {
        private ItemInfo _Item;
        private string _Message;
        private Status _Status;
        private string _StackTrace;

        public ResultInfo()
        {
            Status = Status.None;
        }

        public ItemInfo Item
        {
            get
            {
                return _Item;
            }
            set
            {
                _Item = value;
                OnPropertyChanged("Item");
                OnPropertyChanged("TcmId");
                OnPropertyChanged("ItemType");
                OnPropertyChanged("Title");
                OnPropertyChanged("Icon");
                OnPropertyChanged("Path");
                OnPropertyChanged("WebDav");
            }
        }

        public string Message
        {
            get { return _Message; }
            set
            {
                if (value == _Message) return;
                _Message = value;
                OnPropertyChanged("Message");
            }
        }

        public Status Status
        {
            get { return _Status; }
            set
            {
                if (value == _Status) return;
                _Status = value;
                OnPropertyChanged("StatusIcon");
            }
        }

        public string StackTrace
        {
            get { return _StackTrace; }
            set
            {
                if (value == _StackTrace) return;
                _StackTrace = value;
                OnPropertyChanged("StackTrace");
            }
        }

        public string TcmId
        {
            get
            {
                if (this.Item == null)
                    return string.Empty;
                return this.Item.TcmId;
            }
        }

        public ItemType ItemType
        {
            get
            {
                if (this.Item == null)
                    return ItemType.None;
                return this.Item.ItemType;
            }
        }

        public string Title
        {
            get
            {
                if (this.Item == null)
                    return string.Empty;
                return this.Item.Title;
            }
        }

        public Uri Icon
        {
            get
            {
                if (this.Item == null)
                    return null;
                return this.Item.Icon;
            }
        }

        public Uri StatusIcon
        {
            get
            {
                if (this.Status == Status.Success)
                    return new Uri("pack://application:,,,/TridionDesktopTools.Core;component/Resources/success.png");
                if (this.Status == Status.Warning)
                    return new Uri("pack://application:,,,/TridionDesktopTools.Core;component/Resources/warning.png");
                if (this.Status == Status.Delete)
                    return new Uri("pack://application:,,,/TridionDesktopTools.Core;component/Resources/delete.png");
                if (this.Status == Status.Error)
                    return new Uri("pack://application:,,,/TridionDesktopTools.Core;component/Resources/error.png");

                return new Uri("pack://application:,,,/TridionDesktopTools.Core;component/Resources/info.png");
            }
        }

        public string Path
        {
            get
            {
                if (this.Item == null)
                    return string.Empty;
                return this.Item.Path;
            }
        }

        public string WebDav
        {
            get
            {
                if (this.Item == null)
                    return this.TcmId;
                return this.Item.WebDav;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}