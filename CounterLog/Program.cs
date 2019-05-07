using Serilog;
using Serilog.Core;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;

namespace CounterLog
{
    class Program
    {
        public static Logger logger;

        static void Main(string[] args)
        {
            logger = new LoggerConfiguration()
                .WriteTo
                .File("log.txt")
                .CreateLogger();

            CountLogSample();
        }

        private static void CountLogSample()
        {
            /// A operationId 
            string operationId = Guid.NewGuid().ToString();
            MethodBase currentMethod = MethodBase.GetCurrentMethod();
            string fullMethodName = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

            string methodLog = $"Method: {fullMethodName}";
            string operationMethodLog = $"Operation {operationId}: {methodLog}";

            try
            {
                /// O BeginTimedOperation gera duas linhas no arquivo de log, como as copiadas abaixo, uma no início e outra no final, durante o Dispose.
                /// Ele faz parte do Nuget SerilogMetrics, uma extensão do Serilog
                /// 2019-05-07 09:16:05.189 -03:00 [INF] Beginning operation 09081d7d-6b71-4560-970b-02d12a9fbde9: Method: CounterLog.Program.CountLogSample
                /// 2019-05-07 09:16:06.918 -03:00 [INF] Completed operation 09081d7d-6b71-4560-970b-02d12a9fbde9: Method: CounterLog.Program.CountLogSample in "00:00:01.7169046"(1716 ms)

                using (logger.BeginTimedOperation(methodLog, operationId))
                {
                    /// O Stopwatch parece ser o formato recomendado para captura de tempo de execução, caso não seja possível usar o SerilogMetrics
                    Stopwatch stopWatch = new Stopwatch();
                    stopWatch.Start();

                    Random random = new Random();

                    int sleepTime = 1000 + random.Next(1000);
                    Thread.Sleep(sleepTime);

                    stopWatch.Stop();
                    // Get the elapsed time as a TimeSpan value.
                    long executionTIme = stopWatch.ElapsedMilliseconds;

                    // Format and display the TimeSpan value.
                    Console.WriteLine("{0}, ExecutionTime: {1}", operationMethodLog, executionTIme);
                }
            }
            catch (Exception e)
            {
                /// Recomendo incluir este log em todos os métodos.
                logger.Error(e, operationMethodLog);
            }
        }
    }
}
