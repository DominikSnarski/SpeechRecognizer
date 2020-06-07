using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CognitiveServices.Speech;

namespace SpeechRecognition
{
    public class Recognizer
    {
        public string text;
        public string language;
        public Recognizer()
        {

        }

        public async Task RecognizeSpeechAsync()
        {
            text = "Error";
            language = "Error";

            var config =
                SpeechConfig.FromSubscription("54e5c11f4ba84a95a282d180905efeb1", "westus");

            var autoDetectSourceLanguageConfig = AutoDetectSourceLanguageConfig.FromLanguages(new string[] { "en-US", "de-DE", "pl-PL" });

            using var recognizer = new SpeechRecognizer(config,autoDetectSourceLanguageConfig);

            var result = await recognizer.RecognizeOnceAsync();

            var autoDetectSourceLanguageResult = AutoDetectSourceLanguageResult.FromResult(result);
            var detectedLanguage = autoDetectSourceLanguageResult.Language;

            language = detectedLanguage;

            switch (result.Reason)
            {
                case ResultReason.RecognizedSpeech:
                    text = result.Text;
                    break;
                case ResultReason.NoMatch:
                    text = $"NOMATCH: Rozpoznanie nie udało się.";
                    break;
                case ResultReason.Canceled:
                    var cancellation = CancellationDetails.FromResult(result);

                    if (cancellation.Reason == CancellationReason.Error)
                    {
                        Debug.WriteLine($"CANCELED: ErrorCode={cancellation.ErrorCode}");
                        Debug.WriteLine($"CANCELED: ErrorDetails={cancellation.ErrorDetails}");
                        Debug.WriteLine($"CANCELED: Did you update the subscription info?");
                    }
                    text =  $"CANCELED: Reason={cancellation.Reason}";
                    break;
            }

        }
    }
}
