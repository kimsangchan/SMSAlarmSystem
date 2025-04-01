// 작성자: Sangchan, Kim
// 작성일: 2025-03-31
// 기능: 애플리케이션 시작점 및 서비스 구성
// 설명: 의존성 주입, 데이터베이스 연결, 미들웨어 등 애플리케이션 설정을 구성합니다.

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
            try
            {
                var builder = WebApplication.CreateBuilder(args);

                // 서비스 구성
                ConfigureServices(builder);

                var app = builder.Build();

                // 애플리케이션 파이프라인 구성
                ConfigureMiddleware(app);

                // 애플리케이션 실행
                app.Run();
            }
            catch (Exception ex)
            {
                // 애플리케이션 시작 중 발생한 예외 로깅
                Console.WriteLine($"애플리케이션 시작 중 오류 발생: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 애플리케이션 서비스 구성
        /// </summary>
        /// <param name="builder">웹 애플리케이션 빌더</param>
        private static void ConfigureServices(WebApplicationBuilder builder)
        {
            // 데이터베이스 컨텍스트 등록 - MSSQL 서버 연결 설정
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new InvalidOperationException("데이터베이스 연결 문자열이 구성되지 않았습니다. appsettings.json 파일을 확인하세요.");
                }
                options.UseSqlServer(connectionString);
            });

            // 리포지토리 계층 등록 - 의존성 주입을 위한 인터페이스와 구현체 매핑
            RegisterRepositories(builder.Services);

            // 서비스 계층 등록 - 비즈니스 로직 처리를 위한 서비스들
            RegisterServices(builder.Services, builder.Configuration);

            // MVC 및 Razor Pages 추가 - 웹 UI 구성을 위한 프레임워크 등록
            builder.Services.AddControllersWithViews();
            builder.Services.AddRazorPages();
        }

        /// <summary>
        /// 리포지토리 계층 등록
        /// </summary>
        /// <param name="services">서비스 컬렉션</param>
        private static void RegisterRepositories(IServiceCollection services)
        {
            // 각 도메인 모델에 대한 리포지토리 등록
            services.AddScoped<IMemberRepository, MemberRepository>();             // 회원 리포지토리
            services.AddScoped<IMessageGroupRepository, MessageGroupRepository>(); // 메시지 그룹 리포지토리
            services.AddScoped<IAlarmPointRepository, AlarmPointRepository>();     // 알람 포인트 리포지토리
            services.AddScoped<IMessageRepository, MessageRepository>();           // 메시지 리포지토리
        }

        /// <summary>
        /// 서비스 계층 등록
        /// </summary>
        /// <param name="services">서비스 컬렉션</param>
        /// <param name="configuration">애플리케이션 구성</param>
        private static void RegisterServices(IServiceCollection services, IConfiguration configuration)
        {
            // HttpClient 등록 - SMS 서비스에서 외부 API 호출에 사용
            services.AddHttpClient();

            // SMS 서비스 등록 - 인터페이스와 구현체 매핑 (수정된 부분)
            services.AddSingleton<ISMSService>(provider => {
                var httpClient = provider.GetRequiredService<IHttpClientFactory>().CreateClient();
                var logger = provider.GetRequiredService<ILogger<SMSService>>();

                // 구성에서 API 키와 URL 가져오기 (null 체크 포함)
                var apiKey = configuration["SMSService:ApiKey"];
                if (string.IsNullOrEmpty(apiKey))
                {
                    logger.LogWarning("SMS API 키가 구성되지 않았습니다. 테스트 키를 사용합니다.");
                    apiKey = "test-api-key";
                }

                var apiUrl = configuration["SMSService:ApiUrl"];
                if (string.IsNullOrEmpty(apiUrl))
                {
                    logger.LogWarning("SMS API URL이 구성되지 않았습니다. 테스트 URL을 사용합니다.");
                    apiUrl = "https://test-sms-api.com";
                }

                // 테스트 모드 설정 (기본값: true)
                var useTestMode = configuration.GetValue<bool>("SMSService:UseTestMode", true);

                logger.LogInformation("SMS 서비스 구성: API URL={ApiUrl}, 테스트 모드={TestMode}", apiUrl, useTestMode);

                return new SMSService(httpClient, apiKey, apiUrl, logger, useTestMode);
            });

            // 비즈니스 서비스 계층 등록
            services.AddScoped<IMemberService, MemberService>();                   // 회원 관리 서비스
            services.AddScoped<IMessageGroupService, MessageGroupService>();       // 메시지 그룹 관리 서비스
            services.AddScoped<IAlarmService, AlarmService>();                     // 알람 포인트 및 알람 트리거 서비스
            services.AddScoped<IMessageService, MessageService>();                 // 메시지 서비스
            services.AddScoped(typeof(PaginationHelper<>));                         // 페이지네이션 헬퍼 등록
        }

        /// <summary>
        /// 애플리케이션 미들웨어 구성
        /// </summary>
        /// <param name="app">웹 애플리케이션</param>
        private static void ConfigureMiddleware(WebApplication app)
        {
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
        }
    }
}
