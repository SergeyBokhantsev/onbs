using Interfaces;
using System;
using System.Collections.Generic;
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
        private readonly IProcessRunnerFactory prf;
        private readonly object locker = new object();

        private IProcessRunner speaker;
        private int exceptionsCount;
        private const int maxExceptionsCount = 5;

        public SpeakService(ILogger logger, IConfig config, IProcessRunnerFactory prf)
        {
            this.logger = Ensure.ArgumentIsNotNull(logger);
            this.config = Ensure.ArgumentIsNotNull(config);
            this.prf = Ensure.ArgumentIsNotNull(prf);
        }

        public async Task Speak(string phrase)
        {
            if (exceptionsCount >= maxExceptionsCount)
                return;

            await Task.Run(() =>
            {
                lock (locker)
                {
                    if (exceptionsCount < maxExceptionsCount && EnsureSpeakProcess())
                    {
                        speaker.SendToStandardInput(phrase);
                    }
                    else
                    {
                        logger.Log(this, "Cannot initiate Speak process", LogLevels.Warning);
                    }
                }
            });
        }

        private bool EnsureSpeakProcess()
        {
            if (speaker == null || speaker.HasExited)
            {
                try
                {
                    var pc = new ProcessConfig
                    {
                        ExePath = config.GetString(ConfigNames.SpeakerExe),
                        Args = config.GetString(ConfigNames.SpeakerArgs),
                        RedirectStandardInput = true,
                        RedirectStandardOutput = false
                    };

                    speaker = prf.Create(pc);
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

                    return false;
                }
            }

            return !speaker.HasExited;
        }
    }
}
