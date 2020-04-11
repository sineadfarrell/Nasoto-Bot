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




        public ModuleDialog(ConversationRecognizer luisRecognizer, ILogger<ModuleDialog> logger, LecturerDialog lecturerDialog, ExtracurricularDialog extracurricularDialog, EndConversationDialog endConversationDialog, CampusDialog campusDialog)
            : base(nameof(ModuleDialog))

        {

            _luisRecognizer = luisRecognizer;
            Logger = logger;
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(lecturerDialog);
            AddDialog(extracurricularDialog);
            AddDialog(endConversationDialog);

            AddDialog(campusDialog);
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                IntroStepAsync,
                NumberModulesStepAsync,
                FavModuleAsync,
                ExamorCaFavAsync,
                OpinionFavAsync,
                LeastFavModuleAsync,
                ExamorCaLeastAsync,
                OpinionLeastAsync,
                // FinalStepAsync,
                // NextDialogAsync,
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

            if (luisResult.TopIntent().Equals(Luis.Conversation.Intent.endConversation))
            {
                 await stepContext.Context.SendActivityAsync(
                     MessageFactory.Text("Do you want to end this conversation?"));
                     return await stepContext.ReplaceDialogAsync(nameof(EndStepAsync));
            }

            // Use the text provided in FinalStepAsync or the default if it is the first time.
            var messageText = $"We will discuss your modules now. \n How many modules are you taking this trimester?";
            var elsePromptMessage = new PromptOptions { Prompt = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput) };
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
            if (luisResult.TopIntent().Equals(Luis.Conversation.Intent.endConversation))
            {
                return await stepContext.BeginDialogAsync(nameof(EndConversationDialog)); ;
            }
            var moduleDetails = new ModuleDetails()
            {
                NumberOfModules = luisResult.Entities.NumberOfModules,
            };

            int i = 0;
            string[] numbers = { "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten" };
            switch (luisResult.TopIntent().intent)
            {

                case Luis.Conversation.Intent.discussModule: 
                if(moduleDetails.NumberOfModules != null) {              
                    if (int.TryParse(moduleDetails.NumberOfModules.FirstOrDefault(), out i))
                    {
                        var messageText = $"Ok {moduleDetails.NumberOfModules.FirstOrDefault()} modules. Which module is your favourite, mine would be secure software engineering?";
                        var elsePromptMessage = new PromptOptions { Prompt = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput) };
                        return await stepContext.PromptAsync(nameof(TextPrompt), elsePromptMessage, cancellationToken);

                    }
                    foreach (string x in numbers)
                    {
                        if ((moduleDetails.NumberOfModules.FirstOrDefault().ToLower()).Contains(x))
                        {
                        var messageText = $"Ok {moduleDetails.NumberOfModules.FirstOrDefault()} modules. Which module is your favourite, mine would be secure software engineering?";
                        var elsePromptMessage = new PromptOptions { Prompt = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput) };
                        return await stepContext.PromptAsync(nameof(TextPrompt), elsePromptMessage, cancellationToken);
                        }

                    }
                }
                

                    var didntUnderstandMessageText3 = $"Sorry, I didn't understand that. Could you please rephrase";
                    var elsePromptMessage4 = new PromptOptions { Prompt = MessageFactory.Text(didntUnderstandMessageText3, didntUnderstandMessageText3, InputHints.ExpectingInput) };

                    stepContext.ActiveDialog.State[key: "stepIndex"] = (int)stepContext.ActiveDialog.State["stepIndex"] - 1;
                    return await stepContext.PromptAsync(nameof(TextPrompt), elsePromptMessage4, cancellationToken);
                

                case Luis.Conversation.Intent.None:
                    var didntUnderstandMessageText2 = $"Sorry, I didn't understand that. Could you please rephrase";
                    var elsePromptMessage2 = new PromptOptions { Prompt = MessageFactory.Text(didntUnderstandMessageText2, didntUnderstandMessageText2, InputHints.ExpectingInput) };

                    stepContext.ActiveDialog.State[key: "stepIndex"] = (int)stepContext.ActiveDialog.State["stepIndex"] - 1;
                    return await stepContext.PromptAsync(nameof(TextPrompt), elsePromptMessage2, cancellationToken);

                default:
                    var didntUnderstandMessageText = $"Sorry, I didn't understand that. Could you please rephrase";
                    var elsePromptMessage3 = new PromptOptions { Prompt = MessageFactory.Text(didntUnderstandMessageText, didntUnderstandMessageText, InputHints.ExpectingInput) };

                    stepContext.ActiveDialog.State[key: "stepIndex"] = (int)stepContext.ActiveDialog.State["stepIndex"] - 1;
                    return await stepContext.PromptAsync(nameof(TextPrompt), elsePromptMessage3, cancellationToken);

            }
        }

        private async Task<DialogTurnResult> FavModuleAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (!_luisRecognizer.IsConfigured)
            {
                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the web.config file.", inputHint: InputHints.IgnoringInput), cancellationToken);

                return await stepContext.NextAsync(null, cancellationToken);
            }
            var luisResult = await _luisRecognizer.RecognizeAsync<Luis.Conversation>(stepContext.Context, cancellationToken);

            var moduleDetails = new ModuleDetails()
            {
                ModuleName = luisResult.Entities.Module,
            };

            switch (luisResult.TopIntent().intent)
            {


                case Luis.Conversation.Intent.endConversation:
                    return await stepContext.BeginDialogAsync(nameof(EndConversationDialog));

                case Luis.Conversation.Intent.discussModule:
                    var messageText = " ";
                    var elsePromptMessage = new PromptOptions { Prompt = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput) };

                    if(moduleDetails.ModuleName != null){
                    messageText = $"Is there a final exam for {moduleDetails.ModuleName.FirstOrDefault()}?";
                    elsePromptMessage = new PromptOptions { Prompt = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput) };
                    return await stepContext.PromptAsync(nameof(TextPrompt), elsePromptMessage, cancellationToken);

                    }
                    var didntUnderstandMessageText = $"Sorry, I didn't understand that. Could you please rephrase";
                        var elsePromptMessageNone = new PromptOptions { Prompt = MessageFactory.Text(didntUnderstandMessageText, didntUnderstandMessageText, InputHints.ExpectingInput) };

                        stepContext.ActiveDialog.State[key: "stepIndex"] = (int)stepContext.ActiveDialog.State["stepIndex"] - 1;
                        return await stepContext.PromptAsync(nameof(TextPrompt), elsePromptMessageNone, cancellationToken);
                    

                case Luis.Conversation.Intent.None:
                    {
                        var didntUnderstandMessageText3 = $"Sorry, I didn't understand that. Could you please rephrase";
                        var elsePromptMessageNone3 = new PromptOptions { Prompt = MessageFactory.Text(didntUnderstandMessageText3, didntUnderstandMessageText3, InputHints.ExpectingInput) };

                        stepContext.ActiveDialog.State[key: "stepIndex"] = (int)stepContext.ActiveDialog.State["stepIndex"] - 1;
                        return await stepContext.PromptAsync(nameof(TextPrompt), elsePromptMessageNone3, cancellationToken);
                    }

                default:
                    // Catch all for unhandled intents
                    var didntUnderstandMessageText2 = $"I didn't understand that. Could you please rephrase";
                    var elsePromptMessage2 = new PromptOptions { Prompt = MessageFactory.Text(didntUnderstandMessageText2, didntUnderstandMessageText2, InputHints.ExpectingInput) };

                    stepContext.ActiveDialog.State[key: "stepIndex"] = (int)stepContext.ActiveDialog.State["stepIndex"] - 1;
                    return await stepContext.PromptAsync(nameof(TextPrompt), elsePromptMessage2, cancellationToken);

            }


        }

        private async Task<DialogTurnResult> ExamorCaFavAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (!_luisRecognizer.IsConfigured)
            {
                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the web.config file.", inputHint: InputHints.IgnoringInput), cancellationToken);

                return await stepContext.NextAsync(null, cancellationToken);
            }
            var luisResult = await _luisRecognizer.RecognizeAsync<Luis.Conversation>(stepContext.Context, cancellationToken);

            var moduleDetails = new ModuleDetails()
            {
                ModuleName = luisResult.Entities.Module,
            };
            switch (luisResult.TopIntent().intent)
            {

                case Luis.Conversation.Intent.discussModule:
                    var messageText = $"Ok and why do you like the module?";
                    var elsePromptMessage = new PromptOptions { Prompt = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput) };
                    return await stepContext.PromptAsync(nameof(TextPrompt), elsePromptMessage, cancellationToken);

                case Luis.Conversation.Intent.endConversation:
                    return await stepContext.BeginDialogAsync(nameof(EndConversationDialog)); 



                case Luis.Conversation.Intent.None:
                    var didntUnderstandMessageText2 = $"I didn't understand that. Could you please rephrase";
                    var elsePromptMessage2 = new PromptOptions { Prompt = MessageFactory.Text(didntUnderstandMessageText2, didntUnderstandMessageText2, InputHints.ExpectingInput) };

                    stepContext.ActiveDialog.State[key: "stepIndex"] = (int)stepContext.ActiveDialog.State["stepIndex"] - 1;
                    return await stepContext.PromptAsync(nameof(TextPrompt), elsePromptMessage2, cancellationToken);

                default:
                    var messageText2 = $"Ok and why do you like the module?";
                    var elsePromptMessage3 = new PromptOptions { Prompt = MessageFactory.Text(messageText2, messageText2, InputHints.ExpectingInput) };
                    return await stepContext.PromptAsync(nameof(TextPrompt), elsePromptMessage3, cancellationToken);
            }
        }
        private async Task<DialogTurnResult> ExamorCaLeastAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (!_luisRecognizer.IsConfigured)
            {
                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the web.config file.", inputHint: InputHints.IgnoringInput), cancellationToken);

                return await stepContext.NextAsync(null, cancellationToken);
            }
            var luisResult = await _luisRecognizer.RecognizeAsync<Luis.Conversation>(stepContext.Context, cancellationToken);

            var moduleDetails = new ModuleDetails()
            {
                ModuleName = luisResult.Entities.Module,
            };

            switch (luisResult.TopIntent().intent)
            {

                case Luis.Conversation.Intent.endConversation:
                    return await stepContext.BeginDialogAsync(nameof(EndConversationDialog)); ;

                case Luis.Conversation.Intent.discussModule:
                    var messageText2 = $"What are the reasons for disliking this module?";
                    var elsePromptMessage2 = new PromptOptions { Prompt = MessageFactory.Text(messageText2, messageText2, InputHints.ExpectingInput) };
                    return await stepContext.PromptAsync(nameof(TextPrompt), elsePromptMessage2, cancellationToken);

                // Catch all for unhandled intents
                case Luis.Conversation.Intent.None:
                    var didntUnderstandMessageText2 = $"I didn't understand that. Could you please rephrase";
                    var elsePromptMessage = new PromptOptions { Prompt = MessageFactory.Text(didntUnderstandMessageText2, didntUnderstandMessageText2, InputHints.ExpectingInput) };

                    stepContext.ActiveDialog.State[key: "stepIndex"] = (int)stepContext.ActiveDialog.State["stepIndex"] - 1;
                    return await stepContext.PromptAsync(nameof(TextPrompt), elsePromptMessage, cancellationToken);

                default:
                    var messageText = $"What are the reasons for disliking this module?";
                    var elsePromptMessage3 = new PromptOptions { Prompt = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput) };
                    return await stepContext.PromptAsync(nameof(TextPrompt), elsePromptMessage3, cancellationToken);
            }

        }
        private async Task<DialogTurnResult> OpinionFavAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            if (!_luisRecognizer.IsConfigured)
            {
                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the web.config file.", inputHint: InputHints.IgnoringInput), cancellationToken);

                return await stepContext.NextAsync(null, cancellationToken);
            }
            var luisResult = await _luisRecognizer.RecognizeAsync<Luis.Conversation>(stepContext.Context, cancellationToken);

            var moduleDetails = new ModuleDetails()
            {
                ModuleName = luisResult.Entities.Module,
            };
            switch ((luisResult.TopIntent().intent))
            {

                case Luis.Conversation.Intent.endConversation:
                return await stepContext.BeginDialogAsync(nameof(EndConversationDialog)); ;

            
                case Luis.Conversation.Intent.discussModule:
                    var messageText2 = $"Presumably you have a least favourite. What would you say is your least favourite module?";
                    var elsePromptMessage2 = new PromptOptions { Prompt = MessageFactory.Text(messageText2, messageText2, InputHints.ExpectingInput) };
                    return await stepContext.PromptAsync(nameof(TextPrompt), elsePromptMessage2, cancellationToken);


                case Luis.Conversation.Intent.None:
                    var didntUnderstandMessageText2 = $"I didn't understand that. Could you please rephrase";
                    var elsePromptMessage = new PromptOptions { Prompt = MessageFactory.Text(didntUnderstandMessageText2, didntUnderstandMessageText2, InputHints.ExpectingInput) };

                    stepContext.ActiveDialog.State[key: "stepIndex"] = (int)stepContext.ActiveDialog.State["stepIndex"] - 1;
                    return await stepContext.PromptAsync(nameof(TextPrompt), elsePromptMessage, cancellationToken);

                default:
                    messageText2 = $"Presumably you have a least favourite. What would you say is your least favourite module?";
                    elsePromptMessage2 = new PromptOptions { Prompt = MessageFactory.Text(messageText2, messageText2, InputHints.ExpectingInput) };
                    return await stepContext.PromptAsync(nameof(TextPrompt), elsePromptMessage2, cancellationToken);

            }
        }

        private async Task<DialogTurnResult> LeastFavModuleAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (!_luisRecognizer.IsConfigured)
            {
                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the web.config file.", inputHint: InputHints.IgnoringInput), cancellationToken);

                return await stepContext.NextAsync(null, cancellationToken);
            }
            var luisResult = await _luisRecognizer.RecognizeAsync<Luis.Conversation>(stepContext.Context, cancellationToken);

            var moduleDetails = new ModuleDetails()
            {
                ModuleName = luisResult.Entities.Module,
            };

            switch ((luisResult.TopIntent().intent))
            {

                case Luis.Conversation.Intent.endConversation:
                    return await stepContext.BeginDialogAsync(nameof(EndConversationDialog));


                case Luis.Conversation.Intent.None:

                    var didntUnderstandMessageText2 = $"I didn't understand that. Could you please rephrase";
                    var elsePromptMessage = new PromptOptions { Prompt = MessageFactory.Text(didntUnderstandMessageText2, didntUnderstandMessageText2, InputHints.ExpectingInput) };

                    stepContext.ActiveDialog.State[key: "stepIndex"] = (int)stepContext.ActiveDialog.State["stepIndex"] - 1;
                    return await stepContext.PromptAsync(nameof(TextPrompt), elsePromptMessage, cancellationToken);

                case Luis.Conversation.Intent.discussModule:
                    var messageText = " ";
                    elsePromptMessage = new PromptOptions { Prompt = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput) };

                    if (moduleDetails.ModuleName == null)
                    {
                        messageText = $"Oh no! I don't think I know that module. With the ongoing pandemic do you have a final exam for this module?";
                        elsePromptMessage = new PromptOptions { Prompt = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput) };
                    }
                    else
                    {
                        messageText = $"I don't like {moduleDetails.ModuleName.FirstOrDefault()} either. With the ongoing pandemic is there a final exam for {moduleDetails.ModuleName.FirstOrDefault()}?";
                        elsePromptMessage = new PromptOptions { Prompt = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput) };
                    }

                    return await stepContext.PromptAsync(nameof(TextPrompt), elsePromptMessage, cancellationToken);
                default:
                    messageText = $"Oh no! I don't think I know that module. With the ongoing pandemic is there a final exam for that module?";
                    elsePromptMessage = new PromptOptions { Prompt = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput) };
                    return await stepContext.PromptAsync(nameof(TextPrompt), elsePromptMessage, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> OpinionLeastAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (!_luisRecognizer.IsConfigured)
            {
                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the web.config file.", inputHint: InputHints.IgnoringInput), cancellationToken);

                return await stepContext.NextAsync(null, cancellationToken);
            }
            var luisResult = await _luisRecognizer.RecognizeAsync<Luis.Conversation>(stepContext.Context, cancellationToken);

            var moduleDetails = new ModuleDetails()
            {
                ModuleName = luisResult.Entities.Module,
            };

            switch ((luisResult.TopIntent().intent))
            {

                case Luis.Conversation.Intent.endConversation:
                    return await stepContext.BeginDialogAsync(nameof(EndConversationDialog)); ;

                case Luis.Conversation.Intent.discussModule:
                    var messageText = $"Now we are going to talk about your lecturers.";
                    var Message = MessageFactory.Text(messageText, messageText, InputHints.IgnoringInput);
                    await stepContext.Context.SendActivityAsync(Message, cancellationToken);
                    return await stepContext.BeginDialogAsync(nameof(LecturerDialog));

                default:
                    var didntUnderstandMessageText = $"I didn't understand that. Could you please rephrase";
                    var elsePromptMessage2 = new PromptOptions { Prompt = MessageFactory.Text(didntUnderstandMessageText, didntUnderstandMessageText, InputHints.ExpectingInput) };

                    stepContext.ActiveDialog.State[key: "stepIndex"] = (int)stepContext.ActiveDialog.State["stepIndex"] - 1;
                    return await stepContext.PromptAsync(nameof(TextPrompt), elsePromptMessage2, cancellationToken);
            }
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
            var moduleDetails = new ModuleDetails()
            {
                ModuleName = luisResult.Entities.Module,
            };
            switch (luisResult.TopIntent().intent)

        
            {
                case Luis.Conversation.Intent.endConversation:
                    return await stepContext.BeginDialogAsync(nameof(EndConversationDialog), moduleDetails); ;

                case Luis.Conversation.Intent.discussLecturer:
                    var didntUnderstandMessageText = $"That's great! Why don't we talk about your lecturers.";
                    var didntUnderstandMessage = MessageFactory.Text(didntUnderstandMessageText, didntUnderstandMessageText, InputHints.IgnoringInput);
                    await stepContext.Context.SendActivityAsync(didntUnderstandMessage, cancellationToken);
                    return await stepContext.BeginDialogAsync(nameof(LecturerDialog), moduleDetails, cancellationToken);

                case Luis.Conversation.Intent.discussExtracurricular:
                    var didntUnderstandMessageText2 = $"That's great! Do you do anything in your spare time?";
                    var didntUnderstandMessage2 = MessageFactory.Text(didntUnderstandMessageText2, didntUnderstandMessageText2, InputHints.IgnoringInput);
                    await stepContext.Context.SendActivityAsync(didntUnderstandMessage2, cancellationToken);
                    return await stepContext.BeginDialogAsync(nameof(ExtracurricularDialog), moduleDetails, cancellationToken); ;

                case Luis.Conversation.Intent.discussCampus:
                    var didntUnderstandMessageText3 = $"That's great! Do you do like UCD's campus?";
                    var didntUnderstandMessage3 = MessageFactory.Text(didntUnderstandMessageText3, didntUnderstandMessageText3, InputHints.IgnoringInput);
                    await stepContext.Context.SendActivityAsync(didntUnderstandMessage3, cancellationToken);
                    return await stepContext.BeginDialogAsync(nameof(CampusDialog), moduleDetails, cancellationToken); ;

                

                default:
                    var didntUnderstandMessageText4 = $"Sorry, I didn't get that. Please try rephrasing your message(intent was {luisResult.TopIntent().intent})";
                    var didntUnderstandMessage4 = new PromptOptions { Prompt = MessageFactory.Text(didntUnderstandMessageText4, didntUnderstandMessageText4, InputHints.IgnoringInput)};
                    
                    stepContext.ActiveDialog.State[key: "stepIndex"] = (int)stepContext.ActiveDialog.State["stepIndex"] - 1;
                    return await stepContext.PromptAsync(nameof(TextPrompt), didntUnderstandMessage4, cancellationToken);


                    
                    ;
            }
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
            var moduleDetails = new ModuleDetails()
            {
                ModuleName = luisResult.Entities.Module,
            };

            if (luisResult.TopIntent().Equals(Luis.Conversation.Intent.discussLecturer))
            {
                return await stepContext.BeginDialogAsync(nameof(LecturerDialog), moduleDetails, cancellationToken);
            }
            if (luisResult.TopIntent().Equals(Luis.Conversation.Intent.discussExtracurricular))
            {
                return await stepContext.BeginDialogAsync(nameof(ExtracurricularDialog), moduleDetails, cancellationToken); ;
            }
            if (luisResult.TopIntent().Equals(Luis.Conversation.Intent.discussCampus))
            {
                return await stepContext.BeginDialogAsync(nameof(CampusDialog), moduleDetails, cancellationToken); ;
            }
            if (luisResult.TopIntent().Equals(Luis.Conversation.Intent.endConversation))
            {
                return await stepContext.BeginDialogAsync(nameof(EndConversationDialog), moduleDetails, cancellationToken); ;
            }
            if (luisResult.TopIntent().Equals(Luis.Conversation.Intent.None))
            {
                var didntUnderstandMessageText = $"Sorry, I didn't get that. Please try rephrasing your message(intent was {luisResult.TopIntent().intent})";
                var didntUnderstandMessage = MessageFactory.Text(didntUnderstandMessageText, didntUnderstandMessageText, InputHints.IgnoringInput);
                await stepContext.Context.SendActivityAsync(didntUnderstandMessage, cancellationToken);
            }

            var messageText = $"Would you spend much time on campus after your lectures and tutorials?";
            var elsePromptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
            return await stepContext.BeginDialogAsync(nameof(ExtracurricularDialog), moduleDetails, cancellationToken); ;
        }


        private static bool IsAmbiguous(string timex)
        {
            var timexProperty = new TimexProperty(timex);
            return !timexProperty.Types.Contains(Constants.TimexTypes.Definite);
        }

                     private async Task<DialogTurnResult> EndStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string[] stringPos;
            stringPos = new string[21] { "yes", "ye", "yep", "ya", "yas", "totally", "sure", "ok", "k", "okey", "okay", "alright", "sounds good", "sure thing", "of course", "gladly", "definitely", "indeed", "absolutely","yes please", "please" };
            string[] stringNeg;
            stringNeg = new string[9] { "no", "nope", "no thanks", "unfortunately not", "apologies", "nah", "not now", "no can do", "no thank you" };

            if (!_luisRecognizer.IsConfigured)
            {
                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the web.config file.", inputHint: InputHints.IgnoringInput), cancellationToken);

                return await stepContext.NextAsync(null, cancellationToken);
            }

            var luisResult = await _luisRecognizer.RecognizeAsync<Luis.Conversation>(stepContext.Context, cancellationToken);

            if (stringPos.Any(luisResult.Text.ToLower().Contains))
            {
                ConversationData.PromptedUserForName = true;
                await stepContext.Context.SendActivityAsync(
                     MessageFactory.Text("Goodbye."));
                    return await stepContext.EndDialogAsync();
                 
            }
            if (stringNeg.Any(luisResult.Text.ToLower().Contains))
            {
                var messageText = $"Ok the conversation will continue.";
                var elsePromptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
                return await stepContext.BeginDialogAsync(nameof(MainDialog));
            }
            var didntUnderstandMessageText = $"I didn't understand that. Could you please rephrase";
            var elsePromptMessage2 = new PromptOptions { Prompt = MessageFactory.Text(didntUnderstandMessageText, didntUnderstandMessageText, InputHints.ExpectingInput) };

            stepContext.ActiveDialog.State[key: "stepIndex"] = (int)stepContext.ActiveDialog.State["stepIndex"] - 1;
            return await stepContext.PromptAsync(nameof(TextPrompt), elsePromptMessage2, cancellationToken);

        }


    }
}
