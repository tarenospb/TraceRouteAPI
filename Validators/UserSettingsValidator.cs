using FluentValidation.AspNetCore;
using FluentValidation;
using TraceRouteApi.Settings;
using System;

namespace TraceRouteApi.Validators
{
    public class UserSettingsValidator : AbstractValidator<UserSettings>
    {
        private BrokenMessages _messages;


        public UserSettingsValidator(BrokenMessages messages)
        {
            _messages = messages;
            RuleFor(s => s.fromCity)
                .NotEmpty()
                .WithMessage(_messages.WrongCityMsg);
            RuleFor(s => s.radius)
                .NotEmpty()
                .InclusiveBetween(1, 40)
                .WithMessage(_messages.WrongRadMsg);
            RuleFor(s => s.toCity)
                .NotEmpty()
                .WithMessage(_messages.WrongCityMsg);
            RuleFor(s => s.date)
                .NotEmpty()
                .Length(10)
                .Must(date => ValidateDate(date))
                .WithMessage(_messages.WrongDateMsg);
            RuleFor(s => s.transportType)
                .NotEmpty()
                .NotNull()
                .Must(type => ValidateTransportType(type))
                .WithMessage(_messages.WrongTransportTypeMsg);
        }

        private bool ValidateTransportType(string s)
        {
            if (!string.IsNullOrEmpty(s))
            {
                var list = s.Split(',');
                var correctRec = 0;
                foreach (var i in list) 
                {
                    if (i == "bus" || i == "train" || i == "plane"  || i == "all") 
                    {
                        correctRec++;
                    }
                }
                if (correctRec == list.Length)
                {
                    if (s.Contains("all") && s.Length != 3)
                    {
                        return false;
                    }
                    return true;

                }
                else return false;
            }
            else return false;
        }


        private bool ValidateDate(string s)
        {
            var i = 0;
            bool valid = true;
            if (!string.IsNullOrEmpty(s) && s.Length == 10)
            {
                valid &= (Int32.TryParse(s.Substring(0, 2), out i));
                if (i > 31 || i < 1) return false;
                valid &= (Int32.TryParse(s.Substring(3, 2), out i));
                if (i > 12 || i < 1) return false;
                valid &= (Int32.TryParse(s.Substring(6, 4), out i));
                if (i < 2022) return false;
                valid &= (s[2] == '.');
                valid &= (s[5] == '.');
                return valid;
            }
            else return false;
        }
    } 
}
