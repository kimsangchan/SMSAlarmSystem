// 작성자: Sangchan, Kim
// 작성일: 2025-04-03
// 기능: 외부 데이터베이스 알람 포인트 초기 등록 및 동기화
// 설명: IBSInfo 데이터베이스의 알람 포인트를 안전하게 가져와 등록하고 동기화합니다.
//       외래 키 제약 조건을 고려하여 유효한 MessageGroupId를 사용합니다.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SMSAlarmSystem.Core.Interfaces;
using SMSAlarmSystem.Core.Models;
using SMSAlarmSystem.Infrastructure.Repositories;

namespace SMSAlarmSystem.Services
{
    /// <summary>
    /// 외부 데이터베이스 알람 포인트 초기 등록 및 동기화 서비스
    /// </summary>
    public class AlarmPointSyncService : BackgroundService
    {
        private readonly ILogger<AlarmPointSyncService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _syncInterval = TimeSpan.FromMinutes(15); // 15분마다 동기화
        private bool _initialSyncCompleted = false;

        /// <summary>
        /// 생성자
        /// </summary>
        /// <param name="logger">로깅을 위한 ILogger 인스턴스</param>
        /// <param name="serviceProvider">서비스 제공자</param>
        /// <exception cref="ArgumentNullException">필수 매개변수가 null인 경우</exception>
        public AlarmPointSyncService(
            ILogger<AlarmPointSyncService> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }
        /// <summary>
        /// 백그라운드 서비스 실행 메서드 - BackgroundService 추상 클래스에서 요구하는 필수 구현 메서드
        /// </summary>
        /// <param name="stoppingToken">서비스 중지 요청을 나타내는 취소 토큰</param>
        /// <returns>비동기 작업</returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("알람 포인트 동기화 서비스 시작");

            try
            {
                // 애플리케이션 시작 시 초기 동기화 수행
                await PerformInitialSyncAsync();

                // 주기적 동기화 루프
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        // 초기 동기화가 완료된 경우에만 주기적 동기화 수행
                        if (_initialSyncCompleted)
                        {
                            await SynchronizeAlarmPointsAsync();
                        }

                        // 다음 동기화까지 대기
                        await Task.Delay(_syncInterval, stoppingToken);
                    }
                    catch (OperationCanceledException)
                    {
                        // 취소 요청은 정상 종료로 처리
                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "주기적 알람 포인트 동기화 중 오류 발생: {Message}", ex.Message);

