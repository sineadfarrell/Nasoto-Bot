// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using System.Linq;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;

namespace Microsoft.BotBuilderSamples.Dialogs
{
    public class ModuleDialog : ComponentDialog
    {
         private readonly ConversationRecognizer _luisRecognizer;
        protected readonly ILogger Logger;
        
        
        public ModuleDialog(ConversationRecognizer luisRecognizer,  ILogger<ModuleDialog> logger, LecturerDialog lecturerDialog, ExtracurricularDialog extracurricularDialog, EndConversationDialog endConversationDialog)
            : base(nameof(ModuleDialog))

        {   
            _luisRecognizer = luisRecognizer;
            Logger = logger;
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(lecturerDialog);
            AddDialog(extracurricularDialog);
            AddDialog(endConversationDialog);
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                IntroStepAsync,
                NumberModulesStepAsync,
                NameOfModules, 
                FinalStepAsync,
                NextDialogAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }
        private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (!_luisRecognizer.IsConfigured)
            {
                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the web.config file.", inputHint: InputHints.IgnoringInput), cancellationToken);

                return await stepContext.NextAsync(null, cancellationToken);
            }
             var luisResult = await _luisRecognizer.RecognizeAsync<Luis.Conversation>(stepContext.Context, cancellationToken);
            
             if(luisResult.TopIntent().Equals(Luis.Conversation.Intent.endConversation)){
                return await stepContext.BeginDialogAsync(nameof(EndConversationDialog), cancellationToken);;    
           }

