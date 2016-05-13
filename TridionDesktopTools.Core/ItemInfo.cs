using System;
using System.Collections.Generic;
using System.ComponentModel;
using Tridion.ContentManager.CoreService.Client;
using TridionDesktopTools.Core.Annotations;

namespace TridionDesktopTools.Core
{
    public class ItemInfo : INotifyPropertyChanged
    {
        private List<ItemInfo> _ChildItems;
        private string _TcmId;
        private string _Title;
        private string _Path;
        private bool _IsSelected;
        private bool _IsExpanded;

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

        public string Title
        {
            get { return _Title; }
            set
            {
                if (value == _Title) return;
                _Title = value;
                OnPropertyChanged("Title");
            }
        }
        
        public string Path
        {
            get { return _Path; }
            set
            {
                if (value == _Path) return;
                _Path = value;
                OnPropertyChanged("Path");
            }
        }

        public ItemType ItemType { get; set; }

        public SchemaType SchemaType { get; set; }

        public string MimeType { get; set; }

        public string FromPub { get; set; }

        public bool IsPublished { get; set; }

        public string WebDav { get; set; }

        public bool IsLocalized
        {
            get
            {
                return this.FromPub == "(Local copy)";
            }
        }
        
        public bool IsLocal
        {
            get
            {
                return String.IsNullOrEmpty(this.FromPub);
            }
        }

        public bool IsShared
        {
            get
            {
                return !String.IsNullOrEmpty(this.FromPub) && this.FromPub != "(Local copy)";
            }
        }

        public List<ItemInfo> ChildItems
        {
            get { return _ChildItems; }
            set
            {
                if (Equals(value, _ChildItems)) return;
                _ChildItems = value;
                OnPropertyChanged("ChildItems");
            }
        }

        public ItemInfo Parent { get; set; }

        public bool IsSelected
        {
            get { return _IsSelected; }
            set
            {
                if (value.Equals(_IsSelected)) return;
                _IsSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }

        public bool IsExpanded
        {
            get { return _IsExpanded; }
            set
            {
                if (value.Equals(_IsExpanded)) return;
                _IsExpanded = value;
                OnPropertyChanged("IsExpanded");
            }
        }

        public Uri Icon
        {
            get
            {
                if (this.ItemType == ItemType.Publication)
                    return new Uri("pack://application:,,,/TridionDesktopTools.Core;component/Resources/pub.png");

                if (this.Title == "Categories and Keywords" && this.TcmId.StartsWith("catman-"))
                    return new Uri("pack://application:,,,/TridionDesktopTools.Core;component/Resources/cat_kw.png");

                if (this.ItemType == ItemType.Folder)
                    return new Uri("pack://application:,,,/TridionDesktopTools.Core;component/Resources/fld.png");

                if (this.ItemType == ItemType.StructureGroup)
                    return new Uri("pack://application:,,,/TridionDesktopTools.Core;component/Resources/sg.png");

                if (this.ItemType == ItemType.Schema)
                {
                    if(this.SchemaType == SchemaType.Multimedia)
                        return new Uri("pack://application:,,,/TridionDesktopTools.Core;component/Resources/mschema.png");

                    return new Uri("pack://application:,,,/TridionDesktopTools.Core;component/Resources/schema.png");
                }

                if (this.ItemType == ItemType.PageTemplate)
                    return new Uri("pack://application:,,,/TridionDesktopTools.Core;component/Resources/pt.png");

                if (this.ItemType == ItemType.ComponentTemplate)
                    return new Uri("pack://application:,,,/TridionDesktopTools.Core;component/Resources/ct.png");

                if (this.ItemType == ItemType.Component)
                {
                    if (string.IsNullOrEmpty(this.MimeType))
                        return new Uri("pack://application:,,,/TridionDesktopTools.Core;component/Resources/cmp.png");

                    if (this.MimeType == "image/jpeg")
                        return new Uri("pack://application:,,,/TridionDesktopTools.Core;component/Resources/jpg.png");
                    if (this.MimeType == "image/png")
                        return new Uri("pack://application:,,,/TridionDesktopTools.Core;component/Resources/png.png");
                    if (this.MimeType == "image/gif")
                        return new Uri("pack://application:,,,/TridionDesktopTools.Core;component/Resources/gif.png");
                    if (this.MimeType.StartsWith("audio/"))
                        return new Uri("pack://application:,,,/TridionDesktopTools.Core;component/Resources/mp3.png");
                    if (this.MimeType.StartsWith("video/"))
                        return new Uri("pack://application:,,,/TridionDesktopTools.Core;component/Resources/mov.png");
                    if (this.MimeType == "application/msword")
                        return new Uri("pack://application:,,,/TridionDesktopTools.Core;component/Resources/doc.png");
                    if (this.MimeType == "application/vnd.ms-excel")
                        return new Uri("pack://application:,,,/TridionDesktopTools.Core;component/Resources/xls.png");
                    if (this.MimeType == "application/vnd.ms-powerpoint")
                        return new Uri("pack://application:,,,/TridionDesktopTools.Core;component/Resources/ppt.png");
                    if (this.MimeType == "application/pdf")
                        return new Uri("pack://application:,,,/TridionDesktopTools.Core;component/Resources/pdf.png");

                    return new Uri("pack://application:,,,/TridionDesktopTools.Core;component/Resources/mm.png");
                }

                if (this.ItemType == ItemType.Page)
                    return new Uri("pack://application:,,,/TridionDesktopTools.Core;component/Resources/page.png");

                if (this.ItemType == ItemType.Keyword)
                    return new Uri("pack://application:,,,/TridionDesktopTools.Core;component/Resources/kw.png");

                if (this.ItemType == ItemType.Category)
                    return new Uri("pack://application:,,,/TridionDesktopTools.Core;component/Resources/cat.png");

                if (this.ItemType == ItemType.ProcessDefinition)
                    return new Uri("pack://application:,,,/TridionDesktopTools.Core;component/Resources/proc.png");

                if (this.ItemType == ItemType.VirtualFolder)
                    return new Uri("pack://application:,,,/TridionDesktopTools.Core;component/Resources/bundle.png");

                return new Uri("pack://application:,,,/TridionDesktopTools.Core;component/Resources/tbb.png");
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