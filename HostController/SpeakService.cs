using Interfaces;
using ProcessRunnerNamespace;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HostController
{
    public class SpeakService : ISpeakService
    {
        private readonly ILogger logger;
        private readonly IConfig config;
        private readonly object locker = new object();

        private ProcessRunner speaker;
        private int exceptionsCount;
        private const int maxExceptionsCount = 5;

        public SpeakService(ILogger logger, IConfig config)
        {
            this.logger = Ensure.ArgumentIsNotNull(logger);
            this.config = Ensure.ArgumentIsNotNull(config);
        }

        public async Task Speak(string phrase)
        {
            if (exceptionsCount >= maxExceptionsCount)
                return;

            var cts = new CancellationTokenSource(10000);

            try
            {
                await Task.Run(() =>
                {
                    lock (locker)
                    {
                        if (exceptionsCount < maxExceptionsCount && EnsureSpeakProcess())
                        {
                            var data = Encoding.UTF8.GetBytes(phrase);

                            for (int i = 0; i < data.Length; ++i)
                            {
                                if (!speaker.SendToStandardInput(data[i]))
                                {
                                    i--;
                                    Thread.Sleep(100);
                                }
                            }
                        }
                        else
                        {
                            logger.Log(this, "Cannot initiate Speak process", LogLevels.Warning);
                        }

                        Thread.Sleep(1500);
                    }
                }, cts.Token);
            }
            catch (TaskCanceledException)
            {
                logger.Log(this, "Speak method timeout", LogLevels.Warning);
            }
        }

        private bool EnsureSpeakProcess()
        {
            if (speaker == null)
            {
                try
                {
                    if (speaker.HasExited)
                        ProcessRunner.TryExitEndDispose(speaker);

                    var psi = new ProcessStartInfo
                    {
                        FileName = config.GetString(ConfigNames.SpeakerExe),
                        Arguments = config.GetString(ConfigNames.SpeakerArgs),
                        RedirectStandardInput = true,
                        UseShellExecute = false
                    };

                    speaker = new ProcessRunner(psi, false, false);
                    speaker.Run();
                    exceptionsCount = 0;
                }
                catch (Exception ex)
                {
                    exceptionsCount++;
                    logger.Log(this, ex);

                    if (exceptionsCount >= maxExceptionsCount)
                    {
                        logger.Log(this, "Maximum exceptions count reached for Speaker service. Service will be disabled.", LogLevels.Error);
                    }

                    ProcessRunner.TryExitEndDispose(speaker);
                    speaker = null;

                    return false;
                }
            }

            return !speaker.HasExited;
        }

        public void Shutdown()
        {
            ProcessRunner.TryExitEndDispose(speaker);
        }
    }
}
