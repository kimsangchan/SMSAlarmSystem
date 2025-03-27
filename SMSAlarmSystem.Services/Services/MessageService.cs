// 작성자: Sangchan, Kim
// 작성일: 2025-03-27
// 기능: 메시지 관련 비즈니스 로직을 처리하는 서비스
// 설명: 메시지 CRUD 및 관련 기능을 제공합니다.
using Microsoft.Extensions.Logging;
using SMSAlarmSystem.Core.Interfaces;
using SMSAlarmSystem.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SMSAlarmSystem.Services.Services
{
    public class MessageService
    {
        private readonly IMessageRepository _messageRepository;
        private readonly ILogger<MessageService> _logger;

        public MessageService(IMessageRepository messageRepository, ILogger<MessageService> logger)
        {
            _messageRepository = messageRepository ?? throw new ArgumentNullException(nameof(messageRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<Message>> GetAllMessagesAsync()
        {
            try
            {
                _logger.LogInformation("모든 메시지 조회 시작");
                return await _messageRepository.GetAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "모든 메시지 조회 중 오류 발생");
                return new List<Message>();
            }
        }

        public async Task<Message?> GetMessageByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation("메시지 조회 시작: ID={Id}", id);
                return await _messageRepository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "메시지 조회 중 오류 발생: ID={Id}", id);
                return null;
            }
        }

        public async Task<IEnumerable<Message>> GetMessagesByDateRangeAsync(DateTime start, DateTime end)
        {
            try
            {
                _logger.LogInformation("날짜 범위로 메시지 조회 시작: Start={Start}, End={End}", start, end);
                return await _messageRepository.GetMessagesByDateRangeAsync(start, end);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "날짜 범위로 메시지 조회 중 오류 발생: Start={Start}, End={End}", start, end);
                return new List<Message>();
            }
        }

        public async Task<IEnumerable<Message>> GetMessagesByGroupIdAsync(int groupId)
        {
            try
            {
                _logger.LogInformation("그룹 ID로 메시지 조회 시작: GroupID={GroupId}", groupId);
                return await _messageRepository.GetMessagesByGroupIdAsync(groupId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "그룹 ID로 메시지 조회 중 오류 발생: GroupID={GroupId}", groupId);
                return new List<Message>();
            }
        }

        public async Task<IEnumerable<Message>> GetMessagesByAlarmPointIdAsync(int alarmPointId)
        {
            try
            {
                _logger.LogInformation("알람 포인트 ID로 메시지 조회 시작: AlarmPointID={AlarmPointId}", alarmPointId);
                return await _messageRepository.GetMessagesByAlarmPointIdAsync(alarmPointId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "알람 포인트 ID로 메시지 조회 중 오류 발생: AlarmPointID={AlarmPointId}", alarmPointId);
                return new List<Message>();
            }
        }

        public async Task<bool> AddMessageAsync(Message message)
        {
            if (message == null)
            {
                _logger.LogError("메시지 추가 실패: 메시지 객체가 null입니다.");
                return false;
            }

            try
            {
                _logger.LogInformation("메시지 추가 시작: Content={Content}", message.Content);
                await _messageRepository.AddAsync(message);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "메시지 추가 중 오류 발생: Content={Content}", message.Content);
                return false;
            }
        }

        public async Task<bool> UpdateMessageAsync(Message message)
        {
            if (message == null)
            {
                _logger.LogError("메시지 업데이트 실패: 메시지 객체가 null입니다.");
                return false;
            }

            try
            {
                _logger.LogInformation("메시지 업데이트 시작: ID={Id}, Status={Status}", message.Id, message.Status);
                await _messageRepository.UpdateAsync(message);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "메시지 업데이트 중 오류 발생: ID={Id}, Status={Status}", message.Id, message.Status);
                return false;
            }
        }

        public async Task<bool> DeleteMessageAsync(int id)
        {
            try
            {
                _logger.LogInformation("메시지 삭제 시작: ID={Id}", id);
                var message = await _messageRepository.GetByIdAsync(id);
                if (message == null)
                {
                    _logger.LogWarning("메시지 삭제 실패: 메시지를 찾을 수 없음, ID={Id}", id);
                    return false;
                }
                await _messageRepository.DeleteAsync(message);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "메시지 삭제 중 오류 발생: ID={Id}", id);
                return false;
            }
        }

        public async Task<bool> UpdateMessageStatusAsync(int id, string status)
        {
            if (string.IsNullOrEmpty(status))
            {
                _logger.LogError("메시지 상태 업데이트 실패: 상태가 null 또는 빈 문자열입니다.");
                return false;
            }

            try
            {
                _logger.LogInformation("메시지 상태 업데이트 시작: ID={Id}, Status={Status}", id, status);
                var message = await _messageRepository.GetByIdAsync(id);
                if (message == null)
                {
                    _logger.LogWarning("메시지 상태 업데이트 실패: 메시지를 찾을 수 없음, ID={Id}", id);
                    return false;
                }
                message.Status = status;
                await _messageRepository.UpdateAsync(message);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "메시지 상태 업데이트 중 오류 발생: ID={Id}, Status={Status}", id, status);
                return false;
            }
        }
    }
}
