// �ۼ���: Sangchan, Kim
// �ۼ���: 2025-03-31
// ���: ���ø����̼� ������ �� ���� ����
// ����: ������ ����, �����ͺ��̽� ����, �̵���� �� ���ø����̼� ������ �����մϴ�.

using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SMSAlarmSystem.Core.Interfaces;
using SMSAlarmSystem.Helpers;
using SMSAlarmSystem.Infrastructure.Data;
using SMSAlarmSystem.Infrastructure.Repositories;
using SMSAlarmSystem.Services.Interfaces;
using SMSAlarmSystem.Services.Services;
using System;

namespace SMSAlarmSystem
{
    /// <summary>
    /// ���ø����̼��� �������� �����ϴ� Ŭ����
    /// </summary>
    public class Program
    {
        /// <summary>
        /// ���ø����̼��� ������ �޼���
        /// </summary>
        /// <param name="args">����� �μ�</param>
        public static void Main(string[] args)
        {
            try
            {
                var builder = WebApplication.CreateBuilder(args);

                // ���� ����
                ConfigureServices(builder);

                var app = builder.Build();

                // ���ø����̼� ���������� ����
                ConfigureMiddleware(app);

                // ���ø����̼� ����
                app.Run();
            }
            catch (Exception ex)
            {
                // ���ø����̼� ���� �� �߻��� ���� �α�
                Console.WriteLine($"���ø����̼� ���� �� ���� �߻�: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// ���ø����̼� ���� ����
        /// </summary>
        /// <param name="builder">�� ���ø����̼� ����</param>
        private static void ConfigureServices(WebApplicationBuilder builder)
        {
            // �����ͺ��̽� ���ؽ�Ʈ ��� - MSSQL ���� ���� ����
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new InvalidOperationException("�����ͺ��̽� ���� ���ڿ��� �������� �ʾҽ��ϴ�. appsettings.json ������ Ȯ���ϼ���.");
                }
                options.UseSqlServer(connectionString);
            });

            // �������丮 ���� ��� - ������ ������ ���� �������̽��� ����ü ����
            RegisterRepositories(builder.Services);

            // ���� ���� ��� - ����Ͻ� ���� ó���� ���� ���񽺵�
            RegisterServices(builder.Services, builder.Configuration);

            // MVC �� Razor Pages �߰� - �� UI ������ ���� �����ӿ�ũ ���
            builder.Services.AddControllersWithViews();
            builder.Services.AddRazorPages();
        }

        /// <summary>
        /// �������丮 ���� ���
        /// </summary>
        /// <param name="services">���� �÷���</param>
        private static void RegisterRepositories(IServiceCollection services)
        {
            // �� ������ �𵨿� ���� �������丮 ���
            services.AddScoped<IMemberRepository, MemberRepository>();             // ȸ�� �������丮
            services.AddScoped<IMessageGroupRepository, MessageGroupRepository>(); // �޽��� �׷� �������丮
            services.AddScoped<IAlarmPointRepository, AlarmPointRepository>();     // �˶� ����Ʈ �������丮
            services.AddScoped<IMessageRepository, MessageRepository>();           // �޽��� �������丮
        }

        /// <summary>
        /// ���� ���� ���
        /// </summary>
        /// <param name="services">���� �÷���</param>
        /// <param name="configuration">���ø����̼� ����</param>
        private static void RegisterServices(IServiceCollection services, IConfiguration configuration)
        {
            // HttpClient ��� - SMS ���񽺿��� �ܺ� API ȣ�⿡ ���
            services.AddHttpClient();

            // SMS ���� ��� - �������̽��� ����ü ���� (������ �κ�)
            services.AddSingleton<ISMSService>(provider => {
                var httpClient = provider.GetRequiredService<IHttpClientFactory>().CreateClient();
                var logger = provider.GetRequiredService<ILogger<SMSService>>();

                // �������� API Ű�� URL �������� (null üũ ����)
                var apiKey = configuration["SMSService:ApiKey"];
                if (string.IsNullOrEmpty(apiKey))
                {
                    logger.LogWarning("SMS API Ű�� �������� �ʾҽ��ϴ�. �׽�Ʈ Ű�� ����մϴ�.");
                    apiKey = "test-api-key";
                }

                var apiUrl = configuration["SMSService:ApiUrl"];
                if (string.IsNullOrEmpty(apiUrl))
                {
                    logger.LogWarning("SMS API URL�� �������� �ʾҽ��ϴ�. �׽�Ʈ URL�� ����մϴ�.");
                    apiUrl = "https://test-sms-api.com";
                }

                // �׽�Ʈ ��� ���� (�⺻��: true)
                var useTestMode = configuration.GetValue<bool>("SMSService:UseTestMode", true);

                logger.LogInformation("SMS ���� ����: API URL={ApiUrl}, �׽�Ʈ ���={TestMode}", apiUrl, useTestMode);

                return new SMSService(httpClient, apiKey, apiUrl, logger, useTestMode);
            });

            // ����Ͻ� ���� ���� ���
            services.AddScoped<IMemberService, MemberService>();                   // ȸ�� ���� ����
            services.AddScoped<IMessageGroupService, MessageGroupService>();       // �޽��� �׷� ���� ����
            services.AddScoped<IAlarmService, AlarmService>();                     // �˶� ����Ʈ �� �˶� Ʈ���� ����
            services.AddScoped<IMessageService, MessageService>();                 // �޽��� ����
            services.AddScoped(typeof(PaginationHelper<>));                         // ���������̼� ���� ���
        }

        /// <summary>
        /// ���ø����̼� �̵���� ����
        /// </summary>
        /// <param name="app">�� ���ø����̼�</param>
        private static void ConfigureMiddleware(WebApplication app)
        {
            // ���� ȯ�� ���� - ȯ�濡 ���� ���� ó�� ��� ����
            if (app.Environment.IsDevelopment())
            {
                // ���� ȯ�濡���� ���� ���� ���� ǥ��
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // ���δ��� ȯ�濡���� �Ϲ����� ���� �������� �����̷�Ʈ
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();  // HTTP Strict Transport Security ����
            }

            // HTTPS �����̷��� - ���� ��ȭ�� ���� HTTP ��û�� HTTPS�� �����̷�Ʈ
            app.UseHttpsRedirection();

            // ���� ���� ���� - CSS, JavaScript, �̹��� ���� ���� ���� ����
            app.UseStaticFiles();

            // ����� �̵���� ��� - URL ���Ͽ� ���� ��û ó��
            app.UseRouting();

            // ���� �� ���� �ο� �̵���� ���
            app.UseAuthorization();

            // ��Ʈ�ѷ� ���Ʈ ���� - �⺻ ����� ���� ����
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
        }
    }
}
