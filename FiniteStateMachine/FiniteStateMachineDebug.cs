using System;
using System.Text.RegularExpressions;

namespace BennyBroseph
{
    // Part of the class which allows for testing
    // Separated for organizational reasons since this has nothing to do with using the class
    // This file is not required, but is recommended
    public sealed partial class FiniteStateMachine<T>
    {
        /// <summary>
        /// Attempts to access a debugging messenger. Will do nothing if it cannot be found
        /// </summary>
        /// <param name="a_Type">The string representing the type of message to display</param>
        /// <param name="a_Message">The message to display</param>
        partial void Debug(string a_Type, object a_Message)
        {
#if CONTEXT_DEBUG   // Only compiles if the build is using the 'ContextualDebug' by defining it in the build options
            a_Type = a_Type.ToUpper();
            switch (a_Type)
            {
                case "M":
                case "MESSAGE": ContextualDebug.DebugMessage(a_Message); break;
                case "W":
                case "WARNING": ContextualDebug.DebugWarning(a_Message); break;
                case "E":
                case "ERROR": ContextualDebug.DebugError(a_Message); break;
                default: break;
            }
#elif (!UNITY_EDITOR && DEBUG) // Only compiles when in debug mode and not in unity
            Console.WriteLine(a_Message);
#endif
        }

        /// <summary>
        /// Test function to try out the Finite State Machine
        /// </summary>
#if (!UNITY_EDITOR && DEBUG)   // Only compiles when in debug mode and not in unity
        public void TestMachine()
        {
            Console.WriteLine("Entering test mode...\n");
            Console.WriteLine("Keep in mind that the amount of time shown to be elapsed is only counting the Finite State Machine's function calls");
            Console.WriteLine("Keywords like '(dynamic)' that are used when converting integer values to type 'T' severely slow down computation,\nbut are not used in the actual State Machine code\n");
            Console.WriteLine("Generally, functions compute faster using integer representations of states and slower using strings even though it may seem otherwise.\n\n");

            bool run = true;
            string input;

            while (run)
            {
                Console.WriteLine("CurrentState: " + m_CurrentState); Console.WriteLine();
                Console.WriteLine("What would you like to do?\n");
                Console.WriteLine("1 - Add Transition");
                Console.WriteLine("2 - Check Transitions");
                Console.WriteLine("3 - Change State");
                Console.WriteLine("4 - Check States");
                Console.WriteLine("5 - Exit\n");

                Console.Write(">> "); input = Console.ReadLine();
                Console.Clear();
                switch (input)
                {
                    case "1":
                        {
                            DateTime oldTime = DateTime.Today;
                            while (true)
                            {
                                Console.WriteLine("Enter the first state followed by '->' and then the second state");
                                Console.WriteLine("OR Enter the number of the first state followed by ',' and then the second state\n");
                                PrintStates(); Console.WriteLine();

                                Console.Write(">> "); input = Console.ReadLine();
                                Console.Clear();
                                if (input.IndexOf(",") >= 0)
                                {
                                    input.Trim();

                                    try
                                    {
                                        T[] states = {
                                            (T)(dynamic)Convert.ToInt32(input.Substring(0, input.IndexOf(","))),
                                            (T)(dynamic)Convert.ToInt32(input.Substring(input.LastIndexOf(",") + 1))};

                                        oldTime = DateTime.Now;
                                        if (AddTransition(states[0], states[1]))
                                            Console.WriteLine("Valid transition added");

                                        break;
                                    }
                                    catch (FormatException)
                                    {
                                        Console.WriteLine("'" + input + "' is not at all what I asked for. Try again\n");
                                    }
                                }
                                else
                                {
                                    oldTime = DateTime.Now;
                                    if (AddTransition(input))
                                        Console.WriteLine("Valid transition added");
                                                                        
                                    break;
                                }
                            }
                            if (oldTime != DateTime.Today)
                                Console.WriteLine("Time elapsed: {0} ms",(DateTime.Now - oldTime).TotalMilliseconds);

                        }
                        break;
                    case "2":
                        {
                            PrintTransitions(); Console.WriteLine();

                            Console.Write(">> "); input = Console.ReadLine();
                            Console.Clear();
                        }
                        break;
                    case "3":
                        {
                            DateTime oldTime = DateTime.Today;
                            while (true)
                            {
                                Console.WriteLine("Enter the by name OR by number\n");
                                PrintStates(); Console.WriteLine();

                                Console.Write(">> "); input = Console.ReadLine();
                                Console.Clear();

                                Regex alphaText = new Regex(@"[A-Za-z]+");
                                if (alphaText.IsMatch(input))
                                {
                                    oldTime = DateTime.Now;
                                    if (Transition(input))
                                        Console.WriteLine("Valid state transition");
                                    else
                                        Console.WriteLine("Could not transition to requested state");

                                    break;
                                }
                                else
                                {
                                    try
                                    {
                                        T state = (T)(dynamic)Convert.ToInt32(input);

                                        oldTime = DateTime.Now;
                                        if (Transition(state))
                                            Console.WriteLine("Valid state transition");
                                        else
                                            Console.WriteLine("Could not transition to requested state");
                                        break;
                                    }
                                    catch (FormatException)
                                    {
                                        Console.WriteLine("'" + input + "' is not at all what I asked for. Try again\n");
                                    }
                                }
                            }
                            if (oldTime != DateTime.Today)
                                Console.WriteLine("Time elapsed: {0} ms", (DateTime.Now - oldTime).TotalMilliseconds);
                        }
                        break;
                    case "4":
                        {
                            PrintStates(); Console.WriteLine();

                            Console.Write(">> "); input = Console.ReadLine();
                            Console.Clear();
                        }
                        break;
                    default: run = false; break;
                }
                Console.WriteLine();
            }
            Console.Clear();
            Console.WriteLine("As you can see, string values are slower to compute since they must be validated and parsed. ");
            Console.WriteLine("It's better and faster to only use valid states of type 'T'\n");
            Console.ReadLine();
        }
#endif 
    }
}