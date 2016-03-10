using System;                           // Required for the type 'Enum'
using System.Collections.Generic;       // Required to use 'List<T>' and 'Dictionary<T, T>'
using System.Text.RegularExpressions;   // Required to use Regular Expressions aka. 'Regex'

namespace BennyBroseph
{
    // Usage:
    // enum MyStates { Init, Idle };
    // FiniteStateMachine<MyStates> MyFSM = new FiniteStateMachine<MyStates>(OPTIONAL: MyStates.Idle);
    /// <summary>
    /// My Finite State Machine
    /// </summary>
    /// <typeparam name="T">A 'System.Type' in which 'T.IsEnum()' is true</typeparam>
    [Serializable]
    public sealed partial class FiniteStateMachine<T>
    {
        public delegate bool ValidateTransition();  // Delegate that will be used to determine if a state change is valid by the user

        private T m_CurrentState;   // The current state the machine is in
        private List<T> m_States;   // Cached list of all states in the enumeration

        private Dictionary<string, ValidateTransition> m_Transitions;   // Dynamic dictionary of all transitions as dictated by the user

        // Read-Only property for the current state 'm_CurrentState'
        // Look at me. I'm the captain now.
        public T CurrentState { get { return m_CurrentState; } }

        /// <summary>
        /// Default constructor which will initialize the list and dictionary
        /// </summary>
        public FiniteStateMachine()
        {
            m_States = new List<T>();
            m_Transitions = new Dictionary<string, ValidateTransition>();

            StoreStates();
        }
        /// <summary>
        /// Parameterized constructor which allows a state other than 'm_States[0]' to initialize 'm_CurrentState'
        /// </summary>
        /// <param name="a_InitialState">Used as the current state 'm_CurrentState' on creation</param>
        public FiniteStateMachine(T a_InitialState) : base()
        {
            m_CurrentState = a_InitialState;
        }

        /// <summary>
        /// Attempts to add a new transition to the current list of transitions
        /// </summary>
        /// <param name="a_From">The state to come from</param>
        /// <param name="a_To">The state to go to</param>
        /// <param name="a_IsValidTransition">An optional delegate with no parameters that returns true when the state change is valid and false when it is not</param>
        /// <returns>Returns true if the transition was able to be added and false otherwise</returns>
        public bool AddTransition(T a_From, T a_To, ValidateTransition a_IsValidTransition = null)
        {
            // if 'a_From' and 'a_To' are the same state
            if (a_From.Equals(a_To))
            {
                Debug("Warning", "'" + a_From + "'" + " is the same state as " + "'" + a_To + "'");
                return false;
            }

            // if 'a_From' or 'a_To' is not in the list of states
            if (!m_States.Contains(a_From) || !m_States.Contains(a_To))
            {
                T invalidKey;   // Will decipher which state is invalid
                if (!m_States.Contains(a_From))
                    invalidKey = a_From;
                else
                    invalidKey = a_To;

                Debug("Warning", "'" + invalidKey + "' does not exist in '" + typeof(T) + "'");
                return false;
            }

            // Properly serializes 'a_From' and 'a_To' into the expected key format
            string key = a_From.ToString() + "->" + a_To.ToString();
            // if the key 'key' does not currently exist in 'm_Transitions'
            if (!m_Transitions.ContainsKey(key))
            {
                // if the user did not pass in a delegate to check the transition
                if (a_IsValidTransition == null)
                    m_Transitions[key] = delegate () { return true; };  // Set a default one that always allows the transition
                else
                    m_Transitions[key] = a_IsValidTransition;           // Otherwise use the one they passed in
                return true;
            }
            else
            {
                Debug("Warning", "'" + key + "' already exists as a transition key");
                return false;
            }
        }
        /// <summary>
        /// Attempts to add a new transition to the current list of transitions
        /// </summary>
        /// <param name="a_Key">The string value that represents the transitions key in the dictionary 'm_Transitions'. Excepted to be formatted 'FROM_STATE->TO_STATE'</param>
        /// <param name="a_IsValidTransition"></param>
        /// <returns>Returns true if the transition was able to be added and false otherwise</returns>
        public bool AddTransition(string a_Key, ValidateTransition a_IsValidTransition = null)
        {
            T[] parsedStates = ParseTransitionStates(a_Key);    // Will attempt to turn the string into two states

            // if the parse was successful
            if (parsedStates != null)
                return AddTransition(parsedStates[0], parsedStates[1], a_IsValidTransition);    // Send it along to be put into the dictionary
            else
                return false;
        }

        /// <summary>
        /// Attempts to transition from the current state to the passed parameter
        /// </summary>
        /// <param name="a_To">The state to transition to</param>
        /// <returns>Returns true if the transition completed and false otherwise</returns>
        public bool Transition(T a_To)
        {
            // Converts the current state and the state to transition to into a valid key
            string key = m_CurrentState.ToString() + "->" + a_To.ToString();
            // if they key exists in the transition dictionary
            if (m_Transitions.ContainsKey(key) && m_Transitions[key]())
            {
                m_CurrentState = a_To;  // Set the state
                return true;            // Success
            }
            else
                return false;
        }
        /// <summary>
        /// Attempts to transition from the current state to the passed parameter
        /// </summary>
        /// <param name="a_To">The string value that represents the state to transition to</param>
        /// <returns>Returns true if the transition completed and false otherwise</returns>
        public bool Transition(string a_To)
        {
            try
            {
                return Transition((T)Enum.Parse(typeof(T), a_To)); // Send it along to be transitioned to
            }
            catch (ArgumentException)
            {
                Debug("Error", "'" + a_To + "' does not parse to a valid state");
            }
            return false;
        }

