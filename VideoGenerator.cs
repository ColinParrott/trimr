using FFMpegCore;
using System;
using System.Threading.Tasks;

namespace trimr
{
    class VideoGenerator
    {
        public static Task<bool> GenerateVideo(Uri inputPath, Uri outputPath, TimeSpan start, TimeSpan duration, Action<double> p)
        {
            return FFMpegArguments
                .FromFileInput(inputPath.LocalPath, true, args => args
                .Seek(start))
                .OutputToFile(outputPath.LocalPath, true, options => options
                .UsingMultithreading(true)
                .WithDuration(duration))
                .NotifyOnProgress(p, duration)
                .ProcessAsynchronously();
        }
    }
}
