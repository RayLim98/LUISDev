using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using EchoBot.CognitiveModels;
using EchoBot.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Extensions.Logging;

namespace EchoBot.Dialogs
{
  public class QnADialog : CancelAndHelpDialog
  {
    // Init json handler to reading json files
    public QnADialog()
      : base(nameof(QnADialog))
    {
      // Add a different Dialog
      AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
      AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
      AddDialog(new TextPrompt(nameof(TextPrompt)));
      AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
      {
          QueryPizzaOrderStep,
          EndStep
      }));
      // The initial child Dialog to run.
      InitialDialogId = nameof(WaterfallDialog);
    }
    private async Task<DialogTurnResult> QueryPizzaOrderStep(WaterfallStepContext stepContext, CancellationToken cancellationToken) {
      var customerOrder = (CustomerOrder)stepContext.Options;

      var message = $"You've ordered {customerOrder.Quantity} {customerOrder.Pizza}, {customerOrder.Size} sized pizza(s). Is this correct?";

      // var message = $"You've ordered";
      return await stepContext.PromptAsync(nameof(ConfirmPrompt), 
        new PromptOptions {
          Prompt = MessageFactory.Text(message),
        }, cancellationToken);
    }
    private async Task<DialogTurnResult> EndStep(WaterfallStepContext stepContext, CancellationToken cancellationToken) {
      // return await stepContext.EndDialogAsync(null, cancellationToken);
      return await stepContext.ReplaceDialogAsync(nameof(MainDialog),null, cancellationToken);
    }
  }
}