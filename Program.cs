using System;
using System.Threading;

namespace ideal
{
    class Program
    {
        // The bias between self-satisfaction and disengagement. Default is 4.
        static int boredomLevel = 4;
        static int whirlDuration = 20;
        static int learningPace = 1000;
        static bool enhancedExistence = true;
        static void Main()
        {
            Console.WriteLine("Set bordedom level of " + boredomLevel + " .");
            Console.WriteLine("Starting the experiment...");
            RunExperiment();
        }
        
        static void RunExperiment()
        {
            // Create an existence with a variable boredom level.
            //Existence existence = new Existence(boredomLevel);
            // Create an advanced existence to leverage Level 03 and above.
            Existence existence = new Existence(enhancedExistence);
            existence.MessageToPass += MessagesToConsole;

            for (int i = 0; i < whirlDuration; i++)
            {
                string stepTrace = existence.Step();
                Console.WriteLine(i + ": " + stepTrace);
                Thread.Sleep(learningPace);
            }
        }

        static void MessagesToConsole(object sender, MessageDelegateEventArgs e)
        {
            Console.WriteLine(e.Message);
        }

    }
}
