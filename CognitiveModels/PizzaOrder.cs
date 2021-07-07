using Newtonsoft.Json;
using System.Collections.Generic;
using Microsoft.Bot.Builder;
using Newtonsoft.Json.Linq;
using Microsoft.Bot.Builder.AI.Luis;

namespace EchoBot.CognitiveModels
{
	public class PizzaOrder : IRecognizerConvert
	{
		public string Text { get; set; }
		public string AlteredText { get; set; }
		public IDictionary<string, IntentScore> Intents { get; set; }

		// public JObject Entities { get; set; }
	 	public _Entities Entities { get; set; }
        public _Order Order { get; set; }
        public _Pizza Pizza { get; set; }
		public class _Entities
		{
			public List<_Order> Order { get; set; }
		}


		public class _Order {
			public List<_Pizza> Pizza { get; set; }
		}

		public class _Pizza {
			public List<string> Type { get; set; }
			public List<string> Quantity { get; set; }
            public List<string> Size { get; set; }
		}


		[JsonExtensionData(ReadData = true, WriteData = true)]
		public IDictionary<string, object> Properties { get; set; }
        public void Convert(dynamic result)
        {
            var app = JsonConvert.DeserializeObject<PizzaOrder>(JsonConvert.SerializeObject(result, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
            Text = app.Text;
            AlteredText = app.AlteredText;
            Intents = app.Intents;
            Entities = app.Entities;
            Properties = app.Properties;
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