using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Logging;
using Microsoft.Bot.Schema;
using EchoBot.CognitiveModels;
using EchoBot.Models;
using Microsoft.Bot.Builder.LanguageGeneration;
using Microsoft.Extensions.Configuration;

namespace EchoBot.Dialogs
{
  public class MainDialog : ComponentDialog
  {
    protected readonly ILogger _logger;
    private readonly TestRecognizer _luisRecognizer;
    private readonly Templates _templates;
    // private static Templates _templates;
    // Init json handler to reading json files
    public MainDialog(ILogger<MainDialog> logger, 
                      TestRecognizer luisRecognizer,
                      // IBotServices services,
                      IConfiguration configuration)
      : base(nameof(MainDialog))
    {
      _luisRecognizer = luisRecognizer;
      _logger = logger;

      // Load in LG
      string[] paths = {".","Resources","IntroductoryTemplate.lg"};
      string fullpaths = Path.Combine(paths);
      _templates = Templates.ParseFile(fullpaths);

      // Add a different Dialog
      AddDialog(new PizzaOrderDialog());
      AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
      AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
      AddDialog(new TextPrompt(nameof(TextPrompt)));
      AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
      {
        IntroStepAsync,
        FirstStepAsync,
      }));

      // The initial child Dialog to run.
      InitialDialogId = nameof(WaterfallDialog);
    }
    private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken) {
        if (!_luisRecognizer.IsConfigured) {
            await stepContext.Context.SendActivityAsync(
                MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the appsettings.json file.", inputHint: InputHints.IgnoringInput), cancellationToken);

            return await stepContext.NextAsync(null, cancellationToken);
        }

        // Use the text provided in FinalStepAsync or the default if it is the first time.
        var messageText = stepContext.Options?.ToString() ?? "What can I help you with today?";
        var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
        return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
    }
    private async Task<DialogTurnResult> FirstStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken) {
        var luisResult = await _luisRecognizer.RecognizeAsync<PizzaOrder>(stepContext.Context, cancellationToken);

        switch (luisResult.TopIntent().intent) {
          case "OrderFood":
            var orderDetails = luisResult.Entities.Order[0].Pizza[0];
            var customerOrder = new CustomerOrder() {
              Pizza = orderDetails.Type?[0],
              Size = orderDetails.Size?[0],
              Quantity = orderDetails.Quantity?[0],
            };
            return await stepContext.BeginDialogAsync(nameof(PizzaOrderDialog), customerOrder, cancellationToken);
          case "Greeting":
            var message = _templates.Evaluate("Greeting").ToString();
            await stepContext.Context.SendActivityAsync(MessageFactory.Text(message));
            return await stepContext.ReplaceDialogAsync(nameof(MainDialog), null, cancellationToken);
          case "SayersQnA":
            break;
          default:
          break;
        }
      return await stepContext.NextAsync(luisResult, cancellationToken);
    }
  }
}