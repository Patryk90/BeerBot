using System;
using System.Collections.Generic;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.DateTime;
using Microsoft.Recognizers.Text.Number;

namespace BeerBot.Services
{
    public static class OrderValidation
    {
        public static bool ValidateName(string input, out string name, out string message)
        {
            name = null;
            message = null;

            if (string.IsNullOrWhiteSpace(input))
            {
                message = "Please enter a name that contains at least one character.";
            }
            else
            {
                name = input.Trim();
            }

            return message is null;
        }

        public static bool ValidateAge(string input, out int age, out string message)
        {
            age = 0;
            message = null;

            try
            {
                var results = NumberRecognizer.RecognizeNumber(input, Culture.English);

                foreach (var result in results)
                {
                    // The result resolution is a dictionary, where the "value" entry contains the processed string.
                    if (result.Resolution.TryGetValue("value", out object value))
                    {
                        age = Convert.ToInt32(value);
                        if (age >= 18 && age <= 120)
                        {
                            return true;
                        }
                    }
                }

                message = "Please enter an age between 18 and 120.";
            }
            catch
            {
                message = "I'm sorry, I could not interpret that as an age. Please enter an age between 18 and 120.";
            }

            return message is null;
        }

        public static bool ValidateDate(string input, out string date, out string message)
        {
            date = null;
            message = null;
            try
            {
                var results = DateTimeRecognizer.RecognizeDateTime(input, Culture.English);

                var earliest = DateTime.Now.AddHours(1.0);

                foreach (var result in results)
                {
                    var resolutions = result.Resolution["values"] as List<Dictionary<string, string>>;

                    foreach (var resolution in resolutions)
                    {
                        if (resolution.TryGetValue("value", out string dateString)
                            || resolution.TryGetValue("start", out dateString))
                        {
                            if (DateTime.TryParse(dateString, out DateTime candidate)
                                && earliest < candidate)
                            {
                                date = candidate.ToShortDateString();
                                return true;
                            }
                        }
                    }
                }

                message = "I'm sorry, please enter a date at least an hour out.";
            }
            catch
            {
                message = "I'm sorry, I could not interpret that as an appropriate date. Please enter a date at least an hour out.";
            }

            return false;
        }
    }
}