        /// <summary>
        /// Prints the cached states in the format:
        /// ORDER - STATE
        /// </summary>
        public void PrintStates()
        {
            for (var i = 0; i < m_States.Count; ++i)
                Console.WriteLine(i + " - " + m_States[i].ToString());
        }
        /// <summary>
        /// Prints the currently defined transitions int the format:
        /// ORDER - STATE_FROM->STATE_TO
        /// </summary>
        public void PrintTransitions()
        {
            var i = 0;
            foreach (var iPair in m_Transitions)
            {
                Console.WriteLine(i + " - " + iPair.Key.ToString());
                i++;
            }
        }

        /// <summary>
        /// Grabs each state from the type of enumeration and caches it into a list
        /// </summary>
        /// <returns>Returns true if the type is an enumeration and false if it is not</returns>
        private bool StoreStates()
        {
            // if 'T' is an enumeration type
            if (typeof(T).IsEnum)
            {
                // Iterate through each parsed state 'iState'
                foreach (var iState in Enum.GetValues(typeof(T)))
                    m_States.Add((T)iState);    // Cache it

                m_CurrentState = m_States[0];   // Set the current state to the first found state
                return true;
            }
            else
            {
                Debug("Error", "Incorrect type '" + typeof(T) + "'");
                return false;
            }
        }

        /// <summary>
        /// Attempts to parse two states from a given string
        /// </summary>
        /// <param name="a_States">The string to be parsed</param>
        /// <returns>Returns non-null if two states were able to be parsed or null if it was not parsable</returns>
        private T[] ParseTransitionStates(string a_States)
        {
            // if the states are not in the correct format and could not be salvaged
            if (!ValidateKey(ref a_States))
                return null;

            // Parses the two states in the proper format
            string[] parsedStatesString = {
                a_States.Substring(0, a_States.IndexOf("-")),
                a_States.Substring(a_States.IndexOf(">") + 1) };

            T[] parsedStates; // Holder for the parsed states
            try // Attempts to parse each state from the enumeration
            {
                parsedStates = new T[]{
                    (T)Enum.Parse(typeof(T), parsedStatesString[0]),
                    (T)Enum.Parse(typeof(T), parsedStatesString[1]) };
            }
            // Catch the thrown exception and return null
            catch (ArgumentException)
            {
                Debug("Error", "'" + a_States + "' does not parse to valid states");
                return null;
            }

            return parsedStates;    // Success
        }

        /// <summary>
        /// Validates that a key is in the proper format or otherwise passes it on to be fixed
        /// </summary>
        /// <param name="a_Key">The string value to be validated or fixed</param>
        /// <returns>Returns true if the string is in a valid format</returns>
        private bool ValidateKey(ref string a_Key)
        {
            // Defines a format of "{0}->{1}" where {0}, {1} : From, To
            // "From->To"
            var keyFormat = new Regex(@"^([A-Za-z]{1,}[A-Za-z0-9]*)\-\>(([A-Za-z]{1,}[A-Za-z0-9]*))");
            string oldKey = a_Key;  // Stores the original key value 
            // if the key matches the defined format
            if (keyFormat.IsMatch(a_Key))
                return true;
            else
            {
                // if the key had removable garbage
                if ((a_Key = ParseGarbageKey(a_Key)) != null)
                {
                    // if the key now matches the defined format
                    if (keyFormat.IsMatch(a_Key))
                        return true;
                    else
                    {
                        Debug("Error", "The key '" + oldKey + "' could not be salvaged. Result: '" + a_Key + "'");   // Debug old key
                        return false;
                    }
                }
                else
                    return false;
            }
        }
        /// <summary>
        /// Attempts to remove garbage symbols from the passed string
        /// </summary>
        /// <param name="a_Key">String to be parsed into a valid key</param>
        /// <returns>Returns true if the key had removable garbage or false if it did not</returns>
        private string ParseGarbageKey(string a_Key)
        {
            Debug("Warning", "Key '" + a_Key + "' was not in the correct format. Attempting workaround");   // Feedback to the programmer

            var PossibleGarbage = "!@#$%^&*()-_=+;:'\",<.>/?\\|[{]} ";  // A string containing possible garbage symbols
            int GarbageStart;   // Holder for when a garbage symbol is first found
            // if no garbage exists in the key
            if ((GarbageStart = a_Key.IndexOfAny(PossibleGarbage.ToCharArray())) < 0)
            {
                Debug("Error", "'" + a_Key + "' does not have garbage to remove");  // Couldn't fix it coach
                return null;
            }

            string EnumFrom = a_Key.Substring(0, GarbageStart); // The parsed STATE_FROM
            // Start the substring at the last garbage symbol found and stop at the end
            string EnumTo = a_Key.Substring(a_Key.LastIndexOfAny(PossibleGarbage.ToCharArray()) + 1);   // The parsed STATE_TO
            return EnumFrom + "->" + EnumTo;    // Put it back into an acceptable format
        }

        /// <summary>
        /// Declaration for the 'debug' method. Does not affect code if it is not defined aka. C# magic
        /// Attempts to access a debugging messenger. Will do nothing if it cannot be found
        /// </summary>
        /// <param name="a_Type">The string representing the type of message to display</param>
        /// <param name="a_Message">The message to display</param>
        partial void Debug(string a_Type, object a_Message);
    }
}
