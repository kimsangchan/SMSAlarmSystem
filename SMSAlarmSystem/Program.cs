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
    /// 애플리케이션의 시작점을 정의하는 클래스
    /// </summary>
    public class Program
    {
        /// <summary>
        /// 애플리케이션의 진입점 메서드
        /// </summary>
        /// <param name="args">명령줄 인수</param>
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 데이터베이스 컨텍스트 등록 - MSSQL 서버 연결 설정
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // 리포지토리 계층 등록 - 의존성 주입을 위한 인터페이스와 구현체 매핑
            builder.Services.AddScoped<IMemberRepository, MemberRepository>();             // 회원 리포지토리
            builder.Services.AddScoped<IMessageGroupRepository, MessageGroupRepository>(); // 메시지 그룹 리포지토리
            builder.Services.AddScoped<IAlarmPointRepository, AlarmPointRepository>();     // 알람 포인트 리포지토리
            builder.Services.AddScoped<IMessageRepository, MessageRepository>();           // 메시지 리포지토리

            // HttpClient 등록 - SMS 서비스에서 외부 API 호출에 사용
            builder.Services.AddHttpClient();

            // SMS 서비스 등록 - 테스트 모드 활성화 (수정 후)
            builder.Services.AddSingleton<ISMSService>(provider => {
                var httpClient = provider.GetRequiredService<IHttpClientFactory>().CreateClient();
                var apiKey = builder.Configuration["SMSService:ApiKey"] ?? "test-api-key";
                var apiUrl = builder.Configuration["SMSService:ApiUrl"] ?? "https://test-sms-api.com";
                var logger = provider.GetRequiredService<ILogger<SMSService>>();
                return new SMSService(httpClient, apiKey, apiUrl, logger, true);
            });

            // 비즈니스 서비스 계층 등록 - 비즈니스 로직 처리를 위한 서비스들
            builder.Services.AddScoped<MemberService>();           // 회원 관리 서비스
            builder.Services.AddScoped<MessageGroupService>();     // 메시지 그룹 관리 서비스
            builder.Services.AddScoped<AlarmService>();            // 알람 포인트 및 알람 트리거 서비스
            builder.Services.AddScoped<MessageService>();          // 메시지 관리 서비스

            // MVC 및 Razor Pages 추가 - 웹 UI 구성을 위한 프레임워크 등록
            builder.Services.AddControllersWithViews();
            builder.Services.AddRazorPages();

            var app = builder.Build();

            // 개발 환경 설정 - 환경에 따른 오류 처리 방식 구성
            if (app.Environment.IsDevelopment())
            {
                // 개발 환경에서는 상세한 오류 정보 표시
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // 프로덕션 환경에서는 일반적인 오류 페이지로 리다이렉트
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();  // HTTP Strict Transport Security 적용
            }

            // HTTPS 리다이렉션 - 보안 강화를 위해 HTTP 요청을 HTTPS로 리다이렉트
            app.UseHttpsRedirection();

            // 정적 파일 서비스 - CSS, JavaScript, 이미지 등의 정적 파일 제공
            app.UseStaticFiles();

            // 라우팅 미들웨어 등록 - URL 패턴에 따른 요청 처리
            app.UseRouting();

            // 인증 및 권한 부여 미들웨어 등록
            app.UseAuthorization();

            // 컨트롤러 라우트 설정 - 기본 라우팅 패턴 정의
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            // 애플리케이션 실행
            app.Run();
        }
    }
}
