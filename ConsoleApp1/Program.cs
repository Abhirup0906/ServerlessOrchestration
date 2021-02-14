using System;
using System.Linq;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            int testCases = Convert.ToInt32("1");
            int totalTime = 360;
            int solveTime = 3;

            if (testCases > 100 || testCases<1) Console.WriteLine("0");
            for (int test = 0; test < testCases; test++)
            {
                string[] input = "10 333".Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                int numOfProb = Convert.ToInt32(input[0]);
                int timeToReach = Convert.ToInt32(input[1]);                

                if (totalTime <= timeToReach || (totalTime - timeToReach) < solveTime || numOfProb>15 || numOfProb<1)
                {
                    Console.WriteLine("0");
                    continue;
                }

                if ((timeToReach + (numOfProb * solveTime)) <= totalTime)
                {
                    Console.WriteLine(numOfProb);
                    continue;
                }
                else
                {
                    while(numOfProb>0 && (timeToReach + (numOfProb * solveTime))>=totalTime)
                    {
                        numOfProb = numOfProb - 1;
                    }
                    Console.WriteLine(numOfProb);
                    Console.WriteLine((timeToReach + (numOfProb * solveTime)));
                    //int question;
                    //for (question = 1; question <= numOfProb; question++)
                    //{
                    //    int actualTime = (timeToReach + (question * solveTime));
                    //    if (totalTime <= actualTime || (totalTime - timeToReach) < 3) break;
                    //}
                    //Console.WriteLine((question > numOfProb) ? question - 1 : question);
                }
            }

            Console.ReadLine();
        }
    }
}
