using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Refinery.Core.Abstractions;
using Refinery.Core.Entities;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Refinery.Infrastructure.Ai;

public class AiRefineryGeminiService : IAiRefineryService
{
    private readonly Kernel kernel;
    private readonly IChatCompletionService chatCompletionService;
    
    public AiRefineryGeminiService(string apiKey, string modelId)
    {
        var builder = Kernel.CreateBuilder();
        builder.AddGoogleAIGeminiChatCompletion(modelId,apiKey);

        kernel = builder.Build();
        chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
    }

    public async Task<TicketAnalysisResult> RefineMailAsync(MailData mailData)
    {
        var chatHistory = new ChatHistory();

        chatHistory.AddSystemMessage("""
            Sen usta bir destek sistemi asistanısın. Gelen e-postaları analiz edip JSON formatında yanıtla.
            
            Kurallar:
            1. 'Category' alanı kesinlikle şunlardan biri olmalı: 'Technical', 'Billing', 'Sales', 'General'.
            2. 'Urgency' alanı şunlardan biri olmalı: 'Low', 'Medium', 'High'.
            3. Eğer e-postada telefon numarası veya isim eksikse 'MissingInfo' değerini true yap.
            4. Cevabın SADECE JSON formatında olsun, başka metin ekleme.
        """);

        chatHistory.AddUserMessage($"""
            Lütfen şu e-postayı analiz et:
            Gönderen: {mailData.Sender}
            Konu: {mailData.Subject}
            İçerik: {mailData.Body}
        """);

        var result = await chatCompletionService.GetChatMessageContentAsync(chatHistory);
        var content = result.Content?.Replace("```json", "").Replace("```", "").Trim() ?? "{}";

        try
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<TicketAnalysisResult>(content, options)
                   ?? new TicketAnalysisResult { MissingInfo = true, MissingInfoDetails = "AI boş JSON döndü" };
        }
        catch
        {
            return new TicketAnalysisResult
            {
                Category = "General",
                Summary = "JSON Parse Hatası: " + content.Substring(0, Math.Min(content.Length, 50)),
                Urgency = "Medium"
            };
        }
    }
}
