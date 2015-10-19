using System;
using System.ComponentModel;
using Tridion.ContentManager.CoreService.Client;
using TridionDesktopTools.Core.Annotations;

namespace TridionDesktopTools.Core
{
    public class ResultInfo : INotifyPropertyChanged
    {
        private string _TcmId;
        private string _Message;
        private ItemType _ItemType;
        private Status _Status;
        private string _StackTrace;

        public ResultInfo()
        {
            Status = Status.None;
        }

        public string TcmId
        {
            get { return _TcmId; }
            set
            {
                if (value == _TcmId) return;
                _TcmId = value;
                OnPropertyChanged("TcmId");
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

        public ItemType ItemType
        {
            get { return _ItemType; }
            set
            {
                if (value == _ItemType) return;
                _ItemType = value;
                OnPropertyChanged("Icon");
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

        public Uri Icon
        {
            get
            {
                if (this.ItemType == ItemType.Publication)
                    return new Uri("pack://application:,,,/TridionDesktopTools.Core;component/Resources/pub.png");
                if (this.ItemType == ItemType.Folder)
                    return new Uri("pack://application:,,,/TridionDesktopTools.Core;component/Resources/fld.png");
                if (this.ItemType == ItemType.StructureGroup)
                    return new Uri("pack://application:,,,/TridionDesktopTools.Core;component/Resources/sg.png");
                if (this.ItemType == ItemType.Schema)
                    return new Uri("pack://application:,,,/TridionDesktopTools.Core;component/Resources/schema.png");
                if (this.ItemType == ItemType.PageTemplate)
                    return new Uri("pack://application:,,,/TridionDesktopTools.Core;component/Resources/pt.png");
                if (this.ItemType == ItemType.ComponentTemplate)
                    return new Uri("pack://application:,,,/TridionDesktopTools.Core;component/Resources/ct.png");
                if (this.ItemType == ItemType.Component)
                    return new Uri("pack://application:,,,/TridionDesktopTools.Core;component/Resources/cmp.png");
                if (this.ItemType == ItemType.Page)
                    return new Uri("pack://application:,,,/TridionDesktopTools.Core;component/Resources/page.png");

                return new Uri("pack://application:,,,/TridionDesktopTools.Core;component/Resources/tbb.png");
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
                if (this.Status == Status.Error)
                    return new Uri("pack://application:,,,/TridionDesktopTools.Core;component/Resources/error.png");

                return new Uri("pack://application:,,,/TridionDesktopTools.Core;component/Resources/info.png");
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