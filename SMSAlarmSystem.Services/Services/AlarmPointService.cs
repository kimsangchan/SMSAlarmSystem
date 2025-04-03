using Microsoft.Extensions.Logging;
using SMSAlarmSystem.Core.Interfaces;
using SMSAlarmSystem.Core.Models;
using SMSAlarmSystem.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMSAlarmSystem.Services.Services
{
    /// <summary>
    /// 알람 포인트 서비스 구현
    /// </summary>
    public class AlarmPointService : IAlarmPointService
    {
        private readonly IExternalAlarmPointRepository _externalRepository;
        private readonly IAlarmPointRepository _repository;
        private readonly ILogger<AlarmPointService> _logger;

        /// <summary>
        /// 생성자
        /// </summary>
        /// <param name="externalRepository">외부 알람 포인트 리포지토리</param>
        /// <param name="repository">내부 알람 포인트 리포지토리</param>
        /// <param name="logger">로깅을 위한 ILogger 인스턴스</param>
        /// <exception cref="ArgumentNullException">필수 매개변수가 null인 경우</exception>
        public AlarmPointService(
            IExternalAlarmPointRepository externalRepository,
            IAlarmPointRepository repository,
            ILogger<AlarmPointService> logger)
        {
            _externalRepository = externalRepository ?? throw new ArgumentNullException(nameof(externalRepository));
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// 외부 데이터베이스에서 중복이 제거된 알람 포인트 목록을 조회합니다.
        /// </summary>
        /// <returns>알람 포인트 목록</returns>
        public async Task<IEnumerable<AlarmPoint>> GetExternalAlarmPointsAsync()
        {
            try
            {
                _logger.LogInformation("외부 데이터베이스에서 알람 포인트 조회 시작");

                // 외부 데이터베이스에서 알람 포인트 조회
                var externalAlarmPoints = await _externalRepository.GetDistinctAlarmPointsAsync();

                // null 체크
                if (externalAlarmPoints == null)
                {
                    _logger.LogWarning("외부 데이터베이스에서 알람 포인트를 조회할 수 없습니다.");
                    return new List<AlarmPoint>();
                }

                // 외부 알람 포인트를 내부 모델로 변환
                var alarmPoints = externalAlarmPoints.Select(ap => new AlarmPoint
                {
                    Name = ap.SystemName ?? "알 수 없는 포인트",
                    Description = string.IsNullOrEmpty(ap.ObjDesc)
                        ? ap.ObjName
                        : $"{ap.ObjName} - {ap.ObjDesc}",
                    Condition = $"상한값 = {ap.ObjAbove}, 하한 값 = {ap.ObjBelow}",
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                }).ToList();

                _logger.LogInformation("외부 데이터베이스에서 {Count}개의 알람 포인트를 조회했습니다.", alarmPoints.Count);
                return alarmPoints;
            }
            catch (Exception ex)
            {
                // 예외 로깅 및 재발생
                _logger.LogError(ex, "외부 알람 포인트 조회 중 오류 발생: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// 특정 시스템 코드와 부모 ID에 해당하는 알람 포인트 목록을 조회합니다.
        /// </summary>
        /// <param name="systemCode">시스템 코드</param>
        /// <param name="parentId">부모 ID</param>
        /// <returns>알람 포인트 목록</returns>
        public async Task<IEnumerable<AlarmPoint>> GetAlarmPointsBySystemCodeAndParentIdAsync(int systemCode, int parentId)
        {
            try
            {
                _logger.LogInformation("시스템 코드 {SystemCode}, 부모 ID {ParentId}에 대한 알람 포인트 조회 시작",
                    systemCode, parentId);

                // 유효성 검사
                if (systemCode <= 0 || parentId < 0)
                {
                    _logger.LogWarning("유효하지 않은 매개변수: 시스템 코드 {SystemCode}, 부모 ID {ParentId}",
                        systemCode, parentId);
                    return new List<AlarmPoint>();
                }

                // 외부 데이터베이스에서 알람 포인트 조회
                var externalAlarmPoints = await _externalRepository.GetAlarmPointsBySystemCodeAndParentIdAsync(systemCode, parentId);

                // null 체크
                if (externalAlarmPoints == null)
                {
                    _logger.LogWarning("시스템 코드 {SystemCode}, 부모 ID {ParentId}에 대한 알람 포인트를 조회할 수 없습니다.",
                        systemCode, parentId);
                    return new List<AlarmPoint>();
                }

                // 외부 알람 포인트를 내부 모델로 변환
                var alarmPoints = externalAlarmPoints.Select(ap => new AlarmPoint
                {
                    Name = ap.SystemName ?? "알 수 없는 포인트",
                    Description = string.IsNullOrEmpty(ap.ObjDesc)
                        ? ap.ObjName
                        : $"{ap.ObjName} - {ap.ObjDesc}",
                    Condition = $"상한값 = {ap.ObjAbove}, 하한 값 = {ap.ObjBelow}",
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                }).ToList();

                _logger.LogInformation("시스템 코드 {SystemCode}, 부모 ID {ParentId}에 대해 {Count}개의 알람 포인트를 조회했습니다.",
                    systemCode, parentId, alarmPoints.Count);
                return alarmPoints;
            }
            catch (Exception ex)
            {
                // 예외 로깅 및 재발생
                _logger.LogError(ex, "시스템 코드 {SystemCode}, 부모 ID {ParentId}에 대한 알람 포인트 조회 중 오류 발생: {Message}",
                    systemCode, parentId, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// 외부 데이터베이스에서 알람 포인트를 동기화합니다.
        /// </summary>
        /// <returns>동기화된 알람 포인트 수</returns>
        public async Task<int> SynchronizeAlarmPointsAsync()
        {
            try
            {
                _logger.LogInformation("알람 포인트 동기화 시작");

                // 외부 데이터베이스에서 알람 포인트 조회
                var externalAlarmPoints = await _externalRepository.GetDistinctAlarmPointsAsync();

                // null 체크
                if (externalAlarmPoints == null)
                {
                    _logger.LogWarning("외부 데이터베이스에서 알람 포인트를 조회할 수 없습니다.");
                    return 0;
                }

                int syncCount = 0;
                foreach (var externalPoint in externalAlarmPoints)
                {
                    // null 체크
                    if (string.IsNullOrEmpty(externalPoint.SystemName))
                    {
                        _logger.LogWarning("알람 포인트 이름이 비어 있어 건너뜁니다: ObjectSeq={ObjectSeq}", externalPoint.ObjectSeq);
                        continue;
                    }

                    // 내부 시스템에 맞게 알람 포인트 변환
                    var alarmPoint = new AlarmPoint
                    {
                        Name = externalPoint.ObjName,
                        Description = string.IsNullOrEmpty(externalPoint.ObjDesc)
                            ? externalPoint.ObjName
                            : $"{externalPoint.ObjName} - {externalPoint.ObjDesc}",
                        Condition = $"상한값 = {externalPoint.ObjAbove} AND 하한값 = {externalPoint.ObjBelow}",
                        IsActive = true,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };

                    try
                    {
                        // 이미 존재하는지 확인
                        var existingPoint = await _repository.GetByNameAsync(alarmPoint.Name);
                        if (existingPoint == null)
                        {
                            // 새로운 알람 포인트 추가
                            await _repository.AddAsync(alarmPoint);
                            _logger.LogInformation("새 알람 포인트 추가: {Name}", alarmPoint.Name);
                            syncCount++;
                        }
                        else
                        {
                            // 기존 알람 포인트 업데이트
                            existingPoint.Description = alarmPoint.Description;
                            existingPoint.Condition = alarmPoint.Condition;
                            existingPoint.UpdatedAt = DateTime.Now;
                            await _repository.UpdateAsync(existingPoint);
                            _logger.LogInformation("기존 알람 포인트 업데이트: {Name}", existingPoint.Name);
                            syncCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        // 개별 알람 포인트 처리 중 오류가 발생해도 계속 진행
                        _logger.LogError(ex, "알람 포인트 {Name} 처리 중 오류 발생: {Message}",
                            alarmPoint.Name, ex.Message);
                    }
                }

                _logger.LogInformation("알람 포인트 동기화 완료: {Count}개 처리됨", syncCount);
                return syncCount;
            }
            catch (Exception ex)
            {
                // 예외 로깅 및 재발생
                _logger.LogError(ex, "알람 포인트 동기화 중 오류 발생: {Message}", ex.Message);
                throw;
            }
        }
    }
}