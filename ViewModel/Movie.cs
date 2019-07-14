using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;

namespace Plex_Database_Editor.ViewModel
{
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