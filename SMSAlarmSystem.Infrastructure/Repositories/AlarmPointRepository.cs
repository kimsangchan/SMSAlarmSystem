// 작성자: Sangchan, Kim
// 작성일: 2025-03-27
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
    public class AlarmPointRepository : Repository<AlarmPoint>, IAlarmPointRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AlarmPointRepository> _logger;

        public AlarmPointRepository(ApplicationDbContext context, ILogger<AlarmPointRepository> logger)
            : base(context, logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<AlarmPoint>> GetActiveAlarmPointsAsync()
        {
            try
            {
                _logger.LogDebug("활성화된 알람 포인트 조회 시작");
                return await _context.AlarmPoints.Where(ap => ap.IsActive).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "활성화된 알람 포인트 조회 중 오류 발생");
                return new List<AlarmPoint>();
            }
        }

        public async Task<IEnumerable<AlarmPoint>> GetAlarmPointsByGroupIdAsync(int groupId)
        {
            try
            {
                _logger.LogDebug($"그룹 ID로 알람 포인트 조회 시작: {groupId}");
                return await _context.AlarmPoints.Where(ap => ap.MessageGroupId == groupId).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"그룹 ID로 알람 포인트 조회 중 오류 발생: {groupId}");
                return new List<AlarmPoint>();
            }
        }
    }
}
