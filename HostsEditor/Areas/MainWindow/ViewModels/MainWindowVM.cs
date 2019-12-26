using HostsEditor.Areas.MainWindow.Models;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;

namespace HostsEditorA.Areas.ViewModels.MainWindow
{
    public class MainWindowVM : BindableBase
    {
        private static string UrlPattern = @"http[s]?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}";
        //private static string HostUrlPattern = @"0.0.0.0([]{1,10})? ([]{1,10})? http[s]?:\/\/(www\.)?[-a - zA - Z0 - 9@:%._\+~#=]{1,256}";
        private static string HostsPath = @"C:\Windows\System32\drivers\etc\hosts";

        #region Properties

        private ObservableCollection<UrlListItem> hostsList;
        public ObservableCollection<UrlListItem> HostsList { get => hostsList; set => SetProperty(ref hostsList, value); }

        private ObservableCollection<UrlListItem> importList;
        public ObservableCollection<UrlListItem> ImportList { get => importList; set => SetProperty(ref importList, value); }

        private List<UrlListItem> HostsRemovedList = new List<UrlListItem>();

        private bool isSelectAllImport = true;
        public bool IsSelectAllImport
        {
            get => isSelectAllImport;
            set
            {
                SetProperty(ref isSelectAllImport, value);
            }
        }

        private bool isSelectAllHosts;
        public bool IsSelectAllHosts
        {
            get => isSelectAllHosts;
            set
            {
                SetProperty(ref isSelectAllHosts, value);
            }
        }

        #endregion

        #region Commands

        public DelegateCommand MoveToImportCmd { get; set; }
        public DelegateCommand DeleteFromHostsCmd { get; set; }
        public DelegateCommand MoveToHostsCmd { get; set; }
        public DelegateCommand DeleteFromImportCmd { get; set; }
        public DelegateCommand SaveCmd { get; set; }
        public DelegateCommand AbortCmd { get; set; }
        public DelegateCommand SelectAllHostsCmd { get; set; }
        public DelegateCommand SelectAllImportCmd { get; set; }

        #endregion
        public MainWindowVM()
        {
            HostsList = new ObservableCollection<UrlListItem>(ReadUrlsFromFile(true));
            ImportList = new ObservableCollection<UrlListItem>(ReadUrlsFromFile(false));
            RemoveDuplicationsFromImportBasedOnHost(ImportList, HostsList);

            MoveToHostsCmd = new DelegateCommand(MoveToHosts);
            MoveToImportCmd = new DelegateCommand(MoveToImport);
            DeleteFromHostsCmd = new DelegateCommand(DeleteFromHost);
            DeleteFromImportCmd = new DelegateCommand(DeleteFromImport);
            SaveCmd = new DelegateCommand(Save);
            AbortCmd = new DelegateCommand(Abort);
            SelectAllImportCmd = new DelegateCommand(() => SelectAll(IsSelectAllImport, ImportList));
            SelectAllHostsCmd = new DelegateCommand(() => SelectAll(IsSelectAllHosts, HostsList));
        }

        private void RemoveDuplicationsFromImportBasedOnHost(ObservableCollection<UrlListItem> importList, ObservableCollection<UrlListItem> hostsList)
        {
            for (int i = importList.Count - 1; i >= 0; i--)
            {
                if (HostsList.Any(x => x.Url.Equals(ImportList[i].Url)))
                {
                    importList.RemoveAt(i);
                }
            }
        }

        private void SelectAll(bool isSelected, ObservableCollection<UrlListItem> urlListItems)
        {
            if (urlListItems != null)
            {
                for (int i = 0; i < urlListItems.Count; i++)
                {
                    urlListItems[i].IsSelected = isSelected;
                }
            }
        }

        private void MoveToHosts()
        {
            bool refreshSelectAllHosts = false;

            for (int i = ImportList.Count - 1; i >= 0; i--)
            {
                if (ImportList[i].IsSelected)
                {
                    ImportList[i].IsSelected = false;
                    HostsList.Add(ImportList[i]);
                    ImportList.RemoveAt(i);
                    refreshSelectAllHosts = true;
                }
            }

            if (refreshSelectAllHosts)
            {
                IsSelectAllHosts = false;
            }
        }

