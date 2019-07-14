using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;

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

        public MainViewModel()
        {
            if (IsInDesignMode)
            {
                DatabasePath = @"\\192.168.1.129\plexssd\PlexMetaData\PlexData\Plex Media Server\Plug-in Support\Databases";
                BackupPath = @"D:\Temp";
                Search = "Movie";
                Message = "Error!";
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
                LoadSettings();
            }
        }

        public void LoadSettings()
        {
            DatabasePath = Properties.Settings.Default.DatabasePath;
            BackupPath = Properties.Settings.Default.BackupPath;
        }

        public void SaveSettings()
        {
            Properties.Settings.Default.DatabasePath = DatabasePath;
            Properties.Settings.Default.BackupPath = BackupPath;
            Properties.Settings.Default.Save();
        }

        public RelayCommand CommandSearch => new RelayCommand(SearchMovies);
        public RelayCommand CommandSave => new RelayCommand(Save);
        public RelayCommand CommandClear => new RelayCommand(Clear);
        public RelayCommand CommandBrowseDBPath => new RelayCommand(BrowseDBPath);
        public RelayCommand CommandBrowseBackupPath => new RelayCommand(BrowseBackupPath);

        public void SearchMovies()
        {
            if (!ValidateDatabasePath(DatabasePath))
            {
                return;
            }

            var sqliteConnection = new SQLiteConnection($@"Data Source={DatabasePath};Version=3;");

            sqliteConnection.Open();

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
            else{
                Movies = movies.OrderBy(_ => _.Name).ThenBy(_ => _.Year).ToList();
            }
            
        }

        public void Save()
        {
            if (!ValidateDatabasePath(DatabasePath))
            {
                return;
            }

            if (!BackupDatabase())
            {
                return;
            }

            var sqliteConnection = new SQLiteConnection($@"Data Source={DatabasePath};Version=3;");

            sqliteConnection.Open();

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

        private bool ValidateDatabasePath(string path)
        {
            if (!File.Exists(path))
            {
                Message = "Path Not Found.";
                return false;
            }

            return true;
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

    public class Movie : ViewModelBase
    {
        private DateTime dateAdded;
        private bool isNew;
        private string name;
        private string year;

        public string Name
        {
            get => name; set
            {
                name = value;
                RaisePropertyChanged();
            }
        }

        public string Year
        {
            get => year;
            set
            {
                year = value;
                RaisePropertyChanged();
            }
        }

        public DateTime DateAdded
        {
            get => dateAdded; set
            {
                dateAdded = value;
                RaisePropertyChanged();
            }
        }

        public bool IsNew
        {
            get => isNew; set
            {
                isNew = value;
                RaisePropertyChanged();
            }
        }

        public int Id { get; set; }

        public DateTime OriginalyAvailable { get; set; }

        public int OriginalHash { get; set; }

        public override bool Equals(object obj)
        {
            return obj is Movie movie &&
                   dateAdded == movie.dateAdded &&
                   isNew == movie.isNew &&
                   name == movie.name &&
                   year == movie.year &&
                   Name == movie.Name &&
                   Year == movie.Year &&
                   DateAdded == movie.DateAdded &&
                   IsNew == movie.IsNew &&
                   Id == movie.Id &&
                   OriginalyAvailable == movie.OriginalyAvailable;
        }

        public override int GetHashCode()
        {
            var hashCode = 834298102;
            hashCode = hashCode * -1521134295 + dateAdded.GetHashCode();
            hashCode = hashCode * -1521134295 + isNew.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(name);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(year);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Year);
            hashCode = hashCode * -1521134295 + DateAdded.GetHashCode();
            hashCode = hashCode * -1521134295 + IsNew.GetHashCode();
            hashCode = hashCode * -1521134295 + Id.GetHashCode();
            hashCode = hashCode * -1521134295 + OriginalyAvailable.GetHashCode();
            return hashCode;
        }
    }
}