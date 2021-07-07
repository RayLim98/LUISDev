using Newtonsoft.Json;
using System.Collections.Generic;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.Luis;
using Newtonsoft.Json.Linq;

namespace EchoBot
{
	public class CognitiveModelBase : IRecognizerConvert
	{
		public string Text { get; set; }
		public string AlteredText { get; set; }

		public IDictionary<string, IntentScore> Intents { get; set; }
		public JObject Entities { get; set; }

		[JsonExtensionData(ReadData = true, WriteData = true)]
		public IDictionary<string, object> Properties { get; set; }
		public void Convert(dynamic result)
		{
			throw new System.NotImplementedException();
		}

        public (string intent, double score) TopIntent()
        {
            string maxIntent = "None";
            var max = 0.0;
            foreach (var entry in Intents)
            {
                if (entry.Value.Score > max)
                {
                    maxIntent = entry.Key;
                    max = entry.Value.Score.Value;
                }
            }
            return (maxIntent, max);
        }
	}

}