using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace TelegramBotWPF2
{
    public class TelegramUser : INotifyPropertyChanged, IEquatable<TelegramUser>
    {
        public TelegramUser(long id, string firstName, string lastName, string username)
        {
            this.id = id;
            this.firstName = firstName;
            this.lastName = lastName;
            this.username = username;
            Messages = new ObservableCollection<string>(); 
        }
        private long id;
        public long Id
        {
            get { return this.id; }
            set
            { 
                this.id = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.id)));
            }
        }
        private string firstName;
        public string FirstName
        {
            get { return this.firstName;}
            set 
            {
                this.firstName = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.firstName)));
            }
        }
        private string lastName;
        public string LastName
        {
            get { return this.lastName;} 
            set
            {
                this.lastName = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.lastName)));
            }
        }
        private string username;
        public string Username 
        { 
            get { return this.username;}
            set
            {
                this.username = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.username)));
            }
        }

        public string FullName
        {
            get { return $"{this.firstName} {this.lastName}"; }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public bool Equals(TelegramUser? other) => other?.Id == this.id;
        public ObservableCollection<string> Messages { get; set; }
        public void AddMessage(string message) => Messages.Add(message);


    }
}
