using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.ServiceProcess;

namespace Plex_Database_Editor.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private string databasePath;
        private string search;
        private string backupPath;
        private List<Movie> movies;
        private string message;
        private bool sortRecentFirst;
        private string serviceName;
        private string hostIP;
        private string hostState;
        private string password;
        private string username;
        private string domain;

        public RelayCommand CommandSearch => new RelayCommand(SearchMovies);
        public RelayCommand CommandSave => new RelayCommand(Save);
        public RelayCommand CommandClear => new RelayCommand(Clear);
        public RelayCommand CommandBrowseDBPath => new RelayCommand(BrowseDBPath);

        internal void RefreshServerState()
        {
            HostState = GetServerState();
        }

        public RelayCommand CommandBrowseBackupPath => new RelayCommand(BrowseBackupPath);

        public MainViewModel()
        {
            LoadSettings();
        }

        public void LoadSettings()
        {
            if (IsInDesignMode)
            {
                ServiceName = @"PlexService";
                HostIP = @"192.168.1.129";
                HostState = "Running...";
                DatabasePath = @"\\192.168.1.129\plexssd\PlexMetaData\PlexData\Plex Media Server\Plug-in Support\Databases";
                BackupPath = @"D:\Temp";
                Search = "Movie";
                Message = "Error Message Here!";
                Domain = "domain";
                Username = "User";
                Password = "Pass";
                Movies = new List<Movie> {
                    new Movie { Name = "Movie 1", Year="2007", DateAdded = DateTime.Now, IsNew = false },
                    new Movie { Name = "Movie 2", Year="2001", DateAdded = DateTime.Now, IsNew = false },
                    new Movie { Name = "Movie 3", Year="2002", DateAdded = DateTime.Now, IsNew = true },
                    new Movie { Name = "Movie 4", DateAdded = DateTime.Now, IsNew = false },
                    new Movie { Name = "Movie 5", Year="2003", DateAdded = DateTime.Now, IsNew = false },
                    new Movie { Name = "Movie 6", DateAdded = DateTime.Now, IsNew = true },
                    new Movie { Name = "Movie 7", Year="2004", DateAdded = DateTime.Now, IsNew = false },
                    new Movie { Name = "Movie 8", Year="2005", DateAdded = DateTime.Now, IsNew = false },
                    new Movie { Name = "Movie 9", Year="2007", DateAdded = DateTime.Now, IsNew = false },
                    new Movie { Name = "Movie 10", DateAdded = DateTime.Now, IsNew = false },
                    new Movie { Name = "Movie 11", DateAdded = DateTime.Now, IsNew = false },
                    new Movie { Name = "Movie 12", DateAdded = DateTime.Now, IsNew = false },
                    new Movie { Name = "Movie 13", DateAdded = DateTime.Now, IsNew = false },
                    new Movie { Name = "Movie 14", DateAdded = DateTime.Now, IsNew = false },
                    new Movie { Name = "Movie 15", DateAdded = DateTime.Now, IsNew = false },
                    new Movie { Name = "Movie 16", DateAdded = DateTime.Now, IsNew = false },
                    new Movie { Name = "Movie 17", DateAdded = DateTime.Now, IsNew = false },
                    new Movie { Name = "Movie 18", DateAdded = DateTime.Now, IsNew = false },
                    new Movie { Name = "Movie 19", DateAdded = DateTime.Now, IsNew = false },
                    new Movie { Name = "Movie 20", DateAdded = DateTime.Now, IsNew = false },
                    new Movie { Name = "Movie 21", DateAdded = DateTime.Now, IsNew = false },
                    new Movie { Name = "Movie 22", DateAdded = DateTime.Now, IsNew = false },
                    new Movie { Name = "Movie 23", DateAdded = DateTime.Now, IsNew = false },
                    new Movie { Name = "Movie 24", DateAdded = DateTime.Now, IsNew = false },
                };
            }
            else
            {
                databasePath = Properties.Settings.Default.DatabasePath;
                backupPath = Properties.Settings.Default.BackupPath;
                serviceName = Properties.Settings.Default.ServiceName;
                hostIP = Properties.Settings.Default.HostIP;
                username = Properties.Settings.Default.Username;
                password = Properties.Settings.Default.Password;
                domain = null;
            }
        }

        public void SaveSettings()
        {
            Properties.Settings.Default.ServiceName = ServiceName;
            Properties.Settings.Default.HostIP = HostIP;
            Properties.Settings.Default.DatabasePath = DatabasePath;
            Properties.Settings.Default.BackupPath = BackupPath;
            Properties.Settings.Default.Username = Username;
            Properties.Settings.Default.Password = Password;
            Properties.Settings.Default.Domain = Domain;
            Properties.Settings.Default.Save();
        }

        private SQLiteConnection GetConnection()
        {
            try
            {
                var connection = new SQLiteConnection($@"Data Source={DatabasePath}; Version=3; PRAGMA journal_mode = WAL;", true);
                connection.Open();
                return connection;
            }
            catch (Exception ex)
            {
                Message = $"Error Opening Database. {ex}";
            }

            return null;
        }

        private void StopServer()
        {
            if (!ValidateSettings())
            {
                return;
            }

            try
            {
                ServiceController sc = new ServiceController(ServiceName, HostIP);
                sc.Stop();
            }
            catch (Exception ex)
            {
                HostState = ex.Message;
            }
        }

        private void StartServer()
        {
            if (!ValidateSettings())
            {
                return;
            }

            try
            {
                ServiceController sc = new ServiceController(ServiceName, HostIP);
                sc.Start();
            }
            catch (Exception ex)
            {
                HostState = ex.Message;
            }
        }

        private string GetServerState()
        {
            if (!ValidateSettings())
            {
                return null;
            }

            try
            {
                var context = new WrapperImpersonationContext(Domain, Username, Password);
                if (context.Enter())
                {
                    ServiceController sc = new ServiceController(ServiceName, HostIP);
                    var status = sc.Status.ToString();

                    context.Leave();

                    return status;
                }
                else
                {
                    return context.Error;
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public void SearchMovies()
        {
            if (!ValidateSettings())
            {
                return;
            }

            var sqliteConnection = GetConnection();
            if (sqliteConnection == null)
            {
                return;
            }

            int numRows = 0;

            var movies = new List<Movie>();

            try
            {
                using (SQLiteCommand cmd = sqliteConnection.CreateCommand())
                {
                    cmd.CommandText = $@"select * from metadata_items where title like '%{Search}%' and metadata_type = 1 and title is not null";
                    cmd.CommandType = System.Data.CommandType.Text;

                    SQLiteDataReader reader;
                    reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        numRows++;

                        var movie = new Movie
                        {
                            Id = Convert.ToInt32(reader["id"]),
                            Name = Convert.ToString(reader["title"]),
                            Year = Convert.ToString(reader["year"]),
                            DateAdded = Convert.ToDateTime(reader["added_at"])
                        };

                        movie.IsNew = movie.DateAdded >= DateTime.Now.AddDays(-14);
                        movie.OriginalHash = movie.GetHashCode();
                        movies.Add(movie);
                    }

                    Message = $"Count: {numRows}";
                }
            }
            catch (Exception exc)
            {
                Message = exc.ToString();
                numRows = 0;
            }

            sqliteConnection.Close();

            if (SortRecentFirst)
            {
                Movies = movies.OrderByDescending(_ => _.DateAdded).ThenBy(_ => _.Name).ThenBy(_ => _.Year).ToList();
            }
            else
            {
                Movies = movies.OrderBy(_ => _.Name).ThenBy(_ => _.Year).ToList();
            }

        }

        public void Save()
        {
            if (!ValidateSettings())
            {
                return;
            }

            if (!BackupDatabase())
            {
                return;
            }

            var sqliteConnection = GetConnection();
            if (sqliteConnection == null)
            {
                return;
            }

            int numRows = 0;

            var movies = new List<Movie>();

            try
            {
                Movies.ForEach(m =>
                {
                    if (m.OriginalHash != m.GetHashCode())
                    {
                        if (m.IsNew)
                        {
                            m.DateAdded = DateTime.Now;
                        }
                        else
                        {
                            m.DateAdded = m.OriginalyAvailable;
                        }

                        using (SQLiteCommand cmd = sqliteConnection.CreateCommand())
                        {
                            cmd.CommandText = $@"UPDATE metadata_items SET added_at = '{m.DateAdded.ToString("yyyy-MM-dd HH:mm:ss")}' WHERE id = {m.Id}";
                            cmd.CommandType = System.Data.CommandType.Text;
                            var results = cmd.ExecuteScalar();
                            numRows++;
                        }
                    }
                });

                Message = $"Updated {numRows} records.";
            }
            catch (Exception exc)
            {
                Message = exc.ToString();
                numRows = 0;
            }

            sqliteConnection.Close();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        public void Clear()
        {

        }

        private bool BackupDatabase()
        {
            try
            {
                Directory.CreateDirectory(BackupPath);

                var filename = Path.GetFileName(DatabasePath);
                var backupFilename = $"{DateTime.Now.ToString("yyyy-dd-M--HH-mm")}_{filename}";

                File.Copy(DatabasePath, Path.Combine(BackupPath, backupFilename), true);

                return true;
            }
            catch (Exception ex)
            {
                Message = $"Error Making DB Backup. {ex}";
                return false;
            }
        }

        public void BrowseDBPath()
        {
            var dialog = new System.Windows.Forms.OpenFileDialog();
            dialog.FileName = DatabasePath;
            var result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                DatabasePath = dialog.FileName;
            }
        }

        public void BrowseBackupPath()
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.SelectedPath = BackupPath;
            var result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                BackupPath = dialog.SelectedPath;
            }
        }

        private bool ValidateSettings()
        {
            if (string.IsNullOrEmpty(serviceName) || string.IsNullOrEmpty(HostIP) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                Message = "Please Set Plex Server Info!";
                return false;
            }

            if (!File.Exists(DatabasePath))
            {
                Message = "Database Path Not Found.";
                return false;
            }

            if (!Directory.Exists(BackupPath))
            {
                Message = "Backup Path Not Found.";
                return false;
            }

            return true;
        }

        public string ServiceName
        {
            get => serviceName;
            set
            {
                serviceName = value;
                RaisePropertyChanged();

                SaveSettings();

                HostState = GetServerState();
            }
        }

        public string HostIP
        {
            get => hostIP;
            set
            {
                hostIP = value;
                RaisePropertyChanged();

                SaveSettings();

                HostState = GetServerState();
            }
        }
        public string Domain
        {
            get => domain;
            set
            {
                domain = value;
                RaisePropertyChanged();

                SaveSettings();

                HostState = GetServerState();
            }
        }
        public string Username
        {
            get => username;
            set
            {
                username = value;
                RaisePropertyChanged();

                SaveSettings();

                HostState = GetServerState();
            }
        }
        public string Password
        {
            get => password;
            set
            {
                password = value;
                RaisePropertyChanged();

                SaveSettings();

                HostState = GetServerState();
            }
        }

        public string HostState
        {
            get => hostState;
            set
            {
                hostState = value;
                RaisePropertyChanged();
            }
        }

        public string Message
        {
            get => message;
            set
            {
                message = value;
                RaisePropertyChanged();
            }
        }

        public string DatabasePath
        {
            get
            {
                return databasePath;
            }

            set
            {
                databasePath = value;
                RaisePropertyChanged();

                SaveSettings();
            }
        }

        public string Search
        {
            get
            {
                return search;
            }

            set
            {
                search = value;
                RaisePropertyChanged();
            }
        }

        public string BackupPath
        {
            get
            {
                return backupPath;
            }

            set
            {
                backupPath = value;
                RaisePropertyChanged();

                SaveSettings();
            }
        }

        public List<Movie> Movies
        {
            get => movies;
            set
            {
                movies = value;
                RaisePropertyChanged();
            }
        }

        public bool SortRecentFirst
        {
            get => sortRecentFirst;
            set
            {
                sortRecentFirst = value;
                RaisePropertyChanged();
            }
        }
    }
}