        private void MoveToImport()
        {
            bool refreshSelectAllImport = false;
            for (int i = HostsList.Count - 1; i >= 0; i--)
            {
                if (HostsList[i].IsSelected)
                {
                    HostsList[i].IsSelected = false;
                    ImportList.Add(HostsList[i]);
                    HostsList.RemoveAt(i);
                    refreshSelectAllImport = true;
                }
            }

            if (refreshSelectAllImport)
            {
                IsSelectAllImport = false;
            }
        }

        private void DeleteFromHost()
        {
            DeleteFromList(HostsList, true);
        }

        private void DeleteFromImport()
        {
            DeleteFromList(ImportList, false);
        }

        private void DeleteFromList(ObservableCollection<UrlListItem> list, bool isHostList)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (list[i].IsSelected)
                {
                    if (isHostList)
                    {
                        HostsRemovedList.Add(list[i]);
                    }

                    list.RemoveAt(i);
                }
            }
        }

        private void Save()
        {
            StringBuilder fileContent = new StringBuilder();

            using (var sr = new StreamReader(HostsPath))
            {
                string line;

                while ((line = sr.ReadLine()) != null)
                {
                    if (!HostsRemovedList.Any(x => line.Contains(x.Url)) && !ImportList.Any(x => line.Contains(x.Url)))
                    {
                        fileContent.AppendLine(line);
                    }
                }
                sr.Close();
            }

            using (var sw = new StreamWriter(HostsPath))
            {
                sw.Write(fileContent);
                sw.Close();
            }

            using (StreamWriter streamWriter = File.AppendText(HostsPath))
            {
                for (int i = 0; i < HostsList.Count; i++)
                {
                    if (!HostsList[i].IsFromHost)
                    {
                        streamWriter.WriteLine(string.Format("{0} {1}", "0.0.0.0", HostsList[i].Url));
                    }
                }
                streamWriter.Close();
            }
            MessageBox.Show("Plik host został zmodyfikowany pomyślnie.");
        }
        private void Abort()
        {
            HostsList = new ObservableCollection<UrlListItem>(ReadUrlsFromFile(true));
            ImportList = new ObservableCollection<UrlListItem>(ReadUrlsFromFile(false));
        }

        /// <summary>
        /// Read all url's from file. Set filePath to read url's from specified file.
        /// </summary>
        /// <param name="hostPath"></param>
        /// <returns></returns>
        private List<UrlListItem> ReadUrlsFromFile(bool readFromHosts)
        {
            string filePath = string.Empty;
            if (!readFromHosts)
            {
                filePath = GetFilePath();
                if (string.IsNullOrEmpty(filePath))
                {
                    return new List<UrlListItem>();
                }
            }
            else
            {
                filePath = HostsPath;
                HostsRemovedList = new List<UrlListItem>();
            }

            string fileContent = ReadFileContent(filePath);
            if (string.IsNullOrEmpty(fileContent))
            {
                return new List<UrlListItem>();
            }

            List<UrlListItem> listUrls = GetUrls(fileContent, readFromHosts);
            return listUrls;
        }

        private string GetFilePath()
        {
            string filePath = string.Empty;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                filePath = openFileDialog.FileName;
            }
            return filePath;
        }

        private string ReadFileContent(string filePath)
        {
            string fileContent = string.Empty;
            using (StreamReader fileStream = new StreamReader(filePath))
            {
                fileContent = fileStream.ReadToEnd();
            }
            return fileContent;
        }

        private List<UrlListItem> GetUrls(string text, bool isFromHosts)
        {
            bool isSelected = (isFromHosts) ? false : IsSelectAllImport;
            if (string.IsNullOrEmpty(text))
            {
                return new List<UrlListItem>();
            }

            MatchCollection matches = Regex.Matches(text, UrlPattern, RegexOptions.IgnoreCase);
            List<UrlListItem> listUrl = matches.Distinct().Select(x => x.Value).Distinct().Select(x => new UrlListItem(x, isSelected, isFromHosts)).ToList();

            return listUrl;
        }
    }
}
