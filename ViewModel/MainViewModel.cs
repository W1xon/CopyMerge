using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;

namespace CopyMerge.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private byte _maxStore = 100;
        private ObservableCollection<string> _clipboardStore  = new ObservableCollection<string>();
        public ObservableCollection<string> ClipboardStore
        {
            get => _clipboardStore;
            set
            {
                _clipboardStore = value;
                OnPropertyChanged();
            }
        }
        private string _clipboardPreview;
        public string FirstNote
        {
            get => _firstNote;
            set => Set(ref _firstNote, value);
        }
        private string _firstNote;
        public string ClipboardPreview
        {
            get => _clipboardPreview;
            set => Set(ref _clipboardPreview, value);
        }

        private static MainViewModel _instance;

        public static MainViewModel Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new MainViewModel();
                return _instance;
            }
        }

        private MainViewModel()
        {
            AddToClipboardStore("Скопируйте текст, чтобы история пополнилась");
        }

        public void AddToClipboardStore(string text)
        {
            var existing = _clipboardStore.FirstOrDefault(x => x.Equals(text, StringComparison.OrdinalIgnoreCase));
            if (existing != null)
            {
                _clipboardStore.Remove(existing);
            }

            _clipboardStore.Insert(0, text);

            while (_clipboardStore.Count > _maxStore)
            {
                _clipboardStore.RemoveAt(_clipboardStore.Count - 1);
            }
            ClipboardPreview = _clipboardStore.FirstOrDefault();
            FirstNote = _clipboardStore.FirstOrDefault();
        }

        public void ClearClipboardStore()
        {
            _clipboardStore.Clear();
            ClipboardPreview = "История очищена";
        }

    public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private void Set<T>(ref T field, T value, [CallerMemberName] string name = null)
        {
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                field = value;
                OnPropertyChanged(name);
            }
        }
    }
}