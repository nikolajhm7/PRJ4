using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Client.Library.DTO
{
    public class FriendDTO : ObservableObject
    {
        public string? Name { get; set; }
        public DateTime FriendsSince { get; set; }

        private bool _isPending;
        public bool IsPending
        {
            get => _isPending;
            set => SetProperty(ref _isPending, value);
        }
    }
}