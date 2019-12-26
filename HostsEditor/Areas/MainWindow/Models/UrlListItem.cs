using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Text;

namespace HostsEditor.Areas.MainWindow.Models
{
    public class UrlListItem : BindableBase
    {
        private bool isSelected=false;
        private string url;
        private bool isFromHost;

        public UrlListItem(string url)
        {
            Url = url;
            IsSelected = false;
            IsFromHost = false;
        }

        public UrlListItem(string url,bool isSelected, bool isFromHost)
        {
            Url = url;
            IsSelected = isSelected;
            IsFromHost = isFromHost;
        }

        public string Url { get => url; set =>SetProperty(ref url,value); }
        public bool IsSelected { get => isSelected; set => SetProperty(ref isSelected, value); }
        public bool IsFromHost { get => isFromHost; private set => SetProperty(ref isFromHost, value); }
    }
}
