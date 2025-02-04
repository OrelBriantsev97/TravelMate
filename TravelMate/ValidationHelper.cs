using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace TravelMate
{
    public static class ValidationHelper
    {
        //validator for user entry , return null if pass all valdiation

        public static string UserValidation(string email, string pass, string conpass)
        {
            //checks if email and pass are not empty
            if (!isEntryValid(email) || !isEntryValid(pass) || !isEntryValid(conpass))
            {
                return "Please fill in all fields";
            }
            // Check if password meets the required strength criteria
            if (!IsPasswordValid(pass))
            {
                return "Password must be at least 8 characters, contain a number, and a letter.";
            }
            // Check if passwords match
            if (pass != conpass)
            {
                return "Passwords do not match";
            }

            return null; // Return null if all validations pass
        }

        public static string validateFlightEntries(string flightNumber,string origin,string destination)
        {
            if (!isEntryValid(flightNumber))
            {
                return  "Please enter a valid flight number";

            }
            if (!isEntryValid(origin))
            {
                return "Please select an origin";
            }
            if (!isEntryValid(destination))
            {
                return "Please select destination";
            }

            return null;
        }
        // checks if password contains at least 1 digit and 1 letter and 8 char long
        // if password valid return true
        public static bool IsPasswordValid(string password)
        {
            if (string.IsNullOrWhiteSpace(password)) return false;

            var hasMinimum8Chars = password.Length >= 8;
            var hasNumber = password.Any(char.IsDigit);
            var hasLetter = password.Any(char.IsLetter);

            return hasMinimum8Chars && hasNumber && hasLetter;
        }

        //check if entry is null or space,return true if entry is valid
        public static bool isEntryValid(string entry)
        {
            if (string.IsNullOrWhiteSpace(entry))
            {
                return false;
            }
            return true;
        }

         
    }
}
