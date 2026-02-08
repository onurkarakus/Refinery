using Refinery.Core.Abstractions;
using Refinery.Core.Entities;

namespace Refinery.RefinementWorker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> logger;
        private readonly IRedisStreamService redisStreamService;
        private readonly IAiRefineryService aiRefineryService;

        private const string StreamKey = "ticket_emails";
        private const string ConsumerGroup = "ai_processors";
        private const string ConsumerName = "worker_1";


        public Worker(ILogger<Worker> logger, IRedisStreamService redisStreamService, IAiRefineryService aiRefineryService)
        {
            this.logger = logger;
            this.redisStreamService = redisStreamService;
            this.aiRefineryService = aiRefineryService;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await redisStreamService.CreateGroupAsync(StreamKey, ConsumerGroup);
            await base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Refinement Worker Baþladý. Stream dinleniyor...");

            while (!stoppingToken.IsCancellationRequested)
            {
                await redisStreamService.ConsumeStreamAsync(StreamKey, ConsumerGroup, ConsumerName, ProcessMessage, stoppingToken);

                await Task.Delay(100, stoppingToken);
            }
        }

        private async Task ProcessMessage(MailData mailData)
        {
            logger.LogInformation($"[MAIL ALINDI] Gönderen: {mailData.Sender} | Konu: {mailData.Subject}");

            try
            {
                var analysis = await aiRefineryService.RefineMailAsync(mailData);

                logger.LogInformation("------------------------------------------------");
                logger.LogInformation($"[AI ANALÝZÝ TAMAMLANDI]");
                logger.LogInformation($"Kategori : {analysis.Category}");
                logger.LogInformation($"Aciliyet : {analysis.Urgency}");
                logger.LogInformation($"Özet     : {analysis.Summary}");
                logger.LogInformation($"Eksik Bilgi: {analysis.MissingInfo} ({analysis.MissingInfoDetails})");
                logger.LogInformation("------------------------------------------------");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "AI Ýþlemi sýrasýnda hata oluþtu!");
                throw;
            }


        }
    }
}
