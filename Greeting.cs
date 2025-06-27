
using System.Windows.Media;

namespace POE_Draft
{
    public class Greeting
    {
        public Greeting()
        {
            greet();
        }

        public void greet()
        {
            //Creating an instance for the Voice Greeting
            MediaPlayer voiceGreet = new MediaPlayer();

            //Get the full path automatically
            String fullPath = AppDomain.CurrentDomain.BaseDirectory;

            //Replace the \\bin\\Debug\\net8.0-windows
            String replaced = fullPath.Replace("\\bin\\Debug\\net8.0-windows","");

            //Combine the paths after replacing
            String combinePath = System.IO.Path.Combine(replaced, "0330.wav");

            //Combine the URL as URI
            voiceGreet.Open(new Uri(combinePath, UriKind.Relative));

            //Play the audio
            voiceGreet.Play();
        }
    }
}