                        // 오류 발생 시 짧은 대기 후 재시도
                        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "알람 포인트 동기화 서비스 실행 중 치명적 오류 발생: {Message}", ex.Message);
            }

            _logger.LogInformation("알람 포인트 동기화 서비스 종료");
        }
        // 작성자: Sangchan, Kim
        // 작성일: 2025-04-03
        // 기능: 알람 포인트 동기화 서비스
        // 설명: 외부 데이터베이스의 알람 포인트를 내부 시스템으로 안전하게 동기화합니다.
        //       중복 항목 및 외래 키 제약 조건을 고려하여 예외 상황을 처리합니다.

        /// <summary>
        /// 애플리케이션 시작 시 초기 동기화를 수행합니다.
        /// </summary>
        /// <returns>비동기 작업</returns>
        private async Task PerformInitialSyncAsync()
        {
            try
            {
                _logger.LogInformation("알람 포인트 초기 동기화 시작");

                // 스코프 생성
                using (var scope = _serviceProvider.CreateScope())
                {
                    // 필요한 서비스 가져오기
                    var externalRepository = scope.ServiceProvider.GetRequiredService<IExternalAlarmPointRepository>();
                    var alarmPointRepository = scope.ServiceProvider.GetRequiredService<IAlarmPointRepository>();
                    var messageGroupRepository = scope.ServiceProvider.GetRequiredService<IMessageGroupRepository>();

                    // 컨텍스트 캐시 초기화 (Entity Framework 캐시 문제 해결)
                    if (alarmPointRepository is AlarmPointRepository repository)
                    {
                        repository.ClearContext();
                        _logger.LogInformation("Entity Framework 컨텍스트 캐시를 초기화했습니다.");
                    }

                    // 유효한 메시지 그룹 확인 또는 생성
                    var defaultGroupId = await EnsureDefaultMessageGroupAsync(messageGroupRepository);
                    if (defaultGroupId <= 0)
                    {
                        _logger.LogError("유효한 메시지 그룹을 생성할 수 없습니다. 알람 포인트 동기화를 중단합니다.");
                        return;
                    }

                    // 외부 데이터베이스에서 모든 알람 포인트 조회
                    var externalAlarmPoints = await externalRepository.GetDistinctAlarmPointsAsync();

                    // null 체크
                    if (externalAlarmPoints == null || !externalAlarmPoints.Any())
                    {
                        _logger.LogWarning("외부 데이터베이스에서 알람 포인트를 조회할 수 없거나 결과가 없습니다.");
                        return;
                    }

                    // 현재 등록된 알람 포인트 조회 (AsNoTracking 사용으로 캐시 문제 방지)
                    var existingAlarmPoints = await alarmPointRepository.GetAllAsNoTrackingAsync();
                    var existingNames = existingAlarmPoints.Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);

                    int addedCount = 0;
                    int skippedCount = 0;

                    // 외부 알람 포인트 등록
                    foreach (var externalPoint in externalAlarmPoints)
                    {
                        // null 또는 빈 문자열 체크
                        if (string.IsNullOrEmpty(externalPoint.ObjName))
                        {
                            _logger.LogWarning("알람 포인트 이름이 비어 있어 건너뜁니다: ObjectSeq={ObjectSeq}", externalPoint.ObjectSeq);
                            skippedCount++;
                            continue;
                        }

                        // 이미 존재하는 알람 포인트는 건너뜀 (대소문자 구분 없이 비교)
                        if (existingNames.Contains(externalPoint.ObjName))
                        {
                            _logger.LogDebug("알람 포인트 '{Name}'은(는) 이미 존재하여 건너뜁니다.", externalPoint.ObjName);
                            skippedCount++;
                            continue;
                        }

                        try
                        {
                            // 새 알람 포인트 생성 및 등록
                            var newAlarmPoint = new AlarmPoint
                            {
                                Name = externalPoint.ObjName,
                                Description = string.IsNullOrEmpty(externalPoint.ObjDesc)
                                    ? externalPoint.ObjName
                                    : $"{externalPoint.ObjName} - {externalPoint.ObjDesc}",
                                Condition = $"상한값 = {externalPoint.ObjAbove} AND 하한값 = {externalPoint.ObjBelow}",
                                IsActive = true,
                                ExternalId = externalPoint.ObjectId.ToString(),
                                CreatedAt = DateTime.Now,
                                UpdatedAt = DateTime.Now,
                                MessageGroupId = defaultGroupId
                            };

                            // 중복 체크 없이 직접 추가 (이미 위에서 중복 체크를 수행했음)
                            await alarmPointRepository.AddWithoutDuplicateCheckAsync(newAlarmPoint);
                            addedCount++;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "알람 포인트 '{Name}' 추가 중 오류 발생: {Message}",
                                externalPoint.ObjName, ex.Message);
                        }
                    }

                    _logger.LogInformation("알람 포인트 초기 동기화 완료: {AddedCount}개 추가됨, {SkippedCount}개 건너뜀",
                        addedCount, skippedCount);
                    _initialSyncCompleted = true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "알람 포인트 초기 동기화 중 오류 발생: {Message}", ex.Message);
                _initialSyncCompleted = false;
            }
        }

        /// <summary>
        /// 기본 메시지 그룹이 존재하는지 확인하고, 없으면 생성합니다.
        /// </summary>
        /// <param name="messageGroupRepository">메시지 그룹 리포지토리</param>
        /// <returns>기본 메시지 그룹 ID</returns>
        private async Task<int> EnsureDefaultMessageGroupAsync(IMessageGroupRepository messageGroupRepository)
        {
            try
            {
                // 모든 메시지 그룹 조회
                var messageGroups = await messageGroupRepository.GetAllAsync();

                // 기본 그룹 찾기
                var defaultGroup = messageGroups.FirstOrDefault();

                // 메시지 그룹이 없으면 기본 그룹 생성
                if (defaultGroup == null)
                {
                    _logger.LogInformation("기본 메시지 그룹이 없습니다. 새 그룹을 생성합니다.");

                    var newGroup = new MessageGroup
                    {
                        Name = "기본 그룹",
                        Description = "시스템에서 자동 생성된 기본 그룹",
                        IsActive = true,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };

                    await messageGroupRepository.AddAsync(newGroup);

                    // 생성된 그룹 ID 확인
                    var createdGroups = await messageGroupRepository.GetAllAsync();
                    defaultGroup = createdGroups.FirstOrDefault();

                    if (defaultGroup == null)
                    {
                        _logger.LogError("기본 메시지 그룹 생성 후에도 그룹을 찾을 수 없습니다.");
                        return -1;
                    }
                }

                _logger.LogInformation("기본 메시지 그룹 ID: {GroupId}", defaultGroup.Id);
                return defaultGroup.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "기본 메시지 그룹 확인 중 오류 발생: {Message}", ex.Message);
                return -1;
            }
        }

        /// <summary>
        /// 기본 메시지 그룹 ID를 조회합니다. 없으면 생성합니다.
        /// </summary>
        /// <param name="messageGroupRepository">메시지 그룹 리포지토리</param>
        /// <returns>메시지 그룹 ID</returns>
        private async Task<int> GetDefaultMessageGroupIdAsync(IMessageGroupRepository messageGroupRepository)
        {
            try
            {
                // 모든 메시지 그룹 조회
                var messageGroups = await messageGroupRepository.GetAllAsync();

                // 기본 그룹 찾기
                var defaultGroup = messageGroups.FirstOrDefault();

                // 메시지 그룹이 없으면 기본 그룹 생성
                if (defaultGroup == null)
                {
                    _logger.LogInformation("기본 메시지 그룹이 없습니다. 새 그룹을 생성합니다.");

                    var newGroup = new MessageGroup
                    {
                        Name = "기본 그룹",
                        Description = "시스템에서 자동 생성된 기본 그룹",
                        IsActive = true,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };

                    await messageGroupRepository.AddAsync(newGroup);
                    return newGroup.Id;
                }

                return defaultGroup.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "기본 메시지 그룹 조회 중 오류 발생: {Message}", ex.Message);
                return -1; // 오류 발생 시 -1 반환
            }
        }

        // 작성자: Sangchan, Kim
        // 작성일: 2025-04-03
        // 기능: 알람 포인트 동기화
        // 설명: 외부 데이터베이스의 알람 포인트를 내부 데이터베이스와 동기화합니다.

        private async Task SynchronizeAlarmPointsAsync()
        {
            try
            {
                _logger.LogInformation("알람 포인트 동기화 시작");

                using (var scope = _serviceProvider.CreateScope())
                {
                    var externalRepository = scope.ServiceProvider.GetRequiredService<IExternalAlarmPointRepository>();
                    var alarmPointRepository = scope.ServiceProvider.GetRequiredService<IAlarmPointRepository>();

                    var externalAlarmPoints = await externalRepository.GetDistinctAlarmPointsAsync();
                    if (externalAlarmPoints == null)
                    {
                        _logger.LogWarning("외부 데이터베이스에서 알람 포인트를 조회할 수 없습니다.");
                        return;
                    }

                    int syncCount = 0;
                    foreach (var externalPoint in externalAlarmPoints)
                    {
                        if (string.IsNullOrEmpty(externalPoint.ObjName))
                        {
                            _logger.LogWarning("알람 포인트 이름이 비어 있어 건너뜁니다: ObjectSeq={ObjectSeq}", externalPoint.ObjectSeq);
                            continue;
                        }

                        try
                        {
                            var alarmPoint = new AlarmPoint
                            {
                                Name = externalPoint.ObjName,
                                Description = string.IsNullOrEmpty(externalPoint.ObjDesc) ? externalPoint.ObjName : $"{externalPoint.ObjName} - {externalPoint.ObjDesc}",
                                Condition = $"상한값 = {externalPoint.ObjAbove} AND 하한값 = {externalPoint.ObjBelow}",
                                IsActive = true,
                                ExternalId = externalPoint.ObjectId.ToString(),
                                MessageGroupId = 1 // 기본 메시지 그룹 ID, 필요에 따라 조정
                            };

                            await alarmPointRepository.UpdateOrAddAsync(alarmPoint);
                            syncCount++;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "알람 포인트 '{Name}' 처리 중 오류 발생: {Message}", externalPoint.ObjName, ex.Message);
                        }
                    }

                    _logger.LogInformation("알람 포인트 동기화 완료: {Count}개 처리됨", syncCount);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "알람 포인트 동기화 중 오류 발생: {Message}", ex.Message);
            }
        }

    }
}