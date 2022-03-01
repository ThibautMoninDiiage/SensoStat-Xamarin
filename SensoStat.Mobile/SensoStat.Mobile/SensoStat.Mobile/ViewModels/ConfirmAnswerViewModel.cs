﻿using System;
using System.Threading.Tasks;
using Microsoft.CognitiveServices.Speech;
using Prism.Commands;
using Prism.Navigation;
using Prism.Navigation.Xaml;
using SensoStat.Mobile.Services.Interfaces;
using SensoStat.Mobile.ViewModels.Base;

namespace SensoStat.Mobile.ViewModels
{
    public class ConfirmAnswerViewModel : BaseViewModel
    {
        private readonly ISpeechService _speechService;
        public string UserAnswer { get; set; }
        public ConfirmAnswerViewModel(INavigationService navigationService,ISpeechService speechService) : base(navigationService)
        {
            BackCommand = new DelegateCommand(async () => await OnBackCommand());
            ValidateCommand = new DelegateCommand(async () => await OnValidateCommand());
            _speechService = speechService;
        }

        public DelegateCommand BackCommand { get; set; }
        private async Task OnBackCommand()
        { 
            await _speechService.SpeechRecognizer.StopContinuousRecognitionAsync();
            await NavigationService.GoBackAsync();
        }

        public DelegateCommand ValidateCommand { get; set; }
        private async Task OnValidateCommand()
        {
            await _speechService.SpeechSynthesizer.StopSpeakingAsync();
            if (IsBusy)
            {
                await _speechService.SpeechRecognizer.StopContinuousRecognitionAsync();
            }
            await NavigationService.NavigateAsync(Commons.Constants.EndPage);
        }

        public async override void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);

            await _speechService.TextToSpeech("Pour changer votre réponse dites reformuler. Pour valider votre réponse dites valider.");

            await _speechService.SpeechToText();
            IsBusy = true;

            _speechService.SpeechRecognizer.Recognized += async (object sender, SpeechRecognitionEventArgs e) =>
            {
                if (e.Result.Text.ToLower().Contains("reformuler"))
                {
                    await OnBackCommand();
                }
                else if (e.Result.Text.ToLower().Contains("valider"))
                {
                    await OnValidateCommand();
                }
            };
        }
    }
}
