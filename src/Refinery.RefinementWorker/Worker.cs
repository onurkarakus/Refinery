using Refinery.Core.Abstractions;
using Refinery.Core.Entities;
using Refinery.Infrastructure.Data;

namespace Refinery.RefinementWorker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> logger;
        private readonly IRedisStreamService redisStreamService;
        private readonly IAiRefineryService aiRefineryService;
        private readonly IServiceScopeFactory serviceScopeFactory;

        private const string StreamKey = "ticket_emails";
        private const string ConsumerGroup = "ai_processors";
        private const string ConsumerName = "worker_1";


        public Worker(ILogger<Worker> logger,
            IRedisStreamService redisStreamService,
            IAiRefineryService aiRefineryService,
            IServiceScopeFactory serviceScopeFactory)
        {
            this.logger = logger;
            this.redisStreamService = redisStreamService;
            this.aiRefineryService = aiRefineryService;
            this.serviceScopeFactory = serviceScopeFactory;
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

                using (var scope = serviceScopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<RefineryDbContext>();

                    var ticket = new Ticket
                    {
                        Sender = mailData.Sender,
                        Subject = mailData.Subject,
                        Body = mailData.Body,
                        Category = analysis.Category,
                        Urgency = analysis.Urgency,
                        Summary = analysis.Summary,
                        MissingInfo = analysis.MissingInfo,
                        MissingInfoDetails = analysis.MissingInfoDetails,
                        CreatedAt = DateTime.UtcNow,
                        AiSentiment = analysis.Sentiment,
                        Status = "New"
                    };

                    dbContext.Tickets.Add(ticket);
                    await dbContext.SaveChangesAsync();

                    logger.LogInformation("------------------------------------------------");
                    logger.LogInformation($"[KAYIT BAÞARILI] Ticket ID: {ticket.Id}");
                    logger.LogInformation($"Kategori: {ticket.Category} | Aciliyet: {ticket.Urgency}");
                    logger.LogInformation("------------------------------------------------");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "ProcessMessage Ýþlemi sýrasýnda hata oluþtu!");
                throw;
            }


        }
    }
}
