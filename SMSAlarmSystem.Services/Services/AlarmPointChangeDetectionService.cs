// 작성자: Sangchan, Kim
// 작성일: 2025-04-03
// 기능: 외부 데이터베이스 알람 포인트 변경 감지 서비스
// 설명: IBSInfo 데이터베이스의 알람 포인트 변경사항을 감지하고 동기화합니다.

using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SMSAlarmSystem.Core.Interfaces;
using SMSAlarmSystem.Infrastructure.Data;

namespace SMSAlarmSystem.Services
{
    /// <summary>
    /// 외부 데이터베이스 알람 포인트 변경 감지 및 동기화 서비스
    /// </summary>
    public class AlarmPointChangeDetectionService : BackgroundService
    {
        private readonly ILogger<AlarmPointChangeDetectionService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly ExternalDbSettings _dbSettings;
        private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(30); // 30초마다 변경 확인

        /// <summary>
        /// 생성자
        /// </summary>
        /// <param name="logger">로거</param>
        /// <param name="serviceProvider">서비스 프로바이더</param>
        /// <param name="dbSettings">외부 데이터베이스 설정</param>
        public AlarmPointChangeDetectionService(
            ILogger<AlarmPointChangeDetectionService> logger,
            IServiceProvider serviceProvider,
            IOptions<ExternalDbSettings> dbSettings)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _dbSettings = dbSettings?.Value ?? throw new ArgumentNullException(nameof(dbSettings));
        }

        /// <summary>
        /// 백그라운드 서비스 실행 메서드
        /// </summary>
        /// <param name="stoppingToken">취소 토큰</param>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("알람 포인트 변경 감지 서비스 시작");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    bool hasChanges = await CheckForChangesAsync();
                    if (hasChanges)
                    {
                        await SynchronizeAlarmPointsAsync();
                    }
                    else
                    {
                        _logger.LogDebug("변경 사항 없음. 동기화 건너뜀.");
                    }

                    await Task.Delay(_pollingInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "알람 포인트 변경 감지 중 오류 발생: {Message}", ex.Message);
                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                }
            }

            _logger.LogInformation("알람 포인트 변경 감지 서비스 종료");
        }

        /// <summary>
        /// 변경 로그를 확인하고 처리하는 메서드
        /// </summary>
        private async Task<bool> CheckForChangesAsync()
        {
            if (string.IsNullOrEmpty(_dbSettings.ConnectionString))
            {
                _logger.LogError("외부 데이터베이스 연결 문자열이 구성되지 않았습니다.");
                return false;
            }

            using var connection = new SqlConnection(_dbSettings.ConnectionString);
            try
            {
                await connection.OpenAsync();

                var query = @"
                    SELECT COUNT(1)
                    FROM AlarmPointChangeLog
                    WHERE Processed = 0";

                using var command = new SqlCommand(query, connection);
                int changeCount = (int)await command.ExecuteScalarAsync();

                return changeCount > 0;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "외부 데이터베이스 연결 중 SQL 오류 발생: {Message}", ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "변경 로그 확인 중 예기치 않은 오류 발생: {Message}", ex.Message);
                return false;
            }
        }

       
        

        /// <summary>
        /// 알람 포인트를 동기화하는 메서드
        /// </summary>
        private async Task SynchronizeAlarmPointsAsync()
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var alarmPointService = scope.ServiceProvider.GetRequiredService<IAlarmPointService>();

                var syncCount = await alarmPointService.SynchronizeAlarmPointsAsync();
                _logger.LogInformation("알람 포인트 동기화 완료: {Count}개 처리됨", syncCount);

                await MarkLogsAsProcessedAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "알람 포인트 동기화 중 오류 발생: {Message}", ex.Message);
            }
        }

        /// <summary>
        /// 처리된 로그를 표시하는 메서드
        /// </summary>
        /// <param name="connection">데이터베이스 연결</param>
        private async Task MarkLogsAsProcessedAsync()
        {
            using var connection = new SqlConnection(_dbSettings.ConnectionString);
            try
            {
                await connection.OpenAsync();

                var query = "UPDATE AlarmPointChangeLog SET Processed = 1 WHERE Processed = 0";

                using var command = new SqlCommand(query, connection);
                var rowsAffected = await command.ExecuteNonQueryAsync();
                _logger.LogInformation("{Count}개의 로그를 처리됨으로 표시", rowsAffected);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "로그 처리 상태 업데이트 중 오류 발생: {Message}", ex.Message);
            }
        }
    }
}
