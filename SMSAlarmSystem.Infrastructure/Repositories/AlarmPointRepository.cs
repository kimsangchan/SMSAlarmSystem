// 작성자: Sangchan, Kim
// 작성일: 2025-03-31
// 기능: 알람 포인트 데이터를 관리하는 리포지토리 클래스
// 설명: 이 클래스는 데이터베이스와 상호작용하여 알람 포인트 데이터를 CRUD 방식으로 관리합니다.

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SMSAlarmSystem.Core.Interfaces;
using SMSAlarmSystem.Core.Models;
using SMSAlarmSystem.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SMSAlarmSystem.Infrastructure.Repositories
{
    /// <summary>
    /// 알람 포인트 데이터를 관리하는 리포지토리 클래스
    /// </summary>
    public class AlarmPointRepository : Repository<AlarmPoint>, IAlarmPointRepository
    {
        // 데이터베이스 컨텍스트 (부모 클래스의 필드를 가리기 위해 new 키워드 사용)
        private new readonly ApplicationDbContext _context;

        // 로거 인스턴스 (부모 클래스의 필드를 가리기 위해 new 키워드 사용)
        private new readonly ILogger<AlarmPointRepository> _logger;

        /// <summary>
        /// AlarmPointRepository 생성자
        /// </summary>
        /// <param name="context">데이터베이스 컨텍스트</param>
        /// <param name="logger">로깅을 위한 ILogger 인스턴스</param>
        /// <exception cref="ArgumentNullException">필수 매개변수가 null인 경우 발생</exception>
        public AlarmPointRepository(ApplicationDbContext context, ILogger<AlarmPointRepository> logger)
            : base(context, logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context), "데이터베이스 컨텍스트는 null이 될 수 없습니다.");
            _logger = logger ?? throw new ArgumentNullException(nameof(logger), "로거는 null이 될 수 없습니다.");
        }

        /// <summary>
        /// 활성화된 알람 포인트만 조회합니다.
        /// </summary>
        /// <returns>활성화된 알람 포인트 목록</returns>
        public async Task<IEnumerable<AlarmPoint>> GetActiveAlarmPointsAsync()
        {
            try
            {
                _logger.LogInformation("활성화된 알람 포인트 조회 시작");
                var alarmPoints = await _context.AlarmPoints
                    .Where(ap => ap.IsActive)
                    .OrderBy(ap => ap.Name)
                    .ToListAsync();

                _logger.LogInformation("활성화된 알람 포인트 조회 완료: {Count}개 조회됨", alarmPoints.Count);
                return alarmPoints;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "활성화된 알람 포인트 조회 중 오류 발생: {ErrorMessage}", ex.Message);
                throw; // 서비스 계층에서 예외 처리를 위해 예외를 다시 던짐
            }
        }

        /// <summary>
        /// 특정 그룹에 연결된 알람 포인트를 조회합니다.
        /// </summary>
        /// <param name="groupId">조회할 그룹 ID</param>
        /// <returns>해당 그룹에 연결된 알람 포인트 목록</returns>
        /// <exception cref="ArgumentException">유효하지 않은 그룹 ID인 경우 발생</exception>
        public async Task<IEnumerable<AlarmPoint>> GetAlarmPointsByGroupIdAsync(int groupId)
        {
            // ID 유효성 검사
            if (groupId <= 0)
            {
                throw new ArgumentException("그룹 ID는 0보다 커야 합니다.", nameof(groupId));
            }

            try
            {
                _logger.LogInformation("그룹 ID로 알람 포인트 조회 시작: GroupID={GroupId}", groupId);

                var alarmPoints = await _context.AlarmPoints
                    .Where(ap => ap.MessageGroupId == groupId)
                    .OrderBy(ap => ap.Name)
                    .ToListAsync();

                _logger.LogInformation("그룹 ID로 알람 포인트 조회 완료: GroupID={GroupId}, {Count}개 조회됨",
                    groupId, alarmPoints.Count);

                return alarmPoints;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "그룹 ID로 알람 포인트 조회 중 오류 발생: GroupID={GroupId}, 오류={ErrorMessage}",
                    groupId, ex.Message);
                throw;
            }
        }
    }
}