            // Use the text provided in FinalStepAsync or the default if it is the first time.
           var messageText = $"Let me start by asking you about your modules. \n How many modules are you taking this trimester?";
            var elsePromptMessage = new PromptOptions { Prompt = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput)};
            return await stepContext.PromptAsync(nameof(TextPrompt), elsePromptMessage, cancellationToken);
        }

        private async Task<DialogTurnResult> NumberModulesStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (!_luisRecognizer.IsConfigured)
            {
                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the web.config file.", inputHint: InputHints.IgnoringInput), cancellationToken);

                return await stepContext.NextAsync(null, cancellationToken);
            }
            
            var luisResult = await _luisRecognizer.RecognizeAsync<Luis.Conversation>(stepContext.Context, cancellationToken);
            if(luisResult.TopIntent().Equals(Luis.Conversation.Intent.endConversation)){
                return await stepContext.BeginDialogAsync(nameof(EndConversationDialog), cancellationToken);;    
           }
            var moduleDetails = new ModuleDetails(){
                NumberOfModules = luisResult.Entities.NumberOfModules,
            };

          //TODO : add exception if they say zero, 0, none etc
            
            var messageText = $"What module would you like to discuss?";
            var elsePromptMessage = new PromptOptions { Prompt = MessageFactory.Text( messageText, messageText, InputHints.ExpectingInput)};
            return await stepContext.PromptAsync(nameof(TextPrompt), elsePromptMessage, cancellationToken);
        }

        private async Task<DialogTurnResult> NameOfModules(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (!_luisRecognizer.IsConfigured)
            {
                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the web.config file.", inputHint: InputHints.IgnoringInput), cancellationToken);

                return await stepContext.NextAsync(null, cancellationToken);
            }
            var luisResult = await _luisRecognizer.RecognizeAsync<Luis.Conversation>(stepContext.Context, cancellationToken);
            
            var moduleDetails = new ModuleDetails(){
                ModuleName = luisResult.Entities.Module,
            };
            if(luisResult.TopIntent().Equals(Luis.Conversation.Intent.endConversation)){
                return await stepContext.BeginDialogAsync(nameof(EndConversationDialog), cancellationToken);;    
           }
           
            var messageText = $"I wouldn't be very interested in that topic, why do you like {moduleDetails.ModuleName.FirstOrDefault()}?";
            var elsePromptMessage = new PromptOptions { Prompt = MessageFactory.Text( messageText, messageText, InputHints.ExpectingInput)};
            return await stepContext.PromptAsync(nameof(TextPrompt), elsePromptMessage, cancellationToken);

        
        }
        
        private async Task<DialogTurnResult> LecturerStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            
            var moduleDetails = (ModuleDetails)stepContext.Options;

            // moduleDetails.Lecturer = (string)stepContext.Result;

            var messageText = $"Who is the lecturer for the {moduleDetails.ModuleName} module?";
            var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);

            // return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
            return await stepContext.NextAsync(null, cancellationToken);

        }

        private async Task<DialogTurnResult> ExamStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
             if (!_luisRecognizer.IsConfigured)
            {
                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the web.config file.", inputHint: InputHints.IgnoringInput), cancellationToken);

                return await stepContext.NextAsync(null, cancellationToken);
            }
            var luisResult = await _luisRecognizer.RecognizeAsync<Luis.Conversation>(stepContext.Context, cancellationToken);
            
            var moduleDetails = new ModuleDetails(){
                ModuleName = luisResult.Entities.Module,
            };
            
            var messageText = $"I've heard it very interesting, do you have a final exam?";
            var elsePromptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
            return await stepContext.NextAsync(null, cancellationToken);

        }


    
        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (!_luisRecognizer.IsConfigured)
            {
                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the web.config file.", inputHint: InputHints.IgnoringInput), cancellationToken);

                return await stepContext.NextAsync(null, cancellationToken);
            }
            var luisResult = await _luisRecognizer.RecognizeAsync<Luis.Conversation>(stepContext.Context, cancellationToken);
             var moduleDetails = new ModuleDetails(){
                ModuleName = luisResult.Entities.Module,
            };
           if(luisResult.TopIntent().Equals(Luis.Conversation.Intent.discussLecturer)){
            return await stepContext.BeginDialogAsync(nameof(LecturerDialog), moduleDetails, cancellationToken);
           }
           if(luisResult.TopIntent().Equals(Luis.Conversation.Intent.endConversation)){
                return await stepContext.BeginDialogAsync(nameof(EndConversationDialog), cancellationToken);;    
           }

           
            var messageText = $"I've heard it very interesting, what do you like about the {moduleDetails.ModuleName} ?";
            var elsePromptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
            return await stepContext.NextAsync(null, cancellationToken);
        
        }

        private async Task<DialogTurnResult> NextDialogAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
             if (!_luisRecognizer.IsConfigured)
            {
                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the web.config file.", inputHint: InputHints.IgnoringInput), cancellationToken);

                return await stepContext.NextAsync(null, cancellationToken);
            }
            var luisResult = await _luisRecognizer.RecognizeAsync<Luis.Conversation>(stepContext.Context, cancellationToken);
             var moduleDetails = new ModuleDetails(){
                ModuleName = luisResult.Entities.Module,
            };
           if(luisResult.TopIntent().Equals(Luis.Conversation.Intent.discussLecturer)){
            return await stepContext.BeginDialogAsync(nameof(LecturerDialog), moduleDetails, cancellationToken);
           }
           if(luisResult.TopIntent().Equals(Luis.Conversation.Intent.discussExtracurricular)){
               return await stepContext.BeginDialogAsync(nameof(ExtracurricularDialog), moduleDetails, cancellationToken);;
           }
           if(luisResult.TopIntent().Equals(Luis.Conversation.Intent.discussCampus)){
                return await stepContext.BeginDialogAsync(nameof(ExtracurricularDialog), moduleDetails, cancellationToken);;    
           }
           if(luisResult.TopIntent().Equals(Luis.Conversation.Intent.endConversation)){
                return await stepContext.BeginDialogAsync(nameof(EndConversationDialog), moduleDetails, cancellationToken);;    
           }
            else{
            var messageText = $"Do you stay on campus after your lectures and tutorials?";
            var elsePromptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
            return await stepContext.BeginDialogAsync(nameof(ExtracurricularDialog), moduleDetails, cancellationToken);;   
            }
        }

        private static bool IsAmbiguous(string timex)
        {
            var timexProperty = new TimexProperty(timex);
            return !timexProperty.Types.Contains(Constants.TimexTypes.Definite);
        }
    }
}
