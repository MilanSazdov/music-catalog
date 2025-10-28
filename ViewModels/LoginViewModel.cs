using MusicCatalog.Services;
using MusicCatalog.Utils;
using System;
using System.Windows.Input;

namespace MusicCatalog.ViewModels
{
    
    public class LoginViewModel : ViewModelBase
    {
        private readonly AuthService _authService;

        
        public Action ShowRegisterView { get; set; }
        public Action ShowGlavniAppView { get; set; }

        #region Properties
        private string _email = string.Empty;
        public string Email
        {
            get => _email;
            set
            {
                
                if (SetField(ref _email, value))
                {
                    OnPropertyChanged(nameof(CanLogin));
                }
            }
        }

        private string _password = string.Empty; 
        public string Password
        {
            get => _password;
            set
            {
                
                if (SetField(ref _password, value))
                {
                    OnPropertyChanged(nameof(CanLogin));
                }
            }
        }

        private string _errorMessage = string.Empty;
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetField(ref _errorMessage, value);
        }
        #endregion

        public ICommand LoginCommand { get; }
        public ICommand NavigateToRegisterCommand { get; }

        public LoginViewModel(AuthService authService)
        {
            _authService = authService;

            
            LoginCommand = new RelayCommand(OnLogin, CanLogin);
            NavigateToRegisterCommand = new RelayCommand(OnNavigateToRegister);

            
            ShowRegisterView = () => { };
            ShowGlavniAppView = () => { };
        }

        
        private bool CanLogin(object? parameter)
        {
            
            return !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Password);
        }

        private void OnLogin(object? parameter)
        {
            
            ErrorMessage = string.Empty;

            bool success = _authService.Login(Email, Password);

            if (success)
            {
                
                ClearFields();

                ShowGlavniAppView?.Invoke();
            }
            else
            {
                ErrorMessage = "Pogrešan email, lozinka ili je nalog blokiran.";
                
                Password = string.Empty;
            }
        }

        private void OnNavigateToRegister(object? parameter)
        {
            
            ClearFields();

            
            ShowRegisterView?.Invoke();
        }

        private void ClearFields()
        {
            Email = string.Empty;
            Password = string.Empty;
            ErrorMessage = string.Empty;
        }
    }
}