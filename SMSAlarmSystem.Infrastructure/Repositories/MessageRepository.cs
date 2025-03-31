// 작성자: Sangchan, Kim
// 작성일: 2025-03-31
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
    /// <summary>
    /// 메시지 데이터를 관리하는 리포지토리 클래스
    /// </summary>
    public class MessageRepository : Repository<Message>, IMessageRepository
    {
        // 데이터베이스 컨텍스트 (부모 클래스의 필드를 가리기 위해 new 키워드 사용)
        private new readonly ApplicationDbContext _context;

        // 로거 인스턴스 (부모 클래스의 필드를 가리기 위해 new 키워드 사용)
        private new readonly ILogger<MessageRepository> _logger;

        /// <summary>
        /// MessageRepository 생성자
        /// </summary>
        /// <param name="context">데이터베이스 컨텍스트</param>
        /// <param name="logger">로깅을 위한 ILogger 인스턴스</param>
        /// <exception cref="ArgumentNullException">필수 매개변수가 null인 경우 발생</exception>
        public MessageRepository(ApplicationDbContext context, ILogger<MessageRepository> logger)
            : base(context, logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context), "데이터베이스 컨텍스트는 null이 될 수 없습니다.");
            _logger = logger ?? throw new ArgumentNullException(nameof(logger), "로거는 null이 될 수 없습니다.");
        }

        /// <summary>
        /// 특정 기간 내에 발송된 메시지를 조회합니다.
        /// </summary>
        /// <param name="start">조회 시작 날짜</param>
        /// <param name="end">조회 종료 날짜</param>
        /// <returns>해당 기간 내 발송된 메시지 목록</returns>
        /// <exception cref="ArgumentException">종료 날짜가 시작 날짜보다 이전인 경우 발생</exception>
        public async Task<IEnumerable<Message>> GetMessagesByDateRangeAsync(DateTime start, DateTime end)
        {
            // 날짜 범위 유효성 검사
            if (end < start)
            {
                throw new ArgumentException("종료 날짜는 시작 날짜보다 이후여야 합니다.", nameof(end));
            }

            try
            {
                _logger.LogInformation("날짜 범위로 메시지 조회 시작: {Start} - {End}", start, end);

                var messages = await _context.Messages
                    .Where(m => m.SendTime >= start && m.SendTime <= end)
                    .OrderByDescending(m => m.SendTime) // 최신 메시지부터 정렬
                    .ToListAsync();

                _logger.LogInformation("날짜 범위로 메시지 조회 완료: {Start} - {End}, {Count}개 메시지 조회됨",
                    start, end, messages.Count);

                return messages;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "날짜 범위로 메시지 조회 중 오류 발생: {Start} - {End}, 오류={ErrorMessage}",
                    start, end, ex.Message);
                throw; // 서비스 계층에서 예외 처리를 위해 예외를 다시 던짐
            }
        }

        /// <summary>
        /// 특정 그룹에 발송된 메시지를 조회합니다.
        /// </summary>
        /// <param name="groupId">조회할 그룹 ID</param>
        /// <returns>해당 그룹에 발송된 메시지 목록</returns>
        /// <exception cref="ArgumentException">유효하지 않은 그룹 ID인 경우 발생</exception>
        public async Task<IEnumerable<Message>> GetMessagesByGroupIdAsync(int groupId)
        {
            // ID 유효성 검사
            if (groupId <= 0)
            {
                throw new ArgumentException("그룹 ID는 0보다 커야 합니다.", nameof(groupId));
            }

            try
            {
                _logger.LogInformation("그룹 ID로 메시지 조회 시작: GroupID={GroupId}", groupId);

                var messages = await _context.Messages
                    .Where(m => m.MessageGroupId == groupId)
                    .OrderByDescending(m => m.SendTime) // 최신 메시지부터 정렬
                    .ToListAsync();

                _logger.LogInformation("그룹 ID로 메시지 조회 완료: GroupID={GroupId}, {Count}개 메시지 조회됨",
                    groupId, messages.Count);

                return messages;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "그룹 ID로 메시지 조회 중 오류 발생: GroupID={GroupId}, 오류={ErrorMessage}",
                    groupId, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// 특정 알람 포인트에서 발생한 메시지를 조회합니다.
        /// </summary>
        /// <param name="alarmPointId">조회할 알람 포인트 ID</param>
        /// <returns>해당 알람 포인트에서 발생한 메시지 목록</returns>
        /// <exception cref="ArgumentException">유효하지 않은 알람 포인트 ID인 경우 발생</exception>
        public async Task<IEnumerable<Message>> GetMessagesByAlarmPointIdAsync(int alarmPointId)
        {
            // ID 유효성 검사
            if (alarmPointId <= 0)
            {
                throw new ArgumentException("알람 포인트 ID는 0보다 커야 합니다.", nameof(alarmPointId));
            }

            try
            {
                _logger.LogInformation("알람 포인트 ID로 메시지 조회 시작: AlarmPointID={AlarmPointId}", alarmPointId);

                var messages = await _context.Messages
                    .Where(m => m.AlarmPointId == alarmPointId)
                    .OrderByDescending(m => m.SendTime) // 최신 메시지부터 정렬
                    .ToListAsync();

                _logger.LogInformation("알람 포인트 ID로 메시지 조회 완료: AlarmPointID={AlarmPointId}, {Count}개 메시지 조회됨",
                    alarmPointId, messages.Count);

                return messages;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "알람 포인트 ID로 메시지 조회 중 오류 발생: AlarmPointID={AlarmPointId}, 오류={ErrorMessage}",
                    alarmPointId, ex.Message);
                throw;
            }
        }
    }
}
