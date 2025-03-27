using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SMSAlarmSystem.Core.Interfaces;
using SMSAlarmSystem.Infrastructure.Data;
using SMSAlarmSystem.Infrastructure.Repositories;
using SMSAlarmSystem.Infrastructure.Services;
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
            var builder = WebApplication.CreateBuilder(args);

            // �����ͺ��̽� ���ؽ�Ʈ ��� - MSSQL ���� ���� ����
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // �������丮 ���� ��� - ������ ������ ���� �������̽��� ����ü ����
            builder.Services.AddScoped<IMemberRepository, MemberRepository>();             // ȸ�� �������丮
            builder.Services.AddScoped<IMessageGroupRepository, MessageGroupRepository>(); // �޽��� �׷� �������丮
            builder.Services.AddScoped<IAlarmPointRepository, AlarmPointRepository>();     // �˶� ����Ʈ �������丮
            builder.Services.AddScoped<IMessageRepository, MessageRepository>();           // �޽��� �������丮

            // HttpClient ��� - SMS ���񽺿��� �ܺ� API ȣ�⿡ ���
            builder.Services.AddHttpClient();

            // SMS ���� ��� - �׽�Ʈ ��� Ȱ��ȭ (���� ��)
            builder.Services.AddSingleton<ISMSService>(provider => {
                var httpClient = provider.GetRequiredService<IHttpClientFactory>().CreateClient();
                var apiKey = builder.Configuration["SMSService:ApiKey"] ?? "test-api-key";
                var apiUrl = builder.Configuration["SMSService:ApiUrl"] ?? "https://test-sms-api.com";
                var logger = provider.GetRequiredService<ILogger<SMSService>>();
                return new SMSService(httpClient, apiKey, apiUrl, logger, true);
            });

            // ����Ͻ� ���� ���� ��� - ����Ͻ� ���� ó���� ���� ���񽺵�
            builder.Services.AddScoped<MemberService>();           // ȸ�� ���� ����
            builder.Services.AddScoped<MessageGroupService>();     // �޽��� �׷� ���� ����
            builder.Services.AddScoped<AlarmService>();            // �˶� ����Ʈ �� �˶� Ʈ���� ����
            builder.Services.AddScoped<MessageService>();          // �޽��� ���� ����

            // MVC �� Razor Pages �߰� - �� UI ������ ���� �����ӿ�ũ ���
            builder.Services.AddControllersWithViews();
            builder.Services.AddRazorPages();

            var app = builder.Build();

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

            // ���ø����̼� ����
            app.Run();
        }
    }
}
