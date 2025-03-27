// 작성자: Sangchan, Kim
// 작성일: 2025-03-27
// 기능: 메시지 데이터를 관리하는 리포지토리 클래스
// 설명: 이 클래스는 데이터베이스와 상호작용하여 메시지 데이터를 CRUD 방식으로 관리합니다.
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
    public class MessageRepository : Repository<Message>, IMessageRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MessageRepository> _logger;

        public MessageRepository(ApplicationDbContext context, ILogger<MessageRepository> logger)
            : base(context, logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Message>> GetMessagesByDateRangeAsync(DateTime start, DateTime end)
        {
            try
            {
                _logger.LogDebug($"날짜 범위로 메시지 조회 시작: {start} - {end}");
                return await _context.Messages
                    .Where(m => m.SendTime >= start && m.SendTime <= end)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"날짜 범위로 메시지 조회 중 오류 발생: {start} - {end}");
                return new List<Message>();
            }
        }

        public async Task<IEnumerable<Message>> GetMessagesByGroupIdAsync(int groupId)
        {
            try
            {
                _logger.LogDebug($"그룹 ID로 메시지 조회 시작: {groupId}");
                return await _context.Messages.Where(m => m.MessageGroupId == groupId).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"그룹 ID로 메시지 조회 중 오류 발생: {groupId}");
                return new List<Message>();
            }
        }

        public async Task<IEnumerable<Message>> GetMessagesByAlarmPointIdAsync(int alarmPointId)
        {
            try
            {
                _logger.LogDebug($"알람 포인트 ID로 메시지 조회 시작: {alarmPointId}");
                return await _context.Messages.Where(m => m.AlarmPointId == alarmPointId).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"알람 포인트 ID로 메시지 조회 중 오류 발생: {alarmPointId}");
                return new List<Message>();
            }
        }
    }
}
