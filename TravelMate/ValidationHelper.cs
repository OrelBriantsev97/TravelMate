using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace TravelMate
{
    // Provides validation utilities for user input.
    public static class ValidationHelper
    {
        /// Validates user registration input.
        /// Ensures that all fields are filled, the password meets security criteria, and passwords match. <summary>
        /// <param name="email"> is The user's email</param>
        /// <param name="pass">The user's password.</param>
        /// <param name="conpass">The password confirmation.</param>
        /// <returns>error msg if validation fails , else return null </returns>
        public static string UserValidation(string email, string pass, string conpass)
        {
            //checks if email and pass are not empty
            if (!IsEntryValid(email) || !IsEntryValid(pass) || !IsEntryValid(conpass))
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

        /// Validates user input when entering flight details.
        /// Ensures the flight number, origin, and destination are provided.
        /// <param name="flightNumber">The flight number entered by the user.</param>
        /// <param name="origin">The selected origin airport.</param>
        /// <param name="destination">The selected destination airport.</param>
        /// <returns>
        /// Returns an error message if validation fails.else,returns null.
        /// </returns>
        public static string ValidateFlightEntries(string flightNumber,string origin,string destination)
        {
            if (!IsEntryValid(flightNumber))
            {
                return  "Please enter a valid flight number";

            }
            if (!IsEntryValid(origin))
            {
                return "Please select an origin";
            }
            if (!IsEntryValid(destination))
            {
                return "Please select destination";
            }

            return null;
        }
        /// Checks whether a given password meets security standards.
        /// The password must be at least 8 characters long and contain at least one number and one letter.
        /// <param name="password">The password string to validate.</param>
        /// <returns>Returns true if the password is valid; otherwise, false.</returns>
        public static bool IsPasswordValid(string password)
        {
            if (string.IsNullOrWhiteSpace(password)) return false;

            var hasMinimum8Chars = password.Length >= 8;
            var hasNumber = password.Any(char.IsDigit);
            var hasLetter = password.Any(char.IsLetter);

            return hasMinimum8Chars && hasNumber && hasLetter;
        }

        /// Validates a user entry to ensure it is not null, empty, or whitespace.
        /// <param name="entry">The input string to validate.</param>
        /// <returns>Returns true if the entry is valid; otherwise, false.</returns>
        public static bool IsEntryValid(string entry)
        {
            if (string.IsNullOrWhiteSpace(entry))
            {
                return false;
            }
            return true;
        }

         
    }
}
