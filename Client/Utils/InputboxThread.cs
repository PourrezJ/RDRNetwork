using RDR2;
using RDR2.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDRNetwork.Utils
{
    internal class InputboxThread : Script
    {
        public InputboxThread()
        {
            Tick += (sender, args) =>
            {
                if (ThreadJumper.Count > 0)
                {
                    ThreadJumper.Dequeue().Invoke();
                }
            };
        }

        internal static string GetUserInput(string defaultText, Action spinner)
        {
            return GetUserInput(defaultText, 12, spinner);
        }

        internal static string GetUserInput(string defaultText, int maxLen, Action spinner)
        {
            string output = null;

            ThreadJumper.Enqueue(delegate
            {
                Function.Call(Hash.DISPLAY_ONSCREEN_KEYBOARD, 1, "", "", defaultText, "", "", "", maxLen + 1);

                while (Function.Call<int>(Hash.UPDATE_ONSCREEN_KEYBOARD) == 0)
                {
                    Yield();
                }

                if (Function.Call<int>(Hash.UPDATE_ONSCREEN_KEYBOARD) != 1)
                {
                    output = "";
                }

                output = Function.Call<string>(Hash.GET_ONSCREEN_KEYBOARD_RESULT);
            });

            Main.BlockControls = true;

            Script.Yield();

            while (output == null)
            {
                spinner.Invoke();
                Script.Yield();
            }
            Main.BlockControls = false;
            return output;
        }

        internal static string GetUserInput(int maxLen, Action spinner)
        {
            return GetUserInput("", maxLen, spinner);
        }

        internal static string GetUserInput(Action spinner)
        {
            return GetUserInput("", 40, spinner);
        }

        internal static Queue<Action> ThreadJumper = new Queue<Action>();
    }
}